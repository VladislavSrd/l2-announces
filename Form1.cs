using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using L2_announce;
using System.Net;
using System.IO;

namespace L2_announce
{
    public partial class Form1 : Form
    {
        List<ServerInfo> Servers;
        HashSet<string> ServersExists;
        CookieContainer AuthCookie;
        public Form1()
        {
            InitializeComponent();
            Servers = new List<ServerInfo>();
            ServersExists = new HashSet<string>();
            AuthCookie = new CookieContainer();
            ServicePointManager.DefaultConnectionLimit = 15;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ParseSites();
        }

        void ParseSites()
        {
            listView1.Items.Clear();
            Auth(ref AuthCookie);
            GetDublicatesList();
            if (la2annonsruToolStripMenuItem.Checked)
                ParseServerOne();
            if (mmoanonstopruToolStripMenuItem.Checked)
                ParseServerTwo();
            if (la2oopscomToolStripMenuItem.Checked)
                ParseServerThree();
            
        }

        private void GetDublicatesList()
        {
            ServersExists.Clear();
            var getHtmlWeb = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
                UseCookies = true,
            };
            getHtmlWeb.PreRequest = delegate(HttpWebRequest webRequest)
            {
                webRequest.Timeout = 15000;
                webRequest.Proxy = null;
                webRequest.CookieContainer = AuthCookie;
                return true;
            };

            var doc = getHtmlWeb.Load(@"http://l2open-top.ru/admin.php?mod=editnews&action=list&search_field=&search_author=&search_cat=&fromnewsdate=&tonewsdate=&news_status=0&news_per_page=500&search_order_f=desc&search_order_m=asc&search_order_d=desc&search_order_t=&start_from=0");
            var table = doc.DocumentNode.SelectNodes("//td[@class='list']//a[@title='Редактировать данную новость']");
            if (table != null)
                foreach (var child in table)
                {
                        ServersExists.Add(child.InnerText.ToLower());
                }          
        }
          bool IsDublicate(string ServerName)
        {
            return ServersExists.Contains(ServerName.ToLower());
        }

        void ParseServerOne()
        {
            ServerInfo Server;
            var getHtmlWeb = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.GetEncoding("windows-1251"),
            };
            getHtmlWeb.PreRequest = delegate(HttpWebRequest webRequest)
            {
                webRequest.Timeout = 15000;
                webRequest.Proxy = null;
                return true;
            };
            var doc = getHtmlWeb.Load(@"http://la2-anons.ru/");
            HtmlNodeCollection table = doc.DocumentNode.SelectNodes("//div[contains(@class,'entry')]//table");

