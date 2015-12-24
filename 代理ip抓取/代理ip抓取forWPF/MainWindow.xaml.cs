using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 代理ip抓取forWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Dispatcher.Invoke(new Action(() => { }));
            this.Loaded += Window_Loaded;
        }
        //数据库连接
        SQLiteConnection m_dbConnection;
        SQLiteDataAdapter da;
        DataSet ds;
        //创建一个连接到指定数据库
        void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=123.sqlite;Version=3;");
            m_dbConnection.Open();
        }
        private void showDataGV()
        {
            SQLiteCommand cmd = new SQLiteCommand(m_dbConnection);//实例化SQL命令
            cmd.CommandText = "select * from proxyIpLinksSource";
            //DataSet ds=SQLiteHelper.ExecuteDataset(cmd);
            ds = new DataSet();
            da = new SQLiteDataAdapter(cmd);
            da.Fill(ds);
            //da.Dispose();
            cmd.Connection.Close();
            //cmd.Dispose();
            DG.ItemsSource = ds.Tables[0].DefaultView;
        }
        private int maxThread = 100;//默认可执行线程最大数量
        private void showThreadCount()
        {
            threadcount.Content = "线程数:" + slider_01.Value;
            maxThread = Convert.ToInt32(slider_01.Value);
        }
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        	// 在此处添加事件处理程序实现。
            //showThreadCount();
            connectToDatabase();
            showDataGV();
            DG.LoadingRow += new EventHandler<DataGridRowEventArgs>(DG_LoadingRow); 
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// 在此处添加事件处理程序实现。
            SQLiteCommandBuilder bu = new SQLiteCommandBuilder(da);
            da.Update(ds, "Table");
            MessageBox.Show("保存成功！");
            showDataGV();
        }

        private void DG_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;    //设置行表头的内容值   
        }

        public static string 过滤(string html)
        {
            StringBuilder sb = new StringBuilder();
            var regex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}\b";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var rawRes = html;
            Match m = r.Match(html);
            while (m.Success)
            {
                sb.AppendLine(m.Value);
                m = m.NextMatch();
            }

            return sb.ToString();
        }

        public static string 过滤(string html, string xpath)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            StringBuilder sb = new StringBuilder();

            HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
            IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
            foreach (HtmlNode item in nodeList)
            {
                //MessageBox.Show(item.InnerText);
                if (item.InnerText.Length > 10 && item.InnerText.Contains("."))
                {
                    string a = Regex.Replace(item.InnerText, @"[\n\r]", "-", RegexOptions.IgnoreCase).Replace(" ", "");
                    //采集格式"---115.46.100.118-8123--广西南宁--高匿-HTTP---------------15-11-2712:00-";
                    string[] arr = a.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    sb.AppendLine(arr[0] + ":" + arr[1]);
                }
            }

            return sb.ToString();
        }

        public static object locker = new object();
        public static string txtpath ="\\代理ip\\";
        public static void saveIpTxt(string ips, string txtName)
        {
            lock (locker)
            {

                if (File.Exists(txtpath + txtName + ".txt"))
                {
                    StreamWriter streamWriter = File.AppendText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
                else
                {
                    StreamWriter streamWriter = File.CreateText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
            }
        }
        public ArrayList ipArr;
        public long linecount = 0;
        public static queueresult[] qr;
        /// <summary>
        /// 队列结果
        /// </summary>
        public class queueresult
        {
            private int checkedcount = 0;

            public int Checkedcount
            {
                get { return checkedcount; }
                set { checkedcount = value; }
            }
            private int fail = 0;

            public int Fail
            {
                get { return fail; }
                set { fail = value; }
            }
            private int touming = 0;

            public int Touming
            {
                get { return touming; }
                set { touming = value; }
            }
            private int gaoni = 0;

            public int Gaoni
            {
                get { return gaoni; }
                set { gaoni = value; }
            }
        }
        private void _catch_Click(object sender, RoutedEventArgs e)
        {
            GetHTML_SaveProxyIpMethod gs;
            try
            {
                Thread createWorker = new Thread(delegate()
                {
                    SQLiteCommand cmd = new SQLiteCommand(m_dbConnection);//实例化SQL命令
                    cmd.CommandText = "select * from proxyIpLinksSource where checked=1";
                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();
                    SQLiteDataReader idr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (idr.Read())
                    {
                        //MessageBox.Show(idr[0].ToString());
                        gs = new GetHTML_SaveProxyIpMethod(richTextBox1);
                        gs.Url = idr["Url"].ToString();
                        gs.Url_plus = idr["Url_plus"].ToString();
                        gs.Xpath = idr["Xpath"].ToString();
                        gs.Start = Convert.ToInt32(idr["Start"]);
                        gs.End = Convert.ToInt32(idr["End"]);
                        gs.TxtName = idr["TxtName"].ToString();
                        Thread worker = new Thread(delegate()
                        {
                            gs.DoWork();
                        });
                        worker.IsBackground = true;
                        worker.Start();
                        worker.Join();
                    }
                });
                createWorker.IsBackground = true;
                createWorker.Start();



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

   
    /// <summary>
    /// 根据网址下载HTML源码
    /// 使用解析利器HtmlAgilityPack过滤HTML内容
    /// 生成指定格式保存到txt文件
    /// </summary>
    public class GetHTML_SaveProxyIpMethod
    {
        private string url;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        private string url_plus;

        public string Url_plus
        {
            get { return url_plus; }
            set { url_plus = value; }
        }
        private string xpath;

        public string Xpath
        {
            get { return xpath; }
            set { xpath = value; }
        }
        private string txtName;

        public string TxtName
        {
            get { return txtName; }
            set { txtName = value; }
        }
        private int start;

        public int Start
        {
            get { return start; }
            set { start = value; }
        }
        private int end;

        public int End
        {
            get { return end; }
            set { end = value; }
        }
        
        RichTextBox rtb;

        public GetHTML_SaveProxyIpMethod(RichTextBox rtb)
        {
            this.rtb = rtb;
        }
        //public void appendLine(string text)
        //{
        //    Action<string> actionDelegate = (x) =>
        //    {

        //        this.rtb.AppendText(text + "\r\n");
        //    };
        //    this.Dispatcher.Invoke(actionDelegate, text);
        //}

        public static string 过滤(string html)
        {
            StringBuilder sb = new StringBuilder();
            var regex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}\b";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var rawRes = html;
            Match m = r.Match(html);
            while (m.Success)
            {
                sb.AppendLine(m.Value);
                m = m.NextMatch();
            }

            return sb.ToString();
        }

        public static string 过滤(string html, string xpath)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            StringBuilder sb = new StringBuilder();

            HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
            IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
            foreach (HtmlNode item in nodeList)
            {
                //MessageBox.Show(item.InnerText);
                if (item.InnerText.Length > 10 && item.InnerText.Contains("."))
                {
                    string a = Regex.Replace(item.InnerText, @"[\n\r]", "-", RegexOptions.IgnoreCase).Replace(" ", "");
                    //采集格式"---115.46.100.118-8123--广西南宁--高匿-HTTP---------------15-11-2712:00-";
                    string[] arr = a.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    sb.AppendLine(arr[0] + ":" + arr[1]);
                }
            }

            return sb.ToString();
        }

        public static object locker = new object();
        public static string txtpath = "\\代理ip\\";
        public static void saveIpTxt(string ips, string txtName)
        {
            lock (locker)
            {

                if (File.Exists(txtpath + txtName + ".txt"))
                {
                    StreamWriter streamWriter = File.AppendText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
                else
                {
                    StreamWriter streamWriter = File.CreateText(txtpath + txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
            }
        }
        public void GetHTML_SaveProxyIp(string url, string xpath, string txtName, int model)
        {

            try
            {
                //string Cookie = "_free_proxy_session=BAh7B0kiD3Nlc3Npb25faWQGOgZFVEkiJThiZjU2NDZlZjljOWIxY2ExODA4NTEwYTM1ZjVlMTlmBjsAVEkiEF9jc3JmX3Rva2VuBjsARkkiMThxV1Nza2o0QVNBZG1YdHRVYzRibm5OQkQwckt3V3FwL04wL3RNUDJMU1E9BjsARg%3D%3D--784d7667471701a7b9941447ccef4f4f36935fae; CNZZDATA4793016=cnzz_eid%3D1324463153-1448595402-%26ntime%3D1448595402";
                string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 Safari/537.36 SE 2.X MetaSr 1.0";
                string html = helper.GetResponseString(helper.CreateGetHttpResponse(url, 20, userAgent, null));
                if (model == 0)
                {
                    saveIpTxt(过滤(html), txtName);
                }
                else
                {
                    saveIpTxt(过滤(html, xpath), txtName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DoWork()
        {
            try
            {
                //appendLine(DateTime.Now + "\n" + txtName + url + "开始采集！");
                if (start == 0)
                {
                    GetHTML_SaveProxyIp(url, xpath, txtName, start);
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {

                        GetHTML_SaveProxyIp(url + i + url_plus, xpath, txtName, start);

                    }
                }
                //appendLine(DateTime.Now + "\n" + txtName + url + "采集完毕！\r");
            }
            catch (Exception ex)
            {
                //appendLine(DateTime.Now + "\n" + txtName + url + "采集出错！\n" + ex.Message);
                MessageBox.Show(ex.StackTrace, txtName + ">>" + ex.Message);
            }
        }
    }
}
