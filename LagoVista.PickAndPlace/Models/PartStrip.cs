using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;

namespace LagoVista.PickAndPlace.Models
{
    public class PartStrip : ModelBase
    {
        private Package _package;

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

        [JsonIgnore]
        public int AvailablePartCount
        {
            get => (int)StripLength / (int)(_package == null ? 4 : _package.SpacingX);
        }

        public void SetPackage(Package package)
        {
            _package = package;
        }
    }
}
