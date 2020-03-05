using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyVariable;
using Image = System.Windows.Controls.Image;

namespace Clipbrd_Plus
{
    /// <summary>
    /// history.xaml 的交互逻辑
    /// </summary>
    public partial class history : Window
    {
        public ObservableCollection<HistoryInfo> HistoryList = new ObservableCollection<HistoryInfo>()
        {
            // new HistoryInfo("1","TEXT","3","4","0","TEST"),
        };
        public int ViewTextNumber = ClipBoardSet.Default.ViewTextNumber;
        public history()
        {
            InitializeComponent();

            this.HistoryListView.ItemsSource = HistoryList;

            if (ClipBoardSet.Default.HistoryNumber > 0)
            {
                LoadData("SELECT * FROM history order by id desc limit " + ClipBoardSet.Default.HistoryNumber.ToString());
            }
            else
            {
                LoadData("SELECT * FROM history order by id desc limit 32");
            }


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //设置窗口样式
            if (MyVariable.Variable.TempLoadedHistory == true)
            {
                this.HistoryBar.Visibility = Visibility.Hidden;
                Thickness MyThickness = new Thickness
                {
                    Top = 10
                };

                this.BodyGrid.Margin = MyThickness;
            }

            MyVariable.Variable.HistoryLoaded = true;
        }


        //程序关闭时的操作
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MyVariable.Variable.TempLoadedHistory = false;
            MyVariable.Variable.HistoryLoaded = false;
        }


