﻿using LagoVista.Core.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class SettingsViewModel
    {

        public String SelectedPortId
        {
            get { return Settings.CurrentSerialPort.Id; }
            set
            {
                var port = SerialPorts.Where(prt => prt.Id == value).FirstOrDefault();
                if (port == null)
                    port = SerialPorts.First();
                else
                {
                    port.BaudRate = 115200;
                }

                Settings.CurrentSerialPort = port;
            }
        }

        public ObservableCollection<String> ConnectionTypes { get; private set; }
        public ObservableCollection<String> MachineTypes { get; private set; }
        public ObservableCollection<String> GCodeJogCommands { get; private set; }
        public ObservableCollection<String> MachineOrigins { get; private set; }
        public ObservableCollection<String> MessageOutputLevels { get; private set; }

        public string MessgeOutputLevel
        {
            get { return Settings.MessageVerbosity.ToString(); }
            set
            {
                Settings.MessageVerbosity = (MessageVerbosityLevels)Enum.Parse(typeof(MessageVerbosityLevels), value);
            }
        }

        public String GCodeJogCommand
        {
            get { return Settings.JogGCodeCommand.ToString().Replace("_"," "); }
            set
            {
                Settings.JogGCodeCommand = (JogGCodeCommand)Enum.Parse(typeof(JogGCodeCommand), value);
            }
        }

        public String ConnectionType
        {
            get { return Settings.ConnectionType.ToString().Replace("_", " "); }
            set
            {
                var newValue = value.Replace(" ", "_");
                Settings.ConnectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), newValue);
                RaisePropertyChanged(nameof(CanSetIPAddress));
                RaisePropertyChanged(nameof(CanSelectSerialPort));
            }
        }

        public String MachineOrigin
        {
            get { return Settings.MachineOrigin.ToString().Replace("_", " "); }
            set
            {
                var newValue = value.Replace(" ", "_");
                Settings.MachineOrigin = (MachineOrigin)Enum.Parse(typeof(MachineOrigin), newValue);
            }
        }

        public List<Camera> Cameras { get; private set; }

        public String PositioningCameraId
        {
            get
            {
                return Settings.PositioningCamera != null ? Settings.PositioningCamera.Id : "-1";
            }
            set
            {
                if (value != null && value != "-1")
                {
                    Settings.PositioningCamera = Cameras.Where(cmr => cmr.Id == value).FirstOrDefault();
                }
                else
                {
                    Settings.PositioningCamera = null;
                }
            }
        }

        public String InspectionCameraId
        {
            get
            {
                return Settings.PartInspectionCamera != null ? Settings.PartInspectionCamera.Id : "-1";
            }
            set
            {
                if (value != null && value != "-1")
                {
                    Settings.PartInspectionCamera = Cameras.Where(cmr => cmr.Id == value).FirstOrDefault();
                }
                else
                {
                    Settings.PartInspectionCamera = null;
                }
            }
        }

        public String MachineType
        {
            get { return Settings.MachineType.ToString().Replace("_", "."); }
            set { Settings.MachineType = (FirmwareTypes)Enum.Parse(typeof(FirmwareTypes), value.Replace(".", "_")); }
        }

        public MachineSettings Settings
        {
            get { return _settings; }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; private set; }

        public bool CanSetIPAddress
        {
            get { return Settings.ConnectionType == LagoVista.PickAndPlace.ConnectionTypes.Network && CanChangeMachineConfig; }
        }

        public bool CanSelectSerialPort
        {
            get { return Settings.ConnectionType == LagoVista.PickAndPlace.ConnectionTypes.Serial_Port && CanChangeMachineConfig; }
        }
    }
}
