using DirectShowLib.BDA;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;

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

        public IMachine _machine;



        public StripFeederViewModel(IMachine machine)
        {
            _machine = machine;
            RaisePropertyChanged(nameof(PartStrips));
            RaisePropertyChanged(nameof(StripFeederPackages));
        }

        public void SetMachine(PnPMachine machine)
        {
            _pnpMachine = machine;
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
        }

        public ObservableCollection<StripFeederPackage> StripFeederPackages => _pnpMachine.StripFeederPackages;

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
            _machine.GotoPoint(CurrentStripFeederPackage.LeftX, CurrentStripFeederPackage.BottomY);
        }

        private void GoToStripFeeder()
        {
            _machine.GotoPoint(CurrentStripFeeder.RefHoleXOffset.HasValue ?
                    CurrentStripFeeder.RefHoleXOffset.Value + CurrentStripFeederPackage.LeftX : CurrentStripFeederPackage.LeftX + CurrentStripFeederPackage.DefaultRefHoleXOffset,
                    CurrentStripFeederPackage.BottomY + CurrentStripFeeder.RefHoleYOffset);
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
                        where sfp.StripFeeders.Any(sf => sf.PartStrip?.Id == partStrip.Id)
                        select sfp).FirstOrDefault();

            if(feederPackage != null)
            {
                var part = feederPackage.StripFeeders.FirstOrDefault(sf => sf.PartStrip?.Id == partStrip.Id);
                var referenceHoleX = feederPackage.LeftX + (part.RefHoleXOffset.HasValue ? part.RefHoleXOffset.Value : feederPackage.DefaultRefHoleXOffset);
                var referenceHoleY = feederPackage.BottomY + part.RefHoleYOffset;
                var package = _pnpMachine.Packages.FirstOrDefault(pck => pck.Id == partStrip.PackageId);
                if(package == null)
                {
                    throw new ArgumentNullException("Could not find package for part.");
                }

                var xScaler = 0.9965;

                switch (positionType)
                {
                    case PositionType.ReferenceHole:
                        return new Point2D<double>(referenceHoleX, referenceHoleY);

                    case PositionType.FirstPart:
                        return new Point2D<double>(referenceHoleX + package.CenterXFromHole, referenceHoleY + package.CenterYFromHole);

                    case PositionType.CurrentPart:
                        {
                            var xOffset = partStrip.CurrentPartIndex * package.SpacingX * xScaler;
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
