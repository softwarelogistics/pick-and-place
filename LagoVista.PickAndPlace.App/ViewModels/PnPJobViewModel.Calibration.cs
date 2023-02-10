using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private void AbortMVLocator()
        {
            _mvLocatorState = MVLocatorState.Idle;
            Machine.TopLightOn = false;
            Machine.BottomLightOn = false;
            ShowCircles = false;
            ShowPolygons = false;
            ShowLines = false;
        }

        public async void PerformMachineAlignment()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());
            _mvLocatorState = MVLocatorState.Idle;

            await Machine.SetViewTypeAsync(ViewTypes.Camera);
            Machine.TopLightOn = false;

            Machine.GotoWorkspaceHome();

            SelectMVProfile("mchfiducual");

            Machine.SendCommand(DwellGCode(250));

            GoToFiducial(0);

            ShowCircles = true;

            _mvLocatorState = MVLocatorState.MachineFidicual;
        }

        public void GotoMachineFiducial()
        {
            GoToFiducial(0);
        }

        public async void SetMachineFiducial()
        {
            Machine.Settings.MachineFiducial.X = Machine.NormalizedPosition.X;
            Machine.Settings.MachineFiducial.Y = Machine.NormalizedPosition.Y;
            await Machine.MachineRepo.SaveAsync();
        }

        private void FinalizeCameraCalibration()
        {
            _mvLocatorState = MVLocatorState.Idle;
            foreach (var key in _nozzleCalibration.Keys)
            {
                Debug.WriteLine($"{key},{_nozzleCalibration[key].X},{_nozzleCalibration[key].Y}");
            }

            var maxX = _nozzleCalibration.Values.Max(ca => ca.X);
            var maxY = _nozzleCalibration.Values.Max(ca => ca.Y);

            var minX = _nozzleCalibration.Values.Min(ca => ca.X);
            var minY = _nozzleCalibration.Values.Min(ca => ca.Y);

            var preCalX = Machine.MachinePosition.X;
            var preCalY = Machine.MachinePosition.Y;


            var top = _nozzleCalibration.First(pt => pt.Value.Y == maxY);

            var left = _nozzleCalibration.First(pt => pt.Value.X == minX);
            var right = _nozzleCalibration.First(pt => pt.Value.X == maxX);            

            var topAngle = ((left.Key + right.Key) / 2) - 180;

            var offsetX = top.Value.X / 20.0;
            var offsetY = top.Value.Y / 20.0;

            //var offsetX = ((maxX - minX) / 60.0);
            //var offsetY = ((maxY - minY) / 60.0);

            Machine.SendCommand("G91");
            Machine.SendCommand($"G0 X{-offsetX} Y{-offsetY}");
            Machine.SendCommand("G90");

            CalibrationUpdates.Insert(0, $"Top found at: {topAngle}");
            CalibrationUpdates.Insert(0, $"Setting Offset: {-offsetX},{-offsetY}");

            Debug.WriteLine($"MIN: {minX},{minY} MAX: {maxX},{maxY}, Adjusting to offset: {offsetX},{offsetY} - Top Angle: {topAngle}");

            Machine.SendCommand($"G0 E{topAngle}");
            Machine.SendCommand($"G92 E0 X{preCalX} Y{preCalY}");
        }

        private void PerformBottomCameraCalibration(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            Debug.WriteLine($"Found Circle: {Machine.MachinePosition.X},{Machine.MachinePosition.Y} - {stdDeviation.X},{stdDeviation.Y}");

            if (_targetAngle == Convert.ToInt32(Machine.Tool2))
            {
                samplesAtPoint++;

                if (samplesAtPoint > 30)
                {
                    var avgX = _averagePoints.Average(pt => pt.X);
                    var avgY = _averagePoints.Average(pt => pt.Y);
                    _nozzleCalibration.Add(Convert.ToInt32(Machine.Tool2), new Point2D<double>(avgX, avgY));

                    CalibrationUpdates.Insert(0, $"Angle: {_targetAngle} - {avgX}x{avgY}");

                    _targetAngle = Convert.ToInt32(Machine.Tool2 + 15.0);
                    Machine.SendCommand($"G0 E{_targetAngle}");
                    _averagePoints.Clear();
                    samplesAtPoint = 0;
                    RaisePropertyChanged(nameof(TargetAngle));
                    
                }
                else
                {
                    _averagePoints.Add(new Point2D<double>(point.X, point.Y));
                }

                if (Machine.Tool2 >= 360)
                {
                    FinalizeCameraCalibration();
                }
            }
        }

        public async void SetBoardOffset()
        {
            var deltaX = SelectedPartToBePlaced.X - Machine.MachinePosition.X;
            var deltaY = SelectedPartToBePlaced.Y - Machine.MachinePosition.Y;

            _job.BoardOffset = new Point2D<double>(deltaX.Value, deltaY.Value);
            RaisePropertyChanged(nameof(BoardOffset));
            await SaveJobAsync();
        }

        public async void ClearBoardOffset()
        {
            _job.BoardOffset = new Point2D<double>(0, 0);
            RaisePropertyChanged(nameof(BoardOffset));
            await SaveJobAsync();
        }

        public async void AlignBottomCamera()
        {
            if (Machine.Settings.PartInspectionCamera?.AbsolutePosition != null)
            {
                _nozzleCalibration = new Dictionary<int, Point2D<double>>();
                Machine.SendCommand(SafeHeightGCodeGCode());

                // move to what we think is the top angle.
                Machine.SendCommand($"G0 E0");
                await Machine.SetViewTypeAsync(ViewTypes.Tool1);

                Machine.TopLightOn = false;
                Machine.BottomLightOn = true;
                SelectMVProfile("nozzle");
                ShowCircles = true;

                ShowBottomCamera = true;

                Machine.SendCommand($"G0 X{Machine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{Machine.Settings.PartInspectionCamera.AbsolutePosition.Y} Z{Machine.Settings.PartInspectionCamera.FocusHeight} F1{Machine.Settings.FastFeedRate}");
            }
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            switch (_mvLocatorState)
            {
                case MVLocatorState.MachineFidicual:
                    JogToLocation(point);
                    break;

                case MVLocatorState.BoardFidicual1:
                    JogToLocation(point);
                    break;

                case MVLocatorState.BoardFidicual2:
                    JogToLocation(point);
                    break;
                case MVLocatorState.NozzleCalibration:
                    PerformBottomCameraCalibration(point, diameter, stdDeviation);
                    break;
                default:
                    break;
            }
        }

        public void SetBottomCamera()
        {
            Machine.SendCommand($"G92 X{Machine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{Machine.Settings.PartInspectionCamera.AbsolutePosition.Y}");
        }
    }
}
