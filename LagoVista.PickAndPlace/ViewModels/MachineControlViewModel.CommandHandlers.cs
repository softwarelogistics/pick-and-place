using LagoVista.Core;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MachineControlViewModel
    {
        public void CycleStart()
        {
            Machine.CycleStart();
        }

        public void SoftReset()
        {
            Machine.SoftReset();
        }

        public void FeedHold()
        {
            Machine.FeedHold();
        }

        public void ClearAlarm()
        {
            Machine.ClearAlarm();
        }

        private void RelativeJog(JogDirections direction)
        {
            Machine.SendCommand("G91");

            switch (direction)
            {
                case JogDirections.XPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{XYStepSize.ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(-XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(-XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
            }

            Machine.SendCommand("G90");
        }

        private const double ShaftOffsetCorrection = 0.5;

        private void AbsoluteJog(JogDirections direction)
        {
            var current = Machine.WorkspacePosition;

            switch (direction)
            {
                case JogDirections.XPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(current.X + XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(current.Y + XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(current.Z + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(current.X - XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(current.Y - XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(current.Z - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool0 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool0 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool1 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool1 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.CMinus:
                    {
                        var newAngle = Math.Round(Machine.Tool2 - 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} E{(newAngle).ToDim()} F5000");
                        Machine.SendCommand("G90");

                        Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} E{(newAngle).ToDim()} F5000");
                        /*                        Machine.SendCommand("G91");
                                                Machine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                                                Machine.SendCommand("G90");*/

                        Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");
                    }
                    break;

                case JogDirections.CPlus:
                    {
                        var newAngle = Math.Round(Machine.Tool2 + 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} E{(newAngle).ToDim()} F5000");
                        /*Machine.SendCommand("G91");
                        Machine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                        Machine.SendCommand("G90");*/

                        Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");

                    }
                    break;
            }
        }

        public void Jog(JogDirections direction)
        {
            if ((Machine.Settings.MachineType == FirmwareTypes.Repeteir_PnP ||
                Machine.Settings.MachineType == FirmwareTypes.Marlin_Laser ||
                Machine.Settings.MachineType == FirmwareTypes.Marlin ||
                Machine.Settings.MachineType == FirmwareTypes.GRBL1_1) &&
                (direction != JogDirections.CMinus && direction != JogDirections.CPlus)
                )
            {
                RelativeJog(direction);
            }
            else
            {
                AbsoluteJog(direction);
            }
        }

        public void Home(HomeAxis axis)
        {
            switch (axis)
            {
                case HomeAxis.All:
                    Machine.SendCommand("G28 X Y");
                    Machine.SendCommand("T0");
                    Machine.SendCommand("G28 Z");
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G28 Z");
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.X:
                    Machine.SendCommand("G28 X");
                    break;
                case HomeAxis.Y:
                    Machine.SendCommand("G28 Y");
                    break;
                case HomeAxis.Z:
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T0:
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T1:
                    Machine.SendCommand("G28 P");
                    break;
                case HomeAxis.C:
                    Machine.SendCommand("G28 C");
                    break;
            }
        }

        public void ResetAxisToZero(ResetAxis axis)
        {
            switch (axis)
            {
                case ResetAxis.All:
                    if (Machine.Settings.MachineType == FirmwareTypes.GRBL1_1)
                    {
                        Machine.SendCommand("G10 P0 L20 X0 Y0 Z0");
                    }
                    else
                    {
                        Machine.SendCommand("G10 L20 P0 X0 Y0 Z0 C0");
                    }
                    break;
                case ResetAxis.X:
                    Machine.SendCommand("G10 L20 P0 X0");
                    break;
                case ResetAxis.Y:
                    Machine.SendCommand("G10 L20 P0 Y0");
                    break;
                case ResetAxis.Z:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.T0:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.T1:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.C:
                    Machine.SendCommand("G10 L20 P0 C0");
                    break;


            }
        }

        public void HandleKeyDown(WindowsKey key, bool isShift, bool isControl)
        {
            switch (key)
            {
                case WindowsKey.Home:
                    if (isControl)
                    {
                        Machine.SetWorkspaceHome();
                    }
                    else if (isShift)
                    {
                        Machine.HomingCycle();
                    }
                    else
                    {
                        Machine.GotoWorkspaceHome();
                    }

                    break;

                case WindowsKey.Left:
                    if (isControl)
                    {
                        Jog(JogDirections.CMinus);
                    }
                    else
                    {
                        Jog(JogDirections.XMinus);
                    }
                    break;
                case WindowsKey.Right:
                    if (isControl)
                    {
                        Jog(JogDirections.CPlus);
                    }
                    else
                    {
                        Jog(JogDirections.XPlus);
                    }
                    break;
                case WindowsKey.Up:
                    if (isControl)
                    {
                        Jog(JogDirections.T0Minus);
                    }
                    else
                    {
                        Jog(JogDirections.YPlus);
                    }
                    break;
                case WindowsKey.Down:
                    if (isControl)
                    {
                        Jog(JogDirections.T0Plus);
                    }
                    else
                    {
                        Jog(JogDirections.YMinus);
                    }
                    break;

                case WindowsKey.OemPlus:

                    switch (XYStepMode)
                    {
                        case StepModes.XLarge:
                            XYStepSizeSlider += 10;
                            XYStepSizeSlider = Math.Min(100, XYStepSizeSlider);
                            break;
                        case StepModes.Large:
                            XYStepSizeSlider = XYStepSizeSlider += 5;
                            XYStepSizeSlider = Math.Min(20, XYStepSizeSlider);
                            break;
                        case StepModes.Medium:
                            XYStepSizeSlider = XYStepSizeSlider += 1;
                            XYStepSizeSlider = Math.Min(10, XYStepSizeSlider);
                            break;
                        case StepModes.Small:
                            XYStepSizeSlider = XYStepSizeSlider += 0.1;
                            XYStepSizeSlider = Math.Min(1, XYStepSizeSlider);
                            break;
                        case StepModes.Micro:
                            XYStepSizeSlider = XYStepSizeSlider += 0.01;
                            XYStepSizeSlider = Math.Min(0.1, XYStepSizeSlider);
                            break;
                    }

                    break;
                case WindowsKey.OemMinus:


                    switch (XYStepMode)
                    {
                        case StepModes.XLarge:
                            XYStepSizeSlider = XYStepSizeSlider -= 10;
                            XYStepSizeSlider = Math.Max(20, XYStepSizeSlider);
                            break;
                        case StepModes.Large:
                            XYStepSizeSlider = XYStepSizeSlider -= 2.5;
                            XYStepSizeSlider = Math.Max(10, XYStepSizeSlider);
                            break;
                        case StepModes.Medium:
                            XYStepSizeSlider = XYStepSizeSlider -= 1;
                            XYStepSizeSlider = Math.Max(1, XYStepSizeSlider);
                            break;
                        case StepModes.Small:
                            XYStepSizeSlider = XYStepSizeSlider -= 0.1;
                            XYStepSizeSlider = Math.Max(0.1, XYStepSizeSlider);
                            break;
                        case StepModes.Micro:
                            XYStepSizeSlider = XYStepSizeSlider -= 0.01;
                            XYStepSizeSlider = Math.Max(0.01, XYStepSizeSlider);

                            break;
                    }


                    break;

                case WindowsKey.PageUp:
                    if (isControl)
                    {
                        switch (ZStepMode)
                        {
                            case StepModes.XLarge:
                                break;
                            case StepModes.Large:
                                break;
                            case StepModes.Medium:
                                ZStepMode = StepModes.Large;
                                break;
                            case StepModes.Small:
                                ZStepMode = StepModes.Medium;
                                break;
                            case StepModes.Micro:
                                ZStepMode = StepModes.Small;
                                break;
                        }
                    }
                    else
                    {
                        switch (XYStepMode)
                        {
                            case StepModes.XLarge:
                                break;
                            case StepModes.Large:
                                XYStepMode = StepModes.XLarge;
                                break;
                            case StepModes.Medium:
                                XYStepMode = StepModes.Large;
                                break;
                            case StepModes.Small:
                                XYStepMode = StepModes.Medium;
                                break;
                            case StepModes.Micro:
                                XYStepMode = StepModes.Small;
                                break;
                        }
                    }
                    break;
                case WindowsKey.PageDown:
                    if (isControl)
                    {
                        switch (ZStepMode)
                        {
                            case StepModes.XLarge:
                                ZStepMode = StepModes.Large;
                                break;
                            case StepModes.Large:
                                ZStepMode = StepModes.Medium;
                                break;
                            case StepModes.Medium:
                                ZStepMode = StepModes.Small;
                                break;
                            case StepModes.Small:
                                ZStepMode = StepModes.Micro;

                                break;
                            case StepModes.Micro:
                                break;
                        };
                    }
                    else
                    {
                        switch (XYStepMode)
                        {
                            case StepModes.XLarge:
                                XYStepMode = StepModes.Large;
                                break;
                            case StepModes.Large:
                                XYStepMode = StepModes.Medium;
                                break;
                            case StepModes.Medium:
                                XYStepMode = StepModes.Small;
                                break;
                            case StepModes.Small:

                                XYStepMode = StepModes.Micro;

                                break;
                            case StepModes.Micro:
                                break;
                        };
                    }
                    break;
            }
        }
    }
}