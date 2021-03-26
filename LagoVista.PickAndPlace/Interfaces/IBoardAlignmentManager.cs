using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Interfaces
{

    public enum BoardAlignmentManagerStates
    {
        Idle,
        EvaluatingInitialAlignment,
        CenteringFirstFiducial,
        StabilzingAfterFirstFiducialMove,
        MovingToSecondFiducial,
        StabilzingAfterSecondFiducialMove,
        CenteringSecondFiducial,
        BoardAlignmentDetermined,
        TimedOut,
        Failed,
    }

    public interface IBoardAlignmentManager : IDisposable
    {
        void CornerLocated(Point2D<double> offsetFromCenter);

        void CircleLocated(Point2D<double> offsetFromCenter);

        void SetNewMachineLocation(Point2D<double> newLocation);

        void AlignBoard();

        BoardAlignmentManagerStates State { get; set; }

        void CalculateOffsets();
    }
}
