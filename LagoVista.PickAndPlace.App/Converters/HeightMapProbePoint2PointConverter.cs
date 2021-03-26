using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class HeightMapProbePoint2PointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            var points = value as ObservableCollection<HeightMapProbePoint>;
            if(points == null)
            {
                throw new Exception("Expected ObservableCollection<HeightMapProbePoint>");
            }

            var outputPoints = new Point3DCollection();

            foreach(var point in points)
            {
                outputPoints.Add(new Point3D(point.Point.X, point.Point.Y, point.Point.Z));
            }

            return outputPoints;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
