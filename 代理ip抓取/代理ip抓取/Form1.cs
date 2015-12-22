using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace 代理ip抓取
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

        }
        private int maxThread = 100;//默认可执行线程最大数量
        private static System.Threading.Mutex muxConsole = new System.Threading.Mutex();
        private void Form1_Load(object sender, EventArgs e)
        {
            showThreadCount();
            //SQLiteConnection.CreateFile(Application.StartupPath + "\\123.sqlite");
            connectToDatabase();
            showDataGV();
            dataGridView1.RowHeadersDefaultCellStyle.Padding = new Padding(15);
        }
        private void showThreadCount()
        {
            label3.Text = "线程数:" + trackBar1.Value;
            maxThread = trackBar1.Value;
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
            dataGridView1.DataSource = ds.Tables[0];
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
            public void appendLine(string text)
            {
                Action<string> actionDelegate = (x) =>
                {

                    this.rtb.AppendText(text + "\r\n");
                };
                this.rtb.Invoke(actionDelegate, text);
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
                    appendLine(DateTime.Now + "\n" + txtName + url + "开始采集！");
                    if (start == 0)
                    {
                        GetHTML_SaveProxyIp(url, xpath, txtName,start);
                    }
                    else
                    {
                        for (int i = start; i <= end; i++)
                        {

                            GetHTML_SaveProxyIp(url + i + url_plus, xpath, txtName,start);

                        }
                    }
                    appendLine(DateTime.Now + "\n" + txtName + url + "采集完毕！");
                }
                catch (Exception ex)
                {
                    appendLine(DateTime.Now + "\n" + txtName + url + "采集出错！\n" + ex.Message);
                    MessageBox.Show(ex.StackTrace, txtName + ">>" + ex.Message);
                }
            }
        }
        //public static string ClearHtmlExceptA(string html)
        //{
        //    string acceptable = "tr";
        //    string stringPattern = @"</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
        //    html = Regex.Replace(html, stringPattern, "");
        //    html = Regex.Replace(html, @"[\t\n]", "", RegexOptions.IgnoreCase);
        //    html = Regex.Replace(html, @"[\r]", "", RegexOptions.IgnoreCase);
        //    //html = Regex.Replace(html, @"[\t\n\r\s]","",RegexOptions.IgnoreCase);
        //    return html;
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
        public static string txtpath = Application.StartupPath + "\\代理ip\\";
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
        private void button1_Click(object sender, EventArgs e)
        {
            if (path == null)
            {
                MessageBox.Show("你还没有导入txt！", "友情提示");
                return;
            }
            MessageBox.Show("正在获取您的外网ip地址！", "友情提示");
            ipAddress = getLocalhostIpAddress();
            MessageBox.Show("成功获取您的外网ip地址：\n\r" + ipAddress, "友情提示");
            qr = new queueresult[maxThread];
            checkedCount = 0;
            gaoni = 0;
            touming = 0;
            fail = 0;
            queueID = 0;
            Thread worker = new Thread(delegate()
            {
                try
                {
                    for (int i = 0; i < maxThread; i++)
                    {
                        // 创建指定数量的线程
                        // 是线程调用Run方法
                        // 启动线程
                        Thread trd = new Thread(new ParameterizedThreadStart(checkProxyIp));
                        trd.Name = i.ToString();
                        trd.Start(i);
                        ShowText(trd.Name + "已经创建！\n\r");
                        trd.IsBackground = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
            worker.Start();
        }

        public int checkedCount = 0;
        public int gaoni = 0;
        public int touming = 0;
        public int fail = 0;
        public int queueID = 0;


        public void ShowText(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke((Action<String>)ShowText, str);
                return;
            }
            this.richTextBox1.AppendText(str);
        }

        public void UpdateLabel(queueresult r)
        {
            if (InvokeRequired)
            {
                this.Invoke((Action<queueresult>)UpdateLabel, r);
                return;
            }
            checkedCount += r.Checkedcount;
            fail += r.Fail;
            gaoni += r.Gaoni;
            touming += r.Touming;
            label1.Text = "验证：" + checkedCount + "||失败:" + fail + "||透明:" + touming + "||高匿:" + gaoni;
        }
        /// <summary>
        /// 获取本机的外网ip地址
        /// </summary>
        /// <returns></returns>
        public string ipAddress;
        private string getLocalhostIpAddress()
        {
            string externalIP = string.Empty;
            var regex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            using (var webclient = new WebClient())
            {
                var rawRes = webclient.DownloadString("http://ip.cn");
                externalIP = Regex.Match(rawRes, regex).Value;
            }
            return externalIP;
        }
        private void checkProxyIp(object i)
        {

            try
            {
                //bool releasedFlag = false;
                //muxConsole.WaitOne(); //阻塞队列  
                //Interlocked.Increment(ref poolFlag);//标记+1  
                //if (poolFlag != maxThread) //判断是否等于上限  
                //{
                //    muxConsole.ReleaseMutex(); //如果此线程达不到可执行线程上限,则继续开通,让后面的线程进来  
                //    releasedFlag = true;
                //}

                var queueID = (int)i;
                queueresult q = qr[queueID] = new queueresult();
                ShowText(i + "线程开始,队列id：" + queueID + "需要处理数" + qrr[queueID].Count + "\n\r");
                //while (qrr[queueID].Count != 0)
                while (qrr[queueID].Count > 0)
                {
                    var r = helper.DownLoadHtml("http://ip.cn/", qrr[queueID].Dequeue().ToString(), q, ipAddress);
                    //checkedCount = 0;
                    //fail = 0;
                    //gaoni = 0;
                    //touming = 0;
                    //lock (helper.count_locker)
                    //{
                    UpdateLabel(r);
                }

                ShowText("队列" + i + "验证：" + q.Checkedcount + "||失败:" + q.Fail + "||透明:" + q.Touming + "||高匿:" + q.Gaoni + "\n\r");
                //label1.Text = "验证：" + q.Checkedcount + "||失败:" + q.Fail + "||透明:" + q.Touming + "||高匿:" + q.Gaoni;
                //Interlocked.Decrement(ref poolFlag);
                //if (!releasedFlag) muxConsole.ReleaseMutex();
                //ShowText(Thread.CurrentThread.Name + "线程已经停止运行...\n\r");
            }

            catch (Exception ex)
            {
                ShowText("队列id：" + i + "\n\r" + ex.Message + "\n\r" + ex.StackTrace + "\n\r");
            }

        }
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
        /// <summary>
        /// 抓取西刺代理ip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
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

        Queue[] qrr;
        string[] path = null;
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "打开";
                ofd.Filter = "所有文件|*.txt";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    path = ofd.FileNames;
                }
                if (path == null)
                {
                    return;
                }
                qrr = new Queue[maxThread];
                Thread worker = new Thread(delegate()
                {
                    DateTime dt_read_ip_txt = DateTime.Now;
                    StringBuilder content = new StringBuilder();
                    for (int i = 0; i < path.Length; i++)
                    {
                        using (StreamReader sr = new StreamReader(path[i]))
                        {
                            content.Append(sr.ReadToEnd());//一次性读入内存
                        }
                    }


                    MemoryStream ms = new MemoryStream(Encoding.GetEncoding("GB2312").GetBytes(content.ToString()));//放入内存流，以便逐行读取
                    linecount = 0;
                    string line = string.Empty;
                    ipArr = new ArrayList();
                    Queue myQ = new Queue();


                    using (StreamReader sr = new StreamReader(ms))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Trim() != "")
                            {
                                myQ.Enqueue(line);
                                linecount++;
                            }
                        }
                        label2.Text = "导入计数:" + myQ.Count;

                    }
                    TimeSpan ts = DateTime.Now - dt_read_ip_txt;
                    label2.Text += "条，读取用时:" + ts.TotalMilliseconds + "ms";

                    int a = 0;
                    double oneQueCount = Math.Ceiling((myQ.Count) / (Convert.ToDouble(maxThread)));
                    for (int i = 0; i < maxThread; i++)
                    {
                        qrr[i] = new Queue();
                        for (int j = 0; j < oneQueCount; j++)
                        {
                            if (myQ.Count != 0)
                            {
                                qrr[i].Enqueue(myQ.Dequeue().ToString());

                            }
                        }
                        a += qrr[i].Count;
                    }

                    MessageBox.Show("总数:" + a + "条，已分" + qrr.Length + "小队逐个击破！", "友情提示");
                });
                worker.IsBackground = true;
                worker.Start();
                //worker.Join();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void trackBar1_ValueChanged_1(object sender, EventArgs e)
        {
            showThreadCount();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod(richTextBox1);
                gs.Url = "http://www.004388.com/ip/index_";
                gs.Url_plus = ".html";
                gs.Xpath = "//td[@class='info_list']";
                gs.Start = 2;
                gs.End = 544;
                gs.TxtName = "438ip";
                Thread worker = new Thread(delegate()
                {
                    gs.DoWork();
                });
                worker.IsBackground = true;
                worker.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string html = @"> 43.243.112.79:3128<br /> 43.243.112.86:3128<br /> 43.243.112.87:3128<br /> 43.243.112.88:3128<br /> 43.252.231.157:3128<br /> 43.254.127.189:8080<br /> 43.254.127.190:8080<br /> 45.115.172.216:8080<br /> 45.115.172.37:8080<br /> 45.115.173.168:8080<br /> 45.115.173.187:8080<br /> 45.115.175.126:8080<br /> 45.115.175.130:8080<br /> 45.115.175.179:8080<br /> 45.124.67.59:8000<br /> 45.126.253.2:8080<br /> 45.34.15.137:3128<br /> 45.55.41.80:8080<br /> 45.64.156.90:8080<br /> 45.64.80.209:8080<br /> 45.79.105.121:3128<br /> 45.79.133.35:3128<br /> 46.101.171.49:3128<br /> 46.105.169.214:3128<br /> 46.105.183.93:3128<br /> 46.105.211.32:3128<br /> 46.105.244.83:3128<br /> 46.105.244.85:3128<br /> 46.146.226.231:8080<br /> 46.16.226.10:8080<br /> 46.175.185.53:8080<br /> 46.19.231.50:8080<br /> 46.209.216.107:8080<br /> 46.209.236.138:8080<br /> 46.219.116.2:8081<br /> 46.243.66.113:8080<br /> 46.28.72.141:8080<br /> 46.41.130.135:3128<br /> 46.46.103.32:3128<br /> 46.53.2.64:8080<br /> 46.8.48.26:80<br /> 47.88.0.146:3128<br /> 47.88.1.230:3128<br /> 47.88.1.56:8080<br /> 47.88.137.107:3128<br /> 47.88.139.96:3128<br /> 47.88.14.58:3128<br /> 47.88.5.94:3128<br /> 49.0.39.241:8080<br /> 49.156.47.30:8080<br /> 49.213.12.98:3128<br /> 49.231.224.185:80<br /> 49.88.98.200:8090<br /> 5.10.103.245:8080<br /> 5.135.161.61:3128<br /> 5.135.163.225:8080<br /> 5.135.223.133:8080<br /> 5.135.223.82:8080<br /> 5.140.213.177:6000<br /> 5.150.247.73:8080<br /> 5.160.247.16:8080<br /> 5.160.247.19:8080<br /> 5.160.247.60:8080<br /> 5.175.147.177:3128<br /> 5.189.167.48:3128<br /> 5.189.243.219:8080<br /> 5.196.190.138:8080<br /> 5.196.190.139:8080<br /> 5.196.190.140:8080<br /> 5.196.190.143:8080<br /> 5.196.53.100:8080<br /> 5.196.53.103:8080<br /> 5.196.99.243:3128<br /> 5.2.225.110:3128<br /> 5.220.33.9:8080<br /> 5.255.80.121:3128<br /> 5.39.31.223:3128<br /> 5.39.59.95:3128<br /> 5.53.16.183:8080<br /> 5.56.12.10:8080<br /> 5.56.12.25:8080<br /> 5.56.12.9:8080<br /> 5.62.128.165:8080<br /> 51.254.134.204:3128<br /> 51.255.115.246:3128<br /> 52.1.73.190:3128<br />";
            string externalIP = string.Empty;
            var regex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}\b";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var rawRes = html;
            Match m = r.Match(html);
            while (m.Success)
            {
                MessageBox.Show(m.Value);
                m = m.NextMatch();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod(richTextBox1);
                gs.Url = "http://ip004.com/proxycate_";
                gs.Url_plus = ".html";
                //gs.Xpath = "//*[@id='proxytable']";//starts-with('XML','X')//input[@type='text'] 
                gs.Xpath = "//*[@id='21_ip']";
                gs.Start = 1;
                gs.End = 2669;
                gs.TxtName = "ip004";
                Thread worker = new Thread(delegate()
                {
                    gs.DoWork();
                });
                worker.IsBackground = true;
                worker.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //sqlCommandBuilder bu = new sqlcommandbuilder(dataAdapter);
            //dataAdap.update(dataset, "Table");
            SQLiteCommandBuilder bu = new SQLiteCommandBuilder(da);
            da.Update(ds, "Table");
            MessageBox.Show("保存成功！");
            showDataGV();
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SetDataGridViewRowXh(e, dataGridView1);
        }
        private void SetDataGridViewRowXh(DataGridViewRowPostPaintEventArgs e, DataGridView dataGridView)
        {
            SolidBrush solidBrush = new SolidBrush(dataGridView.RowHeadersDefaultCellStyle.ForeColor);
            int xh = e.RowIndex + 1;
            e.Graphics.DrawString(xh.ToString(CultureInfo.CurrentUICulture), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 5, e.RowBounds.Location.Y + 4);

        }
    }
}


