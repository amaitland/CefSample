﻿using CefSharp;
using CefSharp.Internals;
using CefSharp.WinForms;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefBrowserWrapper
{
    public class CefScreenBrowserWrapper : CefBrowserWrapperBase
    {
        public bool WaitingForUserAction { get; internal set; }

        public CefScreenBrowserWrapper(Form1 form, SingleBrowserInfo browserInfo,
            BrowserSettings browserSettings) : base(browserInfo, browserSettings)
        {
            Form = form;
        }


       

        private void FinishBrowserInitialization()
        {
            //if (Form != null) Form.Invoke(new Action(delegate
            //{
            //    using (var client = Browser.GetDevToolsClient())
            //    {
            //        _ = client.Network.SetUserAgentOverrideAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 - Testing 123");
            //    }
            //}));
        }

        

        public override void CreateBrowserObject(BrowserSettings browserSetting)
        {
            
            
            Browser = new ChromiumWebBrowser();
            //(Browser as ChromiumWebBrowser).ActivateBrowserOnCreation = false;
            (Browser as ChromiumWebBrowser).BrowserSettings = browserSetting;
            //Using RequestContextSettings allows having isolated cache/cookies etc for different ChromiumWebBrowser instances
            RequestContextSettings requestContextSettings = new RequestContextSettings();
            requestContextSettings.PersistSessionCookies = true;
            requestContextSettings.PersistUserPreferences = true;
            //requestContextSettings.sav

            string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                UniCefBrowserWrapperFactory.CefSharpCacheLocalPath,
                SingleBrowserInfo.Proxy.Ip);

            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            requestContextSettings.CachePath = cachePath;
            

            (Browser as ChromiumWebBrowser).RequestContext = new RequestContext(requestContextSettings);
            
            //if (!String.IsNullOrEmpty(BrowserInfo.Proxy.Login))
                (Browser as ChromiumWebBrowser).RequestHandler = 
                    new CustomRequestHandler(SingleBrowserInfo.Proxy.Login, 
                        SingleBrowserInfo.Proxy.Password,
                        SingleBrowserInfo.UserAgent);

            //Browser.tit.TitleChanged += OnBrowserTitleChanged;

            if (!String.IsNullOrEmpty(SingleBrowserInfo.Proxy.Ip))
                (Browser as ChromiumWebBrowser).IsBrowserInitializedChanged += (s, e) =>
                {
                   
                    var br = (ChromiumWebBrowser)s;
                    
                        var v = new Dictionary<string, object>
                        {
                            ["mode"] = "fixed_servers",
                            ["server"] = string.Format("{0}://{1}:{2}", 
                            SingleBrowserInfo.Proxy.Scheme, SingleBrowserInfo.Proxy.Ip, SingleBrowserInfo.Proxy.Port.ToString())
                        };
                        if (!br.GetBrowser().GetHost().RequestContext.SetPreference("proxy", v, out string error))
                            throw new Exception(error);
                 
                };
            
        }


       

        public override void Dispose()
        {
            
            
            base.Dispose();
        }
        protected override Bitmap BrowserScreenshot()
        {
            return ControlSnapshot.Snapshot((Browser as ChromiumWebBrowser));
            //img.Save(@"C:\Users\Public\Pictures\text.bpm", ImageFormat.Bmp);

            
        }

        protected override void SetBrowserSize(Size size)
        {
            (Browser as ChromiumWebBrowser).Size = size;
        }

        public void StartWaitingUserAction()
        {
            Form.Invoke(new Action(
                        delegate
                        {
                            WaitingForUserAction = true;
                            Form.SetForeground();
                            Form.ActivateUserInputForBrowser((Browser as CefSharp.WinForms.ChromiumWebBrowser));
                            
                        }));

        }

    }
}
