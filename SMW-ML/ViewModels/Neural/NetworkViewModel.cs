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
            public int PositionX { get; set; }
            public int PositionY { get; set; }
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

        public static int NodeSize => 13;
        public static int GridSize => 15;
        public static int LeftOffset => 100;
        public static int SpacingBetweenInputs => 5;

        private Node[] middleNodes;
        private bool runningUpdate = false;

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
        }

        public void UpdateNodes(IVector<double> inputs, IVector<double> outputs)
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
        public Node[] MiddleNodes
        {
            get => middleNodes;
            set => this.RaiseAndSetIfChanged(ref middleNodes, value);
        }
        public ObservableCollection<NodeGroupViewModel> Outputs { get; }
        public ObservableCollection<ConnectionViewModel> Connections { get; }
    }
}
