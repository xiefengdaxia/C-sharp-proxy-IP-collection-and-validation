using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace 小说下载
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            string url = textBox1.Text.Trim();;
            //耗时操作在后台进程进行
            Thread worker = new Thread(delegate()
            {
                ShowLoading(true);
                var dict = GetMulu(url, GetHTML(url), "//*[@id=\"list\"]/dl", null);//*[@id="list"]

                foreach (var item in dict)
                {
                    addMulu((item.Key + "|" + item.Value).Split('|'));
                }
                ApendLog("获取目录完毕！");
                var path = Application.StartupPath + "\\txt";
                //先删除整个文件夹及下所有文件
                if (Directory.Exists(path) == true)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
                else
                {
                    //然后再新建一个空的文件夹
                    Directory.CreateDirectory(path);
                }
                ShowLoading(false);
            });
            worker.IsBackground = true;
            worker.Start();

        }
        public static object locker = new object();
        public static string txtpath = Application.StartupPath + "\\txt\\";
        public static void saveTxt(string txt, string txtName)
        {
            lock (locker)
            {

                if (File.Exists(txtpath + txtName + ".txt"))
                {
                    StreamWriter streamWriter = File.AppendText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(txt);
                    streamWriter.Close();
                }
                else
                {
                    StreamWriter streamWriter = File.CreateText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(txt);
                    streamWriter.Close();
                }
            }
        }
        public Dictionary<string, string> GetMulu(string url, string html, string xpath, string mulu)
        {
            var muludict = new Dictionary<string, string>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            if (html.Length < 10)
                return muludict;
            doc.LoadHtml(html);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(mulu + "\n");
            HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
            IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
            foreach (HtmlNode item in nodeList)
            {
                //MessageBox.Show(item.InnerText);
                //if (item.InnerText.Contains("章"))
                if (item.InnerText.Length>0)
                {
                    var arr = item.InnerHtml.Split('"');
                    if (arr.Length > 1)
                    {
                        if (!muludict.ContainsKey(item.InnerText)) muludict.Add(item.InnerText, url + "/" + item.InnerHtml.Split('"')[1]);
                        //sb.AppendLine(item.InnerText + " : " + url + "/" + item.InnerHtml.Split('"')[1]);
                    }
                }
            }
            return muludict;
        }
        public int SaveTxtContent(string url, string html, string xpath, string mulu, int xu)
        {
            var textLenght = 0;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            if (html.Length > 10)
            {
                doc.LoadHtml(html);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(mulu + "\n");
                HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
                IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
                foreach (HtmlNode item in nodeList)
                {

                    if (mulu != null)
                    {
                        sb.AppendLine(item.InnerText.Replace("&nbsp;", "").Replace("\n", "").Replace("\br", ""));
                    }

                }
                textLenght = sb.Length;
                saveTxt(sb.ToString(), (xu + 1).ToString());
            }
            return textLenght;
        }
        public string GetHTML(string url)
        {
            var html = "";
            try
            {
                //string Cookie = "_free_proxy_session=BAh7B0kiD3Nlc3Npb25faWQGOgZFVEkiJThiZjU2NDZlZjljOWIxY2ExODA4NTEwYTM1ZjVlMTlmBjsAVEkiEF9jc3JmX3Rva2VuBjsARkkiMThxV1Nza2o0QVNBZG1YdHRVYzRibm5OQkQwckt3V3FwL04wL3RNUDJMU1E9BjsARg%3D%3D--784d7667471701a7b9941447ccef4f4f36935fae; CNZZDATA4793016=cnzz_eid%3D1324463153-1448595402-%26ntime%3D1448595402";
                string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 Safari/537.36 SE 2.X MetaSr 1.0";
                html = GetResponseString(CreateGetHttpResponse(url, 20, userAgent, null));

            }
            catch (Exception ex)
            {
                // throw ex;
            }
            return html;
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
            request.Timeout = timeout*1000;
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
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //设置代理UserAgent和超时
            request.UserAgent = userAgent;
            request.Timeout = timeout;

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
        /// <summary>
        /// 显示loading加载图
        /// </summary>
        /// <param name="ifshow"></param>
        public void ShowLoading(bool ifshow)
        {
            //无参数,但是返回值为bool类型
            this.Invoke(new Func<bool>(delegate()
            {
                pictureBox1.Visible = ifshow;
                return true; //返回值
            }));
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Visible == true)
            {
                return;
            }
            var path = Application.StartupPath + "\\txt";
            if (Directory.Exists(path) != true)
            {
                
                Directory.CreateDirectory(path);
            }

            //耗时操作在后台进程进行
            Thread worker = new Thread(delegate()
            {
                //显示加载
                ShowLoading(true);
                var fail=1;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[3].Value != null && dataGridView1.Rows[i].Cells[3].Value.ToString() != "已下载")
                    {
                        if (SaveTxtContent(dataGridView1.Rows[i].Cells[2].Value.ToString(), GetHTML(dataGridView1.Rows[i].Cells[2].Value.ToString()), "//*[@id=\"content\"]", dataGridView1.Rows[i].Cells[1].Value.ToString(), i) > 10)
                        {
                            updateDownloadStatus(i, "已下载");
                            //ApendLog(dataGridView1.Rows[i].Cells[1].Value.ToString() + " 下载成功");
                        }
                        else
                        {
                            updateDownloadStatus(i, "失败");
                            ApendLog("【"+dataGridView1.Rows[i].Cells[1].Value.ToString() + "】下载失败，再次点击下载失败的章节");
                            updateLableText("失败章节数:" + fail++);
                            //richTextBox1.AppendText(DateTime.Now + ":\n" + dataGridView1.Rows[i].Cells[1].Value.ToString() + " 下载失败 \n\r");
                        }
                    }
                }
                ApendLog("章节下载完毕！");
                ShowLoading(false);
            });
            worker.IsBackground = true;
            worker.Start();

        }

        public void updateDownloadStatus(int index, string status)
        {
            //无参数,但是返回值为bool类型
            this.Invoke(new Func<bool>(delegate()
            {
                dataGridView1.Rows[index].Cells[3].Value = status;
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[index].Cells[3];
                return true; //返回值
            }));
        }
        public void ApendLog(string text)
        {
            //无参数,但是返回值为bool类型
            this.Invoke(new Func<bool>(delegate()
            {
                var logTime = DateTime.Now.ToString() + ";";
                richTextBox1.AppendText(logTime + text+"\n\r");
                richTextBox1.ScrollToCaret();
                return true; //返回值
            }));
        }
        public void updateLableText(string text)
        {
            //无参数,但是返回值为bool类型
            this.Invoke(new Func<bool>(delegate()
            {

                label3.Text = text; ;
                return true; //返回值
            }));
        }
        public void addMulu(string[] mulu)
        {
            //无参数,但是返回值为bool类型
            this.Invoke(new Func<bool>(delegate()
            {
                int index = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[index].Cells[0].Value = index + 1;
                this.dataGridView1.Rows[index].Cells[1].Value = mulu[0];
                this.dataGridView1.Rows[index].Cells[2].Value = mulu[1];
                this.dataGridView1.Rows[index].Cells[3].Value = "未开始";
                return true; //返回值
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 2)
                return;
            Thread worker = new Thread(delegate ()
            {
                ShowLoading(true);
                var path = Application.StartupPath + "\\txt";
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[3].Value != null && dataGridView1.Rows[i].Cells[3].Value.ToString() == "已下载")
                    {
                        using (StreamReader sr = new StreamReader(path+"\\"+(i+1)+".txt"))
                        {

                            string line = string.Empty;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.Trim() != "")
                                {
                                    sb.Append(line + "\n");
                                }
                            }
                        }
                        saveTxt(sb.ToString(), "刚出炉热乎乎的小说");
                        //File.Delete(path + "\\" + (i + 1) + ".txt");
                        sb.Remove(0, sb.Length);
                    }
                }

                DirectoryInfo dif = new DirectoryInfo(path);
                FileInfo[] fif = dif.GetFiles();
                for (int i = 0; i < fif.Length; i++)
                {
                    if (fif[i].Name != "刚出炉热乎乎的小说.txt")
                    {
                        File.Delete(path + "\\" + fif[i].Name);
                    }
                }
                System.Diagnostics.Process.Start("explorer.exe", path);
                ApendLog("合成txt完毕");
                ShowLoading(false);
            });
            worker.IsBackground = true;
            worker.Start();
        }
    }
}
