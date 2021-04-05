﻿using LagoVista.Core.Models;
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
        }

        private Point2D<double> _boardFiducial1;
        public Point2D<double> BoardFiducial1
        {
            get => _boardFiducial1;
            set => Set(ref _boardFiducial1, value);
        }

        private Point2D<double> _boardFiducial2;
        public Point2D<double> BoardFiducial2
        {
            get => _boardFiducial2;
            set => Set(ref _boardFiducial2, value);
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

            var doc = XDocument.Load(EagleBRDFilePath);

            _board = EagleParser.ReadPCB(doc);

            return Task.FromResult(default(object));
        }
    }
}
