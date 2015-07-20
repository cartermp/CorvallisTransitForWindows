using CorvallisTransitForWindows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorvallisTransitForWindows
{
    public partial class MainPage : Page
    {
        CustomTitleBar customTitleBar = null;

        public void AddCustomTitleBar()
        {
            if (customTitleBar == null)
            {
                customTitleBar = new CustomTitleBar();

                // Make the main page's content a child of the title bar,
                // and make the title bar the new apge content.
                UIElement mainContent = this.Content;
                this.Content = null;
                customTitleBar.SetPageContent(mainContent);
                this.Content = customTitleBar;
            }
        }

        public void RemoveCustomTitleBar()
        {
            if (customTitleBar != null)
            {
                // Take the title bar's page content and make it the window content.
                this.Content = customTitleBar.SetPageContent(null);
                customTitleBar = null;
            }
        }
    }
}
