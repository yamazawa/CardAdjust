using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CardAdjust.Converters;

/// <summary>
/// 読取矩形が個別設定(true)か全体設定(false)かを、対応する色のBrushに変換する
///
/// 全体設定:青、個別設定:黄。
/// </summary>
public class RegionOverrideBrushConverter : IValueConverter
{
    private static readonly Brush IndividualBrush = Brushes.Yellow;
    private static readonly Brush GlobalBrush = Brushes.DodgerBlue;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? IndividualBrush : GlobalBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
