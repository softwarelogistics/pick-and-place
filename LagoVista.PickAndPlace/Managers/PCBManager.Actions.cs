﻿using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class PCBManager
    {

        public Point2D<double> GetAdjustedPoint(Point2D<double> point)
        {
            return point.Rotate(-MeasuredOffsetAngle);
        }

        public void EnableFiducialPicker()
        {
            IsSetFiducialMode = true;
        }
    }
}
