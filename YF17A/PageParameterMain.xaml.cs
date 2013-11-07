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
    /// Interaction logic for PageParameter.xaml
    /// </summary>
    public partial class PageParameterMain : Page,IObserverResult
    {
        public const String TAG = "PageParameterMain.xaml";

        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;
       
      
        public PageParameterMain()
        {
            InitializeComponent();   
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);
            

           InitStatusMap();
           mBeckHoff = Utils.GetBeckHoffInstance();
        }

        private void parameterdown_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterDown.TAG);
        }

        private void parameterup_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterUp.TAG);           
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
            Object resultValue;
            Object senderValue;

            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_RESULT_VALUE, out resultValue);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);


            if (ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarParameter.ACTION_HELP.Equals(senderValue))
                {

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
            //if (KeyboardControl.TAG.Equals(senderName))
            //{
            //    String sourceControlName = senderValue.ToString(); 
            //    String plcVarName = sourceControlName.Replace("btn_", ".");
            //    mBeckHoff.writeInt(plcVarName, resultValue);
            //}
        }

        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);

            Type t;
            mBeckHoff.plcVarTypeMap.TryGetValue(viewTag, out t);
            //float fValue = Convert.ToSingle(value);
            //if (typeof(int) == t && mBeckHoff.plcVarThreadHoldMap.ContainsKey(viewTag))
            //{  
            //    BeckHoff.ThresHold limit;
            //    mBeckHoff.plcVarThreadHoldMap.TryGetValue(viewTag, out limit);
            //    if (limit != null)
            //    {
            //        fValue = fValue / limit.ratio;
            //    }
            //}
            
            if (element.GetType() == typeof(Image))
            {
                Boolean status = (Boolean)value;
                Image iv = element as Image;
                BitmapImage bmTrue = MainWindow.sEnableLight;
                BitmapImage bmFalse = MainWindow.sDisableLight;
                //if(viewTag.Equals())
                //{
                //    bmTrue = MainWindow.sEnableLight ;
                //    bmTrue = MainWindow.sDisableLight ;
                //}             
                if (status) {
                    iv.Source = bmTrue;
                } else {
                    iv.Source = bmFalse;
                }               
            }
            else if (element.GetType() == typeof(TextBlock))
            {
                TextBlock tv = element as TextBlock;
                tv.Text = value.ToString();
            }
            else if (element.GetType() == typeof(Button))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }
        }
        #endregion

        private void btn_setting_click(object sender, RoutedEventArgs e)
        {
            //popup keyboard control
            Button button = sender as Button;
            Point pt1 = button.TranslatePoint(new Point(), this);

            int X0 = (int)pt1.X;
            int Y0 = (int)(pt1.Y + button.ActualHeight);

            if (X0 + keyboard.ActualWidth > this.ActualWidth)
            {
                X0 = (int)(this.ActualWidth - keyboard.ActualWidth);
            }
            if (Y0 + keyboard.ActualHeight > this.ActualHeight)
            {
                Y0 = Y0 - (int)(keyboard.ActualHeight + button.ActualHeight);
            }

            keyboard.Visibility = Visibility.Visible;
            keyboard.UserControlToolTipX = X0;
            keyboard.UserControlToolTipY = Y0;

            Dictionary<String, Object> bundle = new Dictionary<String, Object>();

            String controlName = button.GetValue(Button.NameProperty).ToString();
            String plcVarName = controlName.Replace("btn_", ".");
            BeckHoff.ThresHold limit;
            mBeckHoff.plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
            if (limit != null)
            {
                bundle.Add(PageDataExchange.KEY_THREAD_HOLD, limit);
            }
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, controlName);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, button.Content);
            keyboard.onRecieveResult(bundle);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            keyboard.Visibility = Visibility.Hidden;
        }

        private void btn_testrun_click(object sender, RoutedEventArgs e)
        {
            Boolean bValue = true;
            String plcVarName = ".test_run";
            mBeckHoff.writeBoolean(plcVarName, bValue);
        }

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

            
          //  mStatusMap.Add(".test_run", this.iv_test_run); //test_run	Bool	机器试运转		读/写	按钮/指示灯	M1000.6
            mStatusMap.Add(".test_maker_speed", this.btn_test_maker_speed); //test_maker_speed	Int	卷烟机试运转速度		读/写	数值显示	MW1950
            mStatusMap.Add(".test_packer_speed", this.btn_test_packer_speed); //test_packer_speed	Int	包装机试运转速度		读/写	数值显示	MW1952

            mStatusMap.Add(".Maker_MaxSpeedLimit", this.btn_Maker_MaxSpeedLimit);  //<!--卷烟机最大速度设定（DB8.DBW60）Maker_MaxSpeedLimit-->
            mStatusMap.Add(".Packer_MaxSpeedLimit", this.btn_Packer_MaxSpeedLimit);  //<!--包装机最大速度设定（DB8.DBW62）Packer_MaxSpeedLimit-->

            mStatusMap.Add(".cig_dim", this.btn_cig_dim);  //烟支直径（DB8.DBW46 cig_dim
         }
    }


}
