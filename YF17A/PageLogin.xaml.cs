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
using YF17A.AccessDatabase;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageLogin : Page,IObserverResult
    {
        public const String TAG = "PageLogin.xaml";
        public const string LOGIN = "login";
        public const string LOGOUT = "logout";
        public const string SHOWKEYBOARD = "showKeyBoard";
        public const string KILLKEYBOARD = "killKeyBoard";
        //public cons

        public PageLogin()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);

            LoginState();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);          
        }

        //override IObserverResult
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);          
           
            String action = senderValue.ToString();
            if (PageLogin.LOGIN.Equals(action))
            {
                this.tb_login.Text = "注销";  
            }
            else if (PageLogin.LOGOUT.Equals(action))
            {
                this.tb_login.Text = "登陆";
            }
            else if (PageLogin.KILLKEYBOARD.Equals(action))
            {
                this.btn_keyboard.Visibility = Visibility.Hidden;
                exitKeyboard();
                return;
            }
            else if (PageLogin.SHOWKEYBOARD.Equals(action))
            {
                this.btn_keyboard.Visibility = Visibility.Visible ;
                return;
            }

            LoginState();
        }

        private void LoginState()
        {
            User.Info usr = User.GetInstance().GetCurrentUserInfo();
            this.user.Text = usr.Account;
            this.role.Text = usr.Role;
        }

        private void login_MouseUp(object sender, RoutedEventArgs e)
        {
            User user = User.GetInstance();
            if (user.GetCurrentUserInfo().IsLogin)
            {
                user.Logout();
            }
                        
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
            PageDataExchange.getInstance().putExtra(PageUserRegister.TAG, bundle);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);         
        }
        
        private void exitKeyboard()
        {
            try
            {
                System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("osk"); 
                foreach (System.Diagnostics.Process p in process) 
                { 
                    p.Kill(); 
                } 
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }         
        }

        private void keyboardClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            Info.FileName = "osk.exe";            
            try
            {
                System.Diagnostics.Process Proc = System.Diagnostics.Process.Start(Info);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Console.WriteLine("系统找不到指定的程序文件。\r{0}", ex);
            }
        }
    }
}
