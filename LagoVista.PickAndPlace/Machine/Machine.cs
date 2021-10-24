﻿using System;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using System.ComponentModel;
using LagoVista.PickAndPlace.Util;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.GCode;

namespace LagoVista.PickAndPlace
{
    public partial class Machine : IMachine
    {
        MachinesRepo _machineRepo;

        public event PropertyChangedEventHandler PropertyChanged;

        public Machine(MachinesRepo repo)
        {
            _machineRepo = repo;

            Messages = new System.Collections.ObjectModel.ObservableCollection<Models.StatusMessage>();
            AddStatusMessage(StatusMessageTypes.Info, "Startup.");

            ToolChangeManager = new Managers.ToolChangeManager(this, Services.Logger);
            GCodeFileManager = new Managers.GCodeFileManager(this, Core.PlatformSupport.Services.Logger, ToolChangeManager);
            PCBManager = new Managers.PCBManager(this, Core.PlatformSupport.Services.Logger);
            HeightMapManager = new Managers.HeightMapManager(this, Core.PlatformSupport.Services.Logger, PCBManager);
            ProbingManager = new Managers.ProbingManager(this, Core.PlatformSupport.Services.Logger);
            MachineVisionManager = new Managers.MachineVisionManager(this, Core.PlatformSupport.Services.Logger, PCBManager);

            var pointStabilizationFilter = new PointStabilizationFilter(Constants.PixelToleranceEpsilon, Constants.PixelStabilizationToleranceCount);

            BoardAlignmentManager = new Managers.BoardAlignmentManager(this, Core.PlatformSupport.Services.Logger, PCBManager, pointStabilizationFilter);
        }

        public Task InitAsync()
        {
            IsInitialized = true;

            return Task.FromResult(default(object));
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            Services.DispatcherServices.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
            );
        }

        public void ClearQueue()
        {
            if (Mode != OperatingMode.Manual)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Not in manual mode.");
                return;
            }

            lock (_toSend)
            {
                _toSend.Clear();
            }
        }

        //TODO: Removed Compiled Option
        private static Regex GCodeSplitter = new Regex(@"(G)\s*(\-?\d+\.?\d*)");

        /// <summary>
        /// Updates Status info from each line sent
        /// </summary
        /// <param name="line"></param>
        private void UpdateStatus(string line)
        {
            if (!Connected)
                return;

            //we use a Regex here so G91.1 etc don't get recognized as G91

            foreach (Match m in GCodeSplitter.Matches(line))
            {
                if (m.Groups[1].Value != "G")
                    continue;

                float code = float.Parse(m.Groups[2].Value);

                if (code == 17)
                    Plane = ArcPlane.XY;
                if (code == 18)
                    Plane = ArcPlane.YZ;
                if (code == 19)
                    Plane = ArcPlane.ZX;

                if (code == 20)
                    Unit = ParseUnit.Imperial;
                if (code == 21)
                    Unit = ParseUnit.Metric;

                if (code == 90)
                    DistanceMode = ParseDistanceMode.Absolute;
                if (code == 91)
                    DistanceMode = ParseDistanceMode.Relative;
            }
        }

        public void SetFile(GCodeFile file)
        {
            GCodeFileManager.SetFile(file);
        }

        public bool CanSetMode(OperatingMode mode)
        {
            return true;
        }

        public bool SetMode(OperatingMode mode)
        {
            Mode = mode;
            return true;
        }

        private bool _busy;
        public bool Busy
        {
            set
            {
                _busy = value;
                BusyStatus = value ? "Busy" : "Idle";
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(BusyStatus));
            }
            get => _busy;
        }

        public string BusyStatus { get; set; } = "-";

        public void GotoPoint(double x, double y, double feedRate)
        {
            SendCommand($"G1 X{x.ToDim()} Y{y.ToDim()} F{feedRate}");
        }

        public void GotoPoint(Point2D<double> point, bool rapidMove = true)
        {
            GotoPoint(point.X, point.Y, rapidMove);
        }

        public void GotoPoint(double x, double y, bool rapidMove = true)
        {
            var cmd = rapidMove ? "G0" : "G1";
            if (Settings.MachineType != FirmwareTypes.Repeteir_PnP)
            {
                x = Math.Max(0, Math.Min(x, Settings.WorkAreaWidth));
                y = Math.Max(0, Math.Min(y, Settings.WorkAreaHeight));
            }

            SendCommand($"{cmd} X{x.ToDim()} Y{y.ToDim()}");
        }

        public void GotoPoint(double x, double y, double z, bool rapidMove = true)
        {
            var cmd = rapidMove ? "G0" : "G1";
            if (Settings.MachineType != FirmwareTypes.Repeteir_PnP)
            {
                x = Math.Max(0, Math.Min(x, Settings.WorkAreaWidth));
                y = Math.Max(0, Math.Min(y, Settings.WorkAreaHeight));
            }

            SendCommand($"{cmd} X{x.ToDim()} Y{y.ToDim()} Z{z.ToDim()}");
        }
    }
}