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

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageDebug.xaml
    /// </summary>
    public partial class PageDebugMain : Page, IObserverResult
    {
        public const string TAG = "PageDebugMain.xaml" ;
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;

       
        public PageDebugMain()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);

            InitStatusMap();
            mBeckHoff = Utils.GetBeckHoffInstance();
        }

        private void bucket_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName,PageDebugBucket.TAG);
        }

        private void elevator_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageDebugUp.TAG);
        }

        private void console_Click(object sender, RoutedEventArgs e)
        {
            ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageDebugConsole.TAG);
        }


        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);
            //init views
            foreach (String item in mStatusMap.Keys)
            {
                Object value;
                mBeckHoff.plcVarUserdataMap.TryGetValue(item, out value);
                UpdateView(item, value);
            }

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarParameter.ACTION_HELP.Equals(senderValue))
                {

                }             
                else if (ToolbarParameter.ACTION_SETTING.Equals(senderValue))
                {
                    Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterDown.TAG);
                }
            }
            //beckhoff changed 
            else if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                Object plcValue;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);

                if (mStatusMap.ContainsKey(plcVarName))
                {
                    UpdateView(plcVarName, plcValue);
                }
            }           
        }

        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
            Type t = element.GetType();

            if (t== typeof(Image))
            {
                Boolean status = (Boolean)value;       
                Image iv = element as Image;
                if (status) {
                    iv.Source = MainWindow.sEnableLight ;
                } else{
                    iv.Source = MainWindow.sDisableLight;
                }               
            }
            else if (t == typeof(Button))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }      
        }
        #endregion

        private void InitStatusMap()
        {
            mStatusMap.Add(".Slope_cig_speed", this.tb_Slope_cig_speed);//斜向电机烟支速度（MW1886）Slope_cig_speed	
            mStatusMap.Add(".Store_cig_speed", this.tb_Store_cig_speed);//存储电机烟支速度（MW1884）Store_cig_speed	
            mStatusMap.Add(".Packer_cig_speed", this.tb_Packer_cig_speed);//包装机烟支速度（MW1310）Packer_cig_speed

            mStatusMap.Add(".Corner_cig_speed", this.tb_Corner_cig_speed);//弯道电机烟支速度（MW1304）Corner_cig_speed	
            mStatusMap.Add(".Life_cig_speed", this.tb_Life_cig_speed);//提升电机烟支速度（MW1400）Life_cig_speed	
            mStatusMap.Add(".Transfer_cig_speed", this.tb_Transfer_cig_speed);//传送电机烟支速度（MW1402）Transfer_cig_speed

            mStatusMap.Add(".Sample_cig_speed", this.tb_Sample_cig_speed);//取样电机烟支速度（MW1302）Sample_cig_speed		
            mStatusMap.Add(".Maker_cig_speed", this.tb_Maker_cig_speed);//卷烟机烟支速度（MW1300）Maker_cig_speed
            mStatusMap.Add(".MakerExport_cig_speed", this.tb_MakerExport_cig_speed);//烟机出口电机速度：MW1404 MakerExport_cig_speed

            //mStatusMap.Add(".test_run", this.iv_test_run); //test_run	Bool	机器试运转		读/写	按钮/指示灯	M1000.6
            mStatusMap.Add(".test_maker_speed", this.tb_test_maker_speed); //test_maker_speed	Int	卷烟机试运转速度		读/写	数值显示	MW1950
            mStatusMap.Add(".test_packer_speed", this.tb_test_packer_speed); //test_packer_speed	Int	包装机试运转速度		读/写	数值显示	MW1952
            //mStatusMap.Add(".test_run_unprotected", this.iv_test_run_unprotected);  // test_run_unprotected	Bool	机器强制试运转		读/写	按钮/指示灯	M1000.7
        }

        private void btn_test_run_Click(object sender, RoutedEventArgs e)
        {            
            Button btn = sender as Button;
            String sourceControlName = btn.GetValue(Button.NameProperty).ToString();
            String plcVarName = sourceControlName.Replace("btn_", ".");
            Boolean bValue = true;
            mBeckHoff.writeBoolean(plcVarName, bValue);
        }
    }
}


