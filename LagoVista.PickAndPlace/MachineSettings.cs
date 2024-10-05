using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public class MachineSettings : ModelBase, INotifyPropertyChanged
    {
        public string Id { get; set; }

        public int StatusPollIntervalIdle { get; set; }
        public int StatusPollIntervalRunning { get; set; }
        public int ControllerBufferSize { get; set; }

        public double ViewportArcSplit { get; set; }
        public double ArcToLineSegmentLength { get; set; }
        public double SplitSegmentLength { get; set; }

        private Point2D<double> _knownCalibrationPoint = new Point2D<double>();
        public Point2D<double> KnownCalibrationPoint
        {
            get { return _knownCalibrationPoint; }
            set { Set(ref _knownCalibrationPoint, value); }
        }

        private Point2D<double> _tool1Offset = new Point2D<double>();
        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value); }
        }

        private Point2D<double> _tool2Offset = new Point2D<double>();
        public Point2D<double> Tool2Offset
        {
            get { return _tool2Offset; }
            set { Set(ref _tool2Offset, value); }
        }


        private Point2D<double> _pcbOffset = new Point2D<double>();
        public Point2D<double> PCBOffset
        {
            get { return _pcbOffset; }
            set { Set(ref _pcbOffset, value); }
        }

        private Point3D<double> _defaultWorkspaceHome = new Point3D<double>();
        public Point3D<double> DefaultWorkspaceHome
        {
            get { return _defaultWorkspaceHome; }
            set { Set(ref _defaultWorkspaceHome, value); }
        }

        private Point2D<double> _machineFiducial = new Point2D<double>();
        public Point2D<double> MachineFiducial
        {
            get { return _machineFiducial; }
            set { Set(ref _machineFiducial, value); }
        }

        private int _fastFeedRate = 10000;
        public int FastFeedRate
        {
            get { return _fastFeedRate; }
            set { Set(ref _fastFeedRate, value); }
        }

        SerialPortInfo _currentSerialPort;
        public SerialPortInfo CurrentSerialPort
        {
            get { return _currentSerialPort; }
            set { Set(ref _currentSerialPort, value); }
        }


        SerialPortInfo _serialPort2;
        public SerialPortInfo SerialPort2
        {
            get { return _serialPort2; }
            set { Set(ref _serialPort2, value); }
        }

        ConnectionTypes _connectionType;
        public ConnectionTypes ConnectionType
        {
            get { return _connectionType; }
            set { Set(ref _connectionType, value); }
        }

        private String _ipAddress;
        public String IPAddress
        {
            get { return _ipAddress; }
            set { Set(ref _ipAddress, value); }
        }

        private Point2D<double> _partStripScaler = new Point2D<double>() { X = 1.0, Y = 1.0 };
        public Point2D<double> PartStripScaler
        {
            get => _partStripScaler;
            set => Set(ref _partStripScaler, value);
        }


        [JsonIgnore]
        public double ToolSafeMoveHeight
        {
            get { return _currentNozzle.SafeMoveHeight; }
            set
            {
                _currentNozzle.SafeMoveHeight = value;
                RaisePropertyChanged(nameof(ToolSafeMoveHeight));
            }
        }

        [JsonIgnore]
        public double ToolPickHeight
        {
            get { return _currentNozzle.PickHeight; }
            set
            {
                _currentNozzle.PickHeight = value;
                RaisePropertyChanged(nameof(ToolPickHeight));
            }
        }

        ToolNozzle _currentNozzle = new ToolNozzle();
        public ToolNozzle CurrentNozzle
        {
            get { return _currentNozzle; }
            set
            {
                if(_currentNozzle != null)
                {
                    var currentNozzle = Nozzles.Where(noz => noz.Id == _currentNozzle.Id).FirstOrDefault();
                    if(currentNozzle != null)
                    {
                        currentNozzle.Name = _currentNozzle.Name;
                        currentNozzle.BoardHeight = _currentNozzle.BoardHeight;
                        currentNozzle.PickHeight = _currentNozzle.PickHeight;
                        currentNozzle.SafeMoveHeight = _currentNozzle.SafeMoveHeight;
                    }

                    _currentNozzle = value;                    
                    RaisePropertyChanged(nameof(CurrentNozzle));
                    RaisePropertyChanged(nameof(ToolPickHeight));
                    RaisePropertyChanged(nameof(ToolBoardHeight));
                    RaisePropertyChanged(nameof(ToolSafeMoveHeight));
                }
            }
        }

        ObservableCollection<ToolNozzle> _nozzles = new ObservableCollection<ToolNozzle>();
        public ObservableCollection<ToolNozzle> Nozzles
        {
            get { return _nozzles; }
            set { Set(ref _nozzles, value); }
        }

        /// <summary>
        /// Absolute position of the board in the Z axis, the actual place location will be 
        /// the this location plus the height of the part, we also will likely have different
        /// heights for the different nozzles.
        /// </summary>
        [JsonIgnore]
        public double ToolBoardHeight
        {
            get { return CurrentNozzle.BoardHeight; }
            set
            {
                CurrentNozzle.BoardHeight = value;
                RaisePropertyChanged(nameof(ToolBoardHeight));
            }
        }

        public string DefaultPnPMachineFile { get; set; }

        public bool EnableCodePreview { get; set; }
        public double ProbeSafeHeight { get; set; }
        public double ProbeMaxDepth { get; set; }
        public double ProbeMinimumHeight { get; set; }

        public bool PauseOnToolChange { get; set; }

        public double ProbeHeightMovementFeed { get; set; }

        public int ProbeTimeoutSeconds { get; set; }

        int _workAreaWidth;
        public int WorkAreaWidth
        {
            get { return _workAreaWidth; }
            set { Set(ref _workAreaWidth, value); }
        }

        int _workAreaHeight;
        public int WorkAreaHeight
        {
            get { return _workAreaHeight; }
            set { Set(ref _workAreaHeight, value); }
        }

        public bool AbortOnProbeFail { get; set; }
        public double ProbeFeed { get; set; }

        private double _xyStepSize;
        public double XYStepSize
        {
            get { return _xyStepSize; }
            set { Set(ref _xyStepSize, value); }
        }

        private double _zStepSize;
        public double ZStepSize
        {
            get { return _zStepSize; }
            set { Set(ref _zStepSize, value); }
        }

        private String _machineName;
        public String MachineName
        {
            get { return _machineName; }
            set { Set(ref _machineName, value); }
        }

        MachineOrigin _machineOrigin;
        public MachineOrigin MachineOrigin
        {
            get { return _machineOrigin; }
            set { Set(ref _machineOrigin, value); }
        }

        JogGCodeCommand _jogGCodeCommand;
        public JogGCodeCommand JogGCodeCommand
        {
            get { return _jogGCodeCommand; }
            set { Set(ref _jogGCodeCommand, value); }
        }

        MessageVerbosityLevels _messageVerbosity;
        public MessageVerbosityLevels MessageVerbosity
        {
            get { return _messageVerbosity; }
            set { Set(ref _messageVerbosity, value); }
        }

        private int _jogFeedRate;
        public int JogFeedRate
        {
            get { return _jogFeedRate; }
            set { Set(ref _jogFeedRate, value); }
        }

        StepModes _xyStepMode;
        public StepModes XYStepMode
        {
            get { return _xyStepMode; }
            set { Set(ref _xyStepMode, value); }
        }

        StepModes _zStepMode;
        public StepModes ZStepMode
        {
            get { return _zStepMode; }
            set { Set(ref _zStepMode, value); }
        }

        Camera _positioningCamera;
        public Camera PositioningCamera
        {
            get { return _positioningCamera; }
            set { Set(ref _positioningCamera, value); }
        }

        Camera _partInspectionCamera;
        public Camera PartInspectionCamera
        {
            get { return _partInspectionCamera; }
            set { Set(ref _partInspectionCamera, value); }
        }


        private double _maxX;
        public double MaxX 
        {
            get => _maxX;
            set => Set(ref _maxX, value);
        }

        private double _minX;
        public double MinX
        {
            get => _minX;
            set => Set(ref _minX, value);
        }

        private double _maxY;
        public double MaxY
        {
            get => _maxY;
            set => Set(ref _maxY, value);
        }

        private double _minY;
        public double MinY
        {
            get => _minY;
            set => Set(ref _minY, value);
        }

        public FirmwareTypes MachineType { get; set; }

        private string _settingsName;

        public async static Task<MachineSettings> LoadAsync(string settingsName)
        {
            try
            {
                var settings = await Services.Storage.GetAsync<MachineSettings>("settingsName.json");
                if (settings == null)
                    settings = MachineSettings.Default;

                settings._settingsName = settingsName;

                return settings;
            }
            catch (Exception)
            {
                return MachineSettings.Default;
            }
        }

        public List<string> Validate()
        {
            var errs = new List<string>();

            if (String.IsNullOrEmpty(MachineName))
            {
                errs.Add("Machine Name is Requried.");
            }

            return errs;
        }

        public double ProbeOffset { get; set; }

        public MachineSettings Clone()
        {
            return this.MemberwiseClone() as MachineSettings;
        }

        public static MachineSettings Default
        {
            get
            {
                return new MachineSettings()
                {
                    Id = Guid.NewGuid().ToString(),
                    MachineName = "Machine 1",
                    ProbeOffset = 0.0,
                    ControllerBufferSize = 120,
                    StatusPollIntervalIdle = 1000,
                    StatusPollIntervalRunning = 100,
                    JogFeedRate = 2000,
                    ProbeTimeoutSeconds = 30,
                    MessageVerbosity = MessageVerbosityLevels.Normal,
                    MachineOrigin = MachineOrigin.Bottom_Left,
                    JogGCodeCommand = JogGCodeCommand.G0,
                    ViewportArcSplit = 1,
                    EnableCodePreview = true,
                    ProbeSafeHeight = 5,
                    ProbeMinimumHeight = 1,
                    ProbeMaxDepth = 5,
                    AbortOnProbeFail = false,
                    ProbeFeed = 20,
                    ProbeHeightMovementFeed = 1000,
                    ArcToLineSegmentLength = 1,
                    SplitSegmentLength = 5,
                    XYStepSize = 1,
                    ZStepSize = 1,
                    WorkAreaWidth = 300,
                    WorkAreaHeight = 200
                };
            }
        }

    }
}
