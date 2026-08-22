using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardAdjust.Behaviors;

/// <summary>
/// 要素をドラッグして、祖先のBorder(Canvas上の矩形)の位置・幅・高さを変更する添付ビヘイビア
///
/// 矩形の4辺・4角に配置するリサイズハンドルに、Directionと共に設定する。
/// 上辺・左辺を含む方向は、幅・高さの変更に合わせてCanvas.Left/Topも動かす。
/// ドラッグ中はSetCurrentValueでCanvas.Left/Top・Width/Heightへ書き込むため、宣言済みの{Binding}を破壊せずに反映できる。
/// </summary>
public static class ResizeHandleBehavior
{
    private const double MinSize = 20;

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.RegisterAttached("Direction", typeof(ResizeDirection?), typeof(ResizeHandleBehavior),
            new PropertyMetadata(null, OnDirectionChanged));

    public static void SetDirection(DependencyObject element, ResizeDirection? value) => element.SetValue(DirectionProperty, value);

    public static ResizeDirection? GetDirection(DependencyObject element) => (ResizeDirection?)element.GetValue(DirectionProperty);

    private static readonly Dictionary<FrameworkElement, (Point Mouse, Rect Bounds)> DragOrigins = [];

    private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if (e.OldValue is not null)
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseMove -= OnMouseMove;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        if (e.NewValue is not null)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseMove += OnMouseMove;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        if (FindAncestor<Border>(handle) is not { } target || FindAncestor<Canvas>(handle) is not { } canvas)
            return;

        var bounds = new Rect(Canvas.GetLeft(target), Canvas.GetTop(target), target.ActualWidth, target.ActualHeight);
        DragOrigins[handle] = (e.GetPosition(canvas), bounds);
        handle.CaptureMouse();

        // Border(矩形本体)側のDragMoveBehaviorへイベントを伝播させず、リサイズのみを行わせる。
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        if (!handle.IsMouseCaptured || GetDirection(handle) is not { } direction ||
            FindAncestor<Border>(handle) is not { } target || FindAncestor<Canvas>(handle) is not { } canvas ||
            !DragOrigins.TryGetValue(handle, out var origin))
            return;

        var current = e.GetPosition(canvas);
        var (left, width) = ResizeHorizontal(direction, origin.Bounds, current.X - origin.Mouse.X);
        var (top, height) = ResizeVertical(direction, origin.Bounds, current.Y - origin.Mouse.Y);

        target.SetCurrentValue(Canvas.LeftProperty, left);
        target.SetCurrentValue(Canvas.TopProperty, top);
        target.SetCurrentValue(FrameworkElement.WidthProperty, width);
        target.SetCurrentValue(FrameworkElement.HeightProperty, height);
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var handle = (FrameworkElement)sender;
        handle.ReleaseMouseCapture();
        DragOrigins.Remove(handle);
    }

    // 右辺を動かす方向は幅だけ、左辺を動かす方向は幅とCanvas.Leftの両方を変える(最小サイズを下回らないようクランプする)。
    private static (double Left, double Width) ResizeHorizontal(ResizeDirection direction, Rect origin, double dx)
    {
        if (IsRightEdge(direction))
            return (origin.X, Math.Max(MinSize, origin.Width + dx));

        if (IsLeftEdge(direction))
        {
            var clampedDx = Math.Min(dx, origin.Width - MinSize);
            return (origin.X + clampedDx, origin.Width - clampedDx);
        }

        return (origin.X, origin.Width);
    }

    // 下辺を動かす方向は高さだけ、上辺を動かす方向は高さとCanvas.Topの両方を変える(最小サイズを下回らないようクランプする)。
    private static (double Top, double Height) ResizeVertical(ResizeDirection direction, Rect origin, double dy)
    {
        if (IsBottomEdge(direction))
            return (origin.Y, Math.Max(MinSize, origin.Height + dy));

        if (IsTopEdge(direction))
        {
            var clampedDy = Math.Min(dy, origin.Height - MinSize);
            return (origin.Y + clampedDy, origin.Height - clampedDy);
        }

        return (origin.Y, origin.Height);
    }

    private static bool IsLeftEdge(ResizeDirection d) => d is ResizeDirection.Left or ResizeDirection.TopLeft or ResizeDirection.BottomLeft;

    private static bool IsRightEdge(ResizeDirection d) => d is ResizeDirection.Right or ResizeDirection.TopRight or ResizeDirection.BottomRight;

    private static bool IsTopEdge(ResizeDirection d) => d is ResizeDirection.Top or ResizeDirection.TopLeft or ResizeDirection.TopRight;

    private static bool IsBottomEdge(ResizeDirection d) => d is ResizeDirection.Bottom or ResizeDirection.BottomLeft or ResizeDirection.BottomRight;

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
