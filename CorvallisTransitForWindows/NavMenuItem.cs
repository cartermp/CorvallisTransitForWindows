using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CorvallisTransitForWindows
{
    /// <summary>
    /// Data to represent an item in the nav menu.
    /// 
    /// Copied from the xaml_navigation sample: https://github.com/Microsoft/Windows-universal-samples/tree/master/xaml_navigation/CS
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar
        {
            get
            {
                return (char)this.Symbol;
            }
        }
        
        public object Arguments { get; set; }
    }
}
