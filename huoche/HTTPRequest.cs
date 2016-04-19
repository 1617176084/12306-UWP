using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace huoche
{
    class HTTPRequest
    {
        public delegate void finishd(JsonObject json);
        public delegate void finishdHtml(string html);
        public delegate void err(string err);
        public   HttpClient client = null;
        public static HTTPRequest http = null;
        //token 
        public string REPEAT_SUBMIT_TOKEN;
        public string key_check_isChange;
        public string train_location;
        public string purpose_codes;  //'purpose_codes':'00',
        public string tour_flag;//'tour_flag':'dc'
        public HTTPRequest(){
            initClient();
            train_location = "";
            REPEAT_SUBMIT_TOKEN = "";
            key_check_isChange = "";
        }
      public static  HTTPRequest init()
        {
            if (http == null)
            {
                http = new HTTPRequest();
            }
            return http;
        }
         void    initClient()
        {
            if (client == null)
            {
                var filter = new HttpBaseProtocolFilter();
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                client = new Windows.Web.Http.HttpClient(filter);
            }
    
        }

        async public void sendPOST(string uriStr, Dictionary<string, string> dic, finishd delegatefi,err e)
        {
         //   Dictionary<string, string> dic = new Dictionary<string, string>();

            var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri(uriStr);
            try
            {
                HttpResponseMessage message = await client.PostAsync(uri, content);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    JsonObject json = JsonObject.Parse(message.Content.ToString());

                    delegatefi(json);
                }
                else
                {

                    e("请求失败");
                }
            }
            catch
            {
                e("链接错误");
            }
        }
        async public void sendGet(string uriStr, Dictionary<string, string> dic, finishd delegatefi, err e)
        {
          
            string apendStr = "?";
            foreach(string key in dic.Keys)
            {
                string value = dic[key];
                apendStr += key + "=" + value + "&";
            }
            apendStr = apendStr.Remove(apendStr.Length - 1, 1);
            uriStr += apendStr;
            //var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri(uriStr);
            try
            {
                HttpResponseMessage message = await client.GetAsync(uri);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    JsonObject json = JsonObject.Parse(message.Content.ToString());

                    delegatefi(json);
                }
                else
                {

                    e("请求失败");
                }
            }
            catch
            {
                e("链接错误");
            }
        }
        async public void sendGetHtml(string uriStr, Dictionary<string, string> dic, finishdHtml delegatefi, err e)
        {

            string apendStr = "?";
            if (dic != null)
            {
                foreach (string key in dic.Keys)
                {
                    string value = dic[key];
                    apendStr += key + "=" + value + "&";
                }
                uriStr += apendStr;
            }
        
            //var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri(uriStr);
            try
            {
                //CancellationTokenSource cts = new CancellationTokenSource(7000);
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept-Encoding", "gzip, deflate");  
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
              
               HttpResponseMessage message = await client.GetAsync(uri);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    var contentType = message.Content.Headers.ContentType;
                    if (string.IsNullOrEmpty(contentType.CharSet))
{
                        contentType.CharSet = "utf-8";
                    }
                    //  JsonObject json = JsonObject.Parse(message.Content.ToString());

                    delegatefi(message.Content.ToString());
                }
                else
                {

                    e("请求失败");
                }
            }
            catch
            {
                e("链接错误");
            }
        }
        async public void sendPostHtml(string uriStr, Dictionary<string, string> dic, finishdHtml delegatefi, err e)
        { 
            //var content = new HttpFormUrlEncodedContent(dic);
            Uri uri = new Uri(uriStr);
            try
            {
                //CancellationTokenSource cts = new CancellationTokenSource(7000);
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept-Encoding", "gzip, deflate");  
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
                var content = new HttpFormUrlEncodedContent(dic);
                HttpResponseMessage message = await client.PostAsync(uri,content);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    var contentType = message.Content.Headers.ContentType;
                    if (string.IsNullOrEmpty(contentType.CharSet))
                    {
                        contentType.CharSet = "utf-8";
                    }
                    //  JsonObject json = JsonObject.Parse(message.Content.ToString());

                    delegatefi(message.Content.ToString());
                }
                else
                {

                    e("请求失败");
                }
            }
            catch
            {
                e("链接错误");
            }
        }
        public async Task<string>  getTouken(bool reginstToken)
        {
            if (reginstToken)
            {

                REPEAT_SUBMIT_TOKEN = "";
            }
            if (REPEAT_SUBMIT_TOKEN.Length > 3)
            {
                return REPEAT_SUBMIT_TOKEN;
            }
            string uriStr = "https://kyfw.12306.cn/otn/confirmPassenger/initDc";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("_json_att", "");


            Uri uri = new Uri(uriStr);
            try
            {
                //CancellationTokenSource cts = new CancellationTokenSource(7000);
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept-Encoding", "gzip, deflate");  
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
                var content = new HttpFormUrlEncodedContent(dic);
                HttpResponseMessage message = await client.PostAsync(uri, content);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    var contentType = message.Content.Headers.ContentType;
                    if (string.IsNullOrEmpty(contentType.CharSet))
                    {
                        contentType.CharSet = "utf-8";
                    }
                    //  JsonObject json = JsonObject.Parse(message.Content.ToString());
                    string html = message.Content.ToString();
                    Debug.WriteLine(html);
                    Regex rege = new Regex(@"var globalRepeatSubmitToken = '\w+'");
                    Regex regeCheck = new Regex(@"key_check_isChange':'\w+',");
                    Regex regelocation = new Regex(@"train_location':'\w+'");
                    //public string purpose_codes;  //'purpose_codes':'00',
                    //  public string tour_flag;//'tour_flag':'dc'
                    Regex regepurpose = new Regex(@"purpose_codes':'\w+'");
                    Regex regetour = new Regex(@"tour_flag':'\w+'");
                    Debug.WriteLine(rege.Match(html));
                    if (rege.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = rege.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 3)
                        {
                           REPEAT_SUBMIT_TOKEN = codeArr[1];
                        }
                    }
                    if (regeCheck.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = regeCheck.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 4)
                        {
                            key_check_isChange = codeArr[2];
                        }
                    }
                    if (regelocation.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = regelocation.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 4)
                        {
                            train_location = codeArr[2];
                        }
                    }
                    if (regepurpose.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = regepurpose.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 4)
                        {
                            purpose_codes = codeArr[2];
                        }
                    }
                    if (regetour.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = regetour.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 4)
                        {
                            tour_flag = codeArr[2];
                        }
                    }
                }
                else
                {

                    Debug.WriteLine("请求失败");
                }
           
            }
            catch
            {
                Debug.WriteLine("链接错误");
            }
            return REPEAT_SUBMIT_TOKEN;
        }
        public async Task<string> getTicket()
        {
            if (REPEAT_SUBMIT_TOKEN.Length > 3)
            {
                return REPEAT_SUBMIT_TOKEN;
            }
            string uriStr = "https://kyfw.12306.cn/otn/confirmPassenger/initDc";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("_json_att", "");


            Uri uri = new Uri(uriStr);
            try
            {
                //CancellationTokenSource cts = new CancellationTokenSource(7000);
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept-Encoding", "gzip, deflate");  
                //client.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
                var content = new HttpFormUrlEncodedContent(dic);
                HttpResponseMessage message = await client.PostAsync(uri, content);
                if (message.StatusCode == HttpStatusCode.Ok && message != null)
                {
                    var contentType = message.Content.Headers.ContentType;
                    if (string.IsNullOrEmpty(contentType.CharSet))
                    {
                        contentType.CharSet = "utf-8";
                    }
                    //  JsonObject json = JsonObject.Parse(message.Content.ToString());
                    string html = message.Content.ToString();
                    Debug.WriteLine(html);
                    Regex rege = new Regex(@"'key_check_isChange':'\w+',");
                    Debug.WriteLine(rege.Match(html));
                    if (rege.Match(html).ToString() == "")
                    {

                    }
                    else
                    {
                        string code = rege.Match(html).ToString();
                        string[] codeArr = code.Split('\'');
                        if (codeArr.Length == 3)
                        {
                            REPEAT_SUBMIT_TOKEN = codeArr[1];
                        }
                    }
                }
                else
                {

                    Debug.WriteLine("请求失败");
                }

            }
            catch
            {
                Debug.WriteLine("链接错误");
            }
            return REPEAT_SUBMIT_TOKEN;
        }
        //   private String zipInputStream(InputStream is) throws IOException
        //   {
        //       GZIPInputStream gzip = new GZIPInputStream(is);
        //       BufferedReader in = new BufferedReader(new InputStreamReader(gzip, "UTF-8"));
        //       StringBuffer buffer = new StringBuffer();
        //       String line;  
        //while ((line = in.readLine()) != null)  
        //    buffer.append(line + "\n");  
        //is.close();  
        //return buffer.toString();
        //   }

    }
}
