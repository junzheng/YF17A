﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace YF17A
{
    class Utils
    { 

        private static Dictionary<String, String> sPageNameMaps; //key: page url, value: page name
        private static String sCurrentPath;

        public static void SetWindowBackground(Page page, String bgResName)
        {
            ImageBrush b = new ImageBrush();
            String uriString = @"pack://application:,,," + bgResName;
            b.ImageSource = new BitmapImage(new Uri(uriString));
            b.Stretch = Stretch.Fill;
            page.Background = b;
        }

        public static void NavigateToPage(String panelName, String pageName, Boolean isStacked)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            if (context.IsPageLoaded(pageName))
            {
                return;
            }

            IProtocal prtocal = MainWindow.getProtocalInstance();
            prtocal.navigateToPage(panelName, pageName);

            if (sPageNameMaps == null)
            {
                InitPageNamePathMaps();
            }
            String path;
            sPageNameMaps.TryGetValue(pageName, out path);
            if (path != null)
            {
                context.CommandObserver(PageStatus.TAG, PageStatus.PATH, path);
            }

            if ( panelName.Equals(MainWindow.sFrameReportName))
            {
                if (isStacked && !String.IsNullOrEmpty(sCurrentPath))
                {
                    if (ToolbarParameter.sBackPageStack.Count == 0)
                    {
                        ToolbarParameter.sBackPageStack.Push(sCurrentPath);
                    }
                    else if (!sCurrentPath.Equals(ToolbarParameter.sBackPageStack.Peek()))
                    {
                        ToolbarParameter.sBackPageStack.Push(sCurrentPath);
                    }
                }
                sCurrentPath = pageName;
            }  
        }

        public static void NavigateToPage(String panelName, String pageName)
        {
            NavigateToPage(panelName, pageName, true);        
        }


        public static BeckHoff GetBeckHoffInstance()
        {
            IProtocal protocal = MainWindow.getProtocalInstance();

            return protocal.GetBeckHoff();
        }

        private static void InitPageNamePathMaps()
        {
            sPageNameMaps = new Dictionary<string, string>();
            sPageNameMaps.Add(PageDebugBucket.TAG, "圆筒调试");
            sPageNameMaps.Add(PageDebugConsole.TAG, "控制台调试");
            sPageNameMaps.Add(PageDebugMain.TAG, "调试");
            sPageNameMaps.Add(PageDebugUp.TAG, "提升口调试");
            sPageNameMaps.Add(PageDocument.TAG, "文档");
            sPageNameMaps.Add(PageHome.TAG, "主页");
            sPageNameMaps.Add(PageParameterDown.TAG, "下降口参数设置");
            sPageNameMaps.Add(PageParameterMain.TAG, "参数设置");
            sPageNameMaps.Add(PageParameterUp.TAG, "提升口参数设置");
            sPageNameMaps.Add(PageRuntimeLog.TAG, "日志");
            sPageNameMaps.Add(PageUserRegister.TAG, "用户登录注册");
            sPageNameMaps.Add(PageUserManager.TAG, "用户管理");
            sPageNameMaps.Add(PageUserPassword.TAG, "用户密码修改");
            sPageNameMaps.Add(PageWarningDetail.TAG, "警报详情");
            sPageNameMaps.Add(PageWarningRecord.TAG, "报警记录");
        }

        public static String GetFormatedPlcValue(String plcVarName)
        {
            BeckHoff beckhoff = GetBeckHoffInstance();
            Object value;
            beckhoff.plcVarUserdataMap.TryGetValue(plcVarName, out value);
            YF17A.BeckHoff.ThresHold limit;
            beckhoff.plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
            String content = value.ToString();
            if (limit != null && limit.ratio == 100)
            {
                return Convert.ToSingle(content).ToString("F2");
            }
            else
            {
                return content;
            }
        }

    }
}
