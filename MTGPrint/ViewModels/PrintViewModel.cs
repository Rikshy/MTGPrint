﻿using System.Globalization;
using System.Windows.Data;
using System;

using Caliburn.Micro;

using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class PrintViewModel : Screen
    {
        public PrintViewModel(BackgroundPrinter printer)
        {
            PrintOptions = printer.LoadPrintSettings();
        }

        public Deck Deck { get; set; }
        
        public PrintOptions PrintOptions { get; set; }

        public void Print()
            => TryCloseAsync(true);
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
