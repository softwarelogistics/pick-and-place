using LagoVista.GCode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private async Task SendInstructionSequenceAsync(List<string> cmds)
        {
            var file = GCodeFile.FromList(cmds, Logger);
            Machine.SetFile(file);
            Machine.GCodeFileManager.StartJob();
            while (Machine.Mode == OperatingMode.SendingGCodeFile) await Task.Delay(1);
        }

        private string SafeHeightGCodeGCode()
        {
            return $"G0 Z{Machine.Settings.ToolSafeMoveHeight} F{Machine.Settings.FastFeedRate}";
        }

        private string PickHeightGCode()
        {
            return $"G0 Z{Machine.Settings.ToolPickHeight} F{Machine.Settings.FastFeedRate}";
        }

        private string PlaceHeightGCode(PickAndPlace.Models.Package package)
        {
            return $"G0 Z{Machine.Settings.ToolBoardHeight - package.Height} F{Machine.Settings.FastFeedRate}";
        }

        private string DwellGCode(int pauseMS)
        {
            return $"G4 P{pauseMS}";
        }

        private string RotationGCode(double cRotation)
        {
            if (cRotation == 360)
                cRotation = 0;
            else if (cRotation > 360)
                cRotation -= 360;
            else if (cRotation == 270)
                cRotation = -90;

            var scaledAngle = cRotation;
            return $"G0 E{scaledAngle} F10000";
        }

        private string WaitForComplete()
        {
            return "M400";
        }

        private string ProduceVacuumGCode(bool value)
        {
            switch (Machine.Settings.MachineType)
            {
                case FirmwareTypes.Repeteir_PnP: return $"M42 P27 S{(value ? 255 : 0)}";
                case FirmwareTypes.LagoVista_PnP: return $"M64 S{(value ? 255 : 0)}";
            }

            throw new Exception($"Can't produce vacuum GCode for machien type: {Machine.Settings.MachineType} .");
        }

        private string ProducePuffGCode(bool value)
        {
            switch (Machine.Settings.MachineType)
            {
                case FirmwareTypes.Repeteir_PnP:
                    return ($"M42 P23 S{(value ? 255 : 0)}\n");
            }

            throw new Exception($"Can't produce vacuum GCode for machien type: {Machine.Settings.MachineType} .");
        }
    }
}
