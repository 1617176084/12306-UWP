using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Data.Json;


// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PageText : Page
    {
      
        ObservableCollection<diMing> diming = new ObservableCollection<diMing>();
        List<diMing> suggesArr = new List<diMing>();
        public PageText()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.frame1.Navigate(typeof(JYFindeHuoChe), this.frame);
            this.frame.Navigate(typeof(JyKOngBaiTu));
            getZhanDianXinXi();


            
           this.comboBox.ItemsSource = diming;
            this.comboBoxMuDiDi.ItemsSource = diming;
           // this.autoSugges.ItemsSource = diming;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.frame1.Navigate(typeof(MainPage));
        }
       
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           Debug.Write("chend");
            freashFram(e.NewSize.Width);
        }
        public IEnumerable<diMing> getDiMing(string name)
        {
           return suggesArr.Where(di => di.nameforcon.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) >-1).OrderByDescending(di => di.nameforcon.IndexOf(name, StringComparison.CurrentCultureIgnoreCase));
        }
        void freashFram(double wight)
        {
            int chendNumber = 700;
            //   int miniNumber = 400;
            if (wight < chendNumber)
            {
                split.OpenPaneLength = wight;
                double numberNewWight = wight / 3 * 2;
                if (numberNewWight < chendNumber)
                {
                    if (this.frame.CanGoBack)
                    {
                        split.IsPaneOpen = false;
                    }
                    else
                    {
                        split.IsPaneOpen = true;
                    }
                }
            }
            else
            {
                double numberNewWight = wight / 2;
                split.OpenPaneLength = numberNewWight;
                split.IsPaneOpen = true;
            }

        }
        private void Page_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }

        /**
        获得站点信息
         */
        void getZhanDianXinXi()
        {
            string uri = "http://www.web2mi.com/train/station.js";
            HTTPRequest.init().sendGetHtml(uri, null, (string json) => {
                Debug.Write(json);
                string[] arr = json.Split('@');
                
                foreach (string str in arr)
                {
                    if (str == "") continue;
                    string[] strArr = str.Split('|');
                    diMing di = new diMing();
                    if (strArr.Length == 4)
                    {
                        di.pinyin = strArr[0];
                        di.nameforcon = strArr[1];
                        di.code = strArr[2];
                        di.qiShoushiJian = strArr[3];
                    }
                    if (strArr.Length ==3)
                    {
                        di.pinyin = strArr[0];
                        di.nameforcon = strArr[1];
                        di.code = strArr[2];
                      
                    }
                    diming.Add(di);
                    suggesArr.Add(di);
                }

            }, (string err) =>
            {
                Debug.Write(err);
            });
        }
        public class diMing
        {
            public string pinyin { get; set; }
            public string code { get; set; }
            public string qiShoushiJian { get; set; }
            public string nameforcon { get; set; }
            public override string ToString()
            {
                return nameforcon + qiShoushiJian;
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            //开始查询
            //if (comboBox.SelectedIndex == -1 || comboBoxMuDiDi.SelectedIndex == -1)
            //{
            //    MessageDialog message = new MessageDialog("请选择初始地或者目的地");
            //    await message.ShowAsync();
            //    return;
            //}
            //diMing fromDiMing = comboBox.SelectedItem as diMing;
            //diMing toDiMing = comboBoxMuDiDi.SelectedItem as diMing;
            diMing fromDiMing = autoSuggesChuFa.Tag as diMing;
            diMing toDiMing = autoSuggesMuDiDi.Tag as diMing;
            if (toDiMing == null || fromDiMing == null)
            {
                MessageDialog message = new MessageDialog("请选择初始地或者目的地");
                await message.ShowAsync();
                return;
            }
            //https://kyfw.12306.cn/otn/leftTicket/query?leftTicketDTO.train_date=2015-10-26&leftTicketDTO.from_station=SHH&leftTicketDTO.to_station=ZZF&purpose_codes=ADULT

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("leftTicketDTO.train_date",datePicker.Date.Year+"-"+datePicker.Date.Month.ToString("00") + "-"+  datePicker.Date.Day.ToString("00"));
            dic.Add("leftTicketDTO.from_station", fromDiMing.code);
            dic.Add("leftTicketDTO.to_station", toDiMing.code);
            dic.Add("purpose_codes", "ADULT");
            
            this.frame.Navigate(typeof(JYChePiaoListPage),dic);
       
             
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            freashFram(Window.Current.Bounds.Width - 48);
        }

        private void autoSugges_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

            sender.ItemsSource = getDiMing(sender.Text);
        }

        private void autoSugges_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Tag = args.SelectedItem;
        }

        private void autoSugges_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AutoSuggestBox ssend = sender as AutoSuggestBox;
            ssend.ItemsSource = diming;
        }

        private void autoSugges_GotFocus(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox ssend = sender as AutoSuggestBox;
            ssend.ItemsSource = suggesArr;
        }
    }
 
}
