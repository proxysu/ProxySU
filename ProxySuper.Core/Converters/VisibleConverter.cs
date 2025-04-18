using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProxySuper.Core.Converters
{
    public class VisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMatch = (value != null && value.Equals(parameter));
            // 如果匹配则显示，否则隐藏（Collapsed）
            return isMatch ? Visibility.Visible : Visibility.Hidden;//Collapsed;
            //return value.Equals(true) ? Visibility.Visible : Visibility.Hidden;//Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //if (value == null)
            //{
            //    return false;
            //}

            //if (value.Equals(Visibility.Visible))
            //{
            //    return true;
            //}

            //return false;
        }
    }

    public class BooleanOrToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果任一条件为 true，则返回 Visible，否则返回 Hidden/Collapsed
            foreach (var value in values)
            {
                if (value is bool boolVal && boolVal)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;//Collapsed
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
