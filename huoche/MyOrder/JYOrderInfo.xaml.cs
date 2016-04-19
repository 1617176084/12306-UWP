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
using Windows.UI.Popups;
using System.Text.RegularExpressions;
using Windows.System;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche.MyOrder
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JYOrderInfo : Page
    {
        public JYOrderInfo()
        {
            this.InitializeComponent();
        }
        string parOrderDTOJson = "";
        string sequence_no = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            getOrder();
            geHtmlInitPay();
        }
        void getOrder()
        {
            string uri = "https://kyfw.12306.cn/otn/queryOrder/queryMyOrderNoComplete";
            HTTPRequest.init().sendPOST(uri,new Dictionary<string, string> { { "_json_att", "" } },   (JsonObject json) =>
            {
                Debug.WriteLine("得到未完成订单" + json.ToString());
                if (!json.ContainsKey("data"))
                {
                    noOrder();
                    return;
                }
                JsonObject data = json["data"].GetObject();
               
                if (data.Count >= 2)
                {
                    jixuOrder();
                    JsonArray orderDBList = data["orderDBList"].GetArray();
                  
                    if (orderDBList.Count > 0)
                    {
                        JsonObject jsonOrder = orderDBList[0].GetObject();
                     
                      labChengCheRiQi.Text = jsonOrder["start_train_date_page"].ToString();
                        labCheCi.Text = jsonOrder["train_code_page"].ToString();
                       
                        JsonArray from_station_name_page = jsonOrder["from_station_name_page"].GetArray();
                        JsonArray to_station_name_page = jsonOrder["to_station_name_page"].GetArray();
                        labChuFaDi.Text = from_station_name_page[0].ToString() + jsonOrder["start_time_page"].ToString() + "开";
                        labMuDiDI.Text = to_station_name_page[0].GetString() + jsonOrder["arrive_time_page"].ToString() +"到";
                        labZongZhangShu.Text = jsonOrder["ticket_totalnum"].GetNumber()+"张";
                        labZongPiaoKuan.Text = jsonOrder["ticket_price_all"].GetNumber()/100 +"元";
                        sequence_no = jsonOrder["sequence_no"].GetString();


                        JsonArray tickets = jsonOrder["tickets"].GetArray();
                        for (int i = 0; i < tickets.Count; i++)
                        {
                            JsonObject ticke = tickets[i].GetObject();
                            JsonObject passengerDTO = ticke["passengerDTO"].GetObject();
                            JYUserOrderInfo orderinfo = new JYUserOrderInfo();

                            orderinfo.labName.Text = passengerDTO["passenger_name"].ToString();
                            orderinfo.labZhengJianLeiXing.Text = passengerDTO["passenger_id_type_name"].ToString();
                            orderinfo.labZhengJianHao.Text = passengerDTO["passenger_id_no"].ToString();
                            orderinfo.labPiaoZhong.Text = ticke["ticket_type_name"].ToString();
                            orderinfo.labXiBie.Text = ticke["seat_type_name"].ToString();
                            orderinfo.labCheXiang.Text = ticke["coach_no"].ToString();
                            orderinfo.labXiWeiHao.Text = ticke["seat_name"].ToString();
                            orderinfo.labPiaoJia.Text = ticke["str_ticket_price_page"].ToString();
                            stack.Children.Add(orderinfo);
                        }
                    }
                    else
                    {
                        noOrder();
                    }
                }
                else
                {
                     noOrder();
                }

            }, (string err)=> {
                Debug.WriteLine("得到未完成订单" + err.ToString());
            });
          
        }

        async void noOrder()
        {
            MessageDialog message = new MessageDialog("没有未完成订单");
            await message.ShowAsync();
            返回_Click(null, null);
        }

        private void 返回_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        /// <summary>
        /// 继续我没有完成的订单：
        /// </summary>
        void jixuOrder()
        {
            string uri = "https://kyfw.12306.cn/otn/queryOrder/continuePayNoCompleteMyOrder";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("pay_flag", "pay");
            dic.Add("_json_att", "");
            dic.Add("sequence_no", sequence_no);
            HTTPRequest.init().sendPOST(uri, dic, async (JsonObject json) =>
            {
                JsonObject data = json["data"].GetObject();
                if (data["existError"].GetString() == "N")
                {
                    MessageDialog me = new MessageDialog("继续订单");
                    await me.ShowAsync();
                }
                else
                {
                    MessageDialog me = new MessageDialog("未知原因导致继续订单失败，尝试读取原因：" + json["messages"].ToString() + ",打印全局信息" + json.ToString());

                    await me.ShowAsync();
                }
            }, (string err) => {

            });
        }
        void geHtmlInitPay()
        {
            string orderInfo = "var parOrderDTOJson = '.+";
            string uriPayOrder = "https://kyfw.12306.cn/otn/payOrder/init";
            Dictionary<string, string> dicPayOrder = new Dictionary<string, string>();

            dicPayOrder.Add("_json_att", "");
            dicPayOrder.Add("REPEAT_SUBMIT_TOKEN", "");

            HTTPRequest.init().sendPostHtml(uriPayOrder, dicPayOrder, (string html) => {
                Regex rege = new Regex(orderInfo);

                string code = rege.Match(html).ToString();
                string[] codeArr = code.Split('\'');
                if (codeArr.Length == 3)
                {
                    parOrderDTOJson = codeArr[1];
                }
            }, (string err) =>
            {

            });
        }
        private void shuXinClick(object sender, RoutedEventArgs e)
        {
     



            //取消订单
            string uri = "https://kyfw.12306.cn/otn/payOrder/cancel";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parOrderDTOJson",parOrderDTOJson.Replace("\\",""));
            dic.Add("_json_att", "");
            dic.Add("sequence_no", sequence_no);
            dic.Add("orderRequestDTOJson", "null");
            HTTPRequest.init().sendPOST(uri, dic, async (JsonObject json) =>
            {
                JsonObject data = json["data"].GetObject();
                if (data["cancelStatus"].GetBoolean())
                {
                    MessageDialog me = new MessageDialog("订单取消成功，注意当天最多有三次取消订单机会，超过三次，当天将无法购票");
                    await me.ShowAsync();
                }
                else
                {
                    MessageDialog me = new MessageDialog("未知原因导致取消失败，尝试读取原因："+json["messages"].ToString()+",打印全局信息"+json.ToString());

                    await me.ShowAsync();
                }
            }, (string err) => {

            });
        }

        private async void zhiFuClick(object sender, RoutedEventArgs e)
        {
            MessageDialog mess = new MessageDialog("因为系统限制，支付需要在12306官网支付，请登陆12306官网，找到未完成订单，进行支付。谢谢");
            await mess.ShowAsync();
            await Launcher.LaunchUriAsync(new Uri("http://www.12306.cn/"));
            //调用系统默认的浏览器  
            //      System.Diagnostics.p.Process.Start("http://www.google.cn");

        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog mess = new MessageDialog("因为系统限制，支付需要在12306官网支付，请登陆12306官网，找到未完成订单，进行支付。谢谢");
            await mess.ShowAsync();
            await Launcher.LaunchUriAsync(new Uri("http://www.12306.cn/"));
        }
    }
}
