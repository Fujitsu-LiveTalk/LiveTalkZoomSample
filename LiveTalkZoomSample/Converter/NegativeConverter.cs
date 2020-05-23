/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：NegativeConverter
 * 概要      ：NegativeConverter
*/
using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveTalkZoomSample.Converter
{
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
