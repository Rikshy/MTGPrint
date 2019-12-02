using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System;

using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class PrintViewModel : INotifyPropertyChanged
    {        
        public PrintOptions PrintOptions { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ( value == null || parameter == null )
                return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals( targetValue,
                     StringComparison.InvariantCultureIgnoreCase );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ( value == null || parameter == null )
                return null;

            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if ( useValue )
                return Enum.Parse( targetType, targetValue );

            return null;
        }
    }
}
