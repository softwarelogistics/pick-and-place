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
            //_partIndex = 0;
            Machine.LocationUpdateEnabled = false;

            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            while (SelectedPart != null && !_isPaused)
            {
                await PlacePartAsync(true);
            }

            Machine.LocationUpdateEnabled = true;

            Machine.Vacuum1On = false;
            Machine.Vacuum2On = false;
        }

        public async void PlacePart()
        {
            Machine.LocationUpdateEnabled = false;

            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            await PlacePartAsync();
            Machine.LocationUpdateEnabled = true;
        }

        public async Task PlacePartAsync(bool multiple = false)
        {
            if (_partIndex < SelectedPart.Parts.Count)
            {
                

                _isPlacingParts = true;
                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();

                if (!Machine.Vacuum1On || !Machine.Vacuum2On)
                {
                    Machine.Vacuum1On = true;
                    Machine.Vacuum2On = true;
                    await Task.Delay(1000);
                }

                await Machine.SetViewTypeAsync(ViewTypes.Tool1);
                Machine.SendCommand($"G1 X0 Y0 F{Machine.Settings.FastFeedRate}");

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
                cmds.Add(WaitForComplete());
                cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(true)); // Turn on solenoid 
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(PickHeightGCode()); // Move to pick height                
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(SafeHeightGCodeGCode()); // Go to move height

                var cRotation = SelectedPartToBePlaced.RotateAngle + SelectedPartPackage.RotationInTape;
                if (cRotation != 0)
                {
                    cmds.Add(RotationGCode(cRotation));
                    cmds.Add(WaitForComplete());
                }

                cmds.Add($"G90");
                cmds.Add($"G0 X0 Y0 F{Machine.Settings.FastFeedRate}");
                cmds.Add(GetGoToPartOnBoardGCodeX());
                cmds.Add(GetGoToPartOnBoardGCodeY());
                cmds.Add(PlaceHeightGCode(SelectedPartPackage));
                cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(false));
                cmds.Add(DwellGCode(1500)); // Wait 500ms to let placed part settle in
                cmds.Add(SafeHeightGCodeGCode()); // Return to move height.

                cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                cmds.Add(WaitForComplete());                

                await SendInstructionSequenceAsync(cmds);
                
                if (!multiple)
                {
                    Machine.Vacuum1On = false;
                    Machine.Vacuum2On = false;
                    
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

                await SaveJobAsync();

                _isPlacingParts = false;

                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();
            }
        }


        private string GetGoToPartInTrayGCode()
        {
            Machine.SendCommand($"G1 X0 Y0 F{Machine.Settings.FastFeedRate}");
            var deltaX = Math.Abs(XPartInTray.Value - Machine.MachinePosition.X);
            var deltaY = Math.Abs(YPartInTray.Value - Machine.MachinePosition.Y);
            var feedRate = Machine.Settings.FastFeedRate;

            return $"G0 X{XPartInTray * Machine.Settings.PartStripScaler.X} Y{YPartInTray * Machine.Settings.PartStripScaler.Y} F{feedRate}";
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
            var offset = SelectedPartToBePlaced.X - _job.BoardOffset.X;
            return $"G1 X{offset * Job.BoardScaler.X}  F{Machine.Settings.FastFeedRate}";
        }

        private String GetGoToPartOnBoardGCodeY()
        {
            var offset = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y);
            return $"G1 Y{offset * Job.BoardScaler.Y} F{Machine.Settings.FastFeedRate}";
        }

        public void GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand($"G1 X0 Y0 F{Machine.Settings.FastFeedRate}");
                Machine.SendCommand(GetGoToPartOnBoardGCodeX());
                Machine.SendCommand(GetGoToPartOnBoardGCodeY());
            }
        }
    }
}
