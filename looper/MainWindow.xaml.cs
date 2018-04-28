using System; // need to remove the unused items - reduce bloat.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO; // filewriter
using System.Xml; // added
using System.Diagnostics; // added
using WinForms = System.Windows.Forms;
using System.Windows.Threading;

namespace looper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // PersonalCache does not need to be an xml file. can be a pickle? or plain text? 
        string PersonalCache = @"C:\temp\Feed.xml"; // 'local' cache TODO: change to H:\...
        string ArticleSource = @"C:\temp\server\LoopArticles.xml"; // loop article server TODO: change to server location
        string SysTrayIco = @"C:\temp\loop.ico"; // TODO: change to H:\...
        string ExceptionLog = @"C:\temp\LoopLog.txt"; // TODO: change to H:\...
        Dictionary<string, List<string>> LoopArticlesSvr = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> ArticlesToAppend = new Dictionary<string, List<string>>();
        ObservableCollection<Article> LocalArticles = new ObservableCollection<Article>();
        Alerter Quack = new Alerter(); // composition
        bool articleFound; // flagged true if id on svr is non existent in local
        int UnreadCount;

        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            FirstTimeForEverything(); // first go! 
            WinForms.NotifyIcon ni = new WinForms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon(SysTrayIco);
            ni.Visible = true;
            ni.Text = "SOPA's Loop";
            ni.DoubleClick += delegate (object sender, EventArgs args)
            {
                timer.Stop();

                LoadSvrFileInMem(); // manual polling
                LoadLocFileInMem();
                CompareArticles();

                var ScreenRes = System.Windows.SystemParameters.WorkArea;
                this.Left = ScreenRes.Right - this.Width;
                this.Top = ScreenRes.Bottom - this.Height;
                this.Visibility = Visibility.Visible;
            };
            
            timer.Interval = TimeSpan.FromSeconds(12); // change as required e.g. 15 min or 30 min
            timer.Tick += CountDownExpire;
            timer.Start();
        }

        void CountDownExpire(object sender, EventArgs e)
        {
            LoadSvrFileInMem();
            LoadLocFileInMem();
            CompareArticles();
            CheckforUnviewedArticles();
        }

        private void FirstTimeForEverything()
        {
            PrepXMLStore();
            LoadSvrFileInMem();
            LoadLocFileInMem();
            CompareArticles();
            CheckforUnviewedArticles();
        }

        public void CheckforUnviewedArticles()
        {
            UnreadCount = 0; // reset
            foreach (var i in LocalArticles)
            {
                if (i.State == "New")
                {
                    UnreadCount++;
                }
            }

            if (UnreadCount > 0)
            {
                var ScreenRes = System.Windows.SystemParameters.WorkArea;
                
                Quack.Left = ScreenRes.Right - Quack.Width;
                Quack.Top = ScreenRes.Bottom - Quack.Height;
                Quack.Xunreadcount.Content = UnreadCount;
                Quack.Visibility = Visibility.Visible;
                Quack.ShowActivated = false;
            }
            else // caters for reading all in MainWindow ListBox while Alerter is behind MainWindow i.e. makes Alerter go away when 0 new
            {
                Quack.Visibility = Visibility.Hidden;
            }
        }

        public void LoadSvrFileInMem() // from the 'server'
        {
            try
            {
                // Load xml document from server
                XmlDocument xmldocSvr = new XmlDocument();
                XmlNodeList xmlnodeSvr;
                FileStream fSvr = new FileStream(ArticleSource, FileMode.Open, FileAccess.Read);

                xmldocSvr.PreserveWhitespace = true;
                xmldocSvr.Load(fSvr);
                xmlnodeSvr = xmldocSvr.GetElementsByTagName("item");

                LoopArticlesSvr.Clear(); // clear the generic dictionary

                // the following mechanism loads both files into memory & dictionary for easy comparison
                foreach (XmlNode n in xmlnodeSvr)
                {
                    LoopArticlesSvr[n["id"].InnerText] = new List<string> { };
                    LoopArticlesSvr[n["id"].InnerText].Add(n["id"].InnerText);
                    LoopArticlesSvr[n["id"].InnerText].Add(n["title"].InnerText);
                    LoopArticlesSvr[n["id"].InnerText].Add(n["description"].InnerText);
                    LoopArticlesSvr[n["id"].InnerText].Add(n["link"].InnerText);
                    LoopArticlesSvr[n["id"].InnerText].Add(n["pubDate"].InnerText);
                    LoopArticlesSvr[n["id"].InnerText].Add(n["state"].InnerText);
                }

                fSvr.Close();
            }
            catch(Exception e)
            {
                ExceptionLogAppend(e);
            }
        }

        public void LoadLocFileInMem() // THIS CAN BE REFACTORED - ONE METHOD ONLY PASS IN DICT
        {
            try
            {
                // Load xml document from 'local' i.e. H:\
                XmlDocument xmldocLocal = new XmlDocument();
                XmlNodeList xmlnodeLocal;
                FileStream fPersonal = new FileStream(PersonalCache, FileMode.Open, FileAccess.Read);
                
                xmldocLocal.PreserveWhitespace = true;
                xmldocLocal.Load(fPersonal);
                xmlnodeLocal = xmldocLocal.GetElementsByTagName("item");

                LocalArticles.Clear(); // clear the generic dictionary

                foreach (XmlNode n in xmlnodeLocal)
                {
                    LocalArticles.Add(new Article() {
                        Id=n["id"].InnerText, Title=n["title"].InnerText,
                        Description=n["description"].InnerText, Link=n["link"].InnerText,
                        PubDate=n["pubDate"].InnerText, State=n["state"].InnerText
                    });
                }

                theBox.ItemsSource = LocalArticles.Reverse(); // binder! HURRAH IT VWORKS! 
                
                fPersonal.Close();
            }
            catch (Exception e)
            {
                ExceptionLogAppend(e);
            }
        }

        public void CompareArticles()
        {
            ArticlesToAppend.Clear(); // clear the generic dictionary

            // lock n load cache
            foreach (var i in LoopArticlesSvr)
            {
                articleFound = false; // reset!

                foreach (var j in LocalArticles)
                {
                    if (j.Id == i.Key)
                    {
                        articleFound = true;
                        break;
                    }
                }
                if (articleFound != true)
                {
                    ArticlesToAppend[i.Key] = i.Value;
                }
            }
            WriteBackPersonalCache(); // next!
        }

        public void WriteBackPersonalCache(string mode="")
        {
            try
            {   // remove the last two lines
                string tempFile = System.IO.Path.GetTempFileName();
                using (var reader = new StreamReader(@"C:\temp\Feed.xml"))
                using (var writer = new StreamWriter(tempFile))
                {
                    // add items from 'ArticlesToAppend' to file
                    if (mode == "") // default method call 
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "</channel>" || line == "</rss>")
                            {
                                writer.Write("");
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }

                        foreach (var i in ArticlesToAppend)
                        {
                            writer.WriteLine("\n<item>");
                            writer.WriteLine("<id>" + i.Value[0] + "</id>");
                            writer.WriteLine("<title>" + i.Value[1] + "</title>");
                            writer.WriteLine("<description>" + i.Value[2] + "</description>");
                            writer.WriteLine("<link>" + i.Value[3] + "</link>");
                            writer.WriteLine("<pubDate>" + i.Value[4] + "</pubDate>");
                            writer.WriteLine("<state>" + i.Value[5] + "</state>");
                            writer.WriteLine("</item>");
                        }
                    }
                    
                    else if (mode == "ChangeState")
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "<item>")
                            {
                                writer.Write(""); // stop at <item> why? all will be written back! 
                                break;
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }

                        foreach (var i in LocalArticles) // inefficient !
                        {
                            writer.WriteLine("\n<item>");
                            writer.WriteLine("<id>" + i.Id + "</id>");
                            writer.WriteLine("<title>" + i.Title + "</title>");
                            writer.WriteLine("<description>" + i.Description + "</description>");
                            writer.WriteLine("<link>" + i.Link + "</link>");
                            writer.WriteLine("<pubDate>" + i.PubDate + "</pubDate>");
                            writer.WriteLine("<state>" + i.State + "</state>");
                            writer.WriteLine("</item>");
                        }
                    }

                    // closing tags
                    writer.WriteLine("</channel>");
                    writer.WriteLine("</rss>");
                }
                
                File.Delete(@"C:\temp\Feed.xml");
                File.Move(tempFile, @"C:\temp\Feed.xml");

                LoadLocFileInMem(); // reload as pre-req to display in UI
            }
            catch (Exception e)
            {
                ExceptionLogAppend(e);
            }
        }

        public void ExceptionLogAppend(Exception e) 
        {
            FileStream Elog = File.OpenWrite(ExceptionLog);
            StreamWriter writer = new StreamWriter(Elog);
            writer.Write(e);
            writer.Write("*Pookie*");
            writer.Write(e.Message);
            writer.Write("*******\n");
            writer.Close();
        }

        public void PrepXMLStore() // create file if non existent
        {
            try
            {
                if (!File.Exists(PersonalCache))
                {
                    File.Copy(@"C:\temp\server\LoopArticles.xml", @"C:\temp\Feed.xml", false); // no overwrite TODO: change to H:\...
                }
                if (!File.Exists(ExceptionLog)) // create exception log if non existent
                {
                    File.Create(@"C:\temp\LoopLog.txt"); // TODO: change to H:\...
                }
            }
            catch (Exception e)
            {
                ExceptionLogAppend(e);
            }
        }

        private void NavToURL(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            // just writeback articles to append back to the local cache and reload? try that! hack n slash mofo! 
            Hyperlink _url = (Hyperlink)sender; // Cant't modify the ArticlesToAppend... need to modify LocalArticles observablecollection. need a foreach loop that cycles through e.g. 
            string uniqueId = _url.Tag.ToString();

            foreach (var i in LocalArticles)
            {
                if (i.Id == uniqueId)
                {
                    if (i.State == "")
                    {
                        e.Handled = true;
                        return;
                    }
                    else
                    {
                        i.State = "";
                        break;
                    }
                }
            }

            WriteBackPersonalCache("ChangeState");

            theBox.Items.Refresh();

            e.Handled = true;
        }

        private void TheX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckforUnviewedArticles(); // updates alerter box that may be behind the 
            this.Visibility = Visibility.Hidden;
            timer.Start();
        }
    }
}
