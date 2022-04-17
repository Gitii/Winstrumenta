namespace PackageInstaller.AttachedProperties;

//public class MouseHandler : DependencyObject
//{
//    public static readonly DependencyProperty IsMovableProperty =
//        DependencyProperty.RegisterAttached(
//            "IsMovable",
//            typeof(Boolean),
//            typeof(MouseHandler),
//            new PropertyMetadata(false, OnIsMovableChanged)
//        );

//    public static readonly DependencyProperty IsMovableHookProperty =
//        DependencyProperty.RegisterAttached(
//            "IsMovableHook",
//            typeof(MouseHook),
//            typeof(MouseHandler),
//            new PropertyMetadata(null)
//        );

//    private static void OnIsMovableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is UIElement control)
//        {
//            // clean up existing hook
//            if (control.GetValue(IsMovableHookProperty) is MouseHook hook)
//            {
//                hook.Dispose();
//                control.SetValue(IsMovableHookProperty, null);
//            }

//            if (e.NewValue is true)
//            {
//                // enable hook
//                control.SetValue(IsMovableHookProperty, new MouseHook(control));
//            }
//        }
//    }

//    public static void SetIsMovable(UIElement element, Boolean value)
//    {
//        element.SetValue(IsMovableProperty, value);
//    }

//    public static Boolean GetIsMovable(UIElement element)
//    {
//        return (Boolean)element.GetValue(IsMovableProperty);
//    }

//    public class MouseHook : IDisposable
//    {
//        private UIElement _element;
//        private readonly IGlobalHook _globalHook;
//        private readonly NativeWindow _window;

//        private (int x, int y)? _mouseLocation;

//        public MouseHook(UIElement element, IGlobalHook globalHook, NativeWindow window)
//        {
//            _element = element;
//            _globalHook = globalHook;
//            _window = window;

//            element.PointerPressed += OnPointerPressed;
//            element.PointerReleased += OnPointerReleasedOrSimilar;
//            element.PointerCanceled += OnPointerReleasedOrSimilar;
//            element.PointerCaptureLost += OnPointerReleasedOrSimilar;
//            element.PointerExited += OnPointerReleasedOrSimilar;

//            _globalHook.Hooks.MouseUpExt += HooksOnMouseUpExt;
//        }

//        private void HooksOnMouseUpExt(object? sender, MouseEventExtArgs e)
//        {
//            StopDragging();
//        }

//        private void StopDragging()
//        {
//            _globalHook.Hooks.MouseMoveExt -= HooksOnMouseMoveExt;
//            _mouseLocation = null;
//        }

//        private void OnPointerReleasedOrSimilar(object sender, PointerRoutedEventArgs e)
//        {
//            StopDragging();
//        }

//        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
//        {
//            if (!e.IsGenerated)
//            {
//                StartDragging();
//            }
//        }

//        private void StartDragging()
//        {
//            StopDragging();

//            _globalHook.Hooks.MouseMoveExt += HooksOnMouseMoveExt;
//        }

//        private void HooksOnMouseMoveExt(object? sender, MouseEventExtArgs e)
//        {
//            if (_mouseLocation != null)
//            {
//                var deltaX = e.X - _mouseLocation.Value.x;
//                var deltaY = e.Y - _mouseLocation.Value.y;

//                var windowLocation = _window.Position;

//                _window.Position = new Point() { X = windowLocation.X + deltaX, Y = windowLocation.Y + deltaY };

//                _mouseLocation = (e.X, e.Y);
//            }
//            else
//            {
//                _mouseLocation = (e.X, e.Y);
//            }
//        }

//        public void Dispose()
//        {
//            _element.PointerPressed -= OnPointerPressed;
//            _element.PointerReleased -= OnPointerReleasedOrSimilar;
//            _element.PointerCanceled -= OnPointerReleasedOrSimilar;
//            _element.PointerCaptureLost -= OnPointerReleasedOrSimilar;
//            _element.PointerExited -= OnPointerReleasedOrSimilar;

//            _globalHook.Dispose();
//        }
//    }
//}
