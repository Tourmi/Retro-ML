using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using ReactiveUI;
using SharpNeat.BlackBox;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.ViewModels.Neural
{
    internal class NetworkViewModel : ViewModelBase
    {
        public struct Node
        {
            public static int NodeSize => NetworkViewModel.NodeSize;
            public static int GridSize => NetworkViewModel.GridSize;
            public bool Active { get; set; }
            public double PositionX { get; set; }
            public double PositionY { get; set; }
        }

        public class NodeGroupViewModel : ViewModelBase
        {
            public static int NodeSize => NetworkViewModel.NodeSize;
            public static int GridSize => NetworkViewModel.GridSize;
            public static int LeftOffset => NetworkViewModel.LeftOffset;

            public NodeGroupViewModel(string name, bool useRed = false, int gridWidth = 1, int gridHeight = 1)
            {
                Name = name;
                UseRed = useRed;
                GridWidth = gridWidth;
                GridHeight = gridHeight;
                Nodes = new ObservableCollection<Node>();
                for (int i = 0; i < gridWidth * gridHeight; i++)
                {
                    Nodes.Add(new Node());
                }
            }

            public string Name { get; }
            public bool UseRed { get; }
            public int GridWidth { get; }
            public int GridHeight { get; }

            public ObservableCollection<Node> Nodes { get; set; }
        }

        public class ConnectionViewModel : ViewModelBase
        {
            private Point lineStartPoint;
            private Point lineEndPoint;
            private bool positive;
            private double intensity;

            public ConnectionViewModel(Point startPoint, Point endPoint, bool positive, double intensity)
            {
                LineStartPoint = startPoint;
                LineEndPoint = endPoint;
                Positive = positive;
                Intensity = intensity;
            }

            public Point LineStartPoint
            {
                get => lineStartPoint;
                set => this.RaiseAndSetIfChanged(ref lineStartPoint, value);
            }
            public Point LineEndPoint
            {
                get => lineEndPoint;
                set => this.RaiseAndSetIfChanged(ref lineEndPoint, value);
            }
            public bool Positive
            {
                get => positive;
                set => this.RaiseAndSetIfChanged(ref positive, value);
            }
            public double Intensity
            {
                get => intensity;
                set
                {
                    this.RaiseAndSetIfChanged(ref intensity, value);
                }
            }
        }
        public static int TotalWidth => 1800;
        public static int TotalHeight => 800;

        public static int NodeSize => 14;
        public static int GridSize => 16;
        public static int LeftOffset => 100;
        public static int SpacingBetweenInputs => 5;

        private static readonly Point middleOfNode = new Point(GridSize / 2.0, GridSize / 2.0);

        private bool runningTopologyUpdate = false;
        private bool runningUpdate = false;
        private (int sourceNode, int targetNode, double weight)[][]? currConnectionLayers;

        public NetworkViewModel() : this(new NeuralConfig())
        {

        }

        public NetworkViewModel(NeuralConfig config)
        {
            Inputs = new ObservableCollection<NodeGroupViewModel>();

            foreach (var node in config.InputNodes)
            {
                if (!node.ShouldUse) continue;
                Inputs.Add(new NodeGroupViewModel(node.Name, node.Name == "Dangers", node.TotalWidth, node.TotalHeight));
            }

            Outputs = new ObservableCollection<NodeGroupViewModel>();
            foreach (var node in config.OutputNodes)
            {
                if (!node.ShouldUse) continue;
                Outputs.Add(new(node.Name));
            }

            MiddleNodes = new ObservableCollection<Node>();
            Connections = new ObservableCollection<ConnectionViewModel>();
        }

        public void UpdateTopology((int sourceNode, int targetNode, double weight)[][] connectionLayers, int[] outputIds)
        {
            if (runningTopologyUpdate) return;
            if (IsSameTopology(connectionLayers)) return;
            runningTopologyUpdate = true;

            currConnectionLayers = connectionLayers;
            Dispatcher.UIThread.Post(() =>
            {
                using var delay = DelayChangeNotifications();

                MiddleNodes.Clear();
                Connections.Clear();

                var inputPositions = GetPositions(true);
                var outputPositions = GetPositions(false);

                Dictionary<int, Point> nodePositions = new();
                for (int i = 0; i < inputPositions.Length; i++)
                {
                    nodePositions[i] = inputPositions[i];
                }
                for (int i = 0; i < outputPositions.Length; i++)
                {
                    nodePositions[outputIds[i]] = outputPositions[i];
                }

                double middleXOffset = currConnectionLayers.Length > 2 ? (TotalWidth / 3.0) / currConnectionLayers.Length - 2 : 0;
                double middleStartX = TotalWidth / 3.0 - middleXOffset; //We subtract one offset, since layer 0 is usually the input nodes

                List<(int source, int target, double weight)> connectionsToAdd = new();

                for (int i = 0; i < currConnectionLayers.Length; i++)
                {
                    var currLayer = currConnectionLayers[i];

                    for (int j = 0; j < currLayer.Length; j++)
                    {
                        var (sourceNode, targetNode, weight) = currLayer[j];

                        if (!nodePositions.ContainsKey(sourceNode))
                        {
                            nodePositions[sourceNode] = new Point(middleStartX + i * middleXOffset, Random.Shared.NextDouble() * (TotalHeight - GridSize) + GridSize);
                            MiddleNodes.Add(new Node() { PositionX = nodePositions[sourceNode].X, PositionY = nodePositions[sourceNode].Y });
                        }

                        connectionsToAdd.Add((sourceNode, targetNode, weight));
                    }
                }

                foreach (var (source, target, weight) in connectionsToAdd)
                {
                    //If the target doesn't exist, put it at the right
                    if (!nodePositions.ContainsKey(target))
                    {
                        nodePositions[target] = new Point(middleStartX + (currConnectionLayers.Length - 1) * middleXOffset, Random.Shared.NextDouble() * (TotalHeight - GridSize) + GridSize);
                        MiddleNodes.Add(new Node() { PositionX = nodePositions[target].X, PositionY = nodePositions[target].Y });
                    }

                    Connections.Add(new ConnectionViewModel(nodePositions[source] + middleOfNode, nodePositions[target] + middleOfNode, weight >= 0, Math.Abs(weight) / 5.0 + 0.3));
                }

                runningTopologyUpdate = false;
            });
        }

        private bool IsSameTopology((int sourceNode, int targetNode, double weight)[][] connectionLayers)
        {
            if (currConnectionLayers == null) return false;
            if (connectionLayers.Length != currConnectionLayers.Length) return false;
            for (int i = 0; i < connectionLayers.Length; i++)
            {
                if (connectionLayers[i].Length != currConnectionLayers[i].Length) return false;
                for (int j = 0; j < connectionLayers[i].Length; j++)
                {
                    var (sourceNode, targetNode, weight) = connectionLayers[i][j];
                    var (currSourceNode, currTargetNode, currWeight) = currConnectionLayers[i][j];

                    if (sourceNode != currSourceNode) return false;
                    if (targetNode != currTargetNode) return false;
                    if (weight >= 0 != currWeight >= 0) return false;
                }
            }

            return true;
        }

        private Point[] GetPositions(bool isInput)
        {
            double initialX = -LeftOffset + TotalWidth - GridSize;
            double offsetX = -GridSize;
            var nodeGroup = Outputs;
            if (isInput)
            {
                initialX = LeftOffset;
                offsetX = GridSize;
                nodeGroup = Inputs;
            }

            Point[] result = new Point[nodeGroup.Aggregate(0, (total, next) => total + next.Nodes.Count)];

            double initialY = 0;

            int currIndex = 0;

            for (int i = 0; i < nodeGroup.Count; i++)
            {
                var group = nodeGroup[i];

                double currY = initialY;

                for (int y = 0; y < group.GridHeight; y++)
                {
                    double currX = initialX;
                    for (int x = 0; x < group.GridWidth; x++)
                    {
                        result[currIndex++] = new Point(currX, currY);

                        currX += offsetX;
                    }
                    currY += GridSize;
                }

                initialY += GridSize * group.GridHeight;
                initialY += SpacingBetweenInputs;
            }

            return result;
        }

        public void UpdateNodes(double[] inputs, double[] outputs)
        {
            if (runningUpdate) return;
            runningUpdate = true;
            Dispatcher.UIThread.Post(() =>
            {
                int startIndex = 0;
                using var delay = DelayChangeNotifications();
                foreach (var nodeGroup in Inputs)
                {
                    for (int i = 0; i < nodeGroup.Nodes.Count; i++)
                    {
                        bool shouldBeActive = inputs[i + startIndex] > 0;
                        if (nodeGroup.Nodes[i].Active != shouldBeActive)
                        {
                            nodeGroup.Nodes[i] = new Node() { Active = shouldBeActive };
                        }
                    }

                    startIndex += nodeGroup.Nodes.Count;
                }
                startIndex = 0;
                foreach (var nodeGroup in Outputs)
                {
                    for (int i = 0; i < nodeGroup.Nodes.Count; i++)
                    {
                        bool shouldBeActive = outputs[i + startIndex] > 0;
                        if (nodeGroup.Nodes[i].Active != shouldBeActive)
                        {
                            nodeGroup.Nodes[i] = new Node() { Active = shouldBeActive };
                        }
                    }

                    startIndex += nodeGroup.Nodes.Count;
                }
                runningUpdate = false;
            });
        }

        public ObservableCollection<NodeGroupViewModel> Inputs { get; }
        public ObservableCollection<Node> MiddleNodes { get; }
        public ObservableCollection<NodeGroupViewModel> Outputs { get; }
        public ObservableCollection<ConnectionViewModel> Connections { get; }
    }
}
