using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Media;
using MaterialDesignThemes.Wpf;
using System.Runtime.InteropServices;
using System.Threading;
using NHotkey.Wpf;
using WK.Libraries.SharpClipboardNS;
using MyVariable;

namespace Clipbrd_Plus
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>

    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            Clipboard.Clear();//清空剪切板
            InitializeComponent();

            //防止程序多开
            bool isRun = false;
            Mutex mutex = new Mutex(true, "Clipbrd Plus.exe", out isRun);
            if (!isRun)
            {
                MessageBox.Show("The program has been started!", "Tips", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Environment.Exit(1);
            }


            ReadConfig(); //读取配置
            ReadHotKeyReg(); //注册快捷键
            Dbexist(); //判断数据库是否存在,不存在则写出数据库

            myClipboard.ClipboardChanged += MyClipboard_ClipboardChanged;//添加剪切板监视

        }

        //将数据类型HistoryList绑定到Listview
        public ObservableCollection<HistoryInfo> HistoryList = new ObservableCollection<HistoryInfo>()
        {

        };

        //检测到剪切板变化后的操作
        private void MyClipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            //播放提示音

            try
            {
                SoundPlayer spPlayer = new SoundPlayer(ClipBoardSet.Default.VoicePath);
                spPlayer.Play();
                spPlayer.Dispose();
            }
            catch (Exception exception)
            {

            }




            if (MyVariable.Variable.SetClip != true)
            {




                //实例化数据库操作命令
                SQLiteCommand cmdCommand = PublicFunction.SqLiteConnection.CreateCommand();

                //取系统时间
                DateTime datestart = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
                TimeSpan interval = DateTime.Now - datestart;

                string id, type, time, data, sql, note;

                //生成ID
                id = interval.TotalSeconds.ToString().Replace(".", "").Substring(0, 8);//取秒时间差作为索引ID

                //取现行时间
                time = DateTime.Now.ToString();


                switch (e.ContentType)
                {
                    case SharpClipboard.ContentTypes.Text:

                        PublicFunction.Open(PublicFunction.SqLiteConnection);
                        sql = string.Format("SELECT COUNT(*) FROM history WHERE id={0}", id);
                        cmdCommand.CommandText = sql;
                        var reader = cmdCommand.ExecuteScalar();

                        if (Convert.ToUInt32(reader) != 1)
                        {
                            type = "TEXT";
                            data = PublicFunction.SqlClean(myClipboard.ClipboardText);
                            note = e.SourceApplication.Title + "\n" + data.Length + "\n" + time;
                            sql = string.Format(
                                "INSERT INTO history (id,type,data,date,lock,note) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                                id, type, data, time, 0, note);
                            cmdCommand.CommandText = sql;
                            cmdCommand.ExecuteNonQuery();
                        }

                        break;

                    case SharpClipboard.ContentTypes.Image:
                        PublicFunction.Open(PublicFunction.SqLiteConnection);
                        type = "IMG";
                        BitmapSource tempImage = Clipboard.GetImage();
                        string temppath = string.Format(PublicFunction.ImgPath + "{0}.jpg", id);
                        PublicFunction.SaveImageToJpeg(tempImage, 100, temppath);
                        data = temppath;
                        note = e.SourceApplication.Title + "\n" + tempImage.PixelWidth.ToString() + "x" + tempImage.PixelHeight.ToString() + "\n" + time;
                        sql = string.Format("INSERT INTO history (id,type,data,date,lock,note) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')", id, type, data, time, 0, note);
                        cmdCommand.CommandText = sql;
                        cmdCommand.ExecuteNonQuery();

                        break;

                    case SharpClipboard.ContentTypes.Files:
                        PublicFunction.Open(PublicFunction.SqLiteConnection);
                        type = "FILE";
                        data = myClipboard.ClipboardFile;
                        note = e.SourceApplication.Title + "\n" + time;
                        sql = string.Format("INSERT INTO history (id,type,data,date,lock,note) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')", id, type, data, time, 0, note);
                        cmdCommand.CommandText = sql;
                        cmdCommand.ExecuteNonQuery();

                        break;

                }





            }



        }


        //实例化剪切板监视器
        SharpClipboard myClipboard = new SharpClipboard();




        //数据库文件及相应文件夹是否存在,不存在则创建
        private void Dbexist()
        {
            if (!File.Exists(PublicFunction.DbPath))
            {
                string tempPath = AppDomain.CurrentDomain.BaseDirectory + @"\history\db\";
                string tempPathimg = AppDomain.CurrentDomain.BaseDirectory + @"\history\picture\";
                if (Directory.Exists(tempPath) == false)
                {
                    Directory.CreateDirectory(tempPath);
                    Directory.CreateDirectory(tempPathimg);
                }

                byte[] dbBytes = global::Clipbrd_Plus.Properties.Resources.history;
                FileStream fileStream = new FileStream(PublicFunction.DbPath, FileMode.Create);
                fileStream.Write(dbBytes, 0, dbBytes.Length);
                fileStream.Close();
            }
        }

        //注册快捷键
        private void ReadHotKeyReg()
        {

            //注册打开显示设置页面
            PublicFunction.RegHotKey(Properties.Settings.Default.HKShowSet, "CBP_ShowSet");

            //注册打开历史记录快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKShowHistory, "CBP_ShowHistory");

            //注册显示上一条的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKLastOne, "CBP_ShowLastOne");

            //注册显示上一条并粘贴的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKLastOnePaste, "CBP_ShowLastOnePaste");

            //注册显示下一条的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKNextOne, "CBP_ShowNextOne");

            //注册显示下一条并粘贴的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKNextOnePaste, "CBP_ShowLastOnePaste");

            //注册贴图到窗口的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKPasteToWindow, "CBP_PasteToWindow");

            //注册显示隐藏贴图的快捷键
            PublicFunction.RegHotKey(Properties.Settings.Default.HKShowHide, "CBP_ShowHide");
        }

        //读取配置
        private void ReadConfig()
        {


            // bool StaringRun = Convert.ToBoolean(ConfigurationManager.AppSettings["StaringRun"]);
            bool firstRun = ClipBoardSet.Default.FirstRun;



            if (firstRun == true)
            {
                Window settingWindow = new MainWindow();
                settingWindow.Show();
                MyVariable.Variable.SettingLoaded = true;
                ClipBoardSet.Default.FirstRun = false;
                ClipBoardSet.Default.Save();
            }
            else
            {
                this.Hide();
            }


            int index = ClipBoardSet.Default.LanguageIndex;//取语言索引
            PublicFunction.LanguageShow(index);//设置语言
            InitializationIcon(); //初始化托盘图标


        }

        //初始化托盘图标及右键菜单
        public Forms.NotifyIcon NotifyIcon;

        //初始化托盘图标
        private void InitializationIcon()
        {
            //this.NotifyIcon = new Forms.NotifyIcon();
            this.NotifyIcon = new Forms.NotifyIcon();
            this.NotifyIcon.BalloonTipText = TbNotifyIconInfo.Text; //启动时提示用户已置托盘
            this.NotifyIcon.ShowBalloonTip(2000); //提示隐藏时间
            this.NotifyIcon.Text = TbNotifyIconInfo.Text; //鼠标悬停提示
            this.NotifyIcon.Icon = new Drawing.Icon(@"icon/icon.ico"); //设置程序托盘图标
            this.NotifyIcon.Visible = true; //显示程序托盘图标

            //双击图标弹出历史记录页面
            // NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick1;


            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            //   object NotifyIconSet = FindResource("NotifyIconSet");
            System.Windows.Forms.MenuItem notifyMenuItem1 = new Forms.MenuItem(TbNotifyIconSet.Text);
            notifyMenuItem1.Click += NotifyMenuItem_Click; //托盘图标单击弹出设置页面

            // object NotifyIconHistory = FindResource("NotifyIconHistory");
            System.Windows.Forms.MenuItem notifyMenuItem2 = new Forms.MenuItem(TbNotifyIconHistory.Text);
            notifyMenuItem2.Click += NotifyMenuItem2_Click; //托盘图标单击弹出历史记录页面

            // object NotifyIconExit = FindResource("NotifyIconExit");
            System.Windows.Forms.MenuItem notifyMenuItem3 = new Forms.MenuItem(TbNotifyIconExit.Text);
            notifyMenuItem3.Click += NotifyMenuItem3_Click; //托盘图标单击退出

            System.Windows.Forms.MenuItem[] notifyItems = new Forms.MenuItem[]
                {notifyMenuItem1, notifyMenuItem2, notifyMenuItem3}; //创建右键菜单目录集合
            this.NotifyIcon.ContextMenu = new Forms.ContextMenu(notifyItems); //绑定右键菜单到控件


        }

        //自定义历史记录数据类型
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

        //关闭程序
        private void NotifyMenuItem3_Click(object sender, EventArgs e)
        {
            MyVariable.Variable.SettingLoaded = false;
            this.NotifyIcon.Visible = false;//清除程序的托盘图标
            MyVariable.Variable.SettingLoaded = false;
            this.NotifyIcon.Icon.Dispose();//释放图标资源
            Application.Current.Shutdown();
        }

        //弹出历史记录页面
        private void NotifyMenuItem2_Click(object sender, EventArgs e)
        {

            PublicFunction.ShowHistoryCenter();
        }

        //弹出设置页面
        private void NotifyMenuItem_Click(object sender, EventArgs e)
        {
            PublicFunction.ShowSetting();
        }


        //单击弹出历史记录页面
        private void NotifyIcon_MouseClick(object sender, Forms.MouseEventArgs e)
        {

            //如果历史记录窗口已经打开，则关闭，否则判断是不是需要新建窗口
            if (MyVariable.Variable.TempLoadedHistory == true)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.Title is "history")
                    {
                        //window.ShowInTaskbar = false;
                        window.Close();
                    }

                }
            }
            else
            {
                //如果按下的是左键，则执行显示代码
                if (e.Button == Forms.MouseButtons.Left)
                {
                    // Point MousePoint = new Point(Forms.Control.MousePosition.X, Forms.Control.MousePosition.Y);//取鼠标坐标位置

                    // int TaskBarHeight=(Forms.Screen.PrimaryScreen.Bounds.Height- Forms.SystemInformation.WorkingArea.Height );//取任务栏高度
                    int x = Forms.Control.MousePosition.X;
                    int y = Forms.SystemInformation.WorkingArea.Height;//取工作区（除开任务栏之外的屏幕区域），让程序界面刚好显示在任务栏上方！

                    // MessageBox.Show(TaskBarHeight.ToString());
                    MyVariable.Variable.TempLoadedHistory = true;
                    PublicFunction.ShowHistory(x, y);

                }

            }

        }




        //程序关闭
        private void StartWindow_OnClosing(object sender, CancelEventArgs e)
        {
            NotifyIcon.Visible = false;

        }



    }

}


