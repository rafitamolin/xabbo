﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace b7.Xabbo.WPF.Converters;

public class MultiValueConverter : List<IValueConverter>, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
