using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private bool _isOnLast;

        private int _inspectIndex;
        public int InspectIndex
        {
            get => _inspectIndex;
            set => Set(ref _inspectIndex, value);
        }

        private string _inspectMessage;
        public string InspectMessage
        {
            get => _inspectMessage;
            set => Set(ref _inspectMessage, value);
        }

        public void FirstInspect()
        {
            InspectIndex = 0;
        }

        private void GoToFirstPartInPartsToPlace()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());

            var package = _pnpMachine.Packages.Where(pck => pck.Name == SelectedInspectPart.Package).FirstOrDefault();
            var partStrip = SelectedInspectPart.PartStrip;

            var x = partStrip.ReferenceHoleX + package.CenterXFromHole + (partStrip.CurrentPartIndex * package.SpacingX);
            var y = partStrip.ReferenceHoleY + package.CenterYFromHole;
            var gcode = $"G0 X{x} Y{y} F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);
            _isOnLast = false;

            InspectMessage = $"{partStrip.Value} with package {partStrip.PackageName}, {_inspectIndex} of {ConfigurationParts.Count}.";
        }

        private void GoToLastPartInPartsToPlace()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());

            var package = _pnpMachine.Packages.Where(pck => pck.Name == SelectedInspectPart.Package).FirstOrDefault();
            var partStrip = SelectedInspectPart.PartStrip;

            var x = partStrip.ReferenceHoleX + package.CenterXFromHole + (partStrip.CurrentPartIndex * package.SpacingX) + ((SelectedInspectPart.Parts.Count - 1) * package.SpacingX);
            var y = partStrip.ReferenceHoleY + package.CenterYFromHole;
            var gcode = $"G0 X{x} Y{y} F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);
            _isOnLast = true;

            InspectMessage = $"Last {partStrip.Value} with package {partStrip.PackageName}, {_inspectIndex} of {ConfigurationParts.Count}.";
        }

        public void NextInspect()
        {
            if(_selectedInspectPart == null)
            {
                _selectedInspectPart = ConfigurationParts[InspectIndex];
            }

            if(SelectedInspectPart.Parts.Count == 1 || _isOnLast)
            {
                if (_inspectIndex < ConfigurationParts.Count - 1)
                {
                    InspectIndex++;
                    _selectedInspectPart = ConfigurationParts[InspectIndex];
                    RaisePropertyChanged(nameof(SelectedInspectPart));
                    NextInspectCommand.RaiseCanExecuteChanged();
                    PrevInspectCommand.RaiseCanExecuteChanged();
                    FirstInspectCommand.RaiseCanExecuteChanged();

                    GoToFirstPartInPartsToPlace();                    
                }
            }
            else
            {
                GoToLastPartInPartsToPlace();
            }
        }

        public void PrevInspect()
        {
            if (_isOnLast)
            {
                GoToFirstPartInPartsToPlace();
            }
            else
            {
                if (InspectIndex > 0)
                {
                    InspectIndex--;
                    NextInspectCommand.RaiseCanExecuteChanged();
                    PrevInspectCommand.RaiseCanExecuteChanged();
                    FirstInspectCommand.RaiseCanExecuteChanged();

                    _selectedInspectPart = ConfigurationParts[InspectIndex];
                    RaisePropertyChanged(nameof(SelectedInspectPart));
                    if (SelectedInspectPart.Parts.Count > 1)
                    {
                        GoToLastPartInPartsToPlace();
                    }
                    else
                    {
                        GoToFirstPartInPartsToPlace();
                    }
                }
            }
        }

        public void InspectFirst()
        {
            InspectIndex = 0;
            _selectedInspectPart = ConfigurationParts[InspectIndex];
            RaisePropertyChanged(nameof(SelectedInspectPart));
            GoToFirstPartInPartsToPlace();

            PrevInspectCommand.RaiseCanExecuteChanged();
            NextInspectCommand.RaiseCanExecuteChanged();
            FirstInspectCommand.RaiseCanExecuteChanged();
        }

        PlaceableParts _selectedInspectPart;
        public PlaceableParts SelectedInspectPart
        {
            get => _selectedInspectPart;
            set
            {
                Set(ref _selectedInspectPart, value);
                if (value != null)
                {
                    _inspectIndex = ConfigurationParts.IndexOf(value);
                    GoToFirstPartInPartsToPlace();
                }
            }
        }
    }
}
