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



        public bool ClickXpath2(string xpath)
        {
            bool exist = false;
            var result = Browser.EvaluateScriptAsync(string.Format(@"document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null)).on('mousedown', function() { 
            tId = setTimeout(GFG_Fun, 20000); 
        }).on('mouseup mouseleave', function() { 
            clearTimeout(tId); 
        }); ", xpath.Replace('\'', '\"')));
            result.Wait();
            Thread.Sleep(1000);

            if (result.Result.Success)
                exist = true;

            return exist;
        }

        //public void CheckAccount(string content, string url)
        //{
        //    if (Regex.IsMatch(content, "<div[^<>]*?=.g-recaptcha") && RuCaptchaAPIKey != "")
        //    {

        //        googleKey = Regex.Match(content, @"recaptcha\/api2\/anchor\?ar=1&amp;k=(.*?)&", RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
        //        RemoveStylesFromPage();

        //        string id = GetCapthcaID(googleKey, url);
        //        string response = GetCapthcaResponse(id);

        //        TryLogIn(response,"//textarea[@id='g-recaptcha-response']");
        //        Thread.Sleep(5000);
        //        try
        //        {
        //            ClickXpath("//*[@type='submit']");
        //                Thread.Sleep(2000);
        //        }
        //        catch
        //        {
        //        }

        //        string callbackname = Regex.Match(content, "data-callback=\"(.*?)\"", RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;

        //        var result = Browser.EvaluateScriptAsync(callbackname + "();");
        //        result.Wait();
        //        Thread.Sleep(1000);



        //        try
        //        {
        //             ClickXpath("//div[@id='g-recaptcha']/iframe");

        //                Thread.Sleep(1000);
        //        }
        //        catch
        //        {
        //        }

        //    }
        //}

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

        //private string GetCapthcaResponse(string id)
        //{
        //    string responseString = "";

        //    while (true)
        //    {

        //        string urlResp = string.Format("http://rucaptcha.com/res.php?key={0}&action=get&id={1}", RuCaptchaAPIKey, id);

        //        HttpWebRequest resp_cap = (HttpWebRequest)WebRequest.Create(urlResp);

        //        resp_cap.Method = "GET";

        //        HttpWebResponse response = (HttpWebResponse)resp_cap.GetResponse();

        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader reader = new StreamReader(stream))
        //            {
        //                responseString = reader.ReadToEnd();
        //            }
        //        }

        //        if (responseString == "CAPCHA_NOT_READY")
        //        {
        //            Thread.Sleep(5 * 1000);
        //        }
        //        else
        //        {
        //            return responseString.Remove(0, 3);
        //        }
        //    }
        //}

        //private string GetCapthcaID(string googleKey, string url)
        //{
        //    string responseString = "";

        //    string urlResp = string.Format("http://rucaptcha.com/in.php?key={0}&method=userrecaptcha&googlekey={1}&pageurl={2}&invisible=1", RuCaptchaAPIKey, googleKey, url);

        //    HttpWebRequest resp_cap = (HttpWebRequest)WebRequest.Create(urlResp);

        //    resp_cap.Method = "GET";

        //    HttpWebResponse response = (HttpWebResponse)resp_cap.GetResponse();

        //    using (Stream stream = response.GetResponseStream())
        //    {
        //        using (StreamReader reader = new StreamReader(stream))
        //        {
        //            responseString = reader.ReadToEnd();
        //        }
        //    }
        //    return responseString.Remove(0, 3);
        //}



        //public override void LoadUrl(string url)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    EventHandler<LoadingStateChangedEventArgs> handler = null;
        //    handler = (sender, args) =>
        //    {
        //        if (!args.IsLoading)
        //        {
        //            Browser.LoadingStateChanged -= handler;
        //            tcs.TrySetResultAsync(true);
        //        }
        //    };
        //    Browser.LoadingStateChanged += handler;
        //    if (!string.IsNullOrEmpty(url))
        //    {
        //        Browser.Load(url);
        //    }

        //    if (!tcs.Task.Wait(TimeSpan.FromSeconds(90)))
        //        throw new TimeoutException("—траница не загрузилась в течение 10 сек.");
        //}

       
        


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