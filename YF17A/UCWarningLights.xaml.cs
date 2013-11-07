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
    /// Interaction logic for UCWarningLights.xaml
    /// </summary>
    public partial class UCWarningLights : UserControl,IObserverResult
    {
        public const String TAG = "UCWarningLights.xaml";
        DispatcherTimer tm = new DispatcherTimer();
        private ToolTipHelper mTooltipHelper ;
        BeckHoff mBeckHoff;
        private HashSet<String> mStatusSet = new HashSet<String>(); 

        private enum FlowDynamic { STILL = 0, FORWORD = 1, BACKWORD = 2 };      
        private const int INDEX_QUYANG = 0;
        private const int INDEX_TISHENG = 1;
        private const int INDEX_GAOJIA = 2;
        private const int INDEX_XIAJIANG = 3;
        private const int INDEX_SLOPE = 4;
        private const int INDEX_STORE = 5;
        private const int COUNT_INDEX = 6;

        private int[] mCommonIdx = new int[COUNT_INDEX];
        private FlowDynamic[] mDynamics = new FlowDynamic[COUNT_INDEX];

        private const int COMMON_START = 0;
        private const int COMMON_STOP = 4;
        private const int BUCKET_START = 0;
        private const int BUCKET_STOP = 82;
        private const int BUCKET_FULL = 100;
        private const int BUCKET_LEVEL = 11;
        private const int BUCKET_CAPCITY_PER_LEVEL = 909;
        private int[] mBucketIdx = new int[BUCKET_LEVEL];        
        private int[] mBucketPercents = new int[BUCKET_LEVEL];
        private List<Image> mBucketElements = new List<Image>(BUCKET_LEVEL);

        //private int mBucketCapacity = 1200;

        public UCWarningLights()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);
            mTooltipHelper = new ToolTipHelper(this);
            mBeckHoff = Utils.GetBeckHoffInstance();
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);

            mTooltipHelper.Loaded(sender, e);
            tm.Tick += new EventHandler(tm_Tick);
            tm.Interval = TimeSpan.FromSeconds(0.5);
            tm.Start();

            updateBucket();
            initStatusMap();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
            mTooltipHelper.Unloaded(sender, e);
            tm.Stop();    
        }

        private void initStatusMap()
        {
            mBucketElements.Add(this.iv_flow_bucket0);
            mBucketElements.Add(this.iv_flow_bucket1);
            mBucketElements.Add(this.iv_flow_bucket2);
            mBucketElements.Add(this.iv_flow_bucket3);
            mBucketElements.Add(this.iv_flow_bucket4);
            mBucketElements.Add(this.iv_flow_bucket5);
            mBucketElements.Add(this.iv_flow_bucket6);
            mBucketElements.Add(this.iv_flow_bucket7);
            mBucketElements.Add(this.iv_flow_bucket8);
            mBucketElements.Add(this.iv_flow_bucket9);
            mBucketElements.Add(this.iv_flow_bucket10);

            mStatusSet.Add(".Store_percent");
            mStatusSet.Add(".Corner_cig_speed");
            mStatusSet.Add(".Sample_entrance_sensor");
            mStatusSet.Add(".Sample_cig_speed");
            mStatusSet.Add(".Corner_pid_sp");
            mStatusSet.Add(".Transfer_cig_exist");
            mStatusSet.Add(".Store_cig_speed");
            mStatusSet.Add(".DownPort_hight");
            mStatusSet.Add(".Packer_cig_speed");
            mStatusSet.Add(".Store_entrance_cig_exist");
            mStatusSet.Add(".Slope_cig_speed");
            mStatusSet.Add(".Corner_pid_pv"); 

            foreach (String item in mStatusSet)
            {
                updateStatus(item);
            }
        }

        private void updateBucket()
        {
            Object plcValue;
            mBeckHoff.plcVarUserdataMap.TryGetValue(".Store_percent", out plcValue);
            int capacity =(int) ((float)plcValue * 100);

            // capacity = 9999;

            int n = capacity / BUCKET_CAPCITY_PER_LEVEL;
            float percent = (float)capacity % BUCKET_CAPCITY_PER_LEVEL / BUCKET_CAPCITY_PER_LEVEL;
            int level = (int)(BUCKET_STOP * percent);
            for (int i = 0; i < BUCKET_LEVEL; i++)
            {
                if (i < n)
                {
                    mBucketPercents[i] = BUCKET_FULL;
                }
                else if (i == n)
                {
                    mBucketPercents[i] = level;
                }
                else
                {
                    mBucketPercents[i] = BUCKET_START;
                }
                mBucketIdx[i] = mBucketPercents[i];
            }
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
            
                if (mStatusSet.Contains(plcVarName))
                {
                    updateStatus(plcVarName);
                }
            }
        }

        private void updateStatus(String plcVarName)
        {
            Object plcValue;
            mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);            
            if (".Store_percent".Equals(plcVarName))
            {
                updateBucket();
            }      
    
            //提升机部分显示
            if (".Corner_pid_sp".Equals(plcVarName) || ".Corner_pid_pv".Equals(plcVarName) || ".Corner_work_limit".Equals(plcVarName))
            {
                Object Corner_pid_sp;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out Corner_pid_sp);
                Object Corner_pid_pv;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out Corner_pid_pv);
                Object Corner_work_limit;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out Corner_work_limit);

              
                
                Visibility vis = Visibility.Hidden;
                if (Convert.ToInt32(Corner_pid_sp) > 1000 && Convert.ToInt32( Corner_pid_pv) > Convert.ToInt32(Corner_work_limit))
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_tisheng0.Visibility = vis;
                this.iv_flow_tisheng1.Visibility = vis;
                this.iv_flow_chuizhi.Visibility = vis;               
            } 
            //取样部分显示
            else if (".Sample_entrance_sensor".Equals(plcVarName))
            {
                Boolean hasCigerate = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (hasCigerate)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_sample.Visibility = vis;               
            }
            //高架显示
            else if (".Transfer_cig_exist".Equals(plcVarName))
            {
                Boolean hasCigerate = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (hasCigerate)
                {
                    vis = Visibility.Visible;
                }              
                this.iv_flow_shuiping0.Visibility = vis;
                this.iv_flow_shuiping1.Visibility = vis;
                this.iv_flow_shuiping2.Visibility = vis;
                this.iv_flow_shuiping3.Visibility = vis;         
            }                
          
            //下降口部分显示
            else if (".DownPort_hight".Equals(plcVarName))
            {
                int height = (int)plcValue;
                Visibility vis = Visibility.Hidden;
                if (height > 1000)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_xiajiang0.Visibility = vis;                
            }
            else if (".Packer_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                Visibility vis = Visibility.Hidden;
                if (speed > 100)
                {
                    vis = Visibility.Visible;
                }               
                this.iv_flow_xiajiang1.Visibility = vis;
            }
            //存储器入口部分（即斜向部分）显示
            else if (".Store_entrance_cig_exist".Equals(plcVarName))
            {
                Boolean exist = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (exist)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_slope.Visibility = vis;
            }
           
             //取样速度
            else if (".Sample_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                if (speed > 100)
                {
                    mDynamics[INDEX_QUYANG] = FlowDynamic.FORWORD;
                }
                else
                {
                    mDynamics[INDEX_QUYANG] = FlowDynamic.STILL;
                }
            }
            //提升，高架，下降口 速度
            else if (".Corner_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                // 提升
                if (speed > 100)
                {
                    mDynamics[INDEX_TISHENG] = FlowDynamic.FORWORD;
                    mDynamics[INDEX_GAOJIA] = FlowDynamic.FORWORD;
                    mDynamics[INDEX_XIAJIANG] = FlowDynamic.FORWORD;
                }
                else
                {
                    mDynamics[INDEX_TISHENG] = FlowDynamic.STILL;
                    mDynamics[INDEX_GAOJIA] = FlowDynamic.STILL;
                    mDynamics[INDEX_XIAJIANG] = FlowDynamic.STILL;
                }
            }           
           //斜向速度
            else if (".Slope_cig_speed".Equals(plcVarName))
            {
                int speed = Convert.ToInt32(plcValue.ToString());
                if (speed > 100)
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.FORWORD;
                }
                else if (speed < -100)
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.BACKWORD;
                }
                else
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.STILL;
                }
            }
            //存储速度
            else if (".Store_cig_speed".Equals(plcVarName))
            {
                int speed =Convert.ToInt32(plcValue.ToString());
                if (speed > 100)
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.FORWORD;
                }
                else if (speed < -100)
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.BACKWORD;
                }
                else
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.STILL;
                }
            }
          
        }
        #endregion

        private void tm_Tick(object sender, EventArgs e)
        {
            animateSample();
            animateShuiping();
            animateTisheng();
            animateXiajiang();
            animateSlope();
            animateBucket();

            for (int i = INDEX_QUYANG; i <= INDEX_STORE; i++)
            {
                if (mDynamics[i] == FlowDynamic.STILL)
                {
                    continue;
                }
                else if (mDynamics[i] == FlowDynamic.FORWORD)
                {
                    if (++mCommonIdx[i] > COMMON_STOP)
                    {
                        mCommonIdx[i] = COMMON_START;
                    }
                }
                else if (mDynamics[i] == FlowDynamic.BACKWORD)
                {
                    if (--mCommonIdx[i] < COMMON_START )
                    {
                        mCommonIdx[i] = COMMON_STOP;
                    }
                }
            }
        }

         private void animateSample()
        {
            String imageName = "/image/flow_shuiping_0{0}.png";
            BitmapImage bm = new BitmapImage(new Uri(String.Format(imageName, mCommonIdx[INDEX_QUYANG]), UriKind.Relative));
            this.iv_flow_sample.Source = bm;
        }

        private void  animateBucket()
        {   
            for (int i =0; i < BUCKET_LEVEL; i++)
            {
                int idx = mBucketIdx[i];
                String uriImage = "/image/flow_bucket_0{0}.png";
                Image image = mBucketElements[i];

                if (idx == 0)
                {
                    image.Source = null;
                    continue;
                }
                else if (idx == BUCKET_FULL)
                {
                    uriImage = "/image/flow_bucket_full_0{0}.png";
                    idx = mCommonIdx[INDEX_STORE];
                }
                else if (idx > 9)
                {
                    uriImage = "/image/flow_bucket_{0}.png";
                }                
                BitmapImage bm = new BitmapImage(new Uri(String.Format(uriImage, idx), UriKind.Relative));
                image.Source = bm;
            }
        }

        private void animateShuiping()
        {
            String imageName = "/image/flow_shuiping_0{0}.png";
            BitmapImage bm = new BitmapImage(new Uri(String.Format(imageName, mCommonIdx[INDEX_GAOJIA]), UriKind.Relative));
            this.iv_flow_shuiping0.Source = bm;
            this.iv_flow_shuiping1.Source = bm;
            this.iv_flow_shuiping2.Source = bm;
            this.iv_flow_shuiping3.Source = bm;
        }

        private void animateTisheng()
        {
            String uriImage1 = "/image/flow_tisheng1_0{0}.png";
            BitmapImage bm1 = new BitmapImage(new Uri(String.Format(uriImage1, mCommonIdx[INDEX_TISHENG]), UriKind.Relative));

            String uriImage2 = "/image/flow_tisheng2_0{0}.png";
            BitmapImage bm2 = new BitmapImage(new Uri(String.Format(uriImage2, mCommonIdx[INDEX_TISHENG]), UriKind.Relative));
            
             String uriImage3 = "/image/flow_chuizhi_0{0}.png";
             BitmapImage bm3 = new BitmapImage(new Uri(String.Format(uriImage3, mCommonIdx[INDEX_TISHENG]), UriKind.Relative));
             
            this.iv_flow_tisheng0.Source = bm1;
            this.iv_flow_tisheng1.Source = bm2;     
             this.iv_flow_chuizhi.Source = bm3;
        }

        private void animateXiajiang()
        {
            String uriImage1 = "/image/flow_xiajiang1_0{0}.png";
            BitmapImage bm1 = new BitmapImage(new Uri(String.Format(uriImage1, mCommonIdx[INDEX_XIAJIANG]), UriKind.Relative));

            String uriImage2 = "/image/flow_xiajiang2_0{0}.png";
            BitmapImage bm2 = new BitmapImage(new Uri(String.Format(uriImage2, mCommonIdx[INDEX_XIAJIANG]), UriKind.Relative));

            this.iv_flow_xiajiang0.Source = bm1;
            this.iv_flow_xiajiang1.Source = bm2;          
        }

        private void animateSlope()
        {
            String uriImage1 = "/image/flow_slope_0{0}.png";
            BitmapImage bm1 = new BitmapImage(new Uri(String.Format(uriImage1, mCommonIdx[INDEX_SLOPE]), UriKind.Relative));
              
            this.iv_flow_slope.Source = bm1;    
        }
         
    }
}
