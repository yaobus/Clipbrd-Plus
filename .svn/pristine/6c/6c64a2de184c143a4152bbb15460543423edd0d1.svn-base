using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clipbrd_Plus
{
    /// <summary>
    /// clipsetting.xaml 的交互逻辑
    /// </summary>
    public partial class Clipsetting : UserControl
    {
        public Clipsetting()
        {
            InitializeComponent();
            CbRecText.IsChecked = ClipBoardSet.Default.TextHistory;
            CbRecImg.IsChecked = ClipBoardSet.Default.BitmapHistory;
            CbRecFile.IsChecked = ClipBoardSet.Default.FileHistory;
        }

        private void CbRecText_OnClick(object sender, RoutedEventArgs e)
        {
            ClipBoardSet.Default.TextHistory = Convert.ToBoolean(CbRecText.IsChecked);
            ClipBoardSet.Default.Save();//保存配置
        }

        private void CbRecImg_OnClick(object sender, RoutedEventArgs e)
        {
            ClipBoardSet.Default.BitmapHistory = Convert.ToBoolean(CbRecImg.IsChecked);
            ClipBoardSet.Default.Save();//保存配置
        }

        private void CbRecFile_OnClick(object sender, RoutedEventArgs e)
        {
            ClipBoardSet.Default.FileHistory = Convert.ToBoolean(CbRecFile.IsChecked);
            ClipBoardSet.Default.Save();//保存配置
        }
    }
}
