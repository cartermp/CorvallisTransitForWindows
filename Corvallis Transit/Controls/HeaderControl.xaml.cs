using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Corvallis_Transit.Controls
{
    public sealed partial class HeaderControl : UserControl
    {
        public HeaderControl()
        {
            this.InitializeComponent();

            Loaded += (obj, args) =>
            {
                MainPage.Current.TogglePaneButtonRectChanged += Current_TogglePaneButtonRectChanged;
                this.TitleBar.Margin = new Thickness(MainPage.Current.TogglePaneButtonRect.Right, 0, 0, 0);
            };
        }

        private void Current_TogglePaneButtonRectChanged(MainPage sender, Rect args)
        {
            this.TitleBar.Margin = new Thickness(args.Right, 0, 0, 0);
        }

        public UIElement HeaderContent
        {
            get
            {
                return GetValue(HeaderContentProperty) as UIElement;
            }
            set
            {
                SetValue(HeaderContentProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for HeaderContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(HeaderControl), new PropertyMetadata(DependencyProperty.UnsetValue));
    }
}
