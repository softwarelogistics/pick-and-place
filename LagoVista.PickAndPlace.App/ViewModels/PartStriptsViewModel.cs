﻿using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class PartStriptsViewModel : MachineVisionViewModelBase
    {
        private MachineVisionViewModelBase _parent;
        private PnPMachine _pnpMachine;
        private string _pnpJobFileName;

        enum InspectStates
        {
            ReferenceHole,
            CurrentPart,
            LastPart,
        }

        public PartStriptsViewModel(IMachine machine, MachineVisionViewModelBase parent) : base(machine)
        {
            _parent = parent;
            AddPartStripCommand = new RelayCommand(AddPartStrip, () => _pnpMachine != null);
            SetStripOriginCommand = new RelayCommand(SetStripOrigin, () => SelectedPartStrip != null);
            SetCorrectionCommand = new RelayCommand(SetCorrection, () => SelectedPartStrip != null);
            GoToFirstPartCommand = new RelayCommand(GoToFirstPart, () => SelectedPartStrip != null && SelectedPackage != null);
            GoToLastPartCommand = new RelayCommand(GoToLastPart, () => SelectedPartStrip != null && SelectedPackage != null);
            GoToLastPartUncalibratedCommand = new RelayCommand(GoToLastPartUncalibrated, () => SelectedPartStrip != null && SelectedPackage != null);
            NextPartCommand = new RelayCommand(NextPart, () => SelectedPartStrip != null && SelectedPackage != null && SelectedPartStrip.AvailablePartCount > SelectedPartStrip.TempPartIndex);
            PrevPartCommand = new RelayCommand(PrevPart, () => SelectedPartStrip != null && SelectedPackage != null && SelectedPartStrip.TempPartIndex > 0);

            GoToCurrentPartCommand = new RelayCommand(GoToCurrentPart, () => SelectedPartStrip != null && SelectedPackage != null);
            GoToReferencePointCommand = new RelayCommand(GoToReferencePoint, () => SelectedPartStrip != null);

            SetCurrentPartIndexCommand = new RelayCommand(SetCurrentPartIndex, () => SelectedPartStrip != null);

            InspectNextCommand = new RelayCommand(InspectNext, CanInspectNext);
            InspectPrevCommand = new RelayCommand(InspectPrev, CanInspectPrev);

            SortStripsCommand = new RelayCommand(() => _pnpMachine?.SortPartStrips());
        }

        private void RefreshCommandEnabled()
        {
            AddPartStripCommand.RaiseCanExecuteChanged();
            SetStripOriginCommand.RaiseCanExecuteChanged();
            GoToFirstPartCommand.RaiseCanExecuteChanged();
            GoToLastPartCommand.RaiseCanExecuteChanged();
            GoToLastPartUncalibratedCommand.RaiseCanExecuteChanged();
            SetCorrectionCommand.RaiseCanExecuteChanged();

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

        private bool CanInspectNext()
        {
            return SelectedPartStrip != PartStrips.Last() || _inspectState != InspectStates.LastPart;
        }

        private bool CanInspectPrev()
        {
            return SelectedPartStrip != null && (SelectedPartStrip != PartStrips.First() || _inspectState != InspectStates.ReferenceHole);
        }

        private async void SavePnPMachine()
        {
            await PnPMachineManager.SavePackagesAsync(_pnpMachine, _pnpJobFileName);
        }

        public void GoToReferencePoint()
        {
            var deltaX = Math.Abs(SelectedPartStrip.ReferenceHoleX - Machine.MachinePosition.X);
            var deltaY = Math.Abs(SelectedPartStrip.ReferenceHoleY - Machine.MachinePosition.Y);
            var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;

            _parent.Machine.GotoPoint(SelectedPartStrip.ReferenceHoleX, SelectedPartStrip.ReferenceHoleY, feedRate);
            _inspectState = InspectStates.ReferenceHole;
        }

        public void GoToCurrentPart()
        {
            var partLocationRatio = (double)SelectedPartStrip.CurrentPartIndex / (double)SelectedPartStrip.AvailablePartCount;
            var xOffset = SelectedPartStrip.CorrectionFactorX * partLocationRatio;
            var yOffset = SelectedPartStrip.CorrectionFactorY * partLocationRatio;

            var newX = SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.CurrentPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole + xOffset;
            var newY = SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole + yOffset;

            var deltaX = Math.Abs(newX - Machine.MachinePosition.X);
            var deltaY = Math.Abs(newY - Machine.MachinePosition.Y);
            var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;

            _parent.Machine.GotoPoint(newX, newY, feedRate);
            _inspectState = InspectStates.CurrentPart;
        }

        private void GotoTempPartLocation()
        {
            var partLocationRatio = (double)SelectedPartStrip.TempPartIndex / (double)SelectedPartStrip.AvailablePartCount;

            var xOffset = SelectedPartStrip.CorrectionFactorX * partLocationRatio;
            var yOffset = SelectedPartStrip.CorrectionFactorY * partLocationRatio;

            var newX = SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.TempPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole + xOffset;
            var newY = SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole + yOffset;

            var deltaX = Math.Abs(newX - Machine.MachinePosition.X);
            var deltaY = Math.Abs(newY - Machine.MachinePosition.Y);
            var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;

            _parent.Machine.GotoPoint(newX, newY, feedRate);

            RefreshCommandEnabled();
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

        private InspectStates _inspectState = InspectStates.ReferenceHole;

        public void InspectNext()
        {
            if (SelectedPartStrip == null)
            {
                SelectedPartStrip = PartStrips.First();
                GoToCurrentPart();
            }
            else
            {
                if (_inspectState == InspectStates.ReferenceHole)
                {
                    GoToCurrentPart();
                }
                else if(_inspectState == InspectStates.CurrentPart)
                {
                    GoToLastPart();
                }
                else
                {
                    var idx = PartStrips.IndexOf(SelectedPartStrip);
                    if (idx < PartStrips.Count)
                    {
                        var nextIdx = idx + 1;
                        SelectedPartStrip = PartStrips[nextIdx];
                        GoToReferencePoint();                        
                    }
                }
            }

            InspectNextCommand.RaiseCanExecuteChanged();
            InspectPrevCommand.RaiseCanExecuteChanged();
        }

        public void InspectPrev()
        {
            if(SelectedPartStrip != null)
            {
                if(_inspectState == InspectStates.ReferenceHole)
                {
                    var idx = PartStrips.IndexOf(SelectedPartStrip);
                    if (idx > 0)
                    {
                        SelectedPartStrip = PartStrips[--idx];
                        GoToLastPart();
                    }
                }
                else if(_inspectState == InspectStates.CurrentPart)
                {
                    GoToReferencePoint();
                }
                else if (_inspectState == InspectStates.LastPart)
                {
                    GoToCurrentPart();
                }
            }

            InspectNextCommand.RaiseCanExecuteChanged();
            InspectPrevCommand.RaiseCanExecuteChanged();
        }

        public void SetCurrentPartIndex()
        {
            SelectedPartStrip.CurrentPartIndex = SelectedPartStrip.TempPartIndex;
            SavePnPMachine();
        }

        public void GoToFirstPart()
        {
            SelectedPartStrip.TempPartIndex = 0;
            var newX = SelectedPartStrip.ReferenceHoleX + SelectedPackage.CenterXFromHole;
            var newY = SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole;
            var deltaX = Math.Abs(newX - Machine.MachinePosition.X);
            var deltaY = Math.Abs(newY - Machine.MachinePosition.Y);
            var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;
            _parent.Machine.GotoPoint(newX, newY, feedRate);
            RefreshCommandEnabled();
        }

        public void GoToLastPartUncalibrated()
        {
            var lastPartX = SelectedPartStrip.ReferenceHoleX + SelectedPackage.CenterXFromHole + SelectedPartStrip.StripLength;
            var lastPartY = SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole;
            _parent.Machine.GotoPoint(lastPartX, lastPartY);
            RefreshCommandEnabled();
        }

        public void GoToLastPart()
        {
            SelectedPartStrip.TempPartIndex = Convert.ToInt32(SelectedPartStrip.StripLength / SelectedPackage.SpacingX);

            var partLocationRatio = (double)SelectedPartStrip.TempPartIndex / (double)SelectedPartStrip.AvailablePartCount;
            var xOffset = SelectedPartStrip.CorrectionFactorX * partLocationRatio;
            var yOffset = SelectedPartStrip.CorrectionFactorY * partLocationRatio;

            _parent.Machine.GotoPoint(
                   SelectedPartStrip.ReferenceHoleX + (SelectedPartStrip.TempPartIndex * SelectedPackage.SpacingX) + SelectedPackage.CenterXFromHole + xOffset,
                   SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole + yOffset);

            RefreshCommandEnabled();
            _inspectState = InspectStates.LastPart;
        }

        public void SetStripOrigin()
        {
            SelectedPartStrip.ReferenceHoleX = Machine.MachinePosition.X;
            SelectedPartStrip.ReferenceHoleY = Machine.MachinePosition.Y;
            SavePnPMachine();
        }

        public void SetCorrection()
        {
            var lastPartX = SelectedPartStrip.ReferenceHoleX + SelectedPackage.CenterXFromHole + SelectedPartStrip.StripLength;
            var lastPartY = SelectedPartStrip.ReferenceHoleY + SelectedPackage.CenterYFromHole;
            var deltaX = _parent.Machine.MachinePosition.X - lastPartX;
            var deltaY = _parent.Machine.MachinePosition.Y - lastPartY;
            Debug.WriteLine("DELTA " + deltaX + " " + deltaY);
            SavePnPMachine();
        }

        public void SetMachine(PnPMachine machine, string pnpJobFileName)
        {
            _pnpMachine = machine ?? throw new ArgumentNullException(nameof(machine));
            _pnpJobFileName = pnpJobFileName ?? throw new ArgumentNullException(nameof(pnpJobFileName));
            PackageDefinitions = _pnpMachine.Packages;

            AddPartStripCommand.RaiseCanExecuteChanged();

            foreach (var partStrip in PartStrips)
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
                if (_selectedPackage == null)
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

        public RelayCommand SetCorrectionCommand { get; }
        public RelayCommand SetStripOriginCommand { get; }
        public RelayCommand AddPartStripCommand { get; }
        public RelayCommand GoToReferencePointCommand { get; }
        public RelayCommand SetCurrentPartIndexCommand { get; }

        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToLastPartCommand { get; }
        public RelayCommand GoToLastPartUncalibratedCommand { get; }
        public RelayCommand GoToCurrentPartCommand { get; }

        public RelayCommand NextPartCommand { get; }
        public RelayCommand PrevPartCommand { get; }

        public RelayCommand InspectPrevCommand { get; }
        public RelayCommand InspectNextCommand { get; }

        public RelayCommand SortStripsCommand { get; }
    }
}
