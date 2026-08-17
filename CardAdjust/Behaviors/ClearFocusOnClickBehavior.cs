using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardAdjust.Behaviors;

/// <summary>
/// コントロール以外の場所をクリックしたらフォーカスを外すビヘイビア
///
/// TextBox等にフォーカスが無い場所へは通常フォーカスが移らないため、
/// 編集中のテキストボックスがフォーカスアウトしない問題に対応する。
/// アプリのルート要素に1つだけ設定して使う。
/// </summary>
public static class ClearFocusOnClickBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ClearFocusOnClickBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if (e.NewValue is true)
            element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        else
            element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
    }

    // クリック位置の祖先にControl(TextBox・Button・ListBoxItem等)が無ければ、
    // 何もない場所とみなしてフォーカスを外す。
    // Window自身もControlを継承しているため、祖先探索からは除外する。
    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source || FindAncestorControl(source) is not null)
            return;

        // Keyboard.ClearFocus()はキーボードフォーカスのみを外し、
        // TextBox.LostFocus(論理フォーカス)は発火しない。
        // FocusManagerで論理フォーカスも明示的に外す必要がある。
        if (sender is DependencyObject scopeOwner)
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(scopeOwner), null);

        Keyboard.ClearFocus();
    }

    private static Control? FindAncestorControl(DependencyObject source)
    {
        for (var current = source; current is not null; current = GetParent(current))
        {
            if (current is Control and not Window)
                return (Control)current;
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject d) =>
        d is Visual ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d);
}
