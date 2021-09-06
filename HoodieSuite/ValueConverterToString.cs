using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace HoodieSuite
{
    public class TabItemGetHeaderTextXaml : IValueConverter
    {
        public object Convert(object value, Type typeTarget,
                              object param, CultureInfo culture)
        {
            if (value is TabItem TabItem)
            {
                if (TabItem.Header != null)
                {
                    return TabItem.Header.ToString();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type typeTarget,
                                  object param, CultureInfo culture)
        {
            return null;
        }
    }
    public class TabItemGetIsSelected : IValueConverter
    {
        public object Convert(object value, Type typeTarget,
                              object param, CultureInfo culture)
        {
            if (value is TabItem TabItem)
            {
                return TabItem.IsSelected;
            }
            else
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type typeTarget,
                                  object param, CultureInfo culture)
        {
            return null;
        }
    }
}
