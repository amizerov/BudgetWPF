using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Budget.Model;
using DevExpress.Xpf.Editors.Helpers;

namespace Budget.Converters
{
    public class PercentCategoryToColorWidthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double percent = value.TryConvertToDouble();
            if ("Credit".CompareTo(parameter) == 0)
                return percent > 80 ? Color.FromArgb(255, 136, 185, 103) : Color.FromArgb(255, 229, 131, 132);
            else
                return percent > 80 ? Color.FromArgb(255, 229, 131, 132) : Color.FromArgb(255, 136, 185, 103);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
