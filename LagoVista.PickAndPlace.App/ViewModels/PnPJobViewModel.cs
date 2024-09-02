using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel : MachineVisionViewModelBase
    {
        private bool _isEditing;
        private bool _isPaused;
        private bool _isDirty = false;
        private Dictionary<int, Point2D<double>> _nozzleCalibration;
        private int samplesAtPoint = 0;
        private int _targetAngle = 0;
        private List<Point2D<double>> _averagePoints;
        private bool _isPlacingParts = false;
        private BOM _billOfMaterials;
        private int _partIndex = 0;


        public PnPJobViewModel(IMachine machine, PnPJob job) : base(machine)
        {
            _billOfMaterials = new BOM(job.Board);
            _job = job;
            _isDirty = true;

            AddCommands();

            BuildFlavors = job.BuildFlavors;
            SelectedBuildFlavor = job.BuildFlavors.FirstOrDefault();
            if (SelectedBuildFlavor == null)
            {
                SelectedBuildFlavor = new BuildFlavor()
                {
                    Name = "Default"
                };

                foreach (var entry in _billOfMaterials.SMDEntries)
                {
                    foreach (var component in entry.Components)
                    {
                        component.Included = true;
                        SelectedBuildFlavor.Components.Add(component);
                    }
                }

                job.BuildFlavors.Add(SelectedBuildFlavor);
            }

            StripFeederVM = new StripFeederViewModel(machine, this);
            PackageLibraryVM = new PackageLibraryViewModel();
            PartStripsViewModel = new PartStripsViewModel(machine, this, job, StripFeederVM);
            ToolAlignmentVM = new ToolAlignmentViewModel(machine);
            
            GoToFiducial1Command = new RelayCommand(() => GoToFiducial(1));
            GoToFiducial2Command = new RelayCommand(() => GoToFiducial(2));

            PopulateParts();
            PopulateConfigurationParts();
        }

        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            switch (LocatorState)
            {
                case MVLocatorState.WorkHome:
                    Machine.SetWorkspaceHome();
                    LocatorState = MVLocatorState.Idle;
                    Status = "W/S Home Found";
                    break;
                case MVLocatorState.MachineFidicual:
                    SetNewHome();
                    break;
                case MVLocatorState.NozzleCalibration:
                    PerformBottomCameraCalibration(point, diameter, new Point2D<double>(0, 0));
                    break;
                default:
                    break;
            }
        }

        public override async Task IsClosingAsync()
        {
            await SaveJobAsync();
            await base.IsClosingAsync();
        }
    }
}
