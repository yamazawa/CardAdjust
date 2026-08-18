using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardAdjust.Behaviors;

/// <summary>
/// 要素をドラッグして、祖先のBorder(Canvas上の矩形)の幅・高さを個別に変更する添付ビヘイビア
///
/// 矩形の右下端に配置するリサイズハンドルに設定する。
/// ドラッグ中はSetCurrentValueでWidth/Heightへ書き込むため、宣言済みの{Binding}を破壊せずに反映できる。
/// </summary>
public static class ResizeHandleBehavior
{
    private const double MinSize = 20;

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ResizeHandleBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static readonly Dictionary<FrameworkElement, (Point Mouse, Size Size)> DragOrigins = [];

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseMove += OnMouseMove;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
        else
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseMove -= OnMouseMove;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        if (FindAncestor<Border>(handle) is not { } target || FindAncestor<Canvas>(handle) is not { } canvas)
            return;

        DragOrigins[handle] = (e.GetPosition(canvas), new Size(target.ActualWidth, target.ActualHeight));
        handle.CaptureMouse();

        // Border(矩形本体)側のDragMoveBehaviorへイベントを伝播させず、リサイズのみを行わせる。
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        if (!handle.IsMouseCaptured || FindAncestor<Border>(handle) is not { } target ||
            FindAncestor<Canvas>(handle) is not { } canvas || !DragOrigins.TryGetValue(handle, out var origin))
            return;

        var current = e.GetPosition(canvas);
        var newWidth = Math.Max(MinSize, origin.Size.Width + (current.X - origin.Mouse.X));
        var newHeight = Math.Max(MinSize, origin.Size.Height + (current.Y - origin.Mouse.Y));

        target.SetCurrentValue(FrameworkElement.WidthProperty, newWidth);
        target.SetCurrentValue(FrameworkElement.HeightProperty, newHeight);
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        handle.ReleaseMouseCapture();
        DragOrigins.Remove(handle);
    }

    private static T? FindAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        for (var current = LogicalTreeHelper.GetParent(element); current is not null; current = LogicalTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }
}
