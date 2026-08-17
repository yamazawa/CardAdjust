using System.Windows;
using System.Windows.Controls;

namespace CardAdjust.Behaviors;

/// <summary>
/// TextBoxのSelectionStart/SelectionLengthをバインド可能にする添付ビヘイビア
///
/// TextBox標準のSelectionStart/SelectionLengthはCLRプロパティのみでバインドできないため、
/// SelectionChangedイベントを介して添付プロパティへSetCurrentValueで反映する。
/// </summary>
public static class TextBoxSelectionBehavior
{
    public static readonly DependencyProperty SelectionStartProperty =
        DependencyProperty.RegisterAttached("SelectionStart", typeof(int), typeof(TextBoxSelectionBehavior),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SelectionLengthProperty =
        DependencyProperty.RegisterAttached("SelectionLength", typeof(int), typeof(TextBoxSelectionBehavior),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TextBoxSelectionBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetSelectionStart(DependencyObject element, int value) => element.SetValue(SelectionStartProperty, value);

    public static int GetSelectionStart(DependencyObject element) => (int)element.GetValue(SelectionStartProperty);

    public static void SetSelectionLength(DependencyObject element, int value) => element.SetValue(SelectionLengthProperty, value);

    public static int GetSelectionLength(DependencyObject element) => (int)element.GetValue(SelectionLengthProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        if ((bool)e.NewValue)
            textBox.SelectionChanged += TextBox_SelectionChanged;
        else
            textBox.SelectionChanged -= TextBox_SelectionChanged;
    }

    private static void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        textBox.SetCurrentValue(SelectionStartProperty, textBox.SelectionStart);
        textBox.SetCurrentValue(SelectionLengthProperty, textBox.SelectionLength);
    }
}
