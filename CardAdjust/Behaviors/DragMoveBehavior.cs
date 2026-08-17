using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardAdjust.Behaviors;

/// <summary>
/// Canvas上の要素をドラッグでCanvas.Left/Topへ反映する添付ビヘイビア
///
/// ドラッグ中はSetCurrentValueでCanvas.Left/Topへ書き込むため、
/// 宣言済みの{Binding}を破壊せずに位置を反映できる。
/// </summary>
public static class DragMoveBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DragMoveBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static readonly Dictionary<FrameworkElement, (Point Mouse, Point Element)> DragOrigins = [];

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
        var element = (FrameworkElement)sender;
        if (element.Parent is not Canvas canvas)
            return;

        DragOrigins[element] = (e.GetPosition(canvas), new Point(Canvas.GetLeft(element), Canvas.GetTop(element)));
        element.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var element = (FrameworkElement)sender;
        if (!element.IsMouseCaptured || element.Parent is not Canvas canvas || !DragOrigins.TryGetValue(element, out var origin))
            return;

        var current = e.GetPosition(canvas);
        element.SetCurrentValue(Canvas.LeftProperty, origin.Element.X + (current.X - origin.Mouse.X));
        element.SetCurrentValue(Canvas.TopProperty, origin.Element.Y + (current.Y - origin.Mouse.Y));
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        element.ReleaseMouseCapture();
        DragOrigins.Remove(element);
    }
}
