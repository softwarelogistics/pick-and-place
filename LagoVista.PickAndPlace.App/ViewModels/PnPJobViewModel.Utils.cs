using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        public override async Task InitAsync()
        {
            await base.InitAsync();
            LoadingMask = false;

            Machine.TopLightOn = true;
            Machine.BottomLightOn = false;

            if (!String.IsNullOrEmpty(_job.PnPMachinePath) && System.IO.File.Exists(_job.PnPMachinePath))
            {
                PnPMachine = await PnPMachineManager.GetPnPMachineAsync(_job.PnPMachinePath);
                PackageLibraryVM.SetMachine(PnPMachine);
                PartStripsViewModel.SetMachine(PnPMachine, _job.PnPMachinePath);
                StripFeederVM.SetMachine(PnPMachine, _job.PnPMachinePath);
            }

            await ToolAlignmentVM.InitAsync();
            StartCapture();
        }

        private void PopulateParts()
        {
            Parts.Clear();

            foreach (var entry in _billOfMaterials.SMDEntries)
            {
                if (!Parts.Where(prt => prt.PackageName == entry.Package.Name &&
                                        prt.LibraryName == entry.Package.LibraryName &&
                                        prt.Value == entry.Value).Any())
                {
                    Parts.Add(new Part()
                    {
                        Count = entry.Components.Count,
                        LibraryName = entry.Package.LibraryName,
                        PackageName = entry.Package.Name,
                        Value = entry.Value
                    });
                }
            }
        }

        public void ExportBOM()
        {
            var bldr = new StringBuilder();

            foreach(var partByValue in SelectedBuildFlavor.Components.Where(prt=>prt.Included).GroupBy(prt=>prt.Value))
            {
                var value = partByValue.Key;
                bldr.Append($"{value}\t");
                foreach(var part in partByValue)
                    bldr.Append($"{part.Name}, ");

                bldr.AppendLine();
           }

            System.IO.File.WriteAllText(@$"X:\PartList {SelectedBuildFlavor.Name}.txt", bldr.ToString());
        }

        private void PopulateConfigurationParts()
        {
            ConfigurationParts.Clear();
            if (SelectedBuildFlavor != null)
            {
                var commonParts = SelectedBuildFlavor.Components.Where(prt => prt.Included).GroupBy(prt => prt.Key.ToLower());

                foreach (var entry in commonParts)
                {
                    var part = new PlaceableParts()
                    {
                        Count = entry.Count(),
                        Value = entry.First().Value.ToUpper(),
                        Package = entry.First().PackageName.ToUpper(),

                    };

                    part.Parts = new ObservableCollection<Component>();

                    foreach (var specificPart in entry)
                    {
                        var placedPart = SelectedBuildFlavor.Components.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                        if (placedPart != null)
                        {
                            part.Parts.Add(placedPart);
                        }
                    }

                    ConfigurationParts.Add(part);
                }

                if (_pnpMachine != null)
                {
                    foreach (var part in ConfigurationParts)
                    {
                        StripFeederVM.ResolvePart(part);
                    }
                }

                InspectIndex = 0;
                PrevInspectCommand.RaiseCanExecuteChanged();
                NextInspectCommand.RaiseCanExecuteChanged();
                FirstInspectCommand.RaiseCanExecuteChanged();
            }
        }

        private void SetNewHome()
        {
            Machine.SendCommand($"G92 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y}");
            Machine.SendCommand(SafeHeightGCodeGCode());
            var gcode = $"G1 X0 Y0 F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);

            ShowCircles = false;

            LocatorState = MVLocatorState.Default;
        }
    }
}
