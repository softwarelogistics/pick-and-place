﻿using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.ViewModels;

namespace LagoVista.PickAndPlace.App.Controls
{

    public partial class HeightMapControl : UserControl
    {
        public HeightMapControl()
        {
            InitializeComponent();

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                Loaded += HeightMapControl_Loaded;
            }
        }

        private void HeightMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Machine.GCodeFileManager.PropertyChanged += GCodeFileManager_PropertyChanged;
            ViewModel.Machine.Settings.PropertyChanged += GCodeFileManager_PropertyChanged;
            ViewModel.Machine.PropertyChanged += GCodeFileManager_PropertyChanged;

            ViewModel.Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;

            var x = ViewModel.Machine.Settings.WorkAreaWidth / 2;
            Camera.Position = new Point3D(x, Camera.Position.Y, Camera.Position.Z);
        }
       
        private void RenderBoard(PrintedCircuitBoard board, PCBProject project, bool resetZoomAndView = true)
        {
            var linePoints = new Point3DCollection();

            var modelGroup = new Model3DGroup();
            var copperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
            var grayMaterial = MaterialHelper.CreateMaterial(Colors.DarkGray);

            var scrapX = project == null ? 0 : project.ScrapSides;
            var scrapY = project == null ? 0 : project.ScrapTopBottom;
            var boardThickness = project == null ? 1.60 : project.StockThickness;


            if (_topWiresVisible)
            {
                foreach (var wireSection in board.TopWires.GroupBy(wre => wre.Width))
                {
                    var width = wireSection.First().Width;

                    foreach (var wire in wireSection)
                    {
                        var topWireMeshBuilder = new MeshBuilder(false, false);
                        var boxRect = new Rect3D(wire.Rect.X1 - (width / 2), wire.Rect.Y1, -0.1, width, wire.Rect.Length, 0.2);
                        topWireMeshBuilder.AddBox(boxRect);

                        topWireMeshBuilder.AddCylinder(new Point3D(wire.Rect.X1, wire.Rect.Y1, -0.1), new Point3D(wire.Rect.X1, wire.Rect.Y1, .1), width / 2, 50, true, true);

                        var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = copperMaterial };
                        boxModel.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), wire.Rect.Angle), new Point3D(wire.Rect.X1, wire.Rect.Y1, 0));
                        modelGroup.Children.Add(boxModel);
                    }
                }
            }

            if (_bottomWiresVisible)
            {
                foreach (var wireSection in board.BottomWires.GroupBy(wre => wre.Width))
                {
                    var width = wireSection.First().Width;

                    foreach (var wire in wireSection)
                    {
                        var topWireMeshBuilder = new MeshBuilder(false, false);
                        var boxRect = new Rect3D(wire.Rect.X1 - (width / 2), wire.Rect.Y1, -0.105, width, wire.Rect.Length, 0.2);
                        topWireMeshBuilder.AddBox(boxRect);

                        topWireMeshBuilder.AddCylinder(new Point3D(wire.Rect.X1, wire.Rect.Y1, -0.105), new Point3D(wire.Rect.X1, wire.Rect.Y1, .095), width / 2, 50, true, true);

                        var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = grayMaterial };
                        boxModel.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), wire.Rect.Angle), new Point3D(wire.Rect.X1, wire.Rect.Y1, 0));
                        modelGroup.Children.Add(boxModel);
                    }
                }
            }



            foreach (var element in board.Components)
            {
                foreach (var pad in element.SMDPads)
                {
                    var padMeshBuilder = new MeshBuilder(false, false);

                    padMeshBuilder.AddBox(new Rect3D(pad.OriginX - (pad.DX / 2), pad.OriginY - (pad.DY / 2), -0.1, (pad.DX), (pad.DY), 0.2));
                    var box = new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = element.Layer == 1 ? copperMaterial : grayMaterial };

                    var transformGroup = new Transform3DGroup();
                    transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), element.RotateAngle)));
                    transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(element.X.Value, element.Y.Value, element.Layer == 1 ? 0 : 0.05)));

                    box.Transform = transformGroup;

                    modelGroup.Children.Add(box);
                }

                foreach (var pad in element.Pads)
                {
                    var padCopperMeshBuilder = new MeshBuilder(false, false);
                    padCopperMeshBuilder.AddCylinder(new Point3D(pad.X, pad.Y, 0), new Point3D(pad.X, pad.Y, 0.1), pad.DrillDiameter * 0.75);
                    var padCopper = new GeometryModel3D() { Geometry = padCopperMeshBuilder.ToMesh(true), Material = copperMaterial };
                    modelGroup.Children.Add(padCopper);

                    var padDrillMeshBuilder = new MeshBuilder(false, false);
                    padDrillMeshBuilder.AddCylinder(new Point3D(pad.X, pad.Y, 0), new Point3D(pad.X, pad.Y, 0.101), pad.DrillDiameter / 2);
                    var padDrill = new GeometryModel3D() { Geometry = padDrillMeshBuilder.ToMesh(true), Material = blackMaterial };
                    modelGroup.Children.Add(padDrill);
                }

                if (_pcbVisible)
                {
                    var billBoard = new BillboardTextVisual3D() { Foreground = Brushes.White, Text = element.Name, Position = new Point3D(element.X.Value + scrapX, element.Y.Value + scrapY, 4), FontSize = 14 };
                    viewport.Children.Add(billBoard);
                }
            }

            foreach (var via in board.Vias)
            {
                var padCopperMeshBuilder = new MeshBuilder(false, false);
                padCopperMeshBuilder.AddCylinder(new Point3D(via.X, via.Y, 0), new Point3D(via.X, via.Y, 0.1), (via.DrillDiameter));
                var padCopper = new GeometryModel3D() { Geometry = padCopperMeshBuilder.ToMesh(true), Material = copperMaterial };
                modelGroup.Children.Add(padCopper);

                var padDrillMeshBuilder = new MeshBuilder(false, false);
                padDrillMeshBuilder.AddCylinder(new Point3D(via.X, via.Y, 0), new Point3D(via.X, via.Y, 0.11), via.DrillDiameter / 2);
                var padDrill = new GeometryModel3D() { Geometry = padDrillMeshBuilder.ToMesh(true), Material = blackMaterial };
                modelGroup.Children.Add(padDrill);
            }

            if (_pcbVisible)
            {
                foreach (var circle in board.Holes)
                {
                    var circleMeshBuilder = new MeshBuilder(false, false);
                    circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 0.01), circle.Drill / 2);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
                }

                #region Hold your nose to discover why irregular boards don't render as expected... 
                /* gonna cheat here in next chunk of code...need to make progress, assume all corners are
                 * either square or round.  If rounded, same radius...WILL revisit this at some point, KDW 2/24/2017
                 * FWIW - feel so dirty doing this, but need to move on :*( 
                 * very happy to accept a PR to fix this!  Proper mechanism is to create a polygon and likely subdivide the curve into smaller polygon edges
                 * more work than it's worth right now....sorry again :(
                 */
                //TODO: Render proper edge of board.

                var boardEdgeMeshBuilder = new MeshBuilder(false, false);

                var cornerWires = board.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
                var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;
                if (radius == 0)
                {
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, -boardThickness / 2), board.Width, board.Height, boardThickness);
                }
                else
                {
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, -boardThickness / 2), board.Width - (radius * 2), board.Height - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, radius / 2, -boardThickness / 2), board.Width - (radius * 2), radius, boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height - radius / 2, -boardThickness / 2), board.Width - (radius * 2), radius, boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(radius / 2, board.Height / 2, -boardThickness / 2), radius, board.Height - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width - radius / 2, board.Height / 2, -boardThickness / 2), radius, board.Height - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, radius, -boardThickness), new Point3D(radius, radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, board.Height - radius, -boardThickness), new Point3D(radius, board.Height - radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, radius, -boardThickness), new Point3D(board.Width - radius, radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, board.Height - radius, -boardThickness), new Point3D(board.Width - radius, board.Height - radius, 0), radius, 50, true, true);
                }
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilder.ToMesh(true), Material = greenMaterial });

                #endregion
            }

            PCBLayer.Content = modelGroup;
            PCBLayer.Transform = new TranslateTransform3D(scrapX, scrapY, 0);

            if (project != null)
            {
                var stockGroup = new Model3DGroup();

                var circleMeshBuilder = new MeshBuilder(false, false);
                var holdDownDrills = project.GetHoldDownDrills(board);
                foreach(var drl in holdDownDrills)
                {
                    circleMeshBuilder.AddCylinder(new Point3D(drl.X, drl.Y, -boardThickness), new Point3D(drl.X, drl.Y, 0.01), project.HoldDownDiameter / 2);
                }

                stockGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });

                if (_stockVisible)
                {
                    var stockMeshBuilder = new MeshBuilder(false, false);
                    stockMeshBuilder.AddBox(new Point3D(project.StockWidth / 2, project.StockHeight / 2, -boardThickness / 2), project.StockWidth, project.StockHeight, boardThickness - 0.05);
                    stockGroup.Children.Add(new GeometryModel3D() { Geometry = stockMeshBuilder.ToMesh(true), Material = copperMaterial });
                }

                StockLayer.Content = stockGroup;
            }
            else
            {
                StockLayer.Content = null;
            }

            if (resetZoomAndView)
            {
                RefreshExtents();
            }
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Machine.PCBManager.HasBoard) ||
                e.PropertyName == nameof(ViewModel.Machine.PCBManager.HasProject))
            {
                if (ViewModel.Machine.PCBManager.HasBoard)
                    RenderBoard(ViewModel.Machine.PCBManager.Board, ViewModel.Machine.PCBManager.Project);
            }
            else
            {
                PCBLayer.Content = null;
                BottomWires.Content = null;
                TopWires.Content = null;
            }
        }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Machine.GCodeFileManager.Min) ||
                e.PropertyName == nameof(ViewModel.Machine.GCodeFileManager.Max) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings.WorkAreaWidth) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings.WorkAreaHeight) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings))
            {
                RefreshExtents();
            }
        }

        private void RefreshExtents()
        {
            switch (_imageMode)
            {
                case ImageModes.Front: ShowFrontView(); break;
                case ImageModes.Side: ShowLeftView(); break;
                case ImageModes.Top: ShowTopView(); break;
            }
        }

        public void Clear()
        {
        }

        public MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public bool ModelToolVisible
        {
            get { return ModelTool.Visible; }
            set { ModelTool.Visible = value; }
        }

        const int CAMERA_MOVE_DELTA = 10;

        public enum ImageModes
        {
            Top,
            Side,
            Front,
        }

        private void ShowLeftView()
        {
            double min = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.Y : 0;
            double max = ViewModel.Machine.Settings.WorkAreaHeight;

            if (ViewModel.Machine.PCBManager.HasBoard)
                max = ViewModel.Machine.PCBManager.Board.Height;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                max = ViewModel.Machine.GCodeFileManager.Max.Y;

            var factor = 18.0;

            var delta = (max - min);
            var y = (delta / 2) + min;
            var x = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(4, 0.0001, -1.7);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }

        private void ShowTopView()
        {
            double minX = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.X : 0;
            double maxX = ViewModel.Machine.Settings.WorkAreaWidth;

            if (ViewModel.Machine.PCBManager.HasBoard)
                maxX = ViewModel.Machine.PCBManager.Board.Width;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                maxX = ViewModel.Machine.GCodeFileManager.Max.X;

            double minY = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.Y : 0;
            double maxY = ViewModel.Machine.Settings.WorkAreaHeight;

            if (ViewModel.Machine.PCBManager.HasBoard)
                maxY = ViewModel.Machine.PCBManager.Board.Height;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                maxY = ViewModel.Machine.GCodeFileManager.Max.Y;


            var deltaX = maxX - minX;
            var deltaY = maxY - minY;

            var x = deltaX / 2 + minX;
            var y = deltaY / 2 + minY;

            if (ViewModel.Machine.PCBManager.HasProject)
                y += ViewModel.Machine.PCBManager.Project.ScrapTopBottom;

            Camera.Position = new Point3D(x, y, 400);
            Camera.LookDirection = new Vector3D(0, 0.0001, -1);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";
        }

        private void ShowFrontView()
        {
            double min = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.X : 0;
            double max = ViewModel.Machine.Settings.WorkAreaWidth;

            if (ViewModel.Machine.PCBManager.HasBoard)
                max = ViewModel.Machine.PCBManager.Board.Width;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                max = ViewModel.Machine.GCodeFileManager.Max.X;

            var factor = 18.0;

            var delta = (max - min);
            var x = (delta / 2) + min;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(0.0001, 4, -1.7);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }

        public bool _stockVisible = true;
        public bool _pcbVisible = true;
        public bool _topWiresVisible = true;
        public bool _bottomWiresVisible = true;
        public bool _gcodeVisible = true;

        private void ChangeLayer_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Stock":
                    {
                        _stockVisible = !_stockVisible;
                    }
                    break;
                case "PCB":
                    {
                        _pcbVisible = !_pcbVisible;
                    }
                    break;
                case "TopWires":
                    {
                        _topWiresVisible = !_topWiresVisible;
                    }
                    break;
                case "BottomWires":
                    {
                        _bottomWiresVisible = !_bottomWiresVisible;
                    }
                    break;
                case "GCode":
                    {
                        _gcodeVisible = !_gcodeVisible;
                    }
                    break;
                case "HeightMap":
                    HeightMap.Visible = !HeightMap.Visible;
                    // ModelHeightMapBoundary.Vis
                    //   HeightMapPoints
                    break;
            }

            if (ViewModel.Machine.PCBManager.HasBoard)
                RenderBoard(ViewModel.Machine.PCBManager.Board, ViewModel.Machine.PCBManager.Project, false);
        }

        ImageModes _imageMode = ImageModes.Front;

        private void ChangeView_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Top":
                    {
                        _imageMode = ImageModes.Top;
                        ShowTopView();
                    }
                    break;
                case "Left":
                    {
                        _imageMode = ImageModes.Side;
                        ShowLeftView();
                    }
                    break;
                case "Front":
                    {
                        _imageMode = ImageModes.Front;
                        ShowFrontView();
                    }
                    break;
                case "ZoomIn":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 0.9, Camera.Position.Z * 0.9); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 0.9, Camera.Position.Y, Camera.Position.Z * 0.9); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * .9); break;
                    }


                    break;
                case "ZoomOut":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 1.1, Camera.Position.Z * 1.1); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 1.1, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                    }

                    break;
                case "ShowObject":
                    break;
                case "ShowAll":
                    break;
                case "Up":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "MoveLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Center":
                    break;
                case "Right":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Down":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }

                    break;
                case "Forwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
                case "Backwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
            }

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";
        }
    }
}
