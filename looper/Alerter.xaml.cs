using System.Windows;
using System.Windows.Input;

namespace looper
{
    /// <summary>
    /// Interaction logic for Alerter.xaml
    /// 
    /// </summary>
    public partial class Alerter : Window
    {
        public Alerter()
        {
            InitializeComponent();
        }

        private void closeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
