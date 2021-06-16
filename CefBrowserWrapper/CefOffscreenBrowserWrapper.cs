using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;

using CefSharp.Handler;
using CefSharp.Internals;
using CefSharp.OffScreen;
using System.IO;
using System.Text.RegularExpressions;

using System.Drawing;


namespace CefBrowserWrapper
{
    public class CefOffscreenBrowserWrapper : CefBrowserWrapperBase
    {


        public CefOffscreenBrowserWrapper(SingleBrowserInfo browserInfo,
            BrowserSettings browserSettings) : base(browserInfo, browserSettings)
        {


        }

        protected Task InitializationWaiter(IWebBrowser browser)
        {
            if (browser.IsBrowserInitialized)
                return Task.FromResult(true);

            var tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            handler = (sender, args) =>
            {
                (browser as ChromiumWebBrowser).BrowserInitialized -= handler;

                if (!String.IsNullOrEmpty(SingleBrowserInfo.Proxy.Ip))
                {

                    var br = (ChromiumWebBrowser)Browser;
                    //if (!String.IsNullOrEmpty(BrowserInfo.Proxy))
                    //{
                    var v = new Dictionary<string, object>
                    {
                        ["mode"] = "fixed_servers",
                        ["server"] = string.Format("{0}://{1}:{2}",
                        SingleBrowserInfo.Proxy.Scheme, SingleBrowserInfo.Proxy.Ip, SingleBrowserInfo.Proxy.Port.ToString())
                    };
                    if (!br.GetBrowser().GetHost().RequestContext.SetPreference("proxy", v, out string error))
                        throw new Exception(error);
                }

                tcs.TrySetResultAsync(true);
            };

            (browser as ChromiumWebBrowser).BrowserInitialized += handler;
            return tcs.Task;
        }



        public override void CreateBrowserObject(BrowserSettings browserSetting)
        {
            
            //var settings = new CefSettings();
            //settings.CefCommandLineArgs.Add("disable-image-loading", "1");
            //RequestContextSettings rcs = new RequestContextSettings();
            //Using RequestContextSettings allows having isolated cache/cookies etc for different ChromiumWebBrowser instances

            Browser = new ChromiumWebBrowser("", browserSetting,  new RequestContext(new RequestContextSettings()));
            
            if (!String.IsNullOrEmpty(SingleBrowserInfo.Proxy.Login))
                (Browser as ChromiumWebBrowser).RequestHandler =
                    new CustomRequestHandler(SingleBrowserInfo.Proxy.Login, SingleBrowserInfo.Proxy.Password,
                    SingleBrowserInfo.UserAgent);

            InitializationWaiter((Browser as ChromiumWebBrowser)).Wait();


            RequestContextSettings requestContextSettings = new RequestContextSettings();

            requestContextSettings.PersistSessionCookies = false;
            requestContextSettings.PersistUserPreferences = false;

            string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
               UniCefBrowserWrapperFactory.CefSharpCacheLocalPath,
               SingleBrowserInfo.Proxy.Ip);

            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            requestContextSettings.CachePath = cachePath;

        }

       

        public bool ClickQuerySelector(string queryselector)
        {
            bool exist = false;
            var result = Browser.EvaluateScriptAsync(string.Format("document.querySelector('{0}').click();", queryselector));
            result.Wait();
            Thread.Sleep(1000);

            if (result.Result.Success)
                exist = true;

            return exist;


        }


        public void ButtonEnter()
        {
            KeyEvent k = new KeyEvent();
            k.WindowsKeyCode = 0x0D;
            k.FocusOnEditableField = true;
            k.IsSystemKey = false;
            k.Type = KeyEventType.Char;
            Browser.GetBrowser().GetHost().SendKeyEvent(k);
        }



        private void RemoveStylesFromPage()
        {
            var result = Browser.EvaluateScriptAsync("function removeStyles(e){if(e.removeAttribute('style'),e.childNodes.length>0)for(var o in e.childNodes)1==e.childNodes[o].nodeType&&removeStyles(e.childNodes[o])}removeStyles(document.body);");
            result.Wait();
            Thread.Sleep(1000);
        }

        private void TryLogIn(string capchaResponse, string xpath)
        {
            var result = Browser.EvaluateScriptAsync(string.Format("document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null).iterateNext().value='{1}';", xpath.Replace('\'', '\"'), capchaResponse));
            result.Wait();
            Thread.Sleep(1000);



        }

        


        protected override Bitmap BrowserScreenshot()
        {
            return (Browser as ChromiumWebBrowser).ScreenshotOrNull();
        }

        protected override void SetBrowserSize(Size size)
        {
            (Browser as ChromiumWebBrowser).Size = size;
        }

       
    }

    public class ProxyAuthRequestHandler : RequestHandler
    {
        private readonly string _login;
        private readonly string _password;

        public ProxyAuthRequestHandler(string login, string password)
        {
            _login = login;
            _password = password;
        }

        protected override bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host,
            int port, string realm, string scheme, IAuthCallback callback)
        {

            if (isProxy)
            {
                callback.Continue(_login, _password);
                return true;
            }

            return false;

        }
    }
}