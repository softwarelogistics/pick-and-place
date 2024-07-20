using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PnPJob : ModelBase
    {
        PrintedCircuitBoard _board;
        [JsonIgnore]
        public PrintedCircuitBoard Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public PnPJob()
        {
            Parts = new ObservableCollection<Part>();
            BuildFlavors = new ObservableCollection<BuildFlavor>();
            BoardFiducial1 = new Point2D<double>();
            BoardFiducial2 = new Point2D<double>();
            BoardOffset = new Point2D<double>();
            BoardScaler = new Point2D<double>() { X = 1.0, Y = 1.0 };            
        }

        private Point2D<double> _boardFiducial1;
        public Point2D<double> BoardFiducial1
        {
            get => _boardFiducial1;
            set => Set(ref _boardFiducial1, value);
        }

        private Point2D<double> _boardOffset;
        public Point2D<double> BoardOffset
        {
            get => _boardOffset;
            set => Set(ref _boardOffset, value);
        }

        private Point2D<double> _boardFiducial2;
        public Point2D<double> BoardFiducial2
        {
            get => _boardFiducial2;
            set => Set(ref _boardFiducial2, value);
        }

        private Point2D<double> _boardScaler;
        public Point2D<double> BoardScaler
        {
            get => _boardScaler;
            set => Set(ref _boardScaler, value);
        }

        private string _pnpMachinePath;
        public string PnPMachinePath
        {
            get => _pnpMachinePath;
            set => Set(ref _pnpMachinePath, value);
        }

        public bool DispensePaste { get; set; }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        public ObservableCollection<BuildFlavor> BuildFlavors { get; set; }

        public ObservableCollection<Part> Parts { get; set; }

        public Task OpenAsync()
        {
            if (System.IO.File.Exists(EagleBRDFilePath))
            {
                var doc = XDocument.Load(EagleBRDFilePath);
                _board = EagleParser.ReadPCB(doc);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
            
        }
    }
}
