using MvvmCross.Converters;
using ProxySuper.Core.Models.Hosts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProxySuper.Core.Converters
{
    public class LoginSecretTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return LoginSecretType.Password;
            return parameter;
        }

    }
}
