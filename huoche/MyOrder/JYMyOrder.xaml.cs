using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche.MyOrder
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JYMyOrder : Page
    {
        public JYMyOrder()
        {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //我的订单
            this.Frame.Navigate(typeof(JYOrderInfo));
        }

        private void shuXinClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void yiwanChengClick(object sender, RoutedEventArgs e)
        {

            MessageDialog message = new MessageDialog("此功能下板增加");
            await message.ShowAsync();
        }

        private async void geRenInfoClick(object sender, RoutedEventArgs e)
        {

            MessageDialog message = new MessageDialog("此功能下板增加");
            await message.ShowAsync();
        }

        private async void ChangYongLianXiRenClick(object sender, RoutedEventArgs e)
        {

            MessageDialog message = new MessageDialog("此功能下板增加");
            await message.ShowAsync();
        }
    }
}
