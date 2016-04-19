using huoche.chePiaoInfo;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace huoche
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JYChePiaoInfo : Page
    {
        public JYChePiaoInfo()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            chePiaoCellInfo cell = e.Parameter as chePiaoCellInfo;
            if (cell != null)
            {
                labForCheCi.Text = cell.checi;
              
                
                labForLiCheng.Text = cell.liShiShiJian + cell.jieshuTime + "到达";

                chePiaoCell obj = cell.cell;
                string qishiAddress = obj.queryLeftNewDTO.from_station_name +(obj.queryLeftNewDTO.start_station_name.Equals(obj.queryLeftNewDTO.from_station_name) ? "[始发]" : "[路过]");
                string jieshuAddress =  obj.queryLeftNewDTO.to_station_name + (obj.queryLeftNewDTO.end_station_name.Equals(obj.queryLeftNewDTO.to_station_name) ? "[终到]" : "[路过]") ;
                labForFaDaoZhan.Text = qishiAddress + "-" + jieshuAddress;

                labForKaiCheShiJian.Text = cell.train_date +" "+ cell.qishiTime;
                labForLiCheng.Text = "历时" + obj.queryLeftNewDTO.lishi + "," + (obj.queryLeftNewDTO.day_difference.Equals("0") ? "当天" : "第" + ((int.Parse(obj.queryLeftNewDTO.day_difference) + 1) + "天")) + cell.jieshuTime + "到达";

           //string jieguo =   getYuPiao(obj.queryLeftNewDTO.yp_info);
           //添加预定车票信息
                addYuPiao(obj.queryLeftNewDTO.yp_info,cell);
                HTTPRequest.init().getTouken(true);
                //查询这个车次的时刻表
                string shiKeUri = "https://kyfw.12306.cn/otn/czxx/queryByTrainNo";
                Dictionary<string, string> dicShiKe = new Dictionary<string, string>();
                dicShiKe.Add("train_no",obj.queryLeftNewDTO.train_no);
                dicShiKe.Add("from_station_telecode",obj.queryLeftNewDTO.from_station_telecode);
                dicShiKe.Add("to_station_telecode",obj.queryLeftNewDTO.to_station_telecode);
               dicShiKe.Add("depart_date",cell.train_date);
                HTTPRequest.init().sendGet(shiKeUri, dicShiKe, (JsonObject json) =>
                {
                    Debug.WriteLine(json.ToString());
                 JsonObject data =   json["data"].GetObject();
                 JsonArray   dataArr = data["data"].GetArray();
                for(int i = -1;i< dataArr.Count;i++)
                {
                        if (i >= 0)
                        {
                            JsonObject cellshike = dataArr[i].GetObject();
                            shiKeCell cellMode = JsonHelper<shiKeCell>.Deserialize(cellshike.ToString(), typeof(shiKeCell)) as shiKeCell;
                            JYHuoCheShiKeCell huocheview = new JYHuoCheShiKeCell();
                            huocheview.labForZhanMing.Text = cellMode.station_name;
                            huocheview.labForDaoShi.Text = cellMode.arrive_time;
                            huocheview.labForFaShi.Text = cellMode.start_time;
                            huocheview.LabForTingLiu.Text = cellMode.stopover_time;
                            huocheview.labForShiGuo.Visibility = cellMode.isEnabled.Equals("true") ? Visibility.Visible : Visibility.Collapsed;
                            stackPanel.Children.Add(huocheview);
                        }
                        else if(i == -1)
                        {
                            JYHuoCheShiKeCell huocheview = new JYHuoCheShiKeCell();
                            huocheview.labForZhanMing.Text = "站名";
                            huocheview.labForDaoShi.Text = "到达时间（到站）";
                            huocheview.labForFaShi.Text = "发车时间（离站）";
                            huocheview.LabForTingLiu.Text = "停留时间";
                            huocheview.labForShiGuo.Text = "是否驶过";
                            stackPanel.Children.Add(huocheview);
                        }

                    }
                  
                }, (string err) =>
                {
                    Debug.WriteLine(err.ToString());
                });
            }
         
        }
        void addYuPiao(string code, chePiaoCellInfo cellinfo)
        {
            //  string kaiTou = cheCi.Substring(0, 1);
            string jieguo = "";

            // string[] nameArr = new string[3] { "二等", "一等", "无座" };
            for (int i = 0; i < (code.Length / 10); i++)
            {
                double jiage = getNumberJiaGe(i, code) * 0.1;
                int shuliang = getNumberShuLiang(i, code);
                JYChePiaoYuDingCell cell = new JYChePiaoYuDingCell();
                cell.labForDengJi.Text = getZuoWei(i, code);
                cell.labForJiaQian.Text = "￥" + jiage;
                cell.labForShuLiang.Text = shuliang + " 张 ";
                stackPanel.Children.Add(cell);
                cell.btnForYuDing.Click += (System.Object sender, RoutedEventArgs e) => {
                    //检查是否需要登录
                    string uriLogin = "https://kyfw.12306.cn/otn/login/checkUser";
                Dictionary<string, string> dicLogin = new Dictionary<string, string>();
                dicLogin.Add("_json_att", "");
                    HTTPRequest.init().sendPOST(uriLogin, dicLogin, (JsonObject json) =>
                    {
                        JsonObject data = json["data"].GetObject();
                        string flag = data["flag"].ToString();
                        //是否登陆
                        if (flag.Equals("true"))
                        {
                            //提交检查 看看是否能下单
                            string uriRequstDingDna = "https://kyfw.12306.cn/otn/leftTicket/submitOrderRequest";
                            Dictionary<string, string> dicRequest = new Dictionary<string, string>();
                            dicRequest.Add("secretStr", System.Net.WebUtility.UrlDecode(cellinfo.cell.secretStr));
                            dicRequest.Add("train_date", cellinfo.train_date);
                            dicRequest.Add("back_train_date", cellinfo.back_train_date);
                            dicRequest.Add("tour_flag", "dc");
                            dicRequest.Add("purpose_codes", "ADULT");
                            dicRequest.Add("query_from_station_name", cellinfo.cell.queryLeftNewDTO.from_station_name);
                            dicRequest.Add("query_to_station_name", cellinfo.cell.queryLeftNewDTO.to_station_name);
                            dicRequest.Add("undefined", "");
                            dicRequest.Add("myversion", "undefined");
                            HTTPRequest.init().sendPOST(uriRequstDingDna, dicRequest, (JsonObject jsonRequest) => {
                                Debug.WriteLine(jsonRequest.ToString());

                                if (jsonRequest["status"].GetBoolean())
                                {
                                    Debug.WriteLine("申请订单成功，确定订单可以继续");
                                    this.Frame.Navigate(typeof(JYDingDanXinXiTiJiao),cellinfo);
                                }
                                else
                                {
                                    Regex rege = new Regex("[\u4E00-\u9FFF]+");
                                    string me = "";
                                    foreach (Match NextMatch in rege.Matches(jsonRequest["messages"].ToString()))
                                        {
                                        me +=","+ NextMatch.Value.ToString();
                                      
                                        }

                                    Debug.WriteLine(me);
                                    MessageDialog message = new MessageDialog(me);
                                   
                                   
                                    message.ShowAsync();
                                }
                            }, (string err) => {
                                Debug.WriteLine(err.ToString());
                            });
                        }
                        else
                        {
                            //需要重新登陆
                            this.Frame.Navigate(typeof(MainPage), "购买车票验证");
                        }
                    }, (string err) => {


                    });

                };
                //jieguo += getZuoWei(i, code) + ":  ￥" + jiage + ", " + shuliang + " 张。 ";
            }


             
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
                jieguo += getZuoWei(i,code) + ":  ￥" + jiage + ", " + shuliang+ " 张。 ";
            }

           
            return jieguo;
        }
        int getNumberJiaGe(int i,string code)
        {
        return int.Parse( code.Substring(2 + i * 10, 4));
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

            string[] nameArr = new string[10] { "无座", "软座","软卧", "高级软卧", "硬卧", "硬座", "一等座", "二等座", "特等座","商务座" };
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
                    }
                    break;
                case "10":
                    {

                        if (fenGe.Equals("3"))
                        {
                            needName = nameArr[0];
                        }
                        else {
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
                    needName ="其他";
                    break; 
            }
            return needName;
        }

        private void backClick(object sender, RoutedEventArgs e)
        {

            this.Frame.GoBack();
        }
    }
    public class shiKeCell
    {
          public string  start_station_name{get;set;}  // "北京西",
          public string  arrive_time{get;set;}  // "----",
          public string  station_train_code{get;set;}  // "1303",
          public string  station_name{get;set;}  // "北京西",
          public string  train_class_name{get;set;}  // "普快",
          public string  service_type{get;set;}  // "1",
          public string  start_time{get;set;}  // "22:45",
          public string  stopover_time{get;set;}  // "----",
          public string  end_station_name{get;set;}  // "郑州",
          public string  station_no{get;set;}  // "01",
          public string  isEnabled{get;set;}  // true
    }
}