            if (table != null)
                foreach (var child in table)
                {
                    Server = new ServerInfo();
                    string[] part = child.InnerText.Replace("&nbsp;", "")
                    .Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    Server.Name = part[1].Trim(' ');
                    string isVip = (child.SelectSingleNode((".//img/@src")) != null) ? "VIP" : "";
                    listView1.Items.Add(GetListViewItem(Server.Name, isVip, listView1.Groups[0]));
                    Server.Rates = part[2].Trim(' ');
                    string date;
                     date = part[3].Substring(0, part[3].Length - 7).Trim(' ');
                    Server.Date = GetDateForAnnounce(date);
                    Server.Url = child.SelectSingleNode(".//tr//td//a[@href]").Attributes["href"].Value;
                    Server.Chroniki = part[4].Trim(' ');
                    Servers.Add(Server);
                }
        }

        void ParseServerTwo()
        {
            ServerInfo Server;
            var getHtmlWeb = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            var doc = getHtmlWeb.Load(@"http://mmoanons-top.ru/");
            var divs = doc.GetElementbyId("dle-content").SelectNodes("//div[contains(@class,'mmoanons_top_short_main_blockVIP')]");

            if (divs != null)
                foreach (var child in divs)
                {
                    Server = new ServerInfo();
                    Server.Name = child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_block_titleVIP-')]")).InnerText;
                    listView1.Items.Add(GetListViewItem(Server.Name, "VIP", listView1.Groups[1]));
                    Server.Chroniki = child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_block_categoryVIP-')]")).InnerText;
                    Server.Rates = child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_block_rateVIP-')]")).InnerText;
                    Server.Date = GetDateForAnnounce(child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_block_dateVIP-')]")).InnerText);
                    var desc = child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_fancybox')]")).InnerText.Split(Environment.NewLine.ToCharArray()).Skip(2).ToArray();
                    Server.Description = string.Join(Environment.NewLine, desc).Replace("&nbsp;", "");
                    Server.Url = child.SelectSingleNode((".//div[contains(@class,'mmoanons_top_block_link')]//a[@href]")).Attributes["href"].Value;
                    Servers.Add(Server);
                }
        }

        void ParseServerThree()
        {
            ServerInfo Server;
            var getHtmlWeb = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            var doc = getHtmlWeb.Load(@"http://l2oops.com/");
            var divs = doc.DocumentNode.SelectSingleNode("//div[@class ='announce']").SelectNodes(".//div[@class ='item' and @id='id1']").Take(100);

            if (divs != null)
                foreach (var child in divs)
                {
                    Server = new ServerInfo();
                    Server.Name = child.SelectSingleNode((".//div[@class = 'name']")).InnerText;
                    string isVip = (child.SelectSingleNode((".//div[@class = 'vip']")) != null) ? "VIP" : "";
                    listView1.Items.Add(GetListViewItem(Server.Name, isVip, listView1.Groups[2]));

                    Server.Rates = child.SelectSingleNode((".//div[@class = 'rates']")).InnerText;
                    Server.Chroniki = child.SelectSingleNode((".//div[@class = 'chronicle']")).InnerText;

                    Server.Date = GetDateForAnnounce(child.SelectSingleNode((".//div[@class = 'open-date']")).InnerText);
                    var desc = child.SelectSingleNode((".//div[contains(@class,'short-info')]")).InnerText.Remove(0,28).Split(Environment.NewLine.ToCharArray()).Skip(3).ToArray();
                    
                    Server.Description = string.Join(Environment.NewLine, desc).Replace("&nbsp;", "");
                    if (Server.Description == "                        ") Server.Description = ""; // лишние символы были, хак
                    Server.Url = "http://" + Server.Name;
                    Servers.Add(Server);
                }
        }
        ListViewItem GetListViewItem(string ServerName, string IsVIP, ListViewGroup group)
        {
            var item = new ListViewItem(new string[] { ServerName, IsVIP }, group);
            item.UseItemStyleForSubItems = false;
            if (IsDublicate(ServerName))
            {
                item.SubItems[0].ForeColor = Color.OrangeRed;
            }
            else
                item.SubItems[0].ForeColor = Color.MediumSeaGreen;
            item.SubItems[1].ForeColor = Color.DarkMagenta;
            return item;
        }

        string GetDateForAnnounce(string str)
        {
            if (str == "Завтра" || str == "Завтра,")
            {
                return DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            }
            else if (str == "Сегодня" || str == "Сегодня,")
                return DateTime.Now.ToString("yyyy-MM-dd");
            else if (str == "Вчера" || str == "Вчера,")
                return DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            else
                return Convert.ToDateTime(str).ToString("yyyy-MM-dd");

        }
        private string ClearSpaces(string str)
        {
            return str.Trim(' '); ;
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 1)
                if (listView1.SelectedIndices[0] > -1)
                {
                ServerAdd(Servers[listView1.SelectedIndices[0]], AuthCookie);
                addServerButton.Enabled = false;
                foreach (ListViewItem items in  this.listView1.Items)
                {
                    for (int i = 0; i < items.SubItems.Count; i++)
                    {
                        if (items.SubItems[i].Text == listView1.Items[listView1.SelectedIndices[0]].SubItems[0].Text)
                            items.SubItems[i].ForeColor = Color.OrangeRed;
                    }
                }
              }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label5.Visible = !label5.Visible;
            richTextBox1.Visible = !richTextBox1.Visible;
            if (label5.Visible)
                button3.Text = "<<";
            else
                button3.Text = ">>";
        }
        void Auth(ref CookieContainer AuthCookie)
        {
            string authString = "subaction=dologin&username=parserX&password=srd1112111121srd&selected_language=Russian&x=35&y=44";

            var cookies = new CookieCollection();

            byte[] byteArrayAuth = Encoding.UTF8.GetBytes(authString);

            var coockieHost = new Uri(@"http://l2open-top.ru/");
            var php_adress = new Uri(@"http://l2open-top.ru/admin.php");
            HttpWebRequest AuthRequest = WebRequest.Create(php_adress) as HttpWebRequest;

            SetRequestParameters(AuthRequest);
            AuthRequest.CookieContainer = AuthCookie;
            AuthRequest.ContentLength = byteArrayAuth.Length;
            Stream newStream = AuthRequest.GetRequestStream();
            newStream.Write(byteArrayAuth, 0, authString.Length);

            HttpWebResponse response = (HttpWebResponse)AuthRequest.GetResponse();
            newStream = response.GetResponseStream();


            cookies = response.Cookies;
            AuthCookie.Add(coockieHost, cookies);

            newStream.Close();
            response.Close();
        }
        private void ServerAdd(ServerInfo server, CookieContainer AuthCookie)
        {
            var _parameters = string.Format(@"title={0}&newdate={1}&new_author={2}&category[]={3}&full_story={4}&xfield[domain]={5}&xfield[rate]={6}&action=doaddnews&mod=addnews&approve=1&allow_main=1&allow_comm=1&allow_rating=1&allow_br=1&xfield[vip]=0&expires_action=0&group_extra[2]=0&group_extra[3]=0&group_extra[4]=0&group_extra[5]=0",
                server.Name,
                server.Date,
                "parserX",
                server.GetChronikiCategory(),
                server.Description,
                server.Url,
                server.Rates
                );

            var cookies = new CookieCollection();

            byte[] byteArrayAddServer = Encoding.UTF8.GetBytes(_parameters);

            var php_adress = new Uri(@"http://l2open-top.ru/admin.php");

            HttpWebRequest myReq = WebRequest.Create(php_adress) as HttpWebRequest;
            myReq.KeepAlive = false;
            SetRequestParameters(myReq);
            myReq.Referer = "http://l2open-top.ru/admin.php?mod=addnews&action=addnews";
            myReq.CookieContainer = AuthCookie;
            myReq.ContentLength = byteArrayAddServer.Length;

            Stream dataStream = myReq.GetRequestStream();
            dataStream.Write(byteArrayAddServer, 0, byteArrayAddServer.Length);
            dataStream.Close();
        }
        private void SetRequestParameters(HttpWebRequest myReq)
        {
            myReq.Method = "POST";
            myReq.Proxy = null;
            myReq.UserAgent = " Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Headers.Add("Accept-Language", "ru");
            myReq.Headers.Add("Cache-Control", "max-age=0");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 1)
                if (listView1.SelectedIndices[0] > -1)
                {
                    textBox1.Text = Servers[listView1.SelectedIndices[0]].Name;
                    textBox2.Text = Servers[listView1.SelectedIndices[0]].Rates;
                    textBox3.Text = Servers[listView1.SelectedIndices[0]].Date;
                    textBox4.Text = Servers[listView1.SelectedIndices[0]].Url;
                    textBox5.Text = Servers[listView1.SelectedIndices[0]].Chroniki;
                    if (Servers[listView1.SelectedIndices[0]].Description != "")
                    {
                        richTextBox1.Text = Servers[listView1.SelectedIndices[0]].Description;
                        label5.Visible = true;
                        richTextBox1.Visible = true;
                        button3.Text = "<<";

                    }
                    else
                    {
                        button3.Text = ">>";
                        label5.Visible = false;
                        richTextBox1.Visible = false;
                    }
                }
            addServerButton.Enabled = true;
        }
    }
}
