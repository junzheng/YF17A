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
using System.Windows.Threading;


namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageHelp.xaml
    /// </summary>
    public partial class PageHome : Page, IObserverResult
    {
        public const string TAG = "PageHome.xaml";
  
        private BeckHoff mBeckHoff;
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();   

        public PageHome()
        {
            InitializeComponent();
            mBeckHoff = Utils.GetBeckHoffInstance();

            InitStatusMap();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarMain.TAG);            
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (BeckHoff.TAG.Equals(senderName))
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

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);

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

        private void Canvas_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
          
            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }       
          
        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);

            if (element.GetType() == typeof(Image))
            {
                Image light = element as Image;
                Boolean b = (Boolean)value;
                BitmapImage bmTrue = MainWindow.sEnableLight;
                BitmapImage bmFalse = MainWindow.sDisableLight;
          
                if (b)
                {
                    light.Source = bmTrue;
                }
                else
                {
                    light.Source = bmFalse;
                }                
            }
            else if (element.GetType() == typeof(Button))
            {               
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }
        }

    

        private void InitStatusMap()
        {        
            mStatusMap.Add(".Store_CigNum", this.tb_Store_CigNum); //Store_CigNum	Int	存储烟支数		只读	数值显示	MW1880
            mStatusMap.Add(".Store_percent", this.tb_Store_percent);//Store_percent	Int	存储位置百分比		只读	数值显示	DB8.DBW50        
        }      
    }
}
