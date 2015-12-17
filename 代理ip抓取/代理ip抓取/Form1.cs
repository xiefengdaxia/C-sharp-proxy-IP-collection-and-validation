using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private int maxThread = 50;//默认可执行线程最大数量
        private static System.Threading.Mutex muxConsole = new System.Threading.Mutex();
        private void Form1_Load(object sender, EventArgs e)
        {
            showThreadCount();
        }
        private void showThreadCount()
        {
            label3.Text = "线程数:" + trackBar1.Value;
            maxThread = trackBar1.Value;
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
            public void GetHTML_SaveProxyIp(string url, string xpath, string txtName)
            {

                try
                {
                    //string Cookie = "_free_proxy_session=BAh7B0kiD3Nlc3Npb25faWQGOgZFVEkiJThiZjU2NDZlZjljOWIxY2ExODA4NTEwYTM1ZjVlMTlmBjsAVEkiEF9jc3JmX3Rva2VuBjsARkkiMThxV1Nza2o0QVNBZG1YdHRVYzRibm5OQkQwckt3V3FwL04wL3RNUDJMU1E9BjsARg%3D%3D--784d7667471701a7b9941447ccef4f4f36935fae; CNZZDATA4793016=cnzz_eid%3D1324463153-1448595402-%26ntime%3D1448595402";
                    string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 Safari/537.36 SE 2.X MetaSr 1.0";
                    string html = helper.GetResponseString(helper.CreateGetHttpResponse(url, 50, userAgent, null));
                    saveIpTxt(过滤(html, xpath), txtName);
                }
                catch
                {
                    //throw ex;
                }
            }
            public void DoWork()
            {
                try
                {
                    for (int i = start; i <= end; i++)
                    {

                        GetHTML_SaveProxyIp(url + i + url_plus, xpath, txtName);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.StackTrace);
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
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            StringBuilder sb = new StringBuilder();

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//*[@id='ip_list']");////*[@id="ip_list"]/tbody
            IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
            foreach (HtmlNode item in nodeList)
            {
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

        public static string 过滤(string html, string xpath)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            StringBuilder sb = new StringBuilder();

            HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
            IEnumerable<HtmlNode> nodeList = node.ChildNodes;  //获取该元素所有的父节点的集合
            foreach (HtmlNode item in nodeList)
            {
                MessageBox.Show(item.InnerText);
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
        public static void saveIpTxt(string ips, string txtName)
        {
            lock (locker)
            {
                if (File.Exists(Application.StartupPath + "\\" + txtName + ".txt"))
                {
                    StreamWriter streamWriter = File.AppendText(txtName + ".txt");
                    streamWriter.WriteLine(ips);
                    streamWriter.Close();
                }
                else
                {
                    StreamWriter streamWriter = File.CreateText(txtName + ".txt");
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
            if (path == "")
            {
                MessageBox.Show("你还没有导入txt！", "友情提示");
                return;
            }
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
                    var r = helper.DownLoadHtml("http://ip.cn/", qrr[queueID].Dequeue().ToString(), q);
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
            try
            {
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod();
                gs.Url = "http://www.xicidaili.com/nn/";
                gs.Url_plus = string.Empty;
                gs.Xpath = "//*[@id='ip_list']";
                gs.Start = 1;
                gs.End = 130;
                gs.TxtName = "代理ip";
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

        Queue[] qrr;
        string path = string.Empty;
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "打开";
                ofd.Filter = "所有文件|*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    path = ofd.FileName;
                }
                if (path == "")
                {
                    return;
                }
                qrr = new Queue[maxThread];
                Thread worker = new Thread(delegate()
                {
                    DateTime dt_read_ip_txt = DateTime.Now;
                    string content = string.Empty;
                    using (StreamReader sr = new StreamReader(path))
                    {
                        content = sr.ReadToEnd();//一次性读入内存
                    }
                    MemoryStream ms = new MemoryStream(Encoding.GetEncoding("GB2312").GetBytes(content));//放入内存流，以便逐行读取
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
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod();
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
            try
            {
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod();
                gs.Url = "http://www.kuaidaili.com/free/inha/";
                gs.Url_plus = "/";
                gs.Xpath = "//*[@id='list']/table/tbody";//starts-with('XML','X')
                gs.Start = 1;
                gs.End = 809;
                gs.TxtName = "快代理ip";
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

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                GetHTML_SaveProxyIpMethod gs = new GetHTML_SaveProxyIpMethod();
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
    }
}


