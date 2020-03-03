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
    /// viewsetting.xaml 的交互逻辑
    /// </summary>
    public partial class Viewsetting : UserControl
    {
        public Viewsetting()
        {
            InitializeComponent();
            TbTextNumber.Text = ClipBoardSet.Default.ViewTextNumber.ToString();
            TbImgWidth.Text = ClipBoardSet.Default.ViewImgWidth.ToString();
            TbSoundVolume.Text = ClipBoardSet.Default.ViewSoundVolume.ToString();
        }

        private void TbTextNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ClipBoardSet.Default.ViewTextNumber =Convert.ToInt32( TbTextNumber.Text);
            ClipBoardSet.Default.Save();//保存配置
        }

        private void TbImgWidth_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            
            ClipBoardSet.Default.ViewImgWidth = Convert.ToInt32(TbImgWidth.Text);
            ClipBoardSet.Default.Save();//保存配置
        }

        private void TbSoundVolume_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ClipBoardSet.Default.ViewSoundVolume = Convert.ToInt32(TbSoundVolume.Text);
            ClipBoardSet.Default.Save();//保存配置
        }
    }
}
