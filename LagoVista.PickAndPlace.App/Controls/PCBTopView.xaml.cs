﻿using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Managers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for PCBTopView.xaml
    /// </summary>
    public partial class PCBTopView : UserControl
    {
        public PCBTopView()
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                this.DataContextChanged += PCBTopView_DataContextChanged;
            }
        }

        private void RenderBoard()
        {
            var manager = DataContext as PCBManager;
            if (manager.HasBoard)
            {
                BoardLayout.Children.Clear();

                var offsetX = manager.HasProject ? manager.Project.ScrapSides : 0;
                var offsetY = manager.HasProject ? manager.Project.ScrapTopBottom : 0;

                foreach (var drill in manager.Board.Drills)
                {
                    var ellipse = new Ellipse() { Width = drill.Diameter * 10.0, Height = drill.Diameter * 10.0 };
                    ellipse.Fill = Brushes.Black;
                    var x = Mirrored ? ((manager.Board.Width - (drill.X + (drill.Diameter / 2))) + offsetX) : ((drill.X - (drill.Diameter / 2)) + offsetX);
                    var y = ((manager.Board.Height - (drill.Y + (drill.Diameter / 2))) + offsetY);

                    ellipse.SetValue(Canvas.TopProperty, y * 10);
                    ellipse.SetValue(Canvas.LeftProperty, x * 10);

                    ellipse.Cursor = Cursors.Hand;
                    var drillPoint = new LagoVista.Core.Models.Drawing.Point2D<double>
                    {
                        X = Mirrored ? ((manager.Board.Width - drill.X) + offsetX) : (drill.X + offsetX),
                        Y = drill.Y + offsetY
                    };
                    ellipse.ToolTip = $"{drillPoint.X}x{drillPoint.Y} - {drill.Diameter} Dia.";

                    ellipse.Tag = drillPoint;

                    ellipse.MouseUp += Elipse_MouseUp;
                    BoardLayout.Children.Add(ellipse);
                }

                var outline = new Rectangle();
                outline.Stroke = Brushes.Black;
                outline.StrokeThickness = 2;
                outline.SetValue(Canvas.TopProperty, manager.Project.ScrapTopBottom * 10);
                outline.SetValue(Canvas.LeftProperty, manager.Project.ScrapSides * 10);
                outline.Width = manager.Board.Width * 10;
                outline.Height = manager.Board.Height * 10;

                var cornerWires = manager.Board.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
                var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;
                outline.RadiusX = radius * 10;
                outline.RadiusY = radius * 10;
                BoardLayout.Children.Add(outline);

                BoardLayout.Width = manager.HasProject ? manager.Project.StockWidth * 10 : manager.Board.Width * 10;
                BoardLayout.Height = manager.HasProject ? manager.Project.StockHeight * 10 : manager.Board.Height * 10;

                if (manager.HasProject)
                {
                    foreach (var hole in manager.Project.GetHoldDownDrills(manager.Board))
                    {
                        var ellipse = new Ellipse() { Width = manager.Project.HoldDownDiameter * 10.0, Height = manager.Project.HoldDownDiameter * 10.0 };
                        ellipse.Fill = Brushes.Black;

                        var x = hole.X;
                        var y = hole.Y;

                        ellipse.SetValue(Canvas.TopProperty, ((BoardLayout.Height / 10) - (y + (manager.Project.HoldDownDiameter / 2))) * 10.0);
                        ellipse.SetValue(Canvas.LeftProperty, (x - (manager.Project.HoldDownDiameter / 2)) * 10.0);
                        ellipse.ToolTip = $"{hole.X}x{hole.Y} - {manager.Project.HoldDownDiameter}D";
                        ellipse.Cursor = Cursors.Hand;
                        ellipse.Tag = new Point2D<double>()
                        {
                            X = hole.X,
                            Y = hole.Y
                        };
                        ellipse.MouseUp += Elipse_MouseUp;
                        BoardLayout.Children.Add(ellipse);
                    }
                }
            }
        }

        private void PCBTopView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RenderBoard();
        }

        private bool _mirrored;
        public bool Mirrored
        {
            get { return _mirrored; }
            set
            {
                _mirrored = value;
                RenderBoard();
            }
        }


        bool _shouldSetFirstFiducial = true;

        Point2D<double> _lastPoint = null;

        private void Elipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = (sender as Ellipse).Tag as Point2D<double>;

            var manager = DataContext as PCBManager;
            if (manager.IsSetFiducialMode)
            {
                if (_shouldSetFirstFiducial)
                {
                    manager.FirstFiducial = point;
                }
                else
                {
                    manager.SecondFiducial = point;
                }

                _shouldSetFirstFiducial = !_shouldSetFirstFiducial;
            }
            else
            {
                if (manager.Machine.Connected)
                {
                    point = manager.Machine.PCBManager.GetAdjustedPoint(point);
                    manager.Machine.GotoPoint(point);

                    _lastPoint = point;
                }
            }
        }
    }
}
