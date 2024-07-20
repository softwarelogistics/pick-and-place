using LagoVista.Core;
using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;

namespace LagoVista.PickAndPlace.Models
{
    public class PartStrip : ModelBase
    {
        private Package _package;

        public PartStrip()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        private String _packageId;
        public String PackageId
        {
            get => _packageId;
            set => Set(ref _packageId, value);
        }

        private String _packageName;
        public String PackageName
        {
            get => _packageName;
            set => Set(ref _packageName, value);
        }

        private string _value;
        public string Value
        {
            get => _value;
            set { Set(ref _value, value); }
        }

        private double _referenceHoleX;
        public double ReferenceHoleX
        {
            get => _referenceHoleX;
            set => Set(ref _referenceHoleX, value);
        }

        private double _referenceHoleY;
        public double ReferenceHoleY
        {
            get => _referenceHoleY;
            set => Set(ref _referenceHoleY, value);
        }

        private int _currentPartIndex = 0;
        public int CurrentPartIndex
        {
            get => _currentPartIndex;
            set => Set(ref _currentPartIndex, value);
        }

        private string _partInStripMsg;
        [JsonIgnore]
        public String PartInStripMsg
        {
            get => _partInStripMsg;
            set => Set(ref _partInStripMsg, value);
        }

        private int _tempPartIndex = 0;
        [JsonIgnore]
        public int TempPartIndex
        {
            get => _tempPartIndex;
            set
            {
                Set(ref _tempPartIndex, value);
                RaisePropertyChanged(nameof(PartInStripMsg));

                PartInStripMsg = $"Part {TempPartIndex} of {AvailablePartCount}";
            }
        }

        private double _stripLength;
        public double StripLength
        {
            get => _stripLength;
            set
            {
                Set(ref _stripLength, value);
                RaisePropertyChanged(nameof(AvailablePartCount));
            }
        }

        private double _correctionAngleX = 0;
        public double CorrectionFactorX
        {
            get => _correctionAngleX;
            set => Set(ref _correctionAngleX, value);
        }

        private double _correctionAngleY = 0;
        public double CorrectionFactorY
        {
            get => _correctionAngleY;
            set => Set(ref _correctionAngleY, value);
        }

        private bool _polarized;
        public bool Polarized
        {
            get => _polarized;
            set => Set(ref _polarized, value);
        }

        public string DigiKeyPartNumber { get; set; }

        public int InventoryCount { get; set; }

        public StorageLocation PartUnit { get; set; }
        public StorageLocation PartShelf { get; set; }
        public StorageLocation PartColumn { get; set; }
        public StorageLocation PartRow { get; set; }

        public string Mfg { get; set; }
        public string MfgId { get; set; }

        public string SupplierPartNumber { get; set; }
        
        public string Supplier { get; set; }
        public string SupplierPage { get; set; }

        public string DataSheet { get; set; }

        private bool _active = true;
        public bool Ready
        {
            get => _active;
            set => Set(ref _active, value);
        }

        [JsonIgnore]
        public int AvailablePartCount
        {
            get => (int)StripLength / (int)(_package == null ? 4 : _package.SpacingX);
        }

        public void SetPackage(Package package)
        {
            _package = package;
        }

        public override string ToString()
        {
            return $"{Value} {PackageName}";
        }
    }
}
