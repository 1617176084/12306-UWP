using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

using Windows.Web.Http.Filters;
using Windows.Security.Cryptography.Certificates;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.Data.Json;



//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace huoche
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        string login;
        Windows.Web.Http.HttpClient client;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //  if(this.Frame.BackStack)
            // Uri uri = new Uri("https://kyfw.12306.cn/otn/passcodeNew/getPassCodeNew?module=login&rand=sjrand");
            login = e.Parameter as string;
            

            initClient();
            initGetImage();


        }
        void initClient()
        {
            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            client = new Windows.Web.Http.HttpClient(filter);
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
            
                
                    uriStr = "https://kyfw.12306.cn/otn/passcodeNew/getPassCodeNew?module=login&rand=sjrand";
               
                    Uri uri = new Uri(uriStr);
                //   client.DefaultRequestHeaders.Add()

                Windows.Web.Http.HttpResponseMessage respon = await client.GetAsync(uri);


                if (respon != null && respon.StatusCode == HttpStatusCode.Ok)
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
            ima.Margin = new Thickness(point.Position.X,point.Position.Y, 0, 0);
            ima.delegateRemov += (JYSelectImage imageSele) =>
            {
                selectImageArr.Remove(ima);
            };
            selectImageArr.Add(ima);
          
        }
        /// <summary>
        /// 登陆  提交验证码  用户名 和  密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dengluClic(object sender, RoutedEventArgs e)
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
            if (textBoxName.Text.ToString().Length == 0 || textBoxName.Text.ToString().Length == 0)
            {
                MessageDialog messagedia = new MessageDialog("用户名或密码不能为空");
                messagedia.ShowAsync();
                return;
            }
            randCode = randCode.Remove(randCode.Length - 1, 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("randCode", randCode);
            dic.Add("rand", "sjrand");
            var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri("https://kyfw.12306.cn/otn/passcodeNew/checkRandCodeAnsyn");
            HttpResponseMessage message = await client.PostAsync(uri, content);
            if (message.StatusCode == HttpStatusCode.Ok && message != null)
            {
                JsonObject json = JsonObject.Parse(message.Content.ToString());
               
                JsonObject data = json["data"].GetObject();

              int result = int.Parse( data["result"].GetString());
                if (result == 1)
                {
                    beginLogin();
                }
                else
                {
                    MessageDialog me = new MessageDialog("验证码错误，请重新尝试");
                    me.ShowAsync();
                    closImages();

                    initGetImage();
                }
            }

        }
        void closImages()
        {
            int count = selectImageArr.Count;
            for (int i = 0; i < selectImageArr.Count; i++)
            {
                JYSelectImage ii = selectImageArr[i];

                canvas.Children.Remove(ii);
            }
            selectImageArr.Clear();
        }
        async void  beginLogin()
        {
            string randCode = "";
            foreach (JYSelectImage ii in selectImageArr)
            {
                randCode += ((int)ii.nowSelect.X + "," + ((int)ii.nowSelect.Y - 30) + ",");
            }
            randCode = randCode.Remove(randCode.Length - 1, 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("loginUserDTO.user_name", textBoxName.Text.ToString());
            dic.Add("userDTO.password", textBoxPWD.Password.ToString());



            dic.Add("randCode", randCode);
            dic.Add("randCode_validate", "");
            dic.Add("myversion", "undefined");
            var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri("https://kyfw.12306.cn/otn/login/loginAysnSuggest");
            HttpResponseMessage message = await client.PostAsync(uri, content);
            if (message.StatusCode == HttpStatusCode.Ok && message != null)
            {
                /*
    {
        "validateMessagesShowId": "_validatorMessage",
        "status": true,
        "httpstatus": 200,
        "data": {
            
        },
        "messages": [
            "验证码不正确！"
        ],
        "validateMessages": {
            
        }
    }
                */
                Debug.WriteLine(message.Content.ToString());
          
                JsonObject json = JsonObject.Parse(message.Content.ToString());
              
                if (json["data"].GetObject().Count > 1)
                {
                    MessageDialog me = new MessageDialog("登陆成功请返回，或继续操作");
                    me.ShowAsync();
                    if (login == "购买车票验证")
                    {
                        this.Frame.GoBack();
                    }
                    
                }
                else
                {
                    MessageDialog me = new MessageDialog(json["messages"].ToString());
                    me.ShowAsync();
                    closImages();
                }
            }
        }
        /// <summary>
        /// 获得用户信息
        /// </summary>
      async  void huoDeYongHuInFo()
        {
            
            Uri uri = new Uri("https://kyfw.12306.cn/otn/confirmPassenger/getPassengerDTOs");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("_json_att", "");
            dic.Add("REPEAT_SUBMIT_TOKEN", "");;
            var content = new HttpFormUrlEncodedContent(dic);
         
            HttpResponseMessage message = await client.PostAsync(uri, content);
            if (message.StatusCode == HttpStatusCode.Ok && message != null)
            { 
                Debug.WriteLine(message.Content.ToString());
                MessageDialog me = new MessageDialog(message.Content.ToString());
                me.ShowAsync();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            huoDeYongHuInFo();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
