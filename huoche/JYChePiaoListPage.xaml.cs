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
using Windows.Data.Json;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.Collections.ObjectModel;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JYChePiaoListPage : Page
    {
        public string  train_date;
        public string from_station;

        public string to_station;
        public string purpose_codes ;
        ObservableCollection<chePiaoCellInfo> bindingInfo = new ObservableCollection<chePiaoCellInfo>();
        public JYChePiaoListPage()
        {
            this.InitializeComponent();
         //   this.NavigationCacheMode = NavigationCacheMode.Required;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = this.Frame;
            if (rootFrame == null)
                return;

            // If we can go back and the event has not already been handled, do so.
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

                e.Handled = true;
                rootFrame.GoBack();
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            listview.ItemsSource = bindingInfo;

            Dictionary<string,string> dic =   e.Parameter as Dictionary<string,string>;
            if (dic != null)
            {
           
                  train_date = dic["leftTicketDTO.train_date"];
                //dic.Add("leftTicketDTO.from_station", fromDiMing.code);
                //dic.Add("leftTicketDTO.to_station", toDiMing.code);
                //dic.Add("purpose_codes", "ADULT");
                from_station = dic["leftTicketDTO.from_station"];
                to_station = dic["leftTicketDTO.to_station"];
                purpose_codes = dic["purpose_codes"];

             
                freashData(0);
            }
      

        }
        void freashData(int days)
        {
            DateTime datePicker = Convert.ToDateTime(train_date);
            datePicker =  datePicker.AddDays(days);
            train_date = datePicker.Date.Year + "-" + datePicker.Date.Month.ToString("00") + "-" + datePicker.Date.Day.ToString("00");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("leftTicketDTO.train_date", train_date);
            dic.Add("leftTicketDTO.from_station", from_station);
            dic.Add("leftTicketDTO.to_station", to_station);
            dic.Add("purpose_codes", "ADULT");

            string uristr = "https://kyfw.12306.cn/otn/leftTicket/queryT";
            HTTPRequest.init().sendGet(uristr, dic, (JsonObject json) => {
                Debug.WriteLine(json.ToString());
                if (json["data"].GetArray().Count == 0)
                {

                    MessageDialog mes = new MessageDialog("没有相关车次");
                    mes.ShowAsync();
                    return;
                }
                JsonArray arr = json["data"].GetArray();
                bindingInfo.Clear();
                titleName.Text = train_date + "(" + arr.Count + "趟)";
                for (uint i = 0; i < arr.Count; i++)
                {

                    JsonObject jsonObj = arr.GetObjectAt(i);
                    chePiaoCell obj = JsonHelper<chePiaoCell>.Deserialize(jsonObj.ToString(), typeof(chePiaoCell)) as chePiaoCell;
                    chePiaoCellInfo info = new chePiaoCellInfo();
                    info.train_date = train_date;
                    
                    info.back_train_date = DateTime.Now.Date.Year + "-" + DateTime.Now.Date.Month.ToString("00") + "-" + DateTime.Now.Date.Day.ToString("00");
                    info.cell = obj;
                    info.checi = obj.queryLeftNewDTO.station_train_code;


                    info.qishiAddress = (obj.queryLeftNewDTO.start_station_name.Equals(obj.queryLeftNewDTO.from_station_name) ? "始" : "过") + obj.queryLeftNewDTO.from_station_name;
                    info.jieshuAddress = (obj.queryLeftNewDTO.end_station_name.Equals(obj.queryLeftNewDTO.to_station_name) ? "终" : "过") + obj.queryLeftNewDTO.to_station_name;
                    info.qishiTime = obj.queryLeftNewDTO.start_time + "开";
                    info.jieshuTime = obj.queryLeftNewDTO.arrive_time + "到";
                    info.yuPiao = obj.queryLeftNewDTO.yw_num;
                    info.liShiShiJian = "历时：" + obj.queryLeftNewDTO.lishi + "(" + obj.queryLeftNewDTO.day_difference + "天)";

                    string yuPiao = "【余票】";
                    //if (!obj.queryLeftNewDTO.gr_num.Equals("--"))
                    //{
                    //    yuPiao += "  高级软卧:" + obj.queryLeftNewDTO.gr_num;
                    //}
                    //if (!obj.queryLeftNewDTO.qt_num.Equals("--"))
                    //{
                    //    yuPiao += "  其他:" + obj.queryLeftNewDTO.qt_num;
                    //}
                    //if (!obj.queryLeftNewDTO.rw_num.Equals("--"))
                    //{
                    //    yuPiao += "  软卧:" + obj.queryLeftNewDTO.rw_num;
                    //}
                    //if (!obj.queryLeftNewDTO.rz_num.Equals("--"))
                    //{
                    //    yuPiao += "  软座:" + obj.queryLeftNewDTO.rz_num;
                    //}
                    //if (!obj.queryLeftNewDTO.tz_num.Equals("--"))
                    //{
                    //    yuPiao += "  特等座:" + obj.queryLeftNewDTO.tz_num;
                    //}
                    //if (!obj.queryLeftNewDTO.wz_num.Equals("--"))
                    //{
                    //    yuPiao += "  无座:" + obj.queryLeftNewDTO.wz_num;
                    //}
                    //if (!obj.queryLeftNewDTO.yw_num.Equals("--"))
                    //{
                    //    yuPiao += "  硬卧:" + obj.queryLeftNewDTO.yw_num;
                    //}
                    //if (!obj.queryLeftNewDTO.yz_num.Equals("--"))
                    //{
                    //    yuPiao += "  硬座:" + obj.queryLeftNewDTO.yz_num;
                    //}
                    //if (!obj.queryLeftNewDTO.ze_num.Equals("--"))
                    //{
                    //    yuPiao += "  二等座:" + obj.queryLeftNewDTO.ze_num;
                    //}
                    //if (!obj.queryLeftNewDTO.zy_num.Equals("--"))
                    //{
                    //    yuPiao += "  一等座:" + obj.queryLeftNewDTO.zy_num;
                    //}
                    //if (!obj.queryLeftNewDTO.swz_num.Equals("--"))
                    //{
                    //    yuPiao += "  商务座:" + obj.queryLeftNewDTO.swz_num;
                    //}
                    yuPiao += getYuPiao(obj.queryLeftNewDTO.yp_info);
                    if (obj.buttonTextInfo.Equals("预订"))
                    {
                        info.yuPiao = yuPiao;
                    }
                    else
                    {
                        info.yuPiao = obj.buttonTextInfo;
                    }
                  
                    bindingInfo.Add(info);
                }



            }, (string err) => {
                Debug.WriteLine(err);
            });
        }
        string getYuPiao(string code)
        {
            //  string kaiTou = cheCi.Substring(0, 1);
            string jieguo = "";

            // string[] nameArr = new string[3] { "二等", "一等", "无座" };
            for (int i = 0; i < (code.Length / 10); i++)
            {
                double jiage = getNumberJiaGe(i, code) * 0.1;
                int shuliang = getNumberShuLiang(i, code);
                jieguo += getZuoWei(i, code) + ":  ￥" + jiage + "元,有" + shuliang + " 张。 ";
            }


            return jieguo;
        }
        int getNumberJiaGe(int i, string code)
        {
            return int.Parse(code.Substring(2 + i * 10, 4));
        }
        int getNumberShuLiang(int i, string code)
        {
            return int.Parse(code.Substring(7 + i * 10, 3));
        }
        string getZuoWei(int i, string code)
        {
            //            00 分割为0二等座
            //00 分割为3 无座
            //10 分割为3 无座
            //10 分割为0 硬座
            //30 分割为0 硬卧
            //40 分割为0 软卧
            //MO 分割为0一等座
            //60 分割为 0 高级软卧
            //90 分割为0 商务
            //P0 分割0  特等

            string[] nameArr = new string[10] { "无座", "软座", "软卧", "高级软卧", "硬卧", "硬座", "一等座", "二等座", "特等座", "商务座" };
            string kaitou = code.Substring(i * 10, 2);
            string fenGe = code.Substring(6 + i * 10, 1);

            string needName = "";
            switch (kaitou)
            {
                case "O0":
                    {
                        if (fenGe.Equals("3"))
                        {
                            needName = nameArr[0];
                        }
                        else
                        {
                            needName = nameArr[7];
                        }
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "10":
                    {

                        if (fenGe.Equals("3"))
                        {
                            needName = nameArr[0];
                        }
                        else
                        {
                            needName = needName = nameArr[5];
                        }
                    }
                    break;
                case "30":
                    {
                        needName = nameArr[4];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "40":
                    {
                        needName = nameArr[2];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "60":
                    {
                        needName = nameArr[3];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "90":
                    {
                        needName = nameArr[9];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "M0":
                    {
                        needName = nameArr[6];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                case "P0":
                    {
                        needName = nameArr[8];
                        //  if(fenGe.Equals(""))
                    }
                    break;
                default:
                    needName = "其他";
                    break;
            }
            return needName;
        }
        private void shuXinClick(object sender, RoutedEventArgs e)
        {
            freashData(0);
        }

        private void qianYiTianClick(object sender, RoutedEventArgs e)
        {

            freashData(-1);
        }

        private void houYiTianClick(object sender, RoutedEventArgs e)
        {
            freashData(1);
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Frame.Navigate(typeof(JYChePiaoInfo),listview.SelectedItem);
        }

        private void backClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
    public class chePiaoCellInfo {
        public chePiaoCell cell;
        public string checi { get; set; }
        public string qishiAddress { get; set; }
        public string jieshuAddress { get; set; }
        public string qishiTime { get; set; }
        public string jieshuTime { get; set; }
        public string liShiShiJian { get; set; }
        public string yuPiao { get; set; }
        public string train_date { get; set; }
        public string back_train_date { get; set; }

    }
    public class chePiaoCell
    {
        public class chePiaoinfi
        {
            public string train_no { get; set; }
            public string station_train_code { get; set; }//T1",
            public string start_station_telecode { get; set; }//BJP",
            public string start_station_name { get; set; }//北京",
            public string end_station_telecode { get; set; }//CSQ",
            public string end_station_name { get; set; }//长沙",
            public string from_station_telecode { get; set; }//BJP",
            public string from_station_name { get; set; }//北京",
            public string to_station_telecode { get; set; }//ZZF",
            public string to_station_name { get; set; }//郑州",
            public string start_time { get; set; }//15:25",
            public string arrive_time { get; set; }//22:45",
            public string day_difference { get; set; }//0",
            public string train_class_name { get; set; }//",
            public string lishi { get; set; }//07:20",
            public string canWebBuy { get; set; }//Y",
            public string lishiValue { get; set; }//440",
            public string yp_info { get; set; }//1009303189402510000410093000523016300017",
            public string control_train_day { get; set; }//20301231",
            public string start_train_date { get; set; }//20151031",
            public string seat_feature { get; set; }//W3431333",
            public string yp_ex { get; set; }//10401030",
            public string train_seat_feature { get; set; }//3",
            public string seat_types { get; set; }//1413",
            public string location_code { get; set; }//P2",
            public string from_station_no { get; set; }//01",
            public string to_station_no { get; set; }//03",
            public string control_day { get; set; }//": 59,
            public string sale_time { get; set; }//1000",
            public string is_support_card { get; set; }//0",
            public string gg_num { get; set; }//--",
            public string gr_num { get; set; }//--",
            public string qt_num { get; set; }//--",
            public string rw_num { get; set; }//4",
            public string rz_num { get; set; }//--",
            public string tz_num { get; set; }//--",
            public string wz_num { get; set; }//有",
            public string yb_num { get; set; }//--",
            public string yw_num { get; set; }//17",
            public string yz_num { get; set; }//有",
            public string ze_num { get; set; }//--",
            public string zy_num { get; set; }//--",
            public string swz_num { get; set; }//--"
        }
        public chePiaoinfi queryLeftNewDTO { get; set; }
        public string  secretStr{ get; set; }//MjAxNS0xMC0zMSMwMCNUMSMwNzoyMCMxNToyNSMyNDAwMDAwMFQxMEwjQkpQI1paRiMyMjo0NSPljJfkuqwj6YOR5beeIzAxIzAzIzEwMDkzMDMxODk0MDI1MTAwMDA0MTAwOTMwMDA1MjMwMTYzMDAwMTcjUDIjMTQ0NjI2OTk4NjM1NSMxNDQxMTU5MjAwMDAwI0I2MzBDMUYxQzk5OTNDQUMwRkI0RDNFRDE3RTk0QjdEMDY3NEUxNkRBRTZFMzM0NDgyQjhFOEY4",
          public string buttonTextInfo{ get; set; }//预订"

    }


}
