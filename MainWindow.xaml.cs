using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfApplication2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private static MainWindow mainWin;
        //private static int windowTotalCount = 0;
        private System.Timers.Timer t;
        
        private delegate void saveInfodelegate();
        private string loginFailLog = String.Empty;
        private string username = String.Empty;
        private string password = String.Empty;

        private bool quiteMode = false;
        private bool ifLoginSuccessfuled = false;
        private bool ifHadCleared = false;
        private bool ifAutoLoginStart = false;
        // private bool ifMainWinReady = false;
        /*
         C#中的常量是默认的静态形式的；
         Java中可以加也可以不加static；
         * private static const  string mainVersion = "201705210300";
         */
        public const string setUpExeName = "NetworkLoginClient.exe";
        public const double mainVersion = 20170522.114000;

        IntPtr hwnd;

        public MainWindow()
        {
            singleClinetMode();

            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Closed += new EventHandler(MainWindow_Closed);

            uninstall();
            setup();
            clientInitialization();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //窗口加载完毕才可用，否则会报错。
            hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null) source.AddHook(WndProc);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                HwndSource.FromHwnd(hwnd).RemoveHook(WndProc);
            }
            catch (Exception)
            {

                throw;
            }

        }

        const int WM_COPYDATA = 0x004A;
        //WM_COPYDATA消息的主要目的是允许在进程间传递只读数据。
        //Windows在通过WM_COPYDATA消息传递期间，不提供继承同步方式。
        //其中,WM_COPYDATA对应的十六进制数为0x004A
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        //wpf用此方法
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                /*COPYDATASTRUCT cdata = new COPYDATASTRUCT();
                Type mytype = cdata.GetType();
                cdata = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, mytype);
                MessageBox.Show(cdata.lpData);
                */
                this.WindowState = System.Windows.WindowState.Normal;
                //this.Topmost = true;
                this.Activate();
                this.Focus();
            }
            return IntPtr.Zero;
        }

        //WinFrom用此方法
        /* protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
            case WM_COPYDATA:
                COPYDATASTRUCT cdata = new COPYDATASTRUCT();
                Type mytype = cdata.GetType();
                cdata = (COPYDATASTRUCT)m.GetLParam(mytype);
                this.textBox1.Text = cdata.lpData;
                break;
            default:
                base.DefWndProc(ref m);
                break;
            }
        } */
       /*
       public string TextBoxValue
       {
            get {
                String getterResult = "";
                App.Current.Dispatcher.Invoke((Action)delegate()
                {
                    getterResult = this.textBox1.Text;
                });
                return getterResult;
            }
            set {
                App.Current.Dispatcher.Invoke((Action)delegate()
                {
                    this.textBox1.Text = value;
                });
            }
       }
        */

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int msg, int wParam, ref COPYDATASTRUCT IParam);

        /**/
        private void singleClinetMode() {
            //if (windowTotalCount == 1) {
            //    mainWin = this;
            //}
            System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);
            foreach (System.Diagnostics.Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        if (System.Environment.GetCommandLineArgs().Length == 2 && System.Environment.GetCommandLineArgs()[1] == "-u") {
                            process.Kill();
                            return;
                        }
                        //psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        //mainWin.WindowState = 0;
                        String strSent = "我爱你！！！";
                        byte[] arr = System.Text.Encoding.Default.GetBytes(strSent);
                        int len = arr.Length;
                        COPYDATASTRUCT cdata;
                        cdata.dwData = (IntPtr)100;
                        cdata.lpData = strSent;
                        cdata.cData = len + 1;
                        SendMessage(process.MainWindowHandle, WM_COPYDATA, 0, ref cdata);
                        current.Kill();
                    }
                }
            }
        }


        /*
        protected override void DefWndProc(ref System.Windows.Message m)
        {
            if (m.Msg == 0x104)
            {
                m.Result = (IntPtr)333;
                return;
            }
            else
            {
            }
            base.DefWndProc(ref m);
        }*/
        private void clientInitialization() {

            this.checkBox1.IsChecked = (getInfo("saveme") == "True");
            this.checkBox2.IsChecked = (getInfo("autologin") == "True");
            this.checkBox3.IsChecked = (getInfo("autorun") == "True");
            try {
                this.textBox1.Text = getInfo("username");
                username = this.textBox1.Text;
            }
            catch (Exception ex) {
                //MessageBox.Show(ex.Message + "1111");
                this.loginFailLog += ex.Message + '\n';
            }
            //
            this.Visibility = System.Windows.Visibility.Visible;
            if (this.textBox1.Text == String.Empty)
                this.textBox1.Focus();
            if (this.checkBox1.IsChecked == true)
            { 
                this.passwordBox1.Password = getInfo("password");
                password = this.passwordBox1.Password;
                if (this.checkBox2.IsChecked == true) {
                    quiteMode = true;
                    //autoLogin();
                    ifAutoLoginStart = true;
                }
            }
            ////
            if (this.textBox1.Text != String.Empty && this.passwordBox1.Password == String.Empty)
                this.passwordBox1.Focus();
        }

        private void autoLogin() {
          /*
           * 改进建议：
           * 下面两条语句可以适当考虑前置
           */
           this.loginPanel.Visibility = Visibility.Hidden;
           this.logingingPanel.Visibility = Visibility.Visible;
           t = new System.Timers.Timer(4500);
           t.Enabled = true; //是否触发Elapsed事件
           t.Elapsed += new System.Timers.ElapsedEventHandler(TimerLogin);
           t.AutoReset = false; 
           t.Start();
        }

        private void TimerLogin(object sender, System.Timers.ElapsedEventArgs e)
        {
            t.Stop();
            t.Enabled = false;
            t.Dispose();
            if (login(username, password, true))
                System.Environment.Exit(0);
            /*
             * 如果登录过程未出现异常
             * 并且收到了服务器的响应
             * 但发生了由于密码错误导致的登录失败
             * 此时界面会一直停留
             * 改进措施如下
             * 
             * else
             * !!! this.button3.Click();
             * 即：
                if (this.ifLoginSuccessfuled) return;
                this.loginPanel.Visibility = Visibility.Visible;
                this.logingingPanel.Visibility = Visibility.Hidden;
                if (t != null)
                {
                    t.Stop();
                    t.Enabled = false;
                    t.Dispose();
                }
            */
        }

        private static string buildSetupDir()
        {
            string basePathBak = Directory.GetDirectoryRoot(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "Program Files (x86)\\RCCampusNLC\\";
            
            if (System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName == setUpExeName)
            {
                if (!Directory.Exists(basePathBak))
                    Directory.CreateDirectory(basePathBak);
                return basePathBak;
            }
            
            for (char i = 'D'; i <= 'Z'; i++) {
                if (Directory.Exists(i + ":\\")) {
                    string basePath = i + ":\\Program Files (x86)\\RCCampusNLC\\";
                    if (!Directory.Exists(basePath)) {
                        Directory.CreateDirectory(basePath);
                    }
                    return basePath;
                }
            }
            
            if (!Directory.Exists(basePathBak))
                Directory.CreateDirectory(basePathBak);
            return basePathBak;
        }

        // 安装
        private void setup() {
            RegistryKey NetworkLoginClient = Registry.CurrentUser.CreateSubKey("Software\\RCCampusNLC");
            if (mainVersion <= Convert.ToDouble(NetworkLoginClient.GetValue("mainVersion", 0))) return;
            NetworkLoginClient.SetValue("mainVersion", mainVersion);
            string basePath = buildSetupDir();
            string newFullPath = basePath + setUpExeName;
            // this.Hide();
            if(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName == newFullPath)
                return;
           
            try
            {
                File.Delete(newFullPath);
                //File.Copy(System.Environment.GetCommandLineArgs()[0], newFullPath);
                File.Copy(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, newFullPath);
            }
            catch(Exception ex) {
                this.loginFailLog += ex.Message + '\n';
            }

            RegistryKey clientUninstall = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\RCCampusNLC");
            clientUninstall.SetValue("DisplayIcon", newFullPath);
            clientUninstall.SetValue("DisplayName", "校园网认证");
            clientUninstall.SetValue("DisplayVersion", "1.2.3.0");
            clientUninstall.SetValue("Publisher", "SongJoe Tech");
            clientUninstall.SetValue("InstallLocation", newFullPath);
            clientUninstall.SetValue("UninstallString", newFullPath + " -u");
            clientUninstall.SetValue("EstimatedSize", 111, RegistryValueKind.DWord);
            clientUninstall.SetValue("URLInfoAbout", "http://10.128.255.249");

            string startUpPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Programs) + "\\校园网认证";
            if (!Directory.Exists(startUpPath))
                Directory.CreateDirectory(startUpPath);

            Shortcut scLogin = new Shortcut();
            scLogin.Path = newFullPath;
            scLogin.Arguments = "";//启动参数  
            scLogin.WorkingDirectory = basePath;
            scLogin.Description = "校园网认证客户端 --- 西南大学荣昌校区";
            string lnkPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\校园网登录.lnk";
            if (!File.Exists(lnkPath))
                scLogin.Save(lnkPath);
            string lnkStartUp = startUpPath + "\\校园网登录.lnk";
            if (!File.Exists(lnkStartUp))
                scLogin.Save(lnkStartUp);

            Shortcut scUnintall = new Shortcut();
            scUnintall.Path = newFullPath;
            scUnintall.Arguments = "-u";//启动参数  
            scUnintall.WorkingDirectory = basePath;
            scUnintall.Description = "卸载客户端";
            string lnkUninstallPath = basePath + "\\卸载客户端.lnk";
            if (!File.Exists(lnkUninstallPath))
                scUnintall.Save(lnkUninstallPath);
            string lnkStartUpUninstall = startUpPath + "\\卸载客户端.lnk";
            if (!File.Exists(lnkStartUpUninstall))
                scUnintall.Save(lnkStartUpUninstall);

            System.Diagnostics.Process.Start(newFullPath);
            string selfKillBat = @"@echo off
:tryagain
del %1
if exist %1 goto tryagain
del %0";
            File.WriteAllText("killme.bat", selfKillBat);//写bat文件
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "killme.bat";
            psi.Arguments = "\"" + System.Environment.GetCommandLineArgs()[0] + "\"";
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(psi);
            System.Environment.Exit(0);
        }

        private void uninstall() {
            if (System.Environment.GetCommandLineArgs().Length != 2 || System.Environment.GetCommandLineArgs()[1] != "-u" || !ifSoftwareSubKeyExist("RCCampusNLC")) return;
            string basePath = System.Environment.CurrentDirectory = Directory.GetParent(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).FullName;
            //System.Environment.CurrentDirectory
            //Directory.GetCurrentDirectory()
            try {
                /* 清理注册表 */
                RegistryKey rkSoftWare = Registry.CurrentUser.OpenSubKey("Software", true);
                //
                rkSoftWare.DeleteSubKeyTree("RCCampusNLC", false);
                RegistryKey clientUninstall = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true);
                clientUninstall.DeleteSubKeyTree("RCCampusNLC", false);
                RegistryKey rkRun = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                //取消开机自启动
                rkRun.DeleteValue("RCCampusNLC", false);

                /* 删除文件 */
                string lnkPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\校园网登录.lnk";
                File.Delete(lnkPath);
                string lnkUninstallPath = basePath + "\\卸载客户端.lnk";
                File.Delete(lnkUninstallPath);

                string startUpPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Programs) + "\\校园网认证";
                if (Directory.Exists(startUpPath))
                    Directory.Delete(startUpPath, true);
                // System.IO.Path.GetTempPath() 句末带 \
                string fileName = basePath + "\\RCCampusNLCuninstall.bat";
                StringBuilder removeBat = new StringBuilder();
                removeBat.AppendLine("@echo off");
                removeBat.AppendLine(":tryagain");
                removeBat.AppendLine("del %1");
                removeBat.AppendLine("if exist %1 goto tryagain");
                removeBat.AppendLine("cd ../");
                removeBat.AppendLine("rd /s /q RCCampusNLC");
                //removeBat.AppendFormat("del \"{0}\"",fileName);
                removeBat.Append("\nexit");

                File.WriteAllText(fileName, removeBat.ToString());//写bat文件
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = fileName;
                psi.Arguments = "\"" + System.Environment.GetCommandLineArgs()[0] + "\"";
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process.Start(psi);
                System.Environment.Exit(0);
            }
            catch {
                MessageBox.Show("卸载失败!");
                System.Environment.Exit(0);
            }
        }

        private void saveInfo() {
           if (ifHadCleared) return;
           try {
                RegistryKey rkMain = Registry.CurrentUser;
                RegistryKey rkSoftware = rkMain.OpenSubKey("Software", true);
                RegistryKey rkNetworkLoginClient = rkSoftware.CreateSubKey("RCCampusNLC");
                rkNetworkLoginClient.SetValue("username", this.textBox1.Text);
                if (this.checkBox1.IsChecked == true)
                    rkNetworkLoginClient.SetValue("password", this.passwordBox1.Password);
                rkNetworkLoginClient.SetValue("saveme", this.checkBox1.IsChecked);
               if(this.ifLoginSuccessfuled)
                    rkNetworkLoginClient.SetValue("autologin", this.checkBox2.IsChecked);
               rkNetworkLoginClient.SetValue("autorun", this.checkBox3.IsChecked);
            }
           catch (Exception ex)
           {
              // MessageBox.Show(ex.Message +"22222");
              this.loginFailLog += ex.Message + '\n';
           }


     }
        private string getInfo(string key) {
            string val = String.Empty;
            try
            {
                RegistryKey rkMain = Registry.CurrentUser;
                RegistryKey rkSoftware = rkMain.OpenSubKey("Software", true);
                RegistryKey rkNetworkLoginClient = rkSoftware.CreateSubKey("RCCampusNLC");
                object oInfo = rkNetworkLoginClient.GetValue(key);
                 if (oInfo != null)  //如果值不为空
                {
                    val = oInfo.ToString();
                }
            }
            catch (Exception ex) {
                this.loginFailLog += ex.Message + '\n';
            }
            return val;
        }

        public bool login(string username, string password, bool depute = false)
        {
            ifHadCleared = false;
            bool ifLoginSuccessful = false;
            try
            {
                /*
                 * !!!
                 * 回收垃圾
                 */
                System.GC.Collect();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.3.5/srun_portal_phone.php?ac_id=1");
                request.CookieContainer = new CookieContainer();
                CookieContainer cookie = request.CookieContainer;//如果用不到Cookie，删去即可
                //以下是发送的http头，随便加，其中referer挺重要的，有些网站会根据这个来反盗链
                request.Host = "192.168.3.5";
                request.KeepAlive = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers["Accept-Language"] = "zh-CN,zh;q=0.";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
                request.Method = "POST";

                //request.Timeout = 3000;
                Encoding encoding = Encoding.UTF8;//根据网站的编码自定义  
                byte[] postData = encoding.GetBytes("action=login&ac_id=1&user_ip=&nas_ip=&user_mac=&username=" + username + "&password=" + password + "&save_me=1");//postDataStr即为发送的数据，格式还是和上次说的一样  
                request.ContentLength = postData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postData, 0, postData.Length);
                //发送数据
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // MessageBox.Show(response.StatusCode.ToString());
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);
                string retString = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                ifLoginSuccessful = Regex.IsMatch(retString, "网络已连接");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //this.loginFailLog += ex.Message + '\n';
            }
     
            this.ifLoginSuccessfuled = ifLoginSuccessful;
            
            if (ifLoginSuccessful)
            {
                //saveInfo(); // 保存数据 + 22222222222
                if (depute)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new saveInfodelegate(saveInfo));
                else
                    saveInfo();
            }
            if (!quiteMode) {
                if (ifLoginSuccessful)
                {
                    MessageBox.Show("认证成功，程序将自动退出。", "   提示", MessageBoxButton.OK);
                    System.Environment.Exit(0);
                }
                else {
                    MessageBox.Show("登录失败！", "   提示", MessageBoxButton.OK);                
                }
            }
            // MessageBox.Show(this.loginFailLog);
            return ifLoginSuccessful;
        }
       
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //if (t != null && t.Enabled)
            //    return;
            username = this.textBox1.Text;
            password = this.passwordBox1.Password;
            quiteMode = false;
            login(username, password);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
               this.passwordBox1.SelectAll();
               this.passwordBox1.Focus();
            }
        }

        private void passwordBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
            //if (e.Key.ToString() == "Return") {
                username = this.textBox1.Text;
                password = this.passwordBox1.Password;
                quiteMode = false;
                login(username, password);
            }
        }

        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkBox1.IsChecked == false)
                this.checkBox2.IsChecked = false;
        }

        private void checkBox2_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkBox2.IsChecked == true)
                this.checkBox1.IsChecked = true;
        }

        private void checkBox3_Click(object sender, RoutedEventArgs e)
        {
            autoRun();
        }

        private void autoRun() {
            try
            {
                string path = "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\"";
                RegistryKey rkRun = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (this.checkBox3.IsChecked == true) //设置开机自启动  
                {
                    rkRun.SetValue("RCCampusNLC", path);
                }
                else //取消开机自启动  
                {
                    rkRun.DeleteValue("RCCampusNLC", false);
                }
                rkRun.Close();
            }
            catch {
                MessageBox.Show("操作失败，请检查是否被安全软件禁止。");
            }
        }

        private bool ifSoftwareSubKeyExist(string subKey)
        {
            string[] items = Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames();
            foreach(string item in items)
                if (item == subKey)
                    return true;
            return false;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (!ifSoftwareSubKeyExist("RCCampusNLC"))

            //if (Registry.CurrentUser.OpenSubKey("Software").GetSubKey("SongJoe") == null) {
                return;
            RegistryKey NetworkLoginClient = Registry.CurrentUser.OpenSubKey("Software\\RCCampusNLC", true);
            NetworkLoginClient.DeleteValue("username", false);
            NetworkLoginClient.DeleteValue("password", false);
            NetworkLoginClient.DeleteValue("saveme", false);
            NetworkLoginClient.DeleteValue("autologin", false);
            NetworkLoginClient.DeleteValue("autorun", false);

            ifHadCleared = true;
            this.textBox1.Text = String.Empty;
            this.passwordBox1.Password = String.Empty;
            this.checkBox1.IsChecked = false;
            this.checkBox2.IsChecked = false;
            this.checkBox3.IsChecked = false;
            //清理自启
            autoRun();
            this.textBox1.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           saveInfo();
           if (loginFailLog != String.Empty)
               MessageBox.Show(loginFailLog);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (this.ifLoginSuccessfuled) return;
            /*
            * 改进建议：
            * 下面两条语句可以考虑换位
            */
            this.loginPanel.Visibility = Visibility.Visible;
            this.logingingPanel.Visibility = Visibility.Hidden;
            if (t != null) {
                t.Stop();
                t.Enabled = false;
                t.Dispose();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ifAutoLoginStart)
                autoLogin();
        }
    }
}
