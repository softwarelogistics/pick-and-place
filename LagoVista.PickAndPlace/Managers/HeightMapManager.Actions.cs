﻿using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class HeightMapManager
    {
        private bool _hasSetFirstProbeOffsetToZero = false;

        private double _initialOffset; 

        public async void ProbeCompleted(Vector3 position)
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null)
            {
                CancelProbing();
                return;
            }

            if (HeightMap == null)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without valid Height Map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Height Map");
                Status = HeightMapStatus.NotAvailable;
                CancelProbing();
                return;
            }

            if (_currentPoint == null)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without Current Point.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Current Point");
                Status = HeightMapStatus.NotPopulated;
                CancelProbing();
                return;
            }

            if (!_hasSetFirstProbeOffsetToZero)
            {
                _initialOffset = position.Z;
                HeightMap.SetPointHeight(_currentPoint, 0);
                _hasSetFirstProbeOffsetToZero = true;
                await Task.Delay(2000);
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"First Completed - Zero Z Axis.");
            }
            else
            {
                var offset = position.Z - _initialOffset;
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Completed Point X={_currentPoint.Point.X.ToDim()}, Y={_currentPoint.Point.Y.ToDim()}, Z={offset.ToDim()}");
                HeightMap.SetPointHeight(_currentPoint, offset );
            }

            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Postion as returned {position.Z.ToDim()}.");

            RaisePropertyChanged(nameof(HeightMap));
            _currentPoint = null;

            if (HeightMap.Status == HeightMapStatus.Populated)
            {
                Status = HeightMapStatus.Populated;
                Machine.SendCommand($"G0 Z{Machine.Settings.ProbeSafeHeight.ToDim()}");
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Creating Height Map Completed");
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Next - Apply Height Map to GCode");
                CancelProbing();

                await Core.PlatformSupport.Services.Popups.ShowAsync("Capturing Height Map completed.  You can now save or apply this height map to your GCode.");
            }
            else
            {
                HeightMapProbeNextPoint();
            }
        }

        public void NewHeightMap(HeightMap heightMap)
        {
            HeightMap = heightMap;
            HeightMap.Refresh();
            ConstructVisuals();
        }

        private void ConstructVisuals()
        {
            if (!HasHeightMap)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMapManager_ConstructVisuals", "Attempt to construct visual w/o a height map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to construct visual w/o a height map.");
            }

            if (HeightMap.GridSize == 2.5)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Grid size must be creater than 2.5, current grid size {HeightMap.GridSize}.");
                return;
            }

        }

        private string _heightMapPath;

        public async Task OpenHeightMapAsync(string path)
        {
            _heightMapPath = path;
            HeightMap = await Core.PlatformSupport.Services.Storage.GetAsync<HeightMap>(path);
            HeightMap.Initialized = true;
        }

        public async Task SaveHeightMapAsync(string path)
        {
            _heightMapPath = path;
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, path);
        }

        public async Task SaveHeightMapAsync()
        {
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, _heightMapPath);
        }

        public void CreateTestPattern()
        {
            var heightMap = new Models.HeightMap(Machine, Logger);
            if (Machine.PCBManager.HasBoard)
            {
                heightMap.Min = new Core.Models.Drawing.Vector2(Machine.PCBManager.Project.ScrapSides, Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.Max = new Core.Models.Drawing.Vector2(Machine.PCBManager.Board.Width + Machine.PCBManager.Project.ScrapSides, Machine.PCBManager.Board.Height + Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.GridSize = Machine.PCBManager.Project.HeightMapGridSize;
            }

            heightMap.Refresh();
            heightMap.FillWithTestPattern();
            HeightMap = heightMap;
        }

        public void CloseHeightMap()
        {
            HeightMap = null;
        }

        public void ProbeFailed()
        {
            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");
            CancelProbing();
        }

        public void CancelProbing()
        {
            Machine.SetMode(OperatingMode.Manual);
        }

        HeightMapProbePoint _currentPoint;

        private void HeightMapProbeNextPoint()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Longer in Probing Mode - Can't Continue.");
                CancelProbing();
                return;
            }

            if (!Machine.Connected)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine No Longer Connected - Can't Continue.");
                CancelProbing();
                return;
            }

            if (HeightMap == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Height Map Empty - Can't Continue.");
                CancelProbing();
                return;
            }

            if (HeightMap.Status == HeightMapStatus.Populated)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Unexpected Completion, Please Review before Continuing");
                Machine.SetMode(OperatingMode.Manual);
            }

            _currentPoint = HeightMap.GetNextPoint();
            if (_currentPoint == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Point Available - Can't Continue.");
                return;
            }

            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Point X={_currentPoint.Point.X.ToDim()}, Y={_currentPoint.Point.Y.ToDim()}");

            Machine.SendCommand($"G0 Z{Machine.Settings.ProbeMinimumHeight.ToDim()}");
            Machine.SendCommand($"G0 X{_currentPoint.Point.X.ToDim()} Y{_currentPoint.Point.Y.ToDim()}");

            Machine.SendCommand($"G38.3 Z-{Machine.Settings.ProbeMaxDepth.ToDim()} F{Machine.Settings.ProbeFeed}");
        }

        public void StartProbing()
        {
            if (!Machine.Connected)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Not Connected - Can't start.");
                return;
            }

            if (Machine.Mode != OperatingMode.Manual)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Busy - Can't start.");
                return;
            }

            if (!Machine.Connected || Machine.Mode != OperatingMode.Manual || HeightMap == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Height Map - Can't start.");
                return;
            }

            if (HeightMap.TotalPoints == 0)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Empty Height Map - Can't Start");
                return;
            }

            if (!Machine.SetMode(OperatingMode.ProbingHeightMap))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Couldn't Transition to Probe Mode.");
                return;
            }

            _hasSetFirstProbeOffsetToZero = false;


            Status = HeightMapStatus.Populating;

            Machine.AddStatusMessage(StatusMessageTypes.Info, "Creating Height Map - Started");

            Machine.SendCommand("G90");
            
            HeightMapProbeNextPoint();
        }

        public void Reset()
        {
            if (HeightMap != null)
            {
                HeightMap.Reset();
            }
        }

        public void PauseProbing()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;
        }
    }
}
