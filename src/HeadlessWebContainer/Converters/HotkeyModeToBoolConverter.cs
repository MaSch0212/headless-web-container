using HeadlessWebContainer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HeadlessWebContainer.Converters
{
    [ValueConversion(typeof(HotkeyMode), typeof(bool))]
    public class HotkeyModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HotkeyMode mode)
                return mode == HotkeyMode.KeyPress;
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool @bool)
                return @bool ? HotkeyMode.KeyPress : HotkeyMode.KeyUpDown;
            return Binding.DoNothing;
        }
    }
}
