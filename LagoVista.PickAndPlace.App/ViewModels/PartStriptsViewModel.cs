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
            GoToPartCommand = new RelayCommand(GotoPart, () => SelectedPartStrip != null && SelectedPackage != null);
            GoToReferencePointCommand = new RelayCommand(GoToReferencePoint, () => SelectedPartStrip != null);
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

        public void GotoPart()
        {
            _parent.Machine.GotoPoint(SelectedPartStrip.ReferenceHoleX + SelectedPackage.CenterXFromHole, SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole);
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

                SetStripOriginCommand.RaiseCanExecuteChanged();
                AddPartStripCommand.RaiseCanExecuteChanged();
                GoToPartCommand.RaiseCanExecuteChanged();
                GoToReferencePointCommand.RaiseCanExecuteChanged();
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

                    SetStripOriginCommand.RaiseCanExecuteChanged();
                    AddPartStripCommand.RaiseCanExecuteChanged();
                    GoToPartCommand.RaiseCanExecuteChanged();
                    GoToReferencePointCommand.RaiseCanExecuteChanged();
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
        public RelayCommand GoToPartCommand { get; }
    }
}
