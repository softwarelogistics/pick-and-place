using DirectShowLib.BDA;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public enum PositionType
    {
        ReferenceHole,
        FirstPart,
        CurrentPart,
        LastPart,
        TempPartIndex,
    }

    public class StripFeederViewModel : ViewModelBase
    {
        private PnPMachine _pnpMachine;
        private PnPJobViewModel _jobVM;
        public IMachine _machine;
        string _pnpJobFileName;

        public StripFeederViewModel(IMachine machine, PnPJobViewModel jobVM)
        {
            _machine = machine;
            _jobVM = jobVM;
            RaisePropertyChanged(nameof(PartStrips));
            RaisePropertyChanged(nameof(StripFeederPackages));

            GoToCurrentPartCommand = new RelayCommand(() => GoToPart(PositionType.CurrentPart), () => _selectedPartStrip != null);
            GoToFirstPartCommand = new RelayCommand(() => GoToPart(PositionType.FirstPart), () => _selectedPartStrip != null);
            GoToLastPartCommand = new RelayCommand(() => GoToPart(PositionType.LastPart), () => _selectedPartStrip != null);
            NextPartCommand = new RelayCommand(NextPart, () => SelectedPartStrip != null && SelectedPartStrip != null && SelectedPartStrip.AvailablePartCount > SelectedPartStrip.TempPartIndex);
            PrevPartCommand = new RelayCommand(PrevPart, () => SelectedPartStrip != null && SelectedPartStrip != null && SelectedPartStrip.TempPartIndex > 0);
            SetCurrentPartIndexCommand = new RelayCommand(SetCurrentPartIndex, () => SelectedPartStrip != null);
        }

        public void SetMachine(PnPMachine machine, string fileName)
        {
            _pnpMachine = machine;
            _pnpJobFileName = fileName;

            RaisePropertyChanged(nameof(StripFeederPackages));
            AddStripFeederCommand = new RelayCommand(AddStripFeeder, () => CurrentStripFeederPackage != null);
            AddStripFeederPackageCommand = new RelayCommand(AddStripFeederPackage);

            SaveStripFeederCommand = new RelayCommand(() =>
            {
                if (String.IsNullOrEmpty(CurrentStripFeeder.Id))
                {
                    CurrentStripFeeder.Id = Guid.NewGuid().ToId();
                    CurrentStripFeederPackage.StripFeeders.Add(CurrentStripFeeder);
                }

                CurrentStripFeeder = null;

                RefreshCommandEnabled();
            }, () => CurrentStripFeeder != null);

            SaveStripFeederPackageCommand = new RelayCommand(() =>
            {
                if (String.IsNullOrEmpty(CurrentStripFeederPackage.Id))
                {
                    CurrentStripFeederPackage.Id = Guid.NewGuid().ToId();
                    _pnpMachine.StripFeederPackages.Add(CurrentStripFeederPackage);
                }

                CurrentStripFeederPackage = null;

                RefreshCommandEnabled();
            }, () => CurrentStripFeederPackage != null);

            CancelStripFeederCommand = new RelayCommand(() =>
            {
                CurrentStripFeeder = null;
                RefreshCommandEnabled();
            }, () => CurrentStripFeeder != null);

            CancelStripFeederPackageCommand = new RelayCommand(() =>
            {
                CurrentStripFeeder = null;
                CurrentStripFeederPackage = null;
                RefreshCommandEnabled();
            }, () => CurrentStripFeederPackage != null);

            SetStripFeederPackageBottomLeftCommand = new RelayCommand(() =>
            {
                CurrentStripFeederPackage.BottomY = _machine.MachinePosition.Y;
                CurrentStripFeederPackage.LeftX = _machine.MachinePosition.X;
            }, () => CurrentStripFeederPackage != null);

            SetStripFeederPackageSefaultXCommand = new RelayCommand(() =>
            {
                CurrentStripFeederPackage.DefaultRefHoleXOffset = Math.Round(_machine.MachinePosition.X - CurrentStripFeederPackage.LeftX, 1);
            }, () => CurrentStripFeederPackage != null);

            SetStripXOffsetCommand = new RelayCommand(() =>
            {
                CurrentStripFeeder.RefHoleXOffset = Math.Round(_machine.MachinePosition.X - CurrentStripFeederPackage.LeftX, 1);
            }, () => CurrentStripFeeder != null);

            SetStripYOffsetCommand = new RelayCommand(() =>
            {
                CurrentStripFeeder.RefHoleYOffset = Math.Round(_machine.MachinePosition.Y - CurrentStripFeederPackage.BottomY, 1);
            }, () => CurrentStripFeeder != null);

            GoToStripFeederCommand = new RelayCommand(GoToStripFeeder, () => CurrentStripFeeder != null);

            GoToStripFeederPackageCommand = new RelayCommand(GoToStripFeederPackage, () => CurrentStripFeederPackage != null);

            ClearStripXOffsetCommand = new RelayCommand(() =>
            {
                CurrentStripFeeder.RefHoleXOffset = null;
            }, () => CurrentStripFeeder != null);


        }

        public void RefreshCommandEnabled()
        {
            AddStripFeederCommand.RaiseCanExecuteChanged();
            AddStripFeederPackageCommand.RaiseCanExecuteChanged();
            SaveStripFeederCommand.RaiseCanExecuteChanged();
            SaveStripFeederPackageCommand.RaiseCanExecuteChanged();
            CancelStripFeederCommand.RaiseCanExecuteChanged();
            CancelStripFeederPackageCommand.RaiseCanExecuteChanged();
            SetStripFeederPackageBottomLeftCommand.RaiseCanExecuteChanged();
            SetStripXOffsetCommand.RaiseCanExecuteChanged();
            SetStripYOffsetCommand.RaiseCanExecuteChanged();
            SetStripFeederPackageSefaultXCommand.RaiseCanExecuteChanged();
            ClearStripXOffsetCommand.RaiseCanExecuteChanged();
            GoToStripFeederPackageCommand.RaiseCanExecuteChanged();
            GoToStripFeederCommand.RaiseCanExecuteChanged();
            GoToFirstPartCommand.RaiseCanExecuteChanged();
            GoToLastPartCommand.RaiseCanExecuteChanged();
            GoToCurrentPartCommand.RaiseCanExecuteChanged();

            SetCurrentPartIndexCommand.RaiseCanExecuteChanged();
            NextPartCommand.RaiseCanExecuteChanged();
            PrevPartCommand.RaiseCanExecuteChanged();
        }

        public void GoToPart(PositionType positionType)
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            var feedRate = _machine.Settings.FastFeedRate;
            var positon = GetCurrentPartPosition(_selectedPartStrip, positionType);
            _machine.GotoPoint(positon.X, positon.Y, feedRate);
            _jobVM.ShowBottomCamera = false;
            _jobVM.ShowTopCamera = true;

            var package = _jobVM.Packages.FirstOrDefault(pck => pck.Id == _selectedPartStrip.PackageId);

            _jobVM.PartSizeWidth = Convert.ToInt32(package.Width * 8);
            _jobVM.PartSizeHeight = Convert.ToInt32(package.Length * 8);

            _jobVM.SelectMVProfile("squarepart");

            RefreshCommandEnabled();
        }

        public ObservableCollection<StripFeederPackage> StripFeederPackages => _pnpMachine?.StripFeederPackages;

        StripFeederPackage _currentStripFeederPackage;
        public StripFeederPackage CurrentStripFeederPackage
        {
            get => _currentStripFeederPackage;
            set
            {
                Set(ref _currentStripFeederPackage, value);
                if (value != null)
                {
                    GoToStripFeederPackage();
                }

                RefreshCommandEnabled();
            }
        }

        private void GoToStripFeederPackage()
        {
            _jobVM.ShowCircles = true;
            _jobVM.ShowHarrisCorners = false;
            _jobVM.ShowPolygons = false;
            _jobVM.ShowLines = false;
            _jobVM.SelectMVProfile("tapehole");
            _jobVM.ShowBottomCamera = false;
            _jobVM.ShowTopCamera = true;
            _machine.TopLightOn = false;
            _machine.BottomLightOn = false;

            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            _machine.GotoPoint(CurrentStripFeederPackage.LeftX, CurrentStripFeederPackage.BottomY);
        }

        private void GotoTempPartLocation()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            var location = GetCurrentPartPosition(SelectedPartStrip, PositionType.TempPartIndex);
            if (location != null)
            {
                var deltaX = Math.Abs(location.X - _machine.MachinePosition.X);
                var deltaY = Math.Abs(location.Y - _machine.MachinePosition.Y);

                //    var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;

                _machine.GotoPoint(location.X, location.Y, _machine.Settings.FastFeedRate);
            }

            RefreshCommandEnabled();
        }

        public async void SetCurrentPartIndex()
        {
            SelectedPartStrip.CurrentPartIndex = SelectedPartStrip.TempPartIndex;
            await PnPMachineManager.SavePackagesAsync(_pnpMachine, _pnpJobFileName);
        }



        public void NextPart()
        {
            if (SelectedPartStrip.TempPartIndex < SelectedPartStrip.AvailablePartCount)
            {
                SelectedPartStrip.TempPartIndex++;
                GotoTempPartLocation();
            }
        }

        public void PrevPart()
        {
            if (SelectedPartStrip.TempPartIndex > 0)
            {
                SelectedPartStrip.TempPartIndex--;
                GotoTempPartLocation();
            }
        }
        private void GoToStripFeeder()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            _machine.GotoPoint(CurrentStripFeeder.RefHoleXOffset.HasValue ?
                    CurrentStripFeeder.RefHoleXOffset.Value + CurrentStripFeederPackage.LeftX : CurrentStripFeederPackage.LeftX + CurrentStripFeederPackage.DefaultRefHoleXOffset,
                    CurrentStripFeederPackage.BottomY + CurrentStripFeeder.RefHoleYOffset);

            _jobVM.SelectMVProfile("tapehole");
            _machine.TopLightOn = false;
            _machine.BottomLightOn = false;
        }


        StripFeeder _currentStripFeeder;
        public StripFeeder CurrentStripFeeder
        {
            get => _currentStripFeeder;
            set
            {
                Set(ref _currentStripFeeder, value);
                if (value != null)
                {
                    GoToStripFeeder();
                    if (!EntityHeader.IsNullOrEmpty(value.PartStrip))
                        _selectedPartStrip = _pnpMachine.PartStrips.FirstOrDefault(prt => prt.Id == value.PartStrip.Id);
                    else
                        _selectedPartStrip = null;

                    RaisePropertyChanged(nameof(SelectedPartStrip));
                }

                RefreshCommandEnabled();
            }
        }

        public void AddStripFeeder()
        {
            CurrentStripFeeder = new StripFeeder()
            {
                Name = $"Row {CurrentStripFeederPackage.StripFeeders.Count + 1}",
                Index = CurrentStripFeederPackage.StripFeeders.Count + 1,
                RefHoleYOffset = (CurrentStripFeederPackage.StripFeeders.Count * 9) + 4
            };

            RefreshCommandEnabled();
        }

        public void AddStripFeederPackage()
        {
            CurrentStripFeederPackage = new StripFeederPackage();
            CurrentStripFeeder = null;
            RefreshCommandEnabled();
        }

        public Point2D<double> GetCurrentPartPosition(PartStrip partStrip, PositionType positionType = PositionType.ReferenceHole)
        {
            var feederPackage = (from sfp in _pnpMachine.StripFeederPackages
                        where sfp.StripFeeders.Any(sf => sf.PartStrip?.Id == partStrip?.Id)
                        select sfp).FirstOrDefault();

            if(feederPackage != null)
            {
                var part = feederPackage.StripFeeders.FirstOrDefault(sf => sf.PartStrip?.Id == partStrip?.Id);
                var referenceHoleX = feederPackage.LeftX + (part.RefHoleXOffset.HasValue ? part.RefHoleXOffset.Value : feederPackage.DefaultRefHoleXOffset);
                var referenceHoleY = feederPackage.BottomY + part.RefHoleYOffset;
                var package = _pnpMachine.Packages.FirstOrDefault(pck => pck.Id == partStrip?.PackageId);
                if(package == null)
                {
                    throw new ArgumentNullException("Could not find package for part.");
                }

                var xScaler = 0.9965;

                switch (positionType)
                {
                    case PositionType.ReferenceHole:
                        _selectedPartStrip.TempPartIndex = 0;
                        return new Point2D<double>(referenceHoleX, referenceHoleY);

                    case PositionType.FirstPart:
                        _selectedPartStrip.TempPartIndex = 0;
                        return new Point2D<double>(referenceHoleX + package.CenterXFromHole, referenceHoleY + package.CenterYFromHole);

                    case PositionType.CurrentPart:
                        {
                            var xOffset = partStrip.CurrentPartIndex * package.SpacingX * xScaler;
                            if(_selectedPartStrip != null)
                                _selectedPartStrip.TempPartIndex = _selectedPartStrip.CurrentPartIndex;
                            return new Point2D<double>(referenceHoleX + package.CenterXFromHole + xOffset, referenceHoleY + package.CenterYFromHole);
                        }
                    case PositionType.TempPartIndex:
                        {
                            var xOffset = partStrip.TempPartIndex * package.SpacingX * xScaler;
                            return new Point2D<double>(referenceHoleX + package.CenterXFromHole + xOffset, referenceHoleY + package.CenterYFromHole);
                        }
                    case PositionType.LastPart:
                        {
                            var partCount = Math.Floor(partStrip.StripLength / package.SpacingX);
                            var xOffset = partCount * package.SpacingX * xScaler;
                            _selectedPartStrip.TempPartIndex = Convert.ToInt32(partCount);
                            return new Point2D<double>(referenceHoleX + package.CenterXFromHole + xOffset, referenceHoleY + package.CenterYFromHole);
                        }
                }
            }            

            return null;
        }

        internal void ResolvePart(PlaceableParts part)
        {
            var strip = _pnpMachine.PartStrips.Where(str => str.PackageName == part.Package && str.Value == part.Value).SingleOrDefault();
            if (strip != null)
            {
                var feederPackage = (from sfp in _pnpMachine.StripFeederPackages
                                     where sfp.StripFeeders.Any(sf => sf.PartStrip?.Id == strip.Id)
                                     select sfp).FirstOrDefault();

                if (feederPackage != null)
                {
                    var partOnFeeder = feederPackage.StripFeeders.FirstOrDefault(sf => sf.PartStrip?.Id == strip.Id);
                    part.StripFeederPackage = feederPackage.Name;
                    part.StripFeeder = partOnFeeder.Name;
                    part.PartStrip = strip;
                }
            }
        }

        public ObservableCollection<PartStrip> PartStrips { get => _pnpMachine?.PartStrips; }

        public RelayCommand AddStripFeederCommand { get; private set; }
        public RelayCommand AddStripFeederPackageCommand { get; private set; }

        public RelayCommand SaveStripFeederCommand { get; private set; }
        public RelayCommand SaveStripFeederPackageCommand { get; private set; }

        public RelayCommand CancelStripFeederCommand { get; private set; }
        public RelayCommand CancelStripFeederPackageCommand { get; private set; }

        public RelayCommand SetStripFeederPackageBottomLeftCommand { get; private set; }

        public RelayCommand SetStripFeederPackageSefaultXCommand { get; private set; }

        public RelayCommand SetStripXOffsetCommand { get; private set; }
        public RelayCommand ClearStripXOffsetCommand { get; private set; }
        public RelayCommand SetStripYOffsetCommand { get; private set; }

        public RelayCommand GoToStripFeederPackageCommand { get; private set; }
        public RelayCommand GoToStripFeederCommand { get; private set; }

        public RelayCommand GoToCurrentPartCommand { get; }
        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToLastPartCommand { get; }

        public RelayCommand SetCurrentPartIndexCommand { get; }
        public RelayCommand NextPartCommand { get; }
        public RelayCommand PrevPartCommand { get; }


        private PartStrip _selectedPartStrip;
        public PartStrip SelectedPartStrip
        {
            get => _selectedPartStrip;
            set
            {
                Set(ref _selectedPartStrip, value);
                if (value != null)
                {
                    CurrentStripFeeder.PartStrip = EntityHeader.Create(value.Id, value.ToString());
                }
            }
        }
    }
}
