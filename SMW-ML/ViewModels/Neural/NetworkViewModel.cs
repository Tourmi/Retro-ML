using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using ReactiveUI;
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

            private Node[] nodes;

            public NodeGroupViewModel(string name, bool useRed = false, int gridWidth = 1, int gridHeight = 1, int positionX = 0, int positionY = 0)
            {
                Name = name;
                UseRed = useRed;
                GridWidth = gridWidth;
                GridHeight = gridHeight;
                Nodes = nodes = new Node[GridWidth * GridHeight];
            }

            public string Name { get; }
            public bool UseRed { get; }
            public int GridWidth { get; }
            public int GridHeight { get; }

            public Node[] Nodes
            {
                get => nodes;
                set => this.RaiseAndSetIfChanged(ref nodes, value);
            }
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

        public static int NodeSize => 18;
        public static int GridSize => 20;
        public static int LeftOffset => 100;
        public static int SpacingBetweenInputs => 10;

        private Node[] middleNodes;

        public NetworkViewModel()
        {
            Inputs = new ObservableCollection<NodeGroupViewModel>
            {
                new("Tiles", false, 11, 11),
                new("Dangers", true, 11, 11),
                new("On Ground")
            };
            Inputs.First().Nodes[3] = new Node() { Active = true };
            Inputs.Skip(1).First().Nodes[73] = new Node() { Active = true };
            Inputs.First().Nodes[13] = new Node() { Active = true };
            Inputs.First().Nodes[55] = new Node() { Active = true };
            Inputs.Skip(1).First().Nodes[34] = new Node() { Active = true };
            Inputs.First().Nodes[67] = new Node() { Active = true };
            Inputs.First().Nodes[100] = new Node() { Active = true };

            Outputs = new ObservableCollection<NodeGroupViewModel>()
            {
                new("A"),
                new("B"),
                new("X"),
                new("Y"),
                new("Left"),
                new("Right"),
                new("Up"),
                new("Down"),
                new("Left Shoulder"),
                new("Right Shoulder"),
                new("Start"),
                new("Select"),
            };

            Connections = new ObservableCollection<ConnectionViewModel>()
            {
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 0, GridSize / 2 + GridSize * 0), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2), true, 0.4),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 1, GridSize / 2 + GridSize * 2), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2), true, 1),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 5, GridSize / 2 + GridSize * 4), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2 + (SpacingBetweenInputs + GridSize) * 4), false, 0.5),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 1, GridSize / 2 + GridSize * 6), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2 + (SpacingBetweenInputs + GridSize) * 2), false, 1),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 3, GridSize / 2 + GridSize * 7), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2 + (SpacingBetweenInputs + GridSize) * 2), true, 0.3),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 2, GridSize / 2 + GridSize * 2), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2 + (SpacingBetweenInputs + GridSize) * 7), false, 0.3),
                new(new Point((LeftOffset + GridSize / 2) + GridSize * 1, GridSize / 2 + GridSize * 3), new Point(TotalWidth - (LeftOffset + GridSize / 2), GridSize / 2 + (SpacingBetweenInputs + GridSize) * 3), true, 1),
            };

            MiddleNodes = middleNodes = new Node[]
            {
                new() {Active = true, PositionX = 500, PositionY = 500},
                new() {Active = false, PositionX = 250, PositionY = 750},
                new() {Active = true, PositionX = 250, PositionY = 750}
            };
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
