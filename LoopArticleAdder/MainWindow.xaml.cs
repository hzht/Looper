using System;
using System.IO;
using System.Windows;

namespace LoopArticleAdder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            testLabel.Content = ""; // reset

            if (ArticleTitle.Text == "" || ArticleDesc.Text == "" || ArticleLink.Text == "")
            {
                testLabel.Content = "No field can be left blank!";
                return;
            }

            try
            {
                string tempFile = System.IO.Path.GetTempFileName(); 
                using (var reader = new StreamReader(@"C:\temp\server\LoopArticles.xml"))
                using (var writer = new StreamWriter(tempFile))
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

                    // MOJO
                    writer.WriteLine("\n<item>");
                    writer.WriteLine("<id>" + RandomChar.RandomString(5) + "</id>"); // random number char generator make it 5 glyphs
                    writer.WriteLine("<title>" + ArticleTitle.Text + "</title>");
                    writer.WriteLine("<description>" + ArticleDesc.Text + "</description>");
                    writer.WriteLine("<link>" + ArticleLink.Text + "</link>");
                    writer.WriteLine("<pubDate>" + String.Format("{0:g}", DateTime.Now) + "</pubDate>"); // insert date and time stamp
                    writer.WriteLine("<state>New</state>");
                    writer.WriteLine("</item>");

                    writer.WriteLine("</channel>");
                    writer.WriteLine("</rss>");
                }

                /* 
                 * // odd chance that the file may be read-only and open! alternative coding pattern is to use FileStream xxx = File.OpenWrite(exceptionLog);... StreamWriter Writer = new StreamWriter(xxx); then delete the two last tags and append from there... even there theres a chance of fucking up a readonly file.
                 */
                File.Delete(@"C:\temp\server\LoopArticles.xml"); 
                File.Move(tempFile, @"C:\temp\server\LoopArticles.xml");
            }
            catch(Exception f)
            {
                testLabel.Content = f;
            }
        }
    }
}
