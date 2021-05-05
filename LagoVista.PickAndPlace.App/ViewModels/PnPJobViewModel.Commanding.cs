using LagoVista.Core.Commanding;
using System;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private void AddCommands()
        {
            SaveCommand = new RelayCommand(() => SaveJob());
            CloseCommand = new RelayCommand(Close);
            CloneCommand = new RelayCommand(CloneConfiguration);

            PeformMachineAlignmentCommand = new RelayCommand(PerformMachineAlignment);

            GoToPartOnBoardCommand = new RelayCommand(GoToPartOnBoard);
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            SelectMachineFileCommand = new RelayCommand(SelectMachineFile);

            HomingCycleCommand = new RelayCommand(() => Machine.HomingCycle());

            AlignBottomCameraCommand = new RelayCommand(() => AlignBottomCamera());

            ResetCurrentComponentCommand = new RelayCommand(ResetCurrentComponent, () => SelectedPartStrip != null);

            GoToWorkHomeCommand = new RelayCommand(() => GotoWorkspaceHome());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());

            MoveToPreviousComponentInTapeCommand = new RelayCommand(MoveToPreviousComponent, () => SelectedPartStrip != null && SelectedPartStrip.CurrentPartIndex > 0);
            MoveToNextComponentInTapeCommand = new RelayCommand(MoveToNextComponentInTape, () => SelectedPartStrip != null && SelectedPartStrip.CurrentPartIndex < SelectedPartStrip.ReferenceHoleX);
            RefreshConfigurationPartsCommand = new RelayCommand(PopulateConfigurationParts);
            GoToPartInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            PlaceCurrentPartCommand = new RelayCommand(PlacePart, CanPlacePart);
            PlaceAllPartsCommand = new RelayCommand(PlaceAllParts, CanPlacePart);
            PausePlacmentCommand = new RelayCommand(PausePlacement, CanPausePlacement);

            SetFiducialCalibrationCommand = new RelayCommand((prm) => SetFiducialCalibration(prm));

            CalibrateBottomCameraCommand = new RelayCommand(() => CalibrateBottomCamera());

            NextInspectCommand = new RelayCommand(NextInspect,() => _inspectIndex < ConfigurationParts.Count - 1);
            PrevInspectCommand = new RelayCommand(PrevInspect, () => _inspectIndex > 0);
            FirstInspectCommand = new RelayCommand(FirstInspect, () => _inspectIndex > 0);

            SetBoardOffsetCommand = new RelayCommand(SetBoardOffset, () => SelectedPartToBePlaced != null);

            SetBottomCameraPositionCommand = new RelayCommand(SetBottomCamera, () => Machine.Connected);
        }

        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand GoToCurrentPartCommand { get; private set; }
        public RelayCommand AddFeederCommand { get; private set; }
        public RelayCommand RefreshConfigurationPartsCommand { get; private set; }
        public RelayCommand CloneCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand GoToPartOnBoardCommand { get; private set; }
        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }
        public RelayCommand SelectMachineFileCommand { get; private set; }
        public RelayCommand ResetCurrentComponentCommand { get; set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand PausePlacmentCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand PlaceCurrentPartCommand { get; set; }
        public RelayCommand PlaceAllPartsCommand { get; set; }
        public RelayCommand GoToPartInTrayCommand { get; private set; }
        public RelayCommand PeformMachineAlignmentCommand { get; private set; }
        public RelayCommand GoToFiducial1Command { get; private set; }
        public RelayCommand GoToFiducial2Command { get; private set; }
        public RelayCommand AlignBottomCameraCommand { get; private set; }
        public RelayCommand CalibrateBottomCameraCommand { get; private set; }
        public RelayCommand SetFiducialCalibrationCommand { get; private set; }

        public RelayCommand FirstInspectCommand { get; private set; }
        public RelayCommand NextInspectCommand { get; private set; }
        public RelayCommand PrevInspectCommand { get; private set;  }

        public RelayCommand SetBoardOffsetCommand { get; private set; }
        public RelayCommand SetBottomCameraPositionCommand { get; private set; }


        public bool CanPlacePart()
        {
            if (_selectedPart == null)
                return false;

            if (_isPlacingParts)
                return false;

            return true;
        }

        public bool CanPausePlacement(Object obj)
        {
            return _isPlacingParts;
        }


        public bool CanSaveJob()
        {
            return _isDirty;
        }
    }
}
