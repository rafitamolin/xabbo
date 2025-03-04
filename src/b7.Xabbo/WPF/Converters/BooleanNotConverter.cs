﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace b7.Xabbo.WPF.Converters;

public class BooleanNotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => !((bool)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => !((bool)value);
}
