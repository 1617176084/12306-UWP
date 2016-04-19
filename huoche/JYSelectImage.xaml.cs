using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace huoche
{
    public sealed partial class JYSelectImage : UserControl
    {
       public Point nowSelect;
       public Canvas super;


        public delegate void removefroSuper(JYSelectImage image);
        public event removefroSuper delegateRemov;
        public JYSelectImage()
        {
            this.InitializeComponent();
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            delegateRemov(this);
            super.Children.Remove(this);
        }
    }
}
