using System.Globalization;

namespace CebuFinalApp;

public class FavoriteColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If true (is favorite) -> Red, else -> Light Gray
        return (bool)value ? Color.FromArgb("#E53E3E") : Color.FromArgb("#CBD5E0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}