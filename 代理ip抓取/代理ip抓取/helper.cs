using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace 代理ip抓取
{
    class helper
    {
        public static 代理ip抓取.Form1.queueresult DownLoadHtml(string url, string proxyip, 代理ip抓取.Form1.queueresult q,string localhostIpAddress, int timeout = 10, bool enableProxy = true)
        {
            var result = new 代理ip抓取.Form1.queueresult();
            try
            {
                lock (q)
                {
                    q.Checkedcount++;
                    result.Checkedcount++;
                }
                string html = "";
                string[] arr = proxyip.Split(':');
                var myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "GET";
                myRequest.Timeout = 1000 * timeout;
                myRequest.AllowAutoRedirect = true;
                myRequest.UserAgent = "HttpClient";
                myRequest.Accept = "text/*";
                if (enableProxy)
                {
                    //如果启用WEBPROXY代理
                    var webProxy = new WebProxy(arr[0], int.Parse(arr[1]));
                    myRequest.Proxy = webProxy;
                }
                var myResponse = (HttpWebResponse)myRequest.GetResponse();
                using (var sr = new StreamReader(myResponse.GetResponseStream(), Encoding.GetEncoding((myResponse.CharacterSet))))
                {
                    html = sr.ReadToEnd();
                    myResponse.Close();
                }
                if (html.Length > 10)
                {
                    if (!html.Contains("111.13.62.153"))
                    {
                        saveIpTxt(proxyip, "验证可用√√高匿代理ip");
                        lock (q)
                        {
                            q.Gaoni++;
                            result.Gaoni++;
                        }
                    }
                    else
                    {
                        saveIpTxt(proxyip, "验证可用√透明代理ip");
                        lock (q)
                        {
                            q.Touming++;
                            result.Touming++;
                        }
                    }
                }
                else
                {
                    lock (q)
                    {
                        q.Fail++;
                        result.Fail++;
                    }
                }
                //return html;
            }
            catch(Exception)
            {
                lock (q)
                {
                    q.Fail++;
                    result.Fail++;
                }
                //return null;
                //throw new Exception(ex.Message);
            }
            finally
            {
                
            }
            return result;
        }
        public static string path = Application.StartupPath + "\\代理ip\\";
        public static object locker = new object();
        public static void saveIpTxt(string ips, string txtName)
        {
            lock (locker)
            {
                if (File.Exists(path + txtName + ".txt"))
                {
                    StreamWriter streamWriter = File.AppendText(path + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
                else
                {
                    StreamWriter streamWriter = File.CreateText(path + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
            }
        }
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        public static HttpWebResponse CreateGetHttpResponse(string url, int timeout, string userAgent, CookieCollection cookies)
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";
            //设置代理UserAgent和超时
            request.UserAgent = userAgent;
            request.Headers.Add("Accept-Encoding", "gzip");
            //request.Timeout = timeout;
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int timeout, string userAgent, CookieCollection cookies)
        {
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                //request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //设置代理UserAgent和超时
            //request.UserAgent = userAgent;
            //request.Timeout = timeout; 

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //发送POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }
                byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            string[] values = request.Headers.GetValues("Content-Type");
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            if (webresponse.ContentEncoding.ToLower().Contains("gzip"))
            {



                using (GZipStream stream = new GZipStream(

                    webresponse.GetResponseStream(), CompressionMode.Decompress))
                {

                    using (StreamReader reader = new StreamReader(stream, Encoding.Default))
                    {

                        return reader.ReadToEnd();

                    }

                }

            }
            else if (webresponse.ContentEncoding.ToLower().Contains("deflate"))
            {

                using (DeflateStream stream = new DeflateStream(

                    webresponse.GetResponseStream(), CompressionMode.Decompress))
                {

                    using (StreamReader reader =

                        new StreamReader(stream, Encoding.UTF8))
                    {

                        return reader.ReadToEnd();

                    }

                }

            }
            else
            {
                using (Stream s = webresponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(s, Encoding.UTF8);
                    return reader.ReadToEnd();

                }
            }

        }

        /// <summary>
        /// 验证证书
        /// </summary>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }  
    }
}
