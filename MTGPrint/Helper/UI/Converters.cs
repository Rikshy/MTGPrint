using System.Windows.Markup;
using System.Globalization;
using System.Windows.Data;
using System;

namespace MTGPrint.Helper.UI
{
    public abstract class BaseValueConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
    public class SelectedTabConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (int)value;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (DecklistGrabber.Method)value;
    }
}
