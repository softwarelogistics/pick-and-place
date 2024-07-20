using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Models
{
    public class StripFeederPackage : ModelBase
    {
        public string Id { get; set; }

        private double _bottomY;
        private double _leftX;
        private double _defaultRefHoleXOffset;

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public double BottomY
        {
            get => _bottomY;
            set => Set(ref _bottomY, value);
        }

        public double LeftX
        {
            get => _leftX;
            set => Set(ref _leftX, value);
        }

        public double DefaultRefHoleXOffset
        {
            get => _defaultRefHoleXOffset;
            set => Set(ref _defaultRefHoleXOffset, value);
        }

        public ObservableCollection<StripFeeder> StripFeeders { get; set; } = new ObservableCollection<StripFeeder>();
    }

    public class StripFeeder : ModelBase
    {
        private int _index;

        public string Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public int Index
        {
            get => _index;
            set => Set(ref _index, value);
        }

        private double? _refHoleXOffset;
        private double _refHoleYOffset;

        public double? RefHoleXOffset
        {
            get => _refHoleXOffset;
            set => Set(ref _refHoleXOffset, value);
        }

        public double RefHoleYOffset
        {
            get => _refHoleYOffset;
            set => Set(ref _refHoleYOffset, value);
        }

        private EntityHeader _partStripId;
        public EntityHeader PartStrip 
        { 
            get => _partStripId;
            set => Set(ref _partStripId, value);
        }
    }
}
