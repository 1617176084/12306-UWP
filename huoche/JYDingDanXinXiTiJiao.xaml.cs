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
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Input;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JYDingDanXinXiTiJiao : Page
    {
        public JYDingDanXinXiTiJiao()
        {
            this.InitializeComponent();
        }
        ObservableCollection<zuoType> zuoArr = new ObservableCollection<zuoType>();
        List<userInfo> userArr = new List<userInfo>();
        chePiaoCellInfo cell;

        List<chePiaoInfo.JYUserInfoCell> comboxArr = new List<chePiaoInfo.JYUserInfoCell>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            initGetImage();


            cell = e.Parameter as chePiaoCellInfo;

            labForCheCi.Text = cell.checi + "(" +cell.qishiAddress+"-"+cell.jieshuAddress+ ")";
            labForShiJian.Text = cell.train_date + "("+cell.qishiTime +"-"+cell.jieshuTime+")";
            labForChePiaoXinXi.Text = cell.yuPiao;

            char[] charSeatArr = cell.cell.queryLeftNewDTO.seat_feature.ToCharArray();
            for(int i = 0;i<charSeatArr.Length;i = i+2)
            {
                zuoType a = new zuoType();
                a.nameForm = getSeatType(charSeatArr[i].ToString());
                a.seattype = charSeatArr[i].ToString(); 
                zuoArr.Add(a);
            }

            if (cell != null)
            {

            }
                geiUserInfo();
        }
        public async void geiUserInfo()
        {
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/getPassengerDTOs";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("_json_att", "");
            dic.Add("REPEAT_SUBMIT_TOKEN", await HTTPRequest.init().getTouken(true)); ;
            HTTPRequest.init().sendPOST(uri, dic, (JsonObject json) =>
            {
                Debug.WriteLine(json.ToString());
                if (json["status"].ToString() == "true")
                {
                    JsonObject data = json["data"].GetObject();
                    JsonArray normal_passengers = data["normal_passengers"].GetArray();
                    if (normal_passengers.Count > 0)
                    {

                        for (int i = 0; i < normal_passengers.Count; i++)
                        {
                            JsonObject userObj = normal_passengers[i].GetObject();
                            string userStr = userObj.ToString();
                            userInfo user = JsonHelper<userInfo>.Deserialize(userStr, typeof(userInfo)) as userInfo;
                            userArr.Add(user);
                            chePiaoInfo.JYUserInfoCell cell = new chePiaoInfo.JYUserInfoCell();
                            cell.userinfo = user;
                            cell.labName.Text = user.passenger_name + "  " + user.passenger_id_type_name;
                            cell.labshenfenzheng.Text = user.passenger_id_no;

                            stack.Children.Add(cell);
                            cell.comZuoType.ItemsSource = zuoArr;

                            comboxArr.Add(cell);
                        }
                    }
                    else
                    {
                        MessageDialog message = new MessageDialog("您还没有可选择的购票人，请添加");
                        message.ShowAsync();
                    }

                }
                else
                {
                    MessageDialog message = new MessageDialog(json["messages"].ToString());

                    message.ShowAsync();
                }

            }, (string err) =>
            {
                Debug.WriteLine(err.ToString());

            });
        }
        /// <summary>
        /// 根据座位类型  获得相关的名称
        /// #1->硬座/无座
        //#3->硬卧
        //#4->软卧
        //#7->一等软座
        //#8->二等软座
        //#9->商务座
        //#M->一等座
        //#O->二等座
        //#P->特等座
        //#B->混编硬座
        //        /// </summary>
        /// <param name="seat"></param>
        string getSeatType(string seat)
        {
            string name = "";
            switch (seat)
            {
                case "1":
                    {
                        name = "硬座/无座";
                    }
                    break;
                case "3":
                    {
                        name = "硬卧";
                    }
                    break;
                case "4":
                    {
                        name = "软卧";
                    }
                    break;
                case "7":
                    {
                        name = "一等软座";
                    }
                    break;
                case "9":
                    {
                        name = "商务座";
                    } 
                    break;
                case "M":
                    {
                        name = "一等座";
                    }
                    break;
                case "O":
                    {
                        name = "二等座";
                    }
                    break;
                case "P":
                    {
                        name = "特等座";
                    }
                    break;
                case "B":
                    {
                        name = "混编硬座";
                    }
                    break;
                default:
                    {
                        name = "未知座位";
                    }
                    break;
            }
            return name;

        }
        /// <summary>
        /// 获得图片
        /// </summary>
        async void initGetImage()
        {
            try
            {
                //      StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/srca.cer"));
                string uriStr = "";


                uriStr = "https://kyfw.12306.cn/otn/passcodeNew/getPassCodeNew?module=passenger&rand=randp";

                Uri uri = new Uri(uriStr);
                //   client.DefaultRequestHeaders.Add()
               
                Windows.Web.Http.HttpResponseMessage respon = await HTTPRequest.init().client.GetAsync(uri);


                if (respon != null && respon.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                {
                    BitmapImage bmp = new BitmapImage();
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await respon.Content.WriteToStreamAsync(stream);
                        stream.Seek(0UL);
                        bmp.SetSource(stream);
                        image.Source = bmp;
                        image.Width = bmp.PixelWidth;
                        image.Height = bmp.PixelHeight;
                    }
                }
                else
                {
                    Debug.Write("log  cuowu");
                }
            }
            catch
            {
                Debug.Write("log  cuowu");
            } 
        }
        List<JYSelectImage> selectImageArr = new List<JYSelectImage>();
        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

            PointerPoint pointImage = e.GetCurrentPoint(image);
            PointerPoint point = e.GetCurrentPoint(canvas);
            Debug.Write("Canvas_PointerReleased" + point.Position.ToString());
            JYSelectImage ima = new JYSelectImage();
            ima.nowSelect = pointImage.Position;
            ima.super = canvas;
            canvas.Children.Add(ima);
            ima.Margin = new Thickness(point.Position.X, point.Position.Y, 0, 0);
            ima.delegateRemov += (JYSelectImage imageSele) =>
            {
                selectImageArr.Remove(ima);
            };
            selectImageArr.Add(ima);

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            string randCode = "";
            foreach (JYSelectImage ii in selectImageArr)
            {
                randCode += ((int)ii.nowSelect.X + "," + ((int)ii.nowSelect.Y - 30) + ",");
            }
            if (randCode == "")
            {
                MessageDialog messagedia = new MessageDialog("请注意必须选择验证码");
                messagedia.ShowAsync();
                return;
            }
            //if (textBoxName.Text.ToString().Length == 0 || textBoxName.Text.ToString().Length == 0)
            //{
            //    MessageDialog messagedia = new MessageDialog("用户名或密码不能为空");
            //    messagedia.ShowAsync();
            //    return;
            //}
            randCode = randCode.Remove(randCode.Length - 1, 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("randCode", randCode);
            dic.Add("rand", "randp");
            dic.Add("_json_att", "");
            dic.Add("REPEAT_SUBMIT_TOKEN", await HTTPRequest.init().getTouken(false));

            string uri = "https://kyfw.12306.cn/otn/passcodeNew/checkRandCodeAnsyn";
            HTTPRequest.init().sendPOST(uri, dic, (JsonObject json) =>
            {
                JsonObject data = json["data"].GetObject();

                int result = int.Parse(data["result"].GetString());
                if (result == 1)
                {
                    Debug.WriteLine("验证码提交成功可以下单了");
                    //   beginLogin();
                    checkOrderInfo(randCode);
                }
                else
                {
                    MessageDialog me = new MessageDialog("验证码错误，请重新输入");
                    me.ShowAsync();
                    int count = selectImageArr.Count;
                    for (int i = 0; i < selectImageArr.Count; i++)
                    {
                        JYSelectImage ii = selectImageArr[i];
                     
                        canvas.Children.Remove(ii);
                    }
                    selectImageArr.Clear();
                    initGetImage();
                }
            }, (string err) =>
            {

            });

        }
        StringBuilder passengerTicketStr = new StringBuilder();
        StringBuilder oldPassengerStr = new StringBuilder();
        async void checkOrderInfo(string randCode)
        {
            passengerTicketStr.Clear();
            oldPassengerStr.Clear();
            Dictionary<string, string> dic = new Dictionary<string, string>();
         
            foreach (chePiaoInfo.JYUserInfoCell cellInfo in comboxArr)
            {
                if (cellInfo.checkBox.IsChecked == true)
                {
                    if (cellInfo.comZuoType.SelectedItem == null)
                    {
                        MessageDialog mess = new MessageDialog("请选择座位类型：如硬座");
                        mess.ShowAsync();
                        return;
                    }
                    string seattype = (cellInfo.comZuoType.SelectedItem as zuoType).seattype.ToString();
                    string piaoType = (cellInfo.comPiaoType.SelectedIndex + 1).ToString();
                    //座位,0，身份证类型，passenger_id_type_code，passenger_id_no，mobile_no，N_
                    userInfo userinfo = cellInfo.userinfo;
                    passengerTicketStr.AppendFormat("{0},0,{1},{2},{3},{4},{5},N_", seattype, piaoType, userinfo.passenger_name, userinfo.passenger_id_type_code, userinfo.passenger_id_no, userinfo.mobile_no);
                    oldPassengerStr.AppendFormat("{0},{1},{2},1_", userinfo.passenger_name, userinfo.passenger_id_type_code, userinfo.passenger_id_no);
                }
            }
            passengerTicketStr.Remove(passengerTicketStr.Length - 1, 1);
           dic.Add("cancel_flag", "2");
           dic.Add("bed_level_order_num", "000000000000000000000000000000");
           dic.Add("passengerTicketStr", passengerTicketStr.ToString());
           dic.Add("oldPassengerStr", oldPassengerStr.ToString());
            dic.Add("randCode", randCode);
            dic.Add("_json_att", "");
           dic.Add("REPEAT_SUBMIT_TOKEN", await HTTPRequest.init().getTouken(false));
            dic.Add("myversion", "undefined");
            dic.Add("tour_flag", "dc");
         
            // 
            //   oldPassengerStr.AppendFormat("{0},{1},{2},1_", passenger.passenger_name, passenger.passenger_id_type_code, passenger.passenger_id_no);
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/checkOrderInfo";
            HTTPRequest.init().sendPOST(uri,dic,(JsonObject json)=> {
                JsonObject data = json["data"].GetObject();

                bool result =data["submitStatus"].GetBoolean();
                if (result)
                {
                    Debug.WriteLine("验证码提交成功可以下单了");
                    //   beginLogin();
                    jianChePaiDuiRenShu(randCode);
                }
                else
                {
                    MessageDialog me = new MessageDialog(json.ToString());
                    me.ShowAsync();
                    initGetImage();
                }
            },(string err)=>{
                Debug.WriteLine(err.ToString() + "检查订单详情");
            });
        }
        /// <summary>
        /// 检查排队人数
        /// </summary>
        public async void jianChePaiDuiRenShu(string randCode)
        {
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/getQueueCount";
            string name = (Convert.ToDateTime(cell.train_date).ToString("ddd MMM dd yyy ", DateTimeFormatInfo.InvariantInfo) + DateTime.Now.ToString("HH:mm:ss").ToString() + " GMT+0800 (中国标准时间)").Replace(' ', ' ');
        //    name =   System.Net.WebUtility.UrlEncode(name);
            // (Convert.ToDateTime(cell.train_date).ToString("ddd MMM dd yyy ", DateTimeFormatInfo.InvariantInfo) + DateTime.Now.ToString("HH:mm:ss").Replace(":", "%3A") + " GMT%2B0800  (China Standard Time)").Replace(' ', '+')
            var dic = new Dictionary<string, string>
            {
                {"train_date",name},
                {"train_no", cell.cell.queryLeftNewDTO.train_no},
                {"stationTrainCode", cell.cell.queryLeftNewDTO.station_train_code},
                {"seatType",passengerTicketStr.ToString(0,1)},
                {"fromStationTelecode", cell.cell.queryLeftNewDTO.from_station_telecode},
                {"toStationTelecode", cell.cell.queryLeftNewDTO.end_station_telecode},
                {"leftTicket", cell.cell.queryLeftNewDTO.yp_info},
                {"purpose_codes", HTTPRequest.init().purpose_codes},
                {"_json_att", ""},
                { "REPEAT_SUBMIT_TOKEN", await HTTPRequest.init().getTouken(false) }
        };


               HTTPRequest.init().sendPOST(uri,dic, async (JsonObject json) =>
               {

                   Debug.WriteLine(json.ToString());
                   bool status = json["status"].GetBoolean();
                   if (status)
                   {
                       JsonObject data = json["data"].GetObject();
                       string yuPiao = string.Format("本次列车您选择的[{0}]尚有余票{1}张.", labForCheCi.Text, data["count"].GetString());

                       if (data["op_2"].GetString() == "true")
                       {
                           MessageDialog message = new MessageDialog("目前排队人数已经超过余票张数, 请您选择其他席别或车次.");
                           message.ShowAsync();
                           return;
                       }
                       if (data["countT"].GetString() != "0")
                       {
                         Debug.WriteLine(  string.Format("目前排队人数{0}人.", data["countT"].GetString()));
                           ////等待一秒重新访问
                            await Task.Delay(TimeSpan.FromSeconds(1));
                           //jianChePaiDuiRenShu(randCode);
                           //return;
                       }
                       isCanTiJiao(randCode);
                       //ScrollMessage("成功查询排队详情.");

                   }
                   else
                   {
                       MessageDialog message = new MessageDialog(json["messages"].ToString());
                       message.ShowAsync();
                   }

               }, (string err)=>{

            });
        }
        //确认在队单的位置 ，获得是否可以提交此订单的返回值
        public void isCanTiJiao(string randCode)
        {
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/confirmSingleForQueue";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("passengerTicketStr", passengerTicketStr.ToString());
            dic.Add("oldPassengerStr", oldPassengerStr.ToString());
            dic.Add("randCode",randCode);
            dic.Add("purpose_codes", HTTPRequest.init().purpose_codes);
            dic.Add("key_check_isChange", HTTPRequest.init().key_check_isChange);
            dic.Add("leftTicketStr", cell.cell.queryLeftNewDTO.yp_info);
            dic.Add("train_location", HTTPRequest.init().train_location);
            dic.Add("_json_att","");
            dic.Add("REPEAT_SUBMIT_TOKEN", HTTPRequest.init().REPEAT_SUBMIT_TOKEN);
            dic.Add("dwAll", "N");
            HTTPRequest.init().sendPOST(uri, dic, (JsonObject json) => {
                //{{"validateMessagesShowId":"_validatorMessage","status":true,"httpstatus":200,"data":{"submitStatus":true},"messages":[],"validateMessages":{}}}
                Debug.WriteLine(json.ToString());
                JsonObject data = json["data"].GetObject();
                if (data["submitStatus"].GetBoolean())
                {
                    //去走下一步
                    getWaiteTime();
                }
                else
                {
                    //不能提交  回去重新请求看看试试
                    //   isCanTiJiao(randCode);
                    MessageDialog mess = new MessageDialog(data.ToString());
                    mess.ShowAsync();
                }
            }, (string err) => {
                Debug.WriteLine(err);
            });
        }
        /// <summary>
        /// 确认订单要等待的时间  根绝返回值 每个一秒访问一次，直到返回值为-1
        /// </summary>
        public void getWaiteTime()
        {
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/queryOrderWaitTime";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("random", new Random().NextDouble().ToString());
            dic.Add("tourFlag",HTTPRequest.init().tour_flag);
            dic.Add("_json_att", "");
            dic.Add("REPEAT_SUBMIT_TOKEN", HTTPRequest.init().REPEAT_SUBMIT_TOKEN);
            HTTPRequest.init().sendGet(uri, dic, async (JsonObject json) =>
            {
                JsonObject data = json["data"].GetObject();
                if (data["waitTime"].GetNumber() < 0)
                {
                    string orderId = data["orderId"].GetString();
                    //表示连倒计时都可以了  去提交订单啊  麻痹写了这么久终于等到这个接口了
                    tiJiaoDingDan(orderId);
                }
                else
                {
                    //表示还没到时间  继续请求自己
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    getWaiteTime();

                }

            }, (string err) => {

            }); 
        }
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderSequence_no">orderId 订单ID</param>
        public void tiJiaoDingDan(string orderSequence_no)
        {
            string uri = "https://kyfw.12306.cn/otn/confirmPassenger/resultOrderForDcQueue";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("orderSequence_no", orderSequence_no); 
            dic.Add("_json_att", "");
            dic.Add("REPEAT_SUBMIT_TOKEN", HTTPRequest.init().REPEAT_SUBMIT_TOKEN);
            HTTPRequest.init().sendGet(uri, dic, (JsonObject json) => {
                JsonObject data = json["data"].GetObject();
                if (data["submitStatus"].GetBoolean())
                {
                    //提交成功  可以去付款了
                    this.Frame.Navigate(typeof(MyOrder.JYOrderInfo));
                }
                else
                {
                    //不知道为啥没下单成功  按道理来说不应该啊
                    MessageDialog mess = new MessageDialog(data["errMsg"].GetString());
                    mess.ShowAsync();
                }

            }, (string err) => {

            });
        }

        private void backClick(object sender, RoutedEventArgs e)
        {

            this.Frame.GoBack();
        }
    }

    public class zuoType
    {
        public string seattype { get; set; }
        public string nameForm { get; set; }
        public override string ToString()
        {
            return nameForm + " ";// +seattype;

        }
    }
    public class uaerInfoArr
    {
        public userInfo user{ get; set; } 
        public string name{ get; set; } 
        public string shenfenzheng{ get; set; } 

    }
    public class userInfo
    {
                public string code { get; set; }   // "2",
            public string passenger_name{ get; set; }    // "王俊云",
            public string sex_code{ get; set; }    // "M",
            public string sex_name{ get; set; }    // "男",
            public string born_date{ get; set; }    // "2013-10-05 00:00:00",
            public string country_code{ get; set; }    // "CN",
            public string passenger_id_type_code{ get; set; }    // "1",
            public string passenger_id_type_name{ get; set; }    // "二代身份证",
            public string passenger_id_no{ get; set; }    // "410928199210050616",
            public string passenger_type{ get; set; }    // "1",
            public string passenger_flag{ get; set; }    // "0",
            public string passenger_type_name{ get; set; }    // "成人",
            public string mobile_no{ get; set; }    // "18339782529",
            public string phone_no{ get; set; }    // "",
            public string email{ get; set; }    // "1617176084@qq.com",
            public string address{ get; set; }    // "",
            public string postalcode{ get; set; }    // "",
            public string first_letter{ get; set; }    // "WJY",
            public string recordCount{ get; set; }    // "5",
            public string total_times{ get; set; }    // "99",
            public string index_id{ get; set; }    // "0"
    }
}
