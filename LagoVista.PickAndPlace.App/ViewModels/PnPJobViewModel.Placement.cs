using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        public async void PlaceAllParts()
        {
            _partIndex = 0;
            Machine.LocationUpdateEnabled = false;
            await Machine.SetViewTypeAsync(ViewTypes.Tool1);
            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            while (SelectedPart != null && !_isPaused && _partIndex < SelectedPart.Parts.Count)
            {
                await PlacePartAsync(true);
            }

            // Disable the stepper motor for the rotation of the nozzle, get's very warm
            Machine.SendCommand("M18 E");

            Machine.LocationUpdateEnabled = true;

            Machine.VacuumPump = false;
            Machine.PuffPump = false;

            await SaveJobAsync();
        }

        public async void PlacePart()
        {
            Machine.LocationUpdateEnabled = false;
            if(_partIndex >= SelectedPart.Parts.Count)
            {
                _partIndex = 0;
            }


            await Machine.SetViewTypeAsync(ViewTypes.Tool1);
            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            await PlacePartAsync();
            Machine.LocationUpdateEnabled = true;

            // Disable the stepper motor for the rotation of the nozzle, get's very warm
            Machine.SendCommand("M18 E");
        }

        public async Task PlacePartAsync(bool multiple = false)
        {
            if (_partIndex < SelectedPart.Parts.Count)
            {
                _isPlacingParts = true;
                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();
                GoToRefHoleCommand.RaiseCanExecuteChanged();
                SetRefHoleCommand.RaiseCanExecuteChanged();
                GoToCurrentPartInStripCommand.RaiseCanExecuteChanged();

                if (!Machine.VacuumPump || !Machine.PuffPump)
                {
                    Machine.VacuumPump = true;
                    Machine.PuffPump = true;
                    await Task.Delay(1000);
                }                

                if (_selectPartToBePlaced == null)
                {
                    _partIndex = 0;
                    _selectPartToBePlaced = SelectedPart.Parts[_partIndex];
                    RaisePropertyChanged(nameof(SelectedPartToBePlaced));
                }

                var cmds = new List<string>();

                cmds.Add(SafeHeightGCodeGCode()); // Move to move height
                cmds.Add(GetGoToPartInTrayGCode());
                cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                //cmds.Add(WaitForComplete());
                //cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(true)); // Turn on solenoid 
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(PickHeightGCode()); // Move to pick height                
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(SafeHeightGCodeGCode()); // Go to move height

                var cRotation = SelectedPartToBePlaced.RotateAngle + SelectedPartPackage.RotationInTape;
                if (cRotation != 0 || (!SelectedPartToBePlaced.Polarized && cRotation != 180))
                {
                    if (cRotation > 360)
                        cRotation -= 360;
                    cmds.Add(RotationGCode(cRotation));
                  //  cmds.Add(WaitForComplete());
                }              

                cmds.Add(GetGoToPartOnBoardGCode());

                cmds.Add(PlaceHeightGCode(SelectedPartPackage));
                //cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(false));
                cmds.Add(ProducePuffGCode(true));
                cmds.Add(DwellGCode(50)); // Wait 500ms to let placed part settle in
                cmds.Add(SafeHeightGCodeGCode()); // Return to move height.
                cmds.Add(ProducePuffGCode(false));
                cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                //cmds.Add(WaitForComplete());                

                await SendInstructionSequenceAsync(cmds);
                
                if (!multiple)
                {
                    Machine.VacuumPump = false;
                    Machine.PuffPump = false;
                    
                }

                SelectedPartStrip.CurrentPartIndex++;
                _partIndex++;
                if (_partIndex >= SelectedPart.Parts.Count)
                {
                    SelectedPart = null;
                }
                else
                {
                    _selectPartToBePlaced = SelectedPart.Parts[_partIndex];
                }

                RaisePropertyChanged(nameof(SelectedPartToBePlaced));
                

                _isPlacingParts = false;

                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();
            }
        }


        private string GetGoToPartInTrayGCode()
        {
            var location = StripFeederVM.GetCurrentPartPosition(SelectedPartStrip, PositionType.CurrentPart);
            if (location != null)
            {
                return $"G0 X{location.X} Y{location.Y} F{Machine.Settings.FastFeedRate}";
            }
            else
            {
                var deltaX = Math.Abs(XPartInTray.Value - Machine.MachinePosition.X);
                var deltaY = Math.Abs(YPartInTray.Value - Machine.MachinePosition.Y);
                var feedRate = Machine.Settings.FastFeedRate;
                return $"G0 X{XPartInTray * Machine.Settings.PartStripScaler.X} Y{YPartInTray * Machine.Settings.PartStripScaler.Y} F{feedRate}";
            }
        }

        public void GoToPartPositionInTray()
        {
            if (SelectedPartPackage != null && SelectedPartStrip != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand(GetGoToPartInTrayGCode());
            }
        }

        private String GetGoToPartOnBoardGCodeX()
        {
            var offset = (SelectedPartToBePlaced.X - _job.BoardOffset.X) + Machine.Settings.PCBOffset.X;
            return $"G1 X{offset * Job.BoardScaler.X}  F{Machine.Settings.FastFeedRate}";
        }

        private String GetGoToPartOnBoardGCodeY()
        {
            var offset = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) + Machine.Settings.PCBOffset.Y;
            return $"G1 Y{offset * Job.BoardScaler.Y} F{Machine.Settings.FastFeedRate}";
        }

        private String GetGoToPartOnBoardGCode()
        {
            var offsetX = (SelectedPartToBePlaced.X - _job.BoardOffset.X) + Machine.Settings.PCBOffset.X;
            var offsetY = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) + Machine.Settings.PCBOffset.Y;
            return $"G1 X{offsetX}  Y{offsetY} F{Machine.Settings.FastFeedRate}";
        }

        public async Task GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
                await Machine.SetViewTypeAsync(ViewTypes.Camera);
                Machine.SendCommand(SafeHeightGCodeGCode());

                var offsetY = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) + Machine.Settings.PCBOffset.Y;
                var offsetX = (SelectedPartToBePlaced.X - _job.BoardOffset.X) + Machine.Settings.PCBOffset.X;
                var gcode = $"G1 X{offsetX * Job.BoardScaler.X} Y{offsetY * Job.BoardScaler.Y} F{Machine.Settings.FastFeedRate}";

                Machine.SendCommand(gcode);
            }
        }
    }
}
