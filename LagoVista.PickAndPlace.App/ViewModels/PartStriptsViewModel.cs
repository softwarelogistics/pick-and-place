using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class PartStriptsViewModel : MachineVisionViewModelBase
    {
        private MachineVisionViewModelBase _parent;
        private PnPMachine _pnpMachine;
        private string _pnpJobFileName;

        public PartStriptsViewModel(IMachine machine, MachineVisionViewModelBase parent) : base(machine)
        {
            _parent = parent;
            AddPartStripCommand = new RelayCommand(AddPartStrip, () => _pnpMachine != null);
            SetStripOriginCommand = new RelayCommand(SetStripOrigin, () => SelectedPartStrip != null);
            GoToFirstPartCommand = new RelayCommand(GoToFirstPart, () => SelectedPartStrip != null && SelectedPackage != null);

            NextPartCommand = new RelayCommand(NextPart, () => SelectedPartStrip != null && SelectedPackage != null && SelectedPartStrip.AvailablePartCount > SelectedPartStrip.TempPartIndex);
            PrevPartCommand = new RelayCommand(PrevPart, () => SelectedPartStrip != null && SelectedPackage != null && SelectedPartStrip.TempPartIndex > 0);
            
            GoToCurrentPartCommand = new RelayCommand(GoToCurrentPart, () => SelectedPartStrip != null && SelectedPackage != null);
            GoToReferencePointCommand = new RelayCommand(GoToReferencePoint, () => SelectedPartStrip != null);

            SetCurrentPartIndexCommand = new RelayCommand(SetCurrentPartIndex, () => SelectedPartStrip != null);
        }

        private void RefreshCommandEnabled()
        {
            AddPartStripCommand.RaiseCanExecuteChanged();
            SetStripOriginCommand.RaiseCanExecuteChanged();
            GoToFirstPartCommand.RaiseCanExecuteChanged();

            NextPartCommand.RaiseCanExecuteChanged();
            PrevPartCommand.RaiseCanExecuteChanged();

            GoToCurrentPartCommand.RaiseCanExecuteChanged();
            GoToReferencePointCommand.RaiseCanExecuteChanged();

            SetCurrentPartIndexCommand.RaiseCanExecuteChanged();
        }

        public void AddPartStrip()
        {
            var partStrip = new PartStrip()
            {
                PackageId = "?",
                PackageName = "Uknown",
                Value = "?"
            };

            PartStrips.Add(partStrip);
            SelectedPartStrip = partStrip;
            SavePnPMachine();
        }

        private async void SavePnPMachine()
        {
            await PnPMachineManager.SavePackagesAsync(_pnpMachine, _pnpJobFileName);
        }

        public void GoToReferencePoint()
        {
            _parent.Machine.GotoPoint(SelectedPartStrip.ReferenceHoleX, SelectedPartStrip.ReferenceHoleY);
        }

        public void GoToCurrentPart()
        {
            _parent.Machine.GotoPoint(
                   SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.CurrentPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole,
                   SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole);            
        }

        public void NextPart()
        {
            SelectedPartStrip.TempPartIndex++;

            _parent.Machine.GotoPoint(
                   SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.TempPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole,
                   SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole);

            RefreshCommandEnabled();
        }

        public void PrevPart()
        {
            if (SelectedPartStrip.TempPartIndex > 0)
            {
                SelectedPartStrip.TempPartIndex--;
                _parent.Machine.GotoPoint(
                       SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.TempPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole,
                       SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole);

                RefreshCommandEnabled();
            }
        }

        public void SetCurrentPartIndex()
        {
            SelectedPartStrip.CurrentPartIndex = SelectedPartStrip.TempPartIndex;
            SavePnPMachine();
        }

        public void GoToFirstPart()
        {
            SelectedPartStrip.TempPartIndex = 0;
            _parent.Machine.GotoPoint(SelectedPartStrip.ReferenceHoleX + SelectedPackage.CenterXFromHole, SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole);
            RefreshCommandEnabled();
        }

        public void SetStripOrigin()
        {
            SelectedPartStrip.ReferenceHoleX = Machine.MachinePosition.X;
            SelectedPartStrip.ReferenceHoleY = Machine.MachinePosition.Y;
            SavePnPMachine();
        }

        public void SetMachine(PnPMachine machine, string pnpJobFileName)
        {
            _pnpMachine = machine ?? throw new ArgumentNullException(nameof(machine));
            _pnpJobFileName = pnpJobFileName ?? throw new ArgumentNullException(nameof(pnpJobFileName));
            PackageDefinitions = _pnpMachine.Packages;

            AddPartStripCommand.RaiseCanExecuteChanged();

            foreach(var partStrip in PartStrips)
            {
                partStrip.SetPackage(PackageDefinitions.FirstOrDefault(pck => pck.Id == partStrip.PackageId));
            }

            RaisePropertyChanged(nameof(PartStrips));
            RaisePropertyChanged(nameof(PackageDefinitions));

            RefreshCommandEnabled();
        }

        public ObservableCollection<PartStrip> PartStrips
        {
            get => _pnpMachine?.PartStrips;
        }

        PartStrip _selectedPartStrip;
        public PartStrip SelectedPartStrip
        {
            get => _selectedPartStrip;
            set
            {
                Set(ref _selectedPartStrip, value);
                SetStripOriginCommand.RaiseCanExecuteChanged();
                _selectedPackage = _pnpMachine.Packages.Where(pk => pk.Id == value.PackageId).FirstOrDefault();
                if(_selectedPackage == null)
                {
                    _selectedPackage = PackageDefinitions.First();
                }

                RaisePropertyChanged(nameof(SelectedPackage));
                RefreshCommandEnabled();

                GoToCurrentPart();
            }
        }

        Package _selectedPackage;
        public Package SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                Set(ref _selectedPackage, value);

                if (value != null)
                {
                    SelectedPartStrip.PackageId = value.Id;
                    SelectedPartStrip.PackageName = value.Name;
                    SelectedPartStrip.SetPackage(value);
                    SavePnPMachine();

                    RefreshCommandEnabled();
                }
            }
        }


        ObservableCollection<Package> _packageDefinitions;
        public ObservableCollection<Package> PackageDefinitions
        {
            get => _packageDefinitions;
            set
            {
                _packageDefinitions = new ObservableCollection<Package>(value);
                _packageDefinitions.Insert(0, new Package() { Id = "-1", Name = "-select-" });
            }
        }

        public RelayCommand SetStripOriginCommand { get; }
        public RelayCommand AddPartStripCommand { get; }
        public RelayCommand GoToReferencePointCommand { get; }
        public RelayCommand SetCurrentPartIndexCommand { get; }

        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToCurrentPartCommand { get; }

        public RelayCommand NextPartCommand { get; }
        public RelayCommand PrevPartCommand { get; }
    }
}
