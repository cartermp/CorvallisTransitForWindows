using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Corvallis_Transit.Controls
{
    /// <summary>
    /// Taken verbatim from here:
    /// 
    /// https://github.com/Microsoft/Windows-universal-samples/blob/a1d074c229dc6c3a8e0dd58004192e1b94fff057/TitleBar/cs/CustomTitleBar.xaml.cs
    /// </summary>
    public sealed partial class CustomTitleBar : UserControl, INotifyPropertyChanged
    {
        private CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public CustomTitleBar()
        {
            this.InitializeComponent();
        }

        private void CustomTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            coreTitleBar.LayoutMetricsChanged += OnLayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += OnIsVisibleChanged;

            // The SizeChanged event is raised when the view enters or exits fullscreen mode.
            Window.Current.SizeChanged += OnWindowSizeChanged;

            UpdateLayoutMetrics();
            UpdatePositionAndVisibility();
        }

        private void CustomTitleBar_Unloaded(object sender, RoutedEventArgs e)
        {
            coreTitleBar.LayoutMetricsChanged -= OnLayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged -= OnIsVisibleChanged;
            Window.Current.SizeChanged -= OnWindowSizeChanged;
        }

        private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateLayoutMetrics();
        }

        private void UpdateLayoutMetrics()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("CoreTitleBarHeight"));
                PropertyChanged(this, new PropertyChangedEventArgs("CoreTitleBarPadding"));
            }
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdatePositionAndVisibility();
        }

        private void OnIsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdatePositionAndVisibility();
        }

        private void UpdatePositionAndVisibility()
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                // In fullscreen mode, the title bar overlays the content and may or may not be visible (depends on user preference).
                TitleBar.Visibility = coreTitleBar.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetRow(TitleBar, 1);
            }
            else
            {
                // When not in fullscreen mode, the title bar is visible and does not overlay cotnent.
                TitleBar.Visibility = Visibility.Visible;
                Grid.SetRow(TitleBar, 0);
            }
        }

        UIElement pageContent = null;

        public UIElement SetPageContent(UIElement newContent)
        {
            var old = pageContent;
            if (old != null)
            {
                pageContent = null;
                RootGrid.Children.Remove(old);
            }

            pageContent = newContent;
            if (newContent != null)
            {
                RootGrid.Children.Add(newContent);
                Grid.SetRow(pageContent as FrameworkElement, 1);
            }

            return old;
        }

        #region DataBinding

        public event PropertyChangedEventHandler PropertyChanged;

        public Thickness CoreTitleBarPadding
        {
            get
            {
                // The SystemOverflayLeftInset and SystemOverlayRightInset values are
                // in terms of physical left and right.  Therefore, we need to flip
                // them when our flow direction is RTL.
                if (FlowDirection == FlowDirection.LeftToRight)
                {
                    return new Thickness
                    {
                        Left = coreTitleBar.SystemOverlayLeftInset,
                        Right = coreTitleBar.SystemOverlayRightInset
                    };
                }
                else
                {
                    return new Thickness
                    {
                        Left = coreTitleBar.SystemOverlayRightInset,
                        Right = coreTitleBar.SystemOverlayLeftInset
                    };
                }
            }
        }

        public double CoreTitleBarHeight
        {
            get
            {
                return coreTitleBar.Height;
            }
        }

        #endregion
    }
}
