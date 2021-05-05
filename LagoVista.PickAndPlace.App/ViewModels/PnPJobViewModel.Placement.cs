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
                if (cRotation > 0)
                {
                    cmds.Add(RotationGCode(cRotation));
                    cmds.Add(WaitForComplete());
                }

                cmds.Add(GetGoToPartOnBoardGCode());
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

                await SaveJob();

                _isPlacingParts = false;

                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();
            }
        }


        private string GetGoToPartInTrayGCode()
        {
            return $"G0 X{XPartInTray} Y{YPartInTray} F{Machine.Settings.FastFeedRate}";
        }

        public void GoToPartPositionInTray()
        {
            if (SelectedPartPackage != null && SelectedPartStrip != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand(GetGoToPartInTrayGCode());
            }
        }

        private String GetGoToPartOnBoardGCode()
        {
            return $"G1 X{SelectedPartToBePlaced.X - _job.BoardOffset.X} Y{SelectedPartToBePlaced.Y - _job.BoardOffset.Y} F{Machine.Settings.FastFeedRate}";
        }

        public void GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand(GetGoToPartOnBoardGCode());
            }
        }
    }
}
