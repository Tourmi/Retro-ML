using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using Retro_ML.Configuration;
using Retro_ML.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Retro_ML.Application.ViewModels.Neural
{
    /// <summary>
    /// View model for the network view
    /// </summary>
    internal class NetworkViewModel : ViewModelBase
    {
        private const int MAX_CONNECTIONS = 1000;

        public struct Node
        {
            public static int NodeSize => NetworkViewModel.NodeSize;
            public static int GridSize => NetworkViewModel.GridSize;

            public Node() : this(0, DefaultActiveColor) { }
            public Node(double value, Color targetColor) : this(value, targetColor, 0, 0) { }

            public Node(double value, Color targetColor, double positionX, double positionY)
            {
                Value = value;

                Color currColor;
                if (value <= -1) currColor = NegativeColor;
                else if (value >= 1) currColor = targetColor;
                else if (value < 0)
                {
                    byte r = (byte)(BaseColor.R + (NegativeColor.R - BaseColor.R) * value);
                    byte g = (byte)(BaseColor.G + (NegativeColor.G - BaseColor.G) * value);
                    byte b = (byte)(BaseColor.B + (NegativeColor.B - BaseColor.B) * value);

                    currColor = Color.FromRgb(r, g, b);
                }
                else
                {
                    byte r = (byte)(BaseColor.R + (targetColor.R - BaseColor.R) * value);
                    byte g = (byte)(BaseColor.G + (targetColor.G - BaseColor.G) * value);
                    byte b = (byte)(BaseColor.B + (targetColor.B - BaseColor.B) * value);

                    currColor = Color.FromRgb(r, g, b);
                }

                NodeBrush = Brush.Parse(currColor.ToString());
                PositionX = positionX;
                PositionY = positionY;
            }

            public double Value { get; }
            public IBrush NodeBrush { get; }
            public double PositionX { get; }
            public double PositionY { get; }
        }

        public struct Connection
        {
            public Connection(Point startPoint, Point endPoint, bool positive, double intensity)
            {
                LineStartPoint = startPoint;
                LineEndPoint = endPoint;
                Positive = positive;
                Intensity = intensity;
            }

            public Point LineStartPoint { get; }
            public Point LineEndPoint { get; }
            public bool Positive { get; }
            public double Intensity { get; }
        }

        public class NodeGroupViewModel : ViewModelBase
        {
            public static int NodeSize => NetworkViewModel.NodeSize;
            public static int GridSize => NetworkViewModel.GridSize;
            public static int LeftOffset => NetworkViewModel.LeftOffset;

            public NodeGroupViewModel(string name, Color targetColor, int gridWidth = 1, int gridHeight = 1, bool isOutput = false)
            {
                Name = name;

                TargetColor = targetColor;
                GridWidth = gridWidth;
                GridHeight = gridHeight;
                Nodes = new ObservableCollection<Node>();
                for (int i = 0; i < gridWidth * gridHeight; i++)
                {
                    Nodes.Add(new Node());
                }

                IsOutput = isOutput;
            }

            public string Name { get; }
            public Color TargetColor { get; }
            public int GridWidth { get; }
            public int GridHeight { get; }
            public bool IsOutput { get; }

            public ObservableCollection<Node> Nodes { get; set; }
        }
        public static Color BaseColor = Color.Parse("#444");
        public static Color MiddleNodeColor = Color.Parse("#ACA");
        public static Color NegativeColor = Color.Parse("#000");
        public static Color DefaultActiveColor = Color.Parse("#EEE");
        public static Color DangerActiveColor = Color.Parse("#E00");
        public static Color GoodiesActiveColor = Color.Parse("#0E0");
        public static Color WaterActiveColor = Color.Parse("#00E");

        public static int TotalWidth => 1800;
        public static int TotalHeight => 800;

        public static int GridSize => 12;
        public static int NodeSize => GridSize - 2;
        public static int LeftOffset => 100;
        public static int SpacingBetweenInputs => 5;

        private static readonly Point middleOfNode = new Point(GridSize / 2.0, GridSize / 2.0);

        private bool runningTopologyUpdate = false;
        private bool runningUpdate = false;
        private (int sourceNode, int targetNode, double weight)[][]? currConnectionLayers;

        /// <summary>
        /// Only exists so the XAML designer can work.
        /// </summary>
        public NetworkViewModel() : this(new NeuralConfig())
        {

        }

        public NetworkViewModel(NeuralConfig config)
        {
            Inputs = new ObservableCollection<NodeGroupViewModel>();
            Outputs = new ObservableCollection<NodeGroupViewModel>();
            MiddleNodes = new ObservableCollection<Node>();
            Connections = new ObservableCollection<Connection>();

            Dispatcher.UIThread.Post(() =>
            {
                using var delay = DelayChangeNotifications();
                foreach (var node in config.InputNodes)
                {
                    if (!node.ShouldUse) continue;
                    Color targetColor = DefaultActiveColor;
                    if (node.Name.Contains("Danger")) targetColor = DangerActiveColor;
                    if (node.Name.Contains("Goodies")) targetColor = GoodiesActiveColor;
                    if (node.Name.Contains("Water")) targetColor = WaterActiveColor;

                    Inputs.Add(new NodeGroupViewModel(node.Name, targetColor, node.TotalWidth, node.TotalHeight));
                }

                foreach (var node in config.OutputNodes)
                {
                    if (!node.ShouldUse) continue;
                    Outputs.Add(new(node.Name, DefaultActiveColor, isOutput: true));
                }
            });
        }

        /// <summary>
        /// Updates the connections as well as the middle nodes of the neural network. Does not update them if the current topology is the same as the given one
        /// </summary>
        /// <param name="connectionLayers"></param>
        /// <param name="outputIds"></param>
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

                if (!CheckTooComplex(connectionLayers))
                {
                    UpdateTopologyUI(connectionLayers, outputIds);
                }
                runningTopologyUpdate = false;
            });
        }

        private void UpdateTopologyUI((int sourceNode, int targetNode, double weight)[][] connectionLayers, int[] outputIds)
        {
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

            double middleXOffset = currConnectionLayers!.Length > 2 ? TotalWidth / 3.0 / currConnectionLayers.Length - 2 : 0;
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
                        nodePositions[sourceNode] = new Point(middleStartX + i * middleXOffset, Random.Shared.NextDouble() * (TotalHeight - GridSize));
                        MiddleNodes.Add(new Node(1, MiddleNodeColor, nodePositions[sourceNode].X, nodePositions[sourceNode].Y));
                    }

                    connectionsToAdd.Add((sourceNode, targetNode, weight));
                }
            }

            // Add the connections
            foreach (var (source, target, weight) in connectionsToAdd)
            {
                //If the target doesn't exist, put it at the right
                if (!nodePositions.ContainsKey(target))
                {
                    nodePositions[target] = new Point(middleStartX + (currConnectionLayers.Length - 1) * middleXOffset, Random.Shared.NextDouble() * (TotalHeight - GridSize));
                    MiddleNodes.Add(new Node(1, MiddleNodeColor, nodePositions[target].X, nodePositions[target].Y));
                }

                Connections.Add(new Connection(nodePositions[source] + middleOfNode, nodePositions[target] + middleOfNode, weight >= 0, Math.Abs(weight) / 5.0 + 0.3));
            }
        }

        /// <summary>
        /// Returns whether or not the current topology is the same as the given one.
        /// </summary>
        /// <param name="connectionLayers"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the status of the nodes of the neural network. Note that for now, middle nodes are not updated.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        public void UpdateNodes(double[] inputs, double[] outputs)
        {
            if (runningUpdate) return;
            runningUpdate = true;
            Dispatcher.UIThread.Post(() =>
            {
                using var delay = DelayChangeNotifications();
                UpdateNodes(Inputs, inputs);
                UpdateNodes(Outputs, outputs);
                runningUpdate = false;
            });
        }

        private void UpdateNodes(IEnumerable<NodeGroupViewModel> nodeGroups, double[] states)
        {
            int startIndex = 0;
            foreach (var nodeGroup in nodeGroups)
            {
                for (int i = 0; i < nodeGroup.Nodes.Count; i++)
                {
                    double prevValue = nodeGroup.Nodes[i].Value;
                    double value = states[i + startIndex];
                    if (nodeGroup.IsOutput) value = value > IInput.INPUT_THRESHOLD ? 1 : 0;
                    if (prevValue != value && !(prevValue <= 0 && value <= 0) && !(prevValue >= 1 && value >= 1))
                    {
                        nodeGroup.Nodes[i] = new Node(value, nodeGroup.TargetColor);
                    }
                }

                startIndex += nodeGroup.Nodes.Count;
            }
        }

        private bool CheckTooComplex((int, int, double)[][] connectionLayers)
        {
            bool result;
            int totalConnections = 0;
            foreach (var layer in connectionLayers)
            {
                totalConnections += layer.Length;
            }
            result = totalConnections > MAX_CONNECTIONS;
            TooComplex = result;
            return result;
        }

        public ObservableCollection<NodeGroupViewModel> Inputs { get; }
        public ObservableCollection<Node> MiddleNodes { get; }
        public ObservableCollection<NodeGroupViewModel> Outputs { get; }
        public ObservableCollection<Connection> Connections { get; }
        private bool tooComplex;
        public bool TooComplex
        {
            get => tooComplex;
            set => this.RaiseAndSetIfChanged(ref tooComplex, value);
        }
    }
}
