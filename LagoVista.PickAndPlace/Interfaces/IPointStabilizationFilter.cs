using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPointStabilizationFilter
    {
        void Add(Point2D<double> cameraOffsetPixels);

        bool HasStabilizedPoint { get; }

        int PointCount { get; }

        void Reset();

        Point2D<double> StabilizedPoint { get; }
    }
}
