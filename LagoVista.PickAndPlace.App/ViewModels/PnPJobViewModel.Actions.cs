using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Managers;
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

        public async Task SaveJob()
        {
            if (String.IsNullOrEmpty(FileName))
            {
                FileName = await Popups.ShowSaveFileAsync(Constants.PickAndPlaceProject);
                if (String.IsNullOrEmpty(FileName))
                {
                    return;
                }
            }

            await Storage.StoreAsync(Job, FileName);

            if (!String.IsNullOrEmpty(Job.PnPMachinePath))
            {
                await PnPMachineManager.SavePackagesAsync(PnPMachine, Job.PnPMachinePath);
            }

            _isDirty = false;
            SaveCommand.RaiseCanExecuteChanged();

            SaveProfile();
        }

        private void SetFiducialCalibration(object obj)
        {
            var idx = Convert.ToInt32(obj);
            switch (idx)
            {
                case 1:
                    {
                        var gcode = $"G92 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 2:
                    {
                        var gcode = $"G92 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
            }
        }

        private async void GoToFiducial(int idx)
        {
            Machine.SendCommand(SafeHeightGCodeGCode());

            await Machine.SetViewTypeAsync(ViewTypes.Camera);

            ShowTopCamera = true;

            switch (idx)
            {
                case 0:
                    {
                        var gcode = $"G1 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 1:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 2:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
            }
        }

        public void CalibrateBottomCamera()
        {
            AlignBottomCamera();
            _targetAngle = 0;
            _mvLocatorState = MVLocatorState.NozzleCalibration;
            SelectMVProfile("nozzlecalibration");
            Machine.SendCommand($"G0 E0");
            _averagePoints = new List<Point2D<double>>();

        }


        public void PausePlacement(Object obj)
        {
            _isPlacingParts = false;
            _isPaused = true;
        }


        public void GotoWorkspaceHome()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());
            Machine.GotoWorkspaceHome();
            ShowBottomCamera = false;
            ShowTopCamera = true;
        }

   

        public async void Close()
        {
            await SaveJob();
            CloseScreen();
        }

        public async void CloneConfiguration()
        {
            var clonedName = await Popups.PromptForStringAsync("Cloned configuration name", isRequired: true);
            var clonedFlavor = SelectedBuildFlavor.Clone(clonedName);
            BuildFlavors.Add(clonedFlavor);
            SelectedBuildFlavor = clonedFlavor;
        }

        public async void ResetCurrentComponent()
        {
            SelectedPartStrip.CurrentPartIndex = 0;
            RaisePropertyChanged(nameof(XPartInTray));
            RaisePropertyChanged(nameof(RotationInTape));
            RaisePropertyChanged(nameof(YPartInTray));
            RaisePropertyChanged(nameof(SelectedPartStrip));
            GoToPartPositionInTray();
            await SaveJob();
        }

        public async void MoveToPreviousComponent()
        {
            if (SelectedPartStrip.CurrentPartIndex > 0)
            {
                SelectedPartStrip.CurrentPartIndex--;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));
                RaisePropertyChanged(nameof(SelectedPartStrip));
                await SaveJob();
                GoToPartPositionInTray();
            }
        }

        public async void MoveToNextComponentInTape()
        {
            if (SelectedPartStrip.CurrentPartIndex < SelectedPartStrip.AvailablePartCount)
            {
                SelectedPartStrip.CurrentPartIndex++;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));
                RaisePropertyChanged(nameof(SelectedPartStrip));
                await SaveJob();
                GoToPartPositionInTray();
            }
        }

        public async void SelectMachineFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.PnPMachine);
            if (!String.IsNullOrEmpty(result))
            {
                try
                {
                    Job.PnPMachinePath = result;
                    PnPMachine = await PnPMachineManager.GetPnPMachineAsync(result);
                    await SaveJob();
                }
                catch
                {
                    await Popups.ShowAsync("Could not open packages");
                }
            }
        }

        public void RefreshBoard()
        {
            foreach(var flavor in Job.BuildFlavors)
            {
                foreach (var component in flavor.Components)
                {
                    var part = Job.Board.Components.Where(cmp => cmp.Name == component.Name).FirstOrDefault();
                    component.X = part.X;
                    component.Y = part.Y;
                    component.Rotate = part.Rotate;
                }
            }
        }

        public async void SelectBoardFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(result))
            {
                try
                {
                    Job.EagleBRDFilePath = result;
                    await SaveJob();


                }
                catch
                {
                    await Popups.ShowAsync("Could not open packages");
                }
            }
        }

    }
}
