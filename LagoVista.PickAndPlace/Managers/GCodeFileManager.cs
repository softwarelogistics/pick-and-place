﻿using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode;
using LagoVista.GCode.Commands;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class GCodeFileManager : Core.Models.ModelBase, IGCodeFileManager
    {
        IMachine _machine;
        ILogger _logger;
        IToolChangeManager _toolChangeManager;

        GCodeFile _file;

        bool _isDirty;

        int _tail = 0;
        int _head = 0;

        int? _pendingToolChangeLine = null;

        DateTime? _started;

        public GCodeFileManager(IMachine machine, ILogger logger, IToolChangeManager toolChangeManager)
        {
            _machine = machine;
            _logger = logger;
            _toolChangeManager = toolChangeManager;

            Lines = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            RapidMoves = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Arcs = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
        }

        private async void HandleToolChange(ToolChangeCommand mcode)
        {
            await _toolChangeManager.HandleToolChange(mcode);
            _pendingToolChangeLine = null;
        }

        public GCodeCommand GetNextJobItem()
        {
            if (_started == null)
                _started = DateTime.Now;

            /* If we have queued up a pending tool change, don't send any more lines until tool change completed */
            if (_pendingToolChangeLine != null)
            {
                return null;
            }

            if (Head < _file.Commands.Count)
            {
                var cmd = _file.Commands[Head];
             
                /* If Next Command up is a Tool Change, set the nullable property to that line and bail. */
                if (cmd is ToolChangeCommand)
                {
                    if (_machine.Settings.PauseOnToolChange)
                    {
                        _pendingToolChangeLine = Head;
                    }
                    
                    return null;
                }

                Head++;

                cmd.Status = GCodeCommand.StatusTypes.Queued;

                return cmd;
            }

            return null;
        }

        public void SetGCode(string gcode)
        {
            var file =  GCodeFile.FromString(gcode, _logger);
            FindExtents(file);
            File = file;
        }

        public GCodeCommand CurrentCommand
        {
            get { return _file == null || Tail < _file.Commands.Count ? null : _file.Commands[Tail]; }
        }

        public int CommandAcknowledged()
        {
            var sentCommandLength = _file.Commands[Tail].MessageLength;
            if (_file.Commands[Tail].Status == GCodeCommand.StatusTypes.Sent)
            {
                _file.Commands[Tail].Status = GCodeCommand.StatusTypes.Acknowledged;
                Tail++;

                if (_pendingToolChangeLine != null && _pendingToolChangeLine.Value == Tail)
                {
                    _file.Commands[Tail].Status = GCodeCommand.StatusTypes.Internal;
                    HandleToolChange(_file.Commands[Tail] as ToolChangeCommand);
                    Head++;
                    Tail++;
                }

                if (Tail < _file.Commands.Count)
                {
                    _file.Commands[Tail].StartTimeStamp = DateTime.Now;
                }
                else
                {
                    RaisePropertyChanged(nameof(IsCompleted));
                }

                return sentCommandLength;
            }
            else
            {
                Debug.WriteLine("Attempt to acknowledge command but not sent.");

                return 0;
            }
        }
    }
}           