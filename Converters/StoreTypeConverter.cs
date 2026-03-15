using LiveryManager.Models;
using System.Globalization;
using System.Windows.Data;

namespace LiveryManager.Converters;

[ValueConversion(typeof(StoreType), typeof(string))]
public class StoreTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            StoreType.Steam => "Steam",
            StoreType.MicrosoftStore => "Microsoft Store",
            StoreType.Custom => "Custom",
            _ => string.Empty
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
