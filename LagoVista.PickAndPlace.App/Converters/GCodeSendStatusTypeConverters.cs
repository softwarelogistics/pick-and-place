using LagoVista.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class GCodeSendBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var statusType = (GCodeCommand.StatusTypes)value;
            switch(statusType)
            {
                case GCodeCommand.StatusTypes.Ready: return "White";
                case GCodeCommand.StatusTypes.Queued: return "LightGray";
            }

            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GCodeSendForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var statusType = (GCodeCommand.StatusTypes)value;

            switch (statusType)
            {
                case GCodeCommand.StatusTypes.Ready: return "DarkGray";
                case GCodeCommand.StatusTypes.Queued: return "Black";
                case GCodeCommand.StatusTypes.Sent: return "Blue";
                case GCodeCommand.StatusTypes.Acknowledged: return "Green";
            }

            return "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