        //关闭窗口按钮被单击
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //允许窗口被拖动
        private void HistoryBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        //在临时打开历史记录窗口后鼠标移开则关闭窗口

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            PublicFunction.FindCloseWindow("history");
        }


        //历史记录列表中载入记录
        public void LoadData(string sqltext)
        {

            ExecuteQuery(sqltext);
            HistoryListView.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
        }


        //查询数据库
        public void ExecuteQuery(string sql)
        {
            if (HistoryListView.Items.Count > 0)
            {
                this.HistoryListView.ItemsSource = null;
                HistoryList.Clear();
            }
            this.HistoryListView.ItemsSource = HistoryList;

            PublicFunction.Open(PublicFunction.SqLiteConnection);
            using (var tr = PublicFunction.SqLiteConnection.BeginTransaction())
            {
                using (var command = PublicFunction.SqLiteConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    //执行查询语句返回SQLiteDataReader对象
                    var reader = command.ExecuteReader();


                    //reader.Read()方法会从读出一行匹配的数据到reader中。注意:是一行数据。
                    while (reader.Read())
                    {
                        // 有一系列的Get方法，方法的参数是列数。意思是获取第n列的数据，转成Type返回。
                        // 比如这里的语句，意思就是：获取第0列的数据，转成int值返回。
                        // var temp = reader.GetInt32(0);

                        string id = reader.GetInt32(0).ToString();
                        string type = reader.GetString(1);
                        dynamic tempBytes;
                        try
                        {
                            tempBytes = reader.GetString(2);
                        }
                        catch (Exception e)
                        {
                            tempBytes = "";
                        }

                        string data;
                        switch (type)
                        {
                            case "TEXT":
                                data = PublicFunction.SqlRebuild(tempBytes);
                                if (data.Length > ViewTextNumber)
                                {
                                    data = data.Substring(0, ViewTextNumber);
                                }

                                break;
                            case "IMG":
                                data = PublicFunction.SqlRebuild(tempBytes);
                                break;
                            case "FILE":
                                data = PublicFunction.SqlRebuild(tempBytes);
                                break;
                            default:
                                data = "Byte|" + tempBytes.GetType().ToString();
                                break;
                        }

                        string time = Convert.ToDateTime(reader.GetString(3)).ToString();

                        // dynamic temp = reader.GetInt32(4);
                        //  MessageBox.Show(temp.ToString());
                        string locked = reader.GetString(4);
                        string note = reader.GetString(5);


                        HistoryList.Add(new HistoryInfo(id, type, data, time, locked, note));


                        // MessageBox.Show(note);
                        // ListView myListView = new ListView();

                    }


                }

                tr.Commit();

            }
        }

        //自定义历史记录数据类型.
        public class HistoryInfo
        {
            public string Note { get; set; }

            public string Locked { get; set; }

            public string Time { get; set; }

            public string Data { get; set; }

            public string Type { get; set; }

            public string Id { get; set; }

            public HistoryInfo(string id, string type, string data, string time, string Lock, string note)
            {
                Id = id;
                Type = type;
                Data = data;
                Time = time;
                Locked = Lock;
                Note = note;
            }


        }


        private void HistoryListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyVariable.Variable.SetClip = true;
            int index = HistoryListView.SelectedIndex;

            HistoryInfo historyInfo = HistoryListView.SelectedItem as HistoryInfo;

            if (historyInfo != null && historyInfo is HistoryInfo)
            {
                string type = historyInfo.Type;

                switch (type)
                {
                    case "TEXT":


                        Clipboard.SetText(historyInfo.Data);

                        //  NewTextWindow(historyInfo.Data);
                        break;
                    case "IMG":


                        Clipboard.SetImage(GetBitmapSource(historyInfo.Data));
                        break;
                }



            }
            MyVariable.Variable.SetClip = false;
        }


        private void NewTextWindow(string str)
        {
            //创建基本窗口
            Window newWindow = new Window();

            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.WindowStyle = WindowStyle.ThreeDBorderWindow;
            newWindow.ResizeMode = ResizeMode.CanResize;
            //newWindow.AllowsTransparency=true;
            //创建组件容器

            TextBox textBox1 = new TextBox();
            textBox1.Text = str;
            textBox1.TextWrapping = TextWrapping.WrapWithOverflow;
            textBox1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox1.AcceptsReturn = true;
            textBox1.Margin = new Thickness(0);
            textBox1.VerticalAlignment = VerticalAlignment.Top;
            IAddChild conAddChild = newWindow;
            conAddChild.AddChild(textBox1);

            newWindow.Show();
            newWindow.Topmost = true;
        }



        public Image image;
        public Window TempWindow;
        private void NewImgWindow(string path)
        {
            TempWindow = new Window();
            TempWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            TempWindow.WindowStyle = WindowStyle.None;
            TempWindow.ResizeMode = ResizeMode.CanResize;
            TempWindow.AllowsTransparency = true;

            // Image image = new Image();
            image = new Image();
            ImageSource imageSource = new BitmapImage(new Uri(path));
            image.Source = imageSource;
            image.Margin = new Thickness(0);
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.VerticalAlignment = VerticalAlignment.Top;
            image.MouseDown += Image_MouseDown;
            IAddChild conAddChild = TempWindow;
            conAddChild.AddChild(image);
            TempWindow.Height = imageSource.Height;
            TempWindow.Width = imageSource.Width;
            TempWindow.Show();
            TempWindow.Topmost = true;

        }


        private BitmapSource GetBitmapSource(string path)
        {


            Bitmap bitmap = new Bitmap(path);

            IntPtr intPtr = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                return null;
            }





        }


        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                TempWindow.DragMove();
            }
            else
            {
                TempWindow.Close();
            }


        }

        private void HistoryListView_OnMouseMove(object sender, MouseEventArgs e)
        {
            HistoryListView.ToolTip = HistoryListView.SelectedItem;

        }




        //筛选历史记录类型
        private void TypeChoice_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int index = TypeChoice.SelectedIndex;

            string limitSql = " order by id desc limit " + ClipBoardSet.Default.HistoryNumber.ToString();//限制查询数量

            switch (index)
            {
                case 0:


                    LoadData("SELECT * FROM history " + limitSql);
                    break;
                case 1:

                    LoadData("SELECT * FROM history WHERE type='TEXT'" + limitSql);
                    break;
                case 2:
                    LoadData("SELECT * FROM history WHERE type='IMG'" + limitSql);
                    break;
            }

        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            string sqlText = string.Format("SELECT * FROM history WHERE  data LIKE '%{0}%' And type='TEXT'",
                KeyWordBox.Text);

            LoadData(sqlText);
        }
    }
}
