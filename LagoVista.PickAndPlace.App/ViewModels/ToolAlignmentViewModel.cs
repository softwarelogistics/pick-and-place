﻿using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class ToolAlignmentViewModel : MachineVisionViewModelBase
    {
        public ToolAlignmentViewModel(IMachine machine) : base(machine)
        {

            SetToolOneMovePositionCommand = new RelayCommand(SetTool1MovePosition, () => Machine.Connected);
            SetToolOnePickPositionCommand = new RelayCommand(SetTool1PickPosition, () => Machine.Connected);
            SetToolOnePlacePositionCommand = new RelayCommand(SetTool1PlacePosition, () => Machine.Connected);

            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => Machine.Connected && MarkedTool1Location != null);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => Machine.Connected && MarkedTool1Location != null);

            MarkTool1LocationCommand = new RelayCommand(MarkTool1Location, () => Machine.Connected);
            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => Machine.Connected);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation);
            AddNozzleCommand = new RelayCommand(AddNozzle);
            DeleteNozzleCommand = new RelayCommand(DeleteNozzle);
            SaveCalibrationCommand = new RelayCommand(SaveCalibration, () => IsDirty);
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (Machine.Settings.PartInspectionCamera != null &&
                Machine.Settings.PartInspectionCamera.AbsolutePosition != null)
            {
                BottomCameraLocation = new Point3D<double>()
                {
                    X = Machine.Settings.PartInspectionCamera.AbsolutePosition.X,
                    Y = Machine.Settings.PartInspectionCamera.AbsolutePosition.Y,
                    Z = Machine.Settings.PartInspectionCamera.FocusHeight
                };
            }
            //StartCapture();
        }


        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                Set(ref _isDirty, value);
                SaveCalibrationCommand.RaiseCanExecuteChanged();
            }
        }

        public void AddNozzle()
        {
            Machine.Settings.CurrentNozzle = new Models.ToolNozzle();
            Machine.Settings.CurrentNozzle.Name = "-new nozzle-";
            Machine.Settings.Nozzles.Add(Machine.Settings.CurrentNozzle);
            IsDirty = true;
        }

        public void DeleteNozzle()
        {
            Machine.Settings.Nozzles.Remove(Machine.Settings.CurrentNozzle);
            if (Machine.Settings.Nozzles.Any())
            {
                Machine.Settings.CurrentNozzle = Machine.Settings.Nozzles.First();
            }
            else
            {
                AddNozzle();
            }
        }

        protected override void CaptureStarted()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
            SetToolOneMovePositionCommand.RaiseCanExecuteChanged();
            SetToolOnePickPositionCommand.RaiseCanExecuteChanged();
            SetToolOnePlacePositionCommand.RaiseCanExecuteChanged();
        }

        protected override void CaptureEnded()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
            SetToolOneMovePositionCommand.RaiseCanExecuteChanged();
            SetToolOnePickPositionCommand.RaiseCanExecuteChanged();
            SetToolOnePlacePositionCommand.RaiseCanExecuteChanged();
        }

        public void SetTopCameraLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M75");
            }

            TopCameraLocation = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetBottomCameraLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M71");
            }

            Machine.Settings.PartInspectionCamera.AbsolutePosition = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            Machine.Settings.PartInspectionCamera.FocusHeight = Machine.Tool0;
            BottomCameraLocation = new Point3D<double>()
            {
                X = Machine.Settings.PartInspectionCamera.AbsolutePosition.X,
                Y = Machine.Settings.PartInspectionCamera.AbsolutePosition.Y,
                Z = Machine.Settings.PartInspectionCamera.FocusHeight
            };

            IsDirty = true;
        }

        public void SetTool1MovePosition()
        {
            Machine.Settings.ToolSafeMoveHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M72");
            }

            IsDirty = true;
        }


        public void SetTool1PickPosition()
        {
            Machine.Settings.ToolPickHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M73");
            }

            IsDirty = true;
        }

        public void SetTool1PlacePosition()
        {
            Machine.Settings.ToolBoardHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M74");
            }

            IsDirty = true;
        }

        public void MarkTool1Location()
        {
            MarkedTool1Location = new Point2D<double>()
            {
                X = Machine.MachinePosition.X,
                Y = Machine.MachinePosition.Y,
            };

            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetTool1Location()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M76");
            }

            Machine.Settings.Tool1Offset = new Point2D<double>()
            {
                X = Machine.MachinePosition.X - MarkedTool1Location.X,
                Y = Machine.MachinePosition.Y - MarkedTool1Location.Y,
            };

            IsDirty = true;
        }

        public void SetTool2Location()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M77");
            }

            Machine.Settings.Tool2Offset = new Point2D<double>()
            {
                X = MarkedTool1Location.X - Machine.MachinePosition.X,
                Y = MarkedTool1Location.Y - Machine.MachinePosition.Y,
            };

            IsDirty = true;
        }

        public async void SaveCalibration()
        {
            await Machine.MachineRepo.SaveAsync();
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand SetBottomCameraLocationCommand { get; private set; }

        public RelayCommand SetToolOneLocationCommand { get; private set; }
        public RelayCommand SetToolTwoLocationCommand { get; private set; }
        public RelayCommand SetTopCameraLocationCommand { get; private set; }

        public RelayCommand SetToolOnePlacePositionCommand { get; private set; }
        public RelayCommand SetToolOneMovePositionCommand { get; private set; }
        public RelayCommand SetToolOnePickPositionCommand { get; private set; }
        public RelayCommand SaveCalibrationCommand { get; private set; }
        public RelayCommand MarkTool1LocationCommand { get; private set; }
        public RelayCommand AddNozzleCommand { get; private set; }
        public RelayCommand DeleteNozzleCommand { get; private set; }

        Point2D<double> _topCameraLocation;
        public Point2D<double> TopCameraLocation
        {
            get { return _topCameraLocation; }
            set { Set(ref _topCameraLocation, value); }
        }

        Point3D<double> _bottomCameraLocation;
        public Point3D<double> BottomCameraLocation
        {
            get { return _bottomCameraLocation; }
            set { Set(ref _bottomCameraLocation, value); }
        }

        Point2D<double> _markedTool1Location;
        public Point2D<double> MarkedTool1Location
        {
            get { return _markedTool1Location; }
            set { Set(ref _markedTool1Location, value); }
        }
    }
}
