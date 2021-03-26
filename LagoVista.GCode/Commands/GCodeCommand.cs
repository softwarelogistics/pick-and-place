﻿using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.GCode.Commands
{
	public abstract class GCodeCommand : LagoVista.Core.Models.ModelBase
	{
        public enum StatusTypes
        {
            Ready, // After loaded before being sent.
            Pending, // When we start a job all items are pending.
            Queued, // Job manager says we should send, but it's not sent.
            Sent,   // work loop sent it to the machine
            Acknowledged, // Machine Acknowledged it
            Internal,
        }

        public enum Axis
        {
            XAxis,
            YAxis,
            ZAxis
        }

        public enum RotateDirection
        {
            Clockwise,
            CounterClockwise
        }

        private StatusTypes _status = StatusTypes.Ready;
        public StatusTypes Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public string OriginalLine { get; set; }

        public virtual string Line { get { return OriginalLine.TrimEnd(); } }

        public int LineNumber { get; set; }

        public abstract TimeSpan EstimatedRunTime { get; }

        public DateTime? StartTimeStamp { get; set; }

        public abstract Vector3 CurrentPosition { get; set; }

        public int MessageLength { get { return Line.Length + 1; } }

        public virtual void SetComment(string comment) { }

        public string Command { get; set; }

        public virtual void ApplyOffset(double x, double y, double z = 0) { }

        public virtual void Rotate(double degrees, Point2D<double> origin = null, Axis axis = Axis.ZAxis,RotateDirection direction = RotateDirection.CounterClockwise) { }
    }
}
