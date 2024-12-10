using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp4
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TTSUtility tTSUtility = new TTSUtility();
            tTSUtility.TTSTxt(@"D:\Downloads\Telegram Desktop\soushu2022_com@《我的爆乳巨臀专用肉便器_1_72》精校最全_作者：LIQUID82（王苗壮）_搜书吧_.txt");
        }
    }
}