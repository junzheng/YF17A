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
    public partial class PageDebugConsole : Page,IObserverResult
    {
        public const string TAG = "PageDebugConsole.xaml";
        private Dictionary<String, Image> mStatusMap = new Dictionary<string, Image>();
        private List<Border> mLayoutParts = new List<Border>();
        private int mPageIndex = 0;
        
        private BeckHoff mBeckHoff;      

        public PageDebugConsole()
        {
            InitializeComponent();           
           Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);
           InitStatusMap();

           UpdateStatusLayout();

           mBeckHoff = Utils.GetBeckHoffInstance();
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

          
            //beckhoff changed 
            if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                Object plcValue ;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);

                if (mStatusMap.ContainsKey(plcVarName))
                {
                    UpdateView(plcVarName, plcValue);
                }
            }           
        }

        private void UpdateView(String viewTag, Object value)
        {          
            Boolean status = (Boolean)value  ;
            BitmapImage bitmap = MainWindow.sDisableLight  ; 
            if(status){
                bitmap = MainWindow.sEnableLight;
            }   
            Image iv ;
            mStatusMap.TryGetValue(viewTag, out iv);
            iv.Source = bitmap;
        }
        #endregion

       
        private void btn_part2_Click(object sender, RoutedEventArgs e)
        {
            if (mPageIndex < mLayoutParts.Count - 1)
            {
                mPageIndex++;
            }
            UpdateStatusLayout();
        }

        private void btn_part1_Click(object sender, RoutedEventArgs e)
        {
            if (mPageIndex > 0)
            {
                mPageIndex--;
            }
            UpdateStatusLayout();
        }

        private void UpdateStatusLayout()
        {
            for (int i = 0; i < mLayoutParts.Count; i++)
            {
                Visibility visibility = Visibility.Hidden;
                if (mPageIndex == i)
                {
                    visibility = Visibility.Visible;
                }
                mLayoutParts[i].Visibility = visibility;
            }

            this.btn_part1.IsEnabled = true;
            this.btn_part2.IsEnabled = true;

            if (mPageIndex == 0)
            {
                this.btn_part1.IsEnabled = false;
            }
            else if (mPageIndex == mLayoutParts.Count -1)
            {
                this.btn_part2.IsEnabled = false;
            }
           // String pageIndictor =
            this.tb_page_indictor.Text = (mPageIndex + 1)+ "/" + mLayoutParts.Count;
        }

        private void InitStatusMap()
        {
            mLayoutParts.Add(this.status_layout_part1);
            mLayoutParts.Add(this.status_layout_part2);
            mLayoutParts.Add(this.status_layout_part3);

            /******  I0.0- I0.7 *************/
            mStatusMap.Add(".Emergency_stop", this.iv_Emergency_stop);//I0.0 	紧急停止Emergency_stop
            mStatusMap.Add(".MakerExit_power", this.iv_MakerExit_power);//I0.1 	烟机出口伺服主电源 MakerExit_power
            mStatusMap.Add(".Sample_power", this.iv_Sample_power);//I0.2 	取样伺服主电源 Sample_power
            mStatusMap.Add(".Corner_power", this.iv_Corner_power);//I0.3 	弯道伺服主电源 Corner_power
            mStatusMap.Add(".Lift_power", this.iv_Lift_power);//I0.4 	提升伺服主电源 Lift_power
            mStatusMap.Add(".Transfer_power", this.iv_Transfer_power);//I0.5 	传送伺服主电源    Transfer_power
            mStatusMap.Add(".Slope_power", this.iv_Slope_power);//I0.6 	斜向伺服主电源 Slope_power
            mStatusMap.Add(".Store_power", this.iv_Store_power);//I0.7 	存储伺服主电源 Store_power          

            /******  I1.0- I1.7 *************/
            mStatusMap.Add(".MakerExit_servo_fault", this.iv_MakerExit_servo_fault);//I1.2	卷烟机伺服驱动异常 MakerExit_servo_fault
            mStatusMap.Add(".Sample_servo_fault", this.iv_Sample_servo_fault);//I1.3	取样伺服驱动异常 Sample_servo_fault
            mStatusMap.Add(".Corner_servo_fault", this.iv_Corner_servo_fault);//I1.4	弯道伺服驱动异常 Corner_servo_fault
            mStatusMap.Add(".Lift_servo_fault", this.iv_Lift_servo_fault);//I1.5	提升伺服驱动异常 Lift_servo_fault
            mStatusMap.Add(".Transfer_servo_fault", this.iv_Transfer_servo_fault);//I1.6	传送伺服驱动异常 Transfer_servo_fault
            mStatusMap.Add(".Slope_servo_fault", this.iv_Slope_servo_fault);//I1.7	斜向伺服驱动异常Slope_servo_fault           

            /******  I2.0- I2.7 *************/

            mStatusMap.Add(".Store_servo_fault", this.iv_Store_servo_fault); //Store_servo_fault	Bool	存储伺服控制器故障	A19	只读	指示灯/报警条显示	I2.0
            //Spare11	Bool	备用		只读	指示灯	I2.1
            //I2.2	备用Spare22
            mStatusMap.Add(".Elevater_man_auto_sw", this.iv_Elevater_man_auto_sw); //Elevater_man_auto_sw	Bool	提升机手动/自动方式	SA101	只读	指示灯	I2.3           
            mStatusMap.Add(".Elevater_start_pb", this.iv_Elevater_start_pb); //Elevater_start_pb	Bool	提升机启动按钮	SB102	只读	指示灯	I2.4
            mStatusMap.Add(".Elevater_reset_pb", this.iv_Elevater_reset_pb); //Elevater_reset_pb	Bool	提升机复位按钮	SB103	只读	指示灯	I2.5
            mStatusMap.Add(".Elevater_stop_pb", this.iv_Elevater_stop_pb); //Elevater_stop_pb	Bool	提升机停机按钮	SB104	只读	指示灯	I2.6
            mStatusMap.Add(".Elevater_e_stop", this.iv_Elevater_e_stop); //Elevater_e_stop	Bool	提升机紧急停止按钮	SB105	只读	指示灯/报警条显示	I2.7         

            /******  I3.0- I3.7 *************/          
            //I3.0	备用Spare30
            //I3.1	备用Spare31
            mStatusMap.Add(".MakerExit_sensor", this.iv_MakerExit_sensor); ////I3.2	卷烟机出口有烟 MakerExit_sensor
            mStatusMap.Add(".Sample_entrance_sensor", this.iv_Sample_entrance_sensor); //Sample_entrance_sensor	Bool	取样入口有烟传感器	B-PSW101	只读	指示灯	I2.3
            mStatusMap.Add(".Sample_entrance_jam_sensor", this.iv_Sample_entrance_jam_sensor); //Sample_entrance_jam_sensor	Bool	取样入口堵塞传感器（备用）	B-PSW102	只读	指示灯	I2.4
            mStatusMap.Add(".Corner_entrance_jam_sensor", this.iv_Corner_entrance_jam_sensor); //Corner_entrance_jam_sensor	Bool	弯道入口堵塞传感器	B-PRX101	只读	指示灯	I2.5
            mStatusMap.Add(".MakerExit_jam_sensor", this.iv_MakerExit_jam_sensor);//I3.6	卷烟机出口堵塞（备用） MakerExit_jam_sensor
           //I3.7	备用Spare37
            
            /******  I4.0- I4.7 *************/
            mStatusMap.Add(".StoreUnit_man_auto_sw", this.iv_StoreUnit_man_auto_sw); //StoreUnit_man_auto_sw	Bool	存储器手动/自动方式	SA201	只读	指示灯	I3.0
             //I4.1	全排空Spare41
            mStatusMap.Add(".StoreUnit_start_button", this.iv_StoreUnit_start_button); //I4.2	启动 StoreUnit_start_button
            mStatusMap.Add(".StoreUnit_reset_button", this.iv_StoreUnit_reset_button); //I4.3	复位 StoreUnit_reset_button
            mStatusMap.Add(".StoreUnit_stop_button", this.iv_StoreUnit_stop_button); //I4.4	停机 StoreUnit_stop_button
            mStatusMap.Add(".StoreUnit_e_stop_button", this.iv_StoreUnit_e_stop_button); //I4.5	紧急停止 StoreUnit_e_stop_button
            //I4.6	备用Spare46
            //I4.7	备用Spare47  

             /******  I5.0- I5.7 *************/
            mStatusMap.Add(".Downport_jam_sensor", this.iv_Downport_jam_sensor); //I5.0	下降口堵塞 Downport_jam_sensor
            mStatusMap.Add(".Slope_empty", this.iv_Slope_empty); //I5.1	斜向通道空 Slope_empty
            mStatusMap.Add(".Transfer_cig_exist", this.iv_Transfer_cig_exist); //I5.2	高架烟支存在 Transfer_cig_exist
            mStatusMap.Add(".Transfer_overload_sensor", this.iv_Transfer_overload_sensor); //I5.3	传送过载 Transfer_overload_sensor
            //I5.4	备用Spare54 
             //I5.5	备用Spare55
           //I5.6	备用Spare56
            //I5.7	备用Spare57

            /******  I6.0- I6.7 *************/
            mStatusMap.Add(".Store_full", this.iv_Store_full);//I6.0	存储器满 Store_full
            mStatusMap.Add(".Store_empty", this.iv_Store_empty);//I6.1	存储器空 Store_empty
            mStatusMap.Add(".Store_overload", this.iv_Store_overload); //I6.2	存储器过载 Store_overload
            mStatusMap.Add(".Store_entrance_cig_exist", this.iv_Store_entrance_cig_exist);//I6.3	存储器入口有烟 Store_entrance_cig_exist
            mStatusMap.Add(".Store_entrance_jam", this.iv_Store_entrance_jam); //I6.4	存储器入口堵塞 Store_entrance_jam
            mStatusMap.Add(".Store_overlimit", this.iv_Store_overlimit);//I6.5	存储器极限 Store_overlimit
            mStatusMap.Add(".Store_running", this.iv_Store_running);//I6.6	存储器运行中  Store_running
            mStatusMap.Add(".Store_enabled", this.iv_Store_enabled);//I6.7	存储器使能中 Store_enabled
            
            /******  Q4.0- Q4.7 *************/
            mStatusMap.Add(".sample_servo_enable_Q", this.iv_sample_servo_enable_Q);//Q4.0	取样伺服驱动器使能 sample_servo_enable_Q
            mStatusMap.Add(".Corner_servo_enable_Q", this.iv_Corner_servo_enable_Q);//Q4.1	弯道伺服驱动器使能 Corner_servo_enable_Q
            mStatusMap.Add(".Lift_servo_enable_Q", this.iv_Lift_servo_enable_Q);//Q4.2	提升伺服驱动器使能 Lift_servo_enable_Q
            mStatusMap.Add(".Transfer_servo_enable_Q", this.iv_Transfer_servo_enable_Q);//Q4.3	传送伺服驱动器使能 Transfer_servo_enable_Q
            mStatusMap.Add(".Slope_servo_enable_Q", this.iv_Slope_servo_enable_Q);//Q4.4	斜向伺服驱动器使能 Slope_servo_enable_Q
            mStatusMap.Add(".Store_servo_enable_Q", this.iv_Store_servo_enable_Q);//Q4.5	存储伺服驱动器使能 Store_servo_enable_Q
            //Q4.6	备用SpareOutput46
            //Q4.7	备用SpareOutput47

            /******  Q5.0- Q5.7 *************/
            mStatusMap.Add(".Elevater_start_Q", this.iv_Elevater_start_Q);    //Q5.0	启动指示 Elevater_start_Q
            mStatusMap.Add(".Elevater_reset_Q", this.iv_Elevater_reset_Q); //Q5.1	复位指示 Elevater_reset_Q
            mStatusMap.Add(".Elevater_stop_Q", this.iv_Elevater_stop_Q); //Q5.2	停机指示 Elevater_stop_Q
            mStatusMap.Add(".Elevater_FaultReset_Q", this.iv_Elevater_FaultReset_Q); //Q5.3	存储器故障复位 Elevater_FaultReset_Q
            mStatusMap.Add(".Maker_enable_relay_Q", this.iv_Maker_enable_relay_Q); //Q5.4	卷烟机允许 Maker_enable_relay_Q
              //Q5.5	备用SpareOutput55
             //Q5.6	备用SpareOutput56
            mStatusMap.Add(".Maker_QuickStop_Q", this.iv_Maker_QuickStop_Q); //Q5.7	卷烟机快停 Maker_QuickStop_Q

            /******  Q80- Q8.7 *************/
           //Q8.0	存储振动检测 SpareOutput80
           //Q8.1	备用SpareOutput81
            mStatusMap.Add(".Packer_enable_relay_Q", this.iv_Packer_enable_relay_Q);//Q8.2	包装机允许 Packer_enable_relay_Q
            mStatusMap.Add(".Packer_LowSpeed_request_Q", this.iv_Packer_LowSpeed_request_Q);//Q8.3	包装机低速请求 Packer_LowSpeed_request_Q
            //Q8.4	备用SpareOutput84
            //Q8.5	备用SpareOutput85
            //Q8.6	备用SpareOutput86
            mStatusMap.Add(".StoreUnit_start_Q", this.iv_StoreUnit_start_Q);//Q8.7	启动指示 StoreUnit_start_Q

            /******  Q9.0- Q9.7 *************/ 
            mStatusMap.Add(".StoreUnit_reset_Q", this.iv_StoreUnit_reset_Q);//Q9.0	复位指示 StoreUnit_reset_Q
            mStatusMap.Add(".StoreUnit_stop_Q", this.iv_StoreUnit_stop_Q);//Q9.1	停机指示 StoreUnit_stop_Q
             //Q9.2	急停指示 SpareOutput92
            //Q9.3	备用SpareOutput93
            //Q9.4	备用SpareOutput94
            //Q9.5	备用SpareOutput95
            //Q9.6	备用SpareOutput96
            //Q9.7	备用SpareOutput97          

        }

    }
}






  







