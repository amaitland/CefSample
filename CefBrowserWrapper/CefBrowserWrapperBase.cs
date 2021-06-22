
using CefSharp;
using CefSharp.Handler;
using CefSharp.Internals;
using CefSharp.OffScreen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefBrowserWrapper
{
    public abstract class CefBrowserWrapperFactoryBase
    {
        public CefBrowserWrapperFactoryBase()
        {
            
        }
        public abstract CefBrowserWrapperBase Create(
            bool javascriptEnabled,
            bool imageLoading,
            int loadTimeoutMs,
            SingleBrowserInfo singleBrowserInfo);

        public abstract void ShowForm();
        public abstract void HideForm();
        public abstract void Dispose();
    }

    public class UniCefBrowserWrapperFactory : CefBrowserWrapperFactoryBase
    {

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        Thread _thread;
        ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        Form1 Form;
        bool _windowVisible;
        protected ConcurrentDictionary<CefSharp.WinForms.ChromiumWebBrowser, CefScreenBrowserWrapper> browserWrapperDictionary;
        
        public UniCefBrowserWrapperFactory(bool windowVisible) :base()
        {
            _windowVisible = windowVisible;
            if (_windowVisible)
            {
                browserWrapperDictionary = new ConcurrentDictionary<CefSharp.WinForms.ChromiumWebBrowser, CefScreenBrowserWrapper>();
                CreateForm();
                ShowForm();
            }
            //if (_browserInfo.WindowVisible)
            //{
            //    CefSharp.Cef.Initialize(new CefSharp.WinForms.CefSettings() { RootCachePath = @"C:\Users\User\Documents" });
            //}

            //else
            //{
            //    CefSharp.Cef.Initialize(new CefSharp.OffScreen.CefSettings() { RootCachePath = @"C:\Users\User\Documents" });
            //}
        }
        public static string CefSharpCacheLocalPath = @"Cefsharp\Cache";
        public static void CefGlobalInit()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), CefSharpCacheLocalPath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            CefSharp.Cef.Initialize(new CefSharp.WinForms.CefSettings() { RootCachePath = path }) ;

        }
        public override CefBrowserWrapperBase Create(
            bool javascriptEnabled,
            bool imageLoading,
            int loadTimeoutMs,
            SingleBrowserInfo singleBrowserInfo)
        {
            

            BrowserSettings browserSettings = new BrowserSettings
            {
                //FileAccessFromFileUrls = CefState.Enabled,
                //UniversalAccessFromFileUrls = CefState.Enabled,
                Javascript = javascriptEnabled ? CefState.Enabled : CefState.Disabled,
                ImageLoading = imageLoading ? CefState.Enabled : CefState.Disabled,
                
                //JavascriptAccessClipboard = CefState.Enabled,
                //JavascriptCloseWindows = CefState.Enabled,
                //JavascriptDomPaste = CefState.Enabled
            };

            CefBrowserWrapperBase retVal = null;

            if (_windowVisible)
            {
                retVal = new CefScreenBrowserWrapper(Form, singleBrowserInfo, browserSettings);
                browserWrapperDictionary.TryAdd((retVal.GetBrowser() as CefSharp.WinForms.ChromiumWebBrowser), (CefScreenBrowserWrapper)retVal);
            }
            else
            {
                retVal = new CefOffscreenBrowserWrapper(singleBrowserInfo, browserSettings);
            }

            retVal.Factory = this;
            retVal.LoadTimeoutMs = loadTimeoutMs;

            return retVal;
        }

        public override void Dispose()
        {
            if (Form != null) Form.Invoke(new Action(delegate
            {
                //_manualWinformsManager.Dispose();
                _cancellationTokenSource.Cancel();
               
            }));

        }

        public override void ShowForm()
        {
            //_manualWinformsManager.ShowForm(Form);
            //if (Form != null) Form.Invoke(new Action(delegate
            //{
            //    Form.Show();
            //}));
            Form.ShowFormFlag = true;
            manualResetEvent.WaitOne();

            //FinishBrowserInitialization();
        }

        public override void HideForm()
        {
            Form.HideFormFlag = true;
        }

        private void CreateForm()
        {
            Form = new Form1( manualResetEvent);
            Form.ProceedButtonClicked += (_, browser) =>
            {
                browserWrapperDictionary[browser].WaitingForUserAction = false;
            };

            _thread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    if (Form.ShowFormFlag) { Form.ShowFormFlag = false; Form.Show(); }
                    if (Form.HideFormFlag) { Form.HideFormFlag = false; Form.Hide(); }
                    Application.DoEvents();
                   
                    Thread.Sleep(100);
                }

                Form.Close();
                Form.Dispose();
                
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }
    }
    //await Task.Delay(TimeSpan.FromMilliseconds(100));
    public abstract class CefBrowserWrapperBase : IBrowserWrapper
    {
        public CefBrowserWrapperFactoryBase Factory;
        protected Form1 Form = null;
        protected IWebBrowser Browser { get; set; }

        protected SingleBrowserInfo SingleBrowserInfo { get;set; }
        protected BrowserSettings BrowserSettings { get;  }
        public int LoadTimeoutMs { get; set; } = 15000;
        public CefBrowserWrapperBase(SingleBrowserInfo singleBrowserInfo, BrowserSettings browserSettings)
        {
            SingleBrowserInfo = singleBrowserInfo;
            BrowserSettings = browserSettings;
            //Monitor parent process exit and close subprocesses if parent process exits first
            //This will at some point in the future becomes the default
            // dCefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            // if (!Cef.IsInitialized) Cef.Initialize(CreateCefSettings());

            CreateBrowserObject(browserSettings);

            

            //if (Form != null) Form.Invoke(new Action(delegate
            //{
            //using (var client = Browser.GetDevToolsClient())
            //{
            //    _ = client.Network.SetUserAgentOverrideAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 - Testing 123");
            //}
            //}));

            //SetProxy(browserInfo.Proxy);
        }

        //protected abstract Task InitializationWaiter(IWebBrowser browser);

        //public virtual void UpdateBrowserObject(BrowserInfo browserInfo)
        //{
        //    BrowserInfo = browserInfo;

        //    CreateBrowserObject(BrowserSettings);
        //}

        public IWebBrowser GetBrowser()
        {
            return Browser;
        }
        public abstract void CreateBrowserObject(BrowserSettings browserSettings);
        //public virtual void ShowForm()
        //{

        //}
        private string ExtractTitleFromPageCode(string pageCode)
        {
            return Regex.Match(pageCode, "<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
        }
        public virtual void LoadUrl(string url)
        {
           
            if (Form != null) Form.Invoke(new Action(delegate
                {
                    Form.ShowBrowser((Browser as CefSharp.WinForms.ChromiumWebBrowser));
                    //Form.ShowLoadProgressBar((Browser as CefSharp.WinForms.ChromiumWebBrowser), true);
                    try
                    {
                        
                        LoadUrlCore(url);
                        string title = ExtractTitleFromPageCode(GetHtmlCore());
                        
                        Form.SetTabName((Browser as CefSharp.WinForms.ChromiumWebBrowser),!String.IsNullOrEmpty(title)?title:url);// SingleBrowserInfo.Proxy.Ip);// 

                    }
                    catch (Exception exp)
                    {
                        //Form.ShowLoadProgressBar((Browser as CefSharp.WinForms.ChromiumWebBrowser), false);
                        Console.WriteLine(exp.Message);
                        throw;
                    }
                    finally
                    {
                        //Form.ShowLoadProgressBar((Browser as CefSharp.WinForms.ChromiumWebBrowser), false);
                        //Form.HideBrowser((Browser as CefSharp.WinForms.ChromiumWebBrowser));
                    }
                    
                }));
            else
            {
                LoadUrlCore(url);
            }
        }

        public virtual void HideBrowser()
        {
            if (Form != null) Form.Invoke(new Action(delegate
            {
                Form.HideBrowser((Browser as CefSharp.WinForms.ChromiumWebBrowser));
            }));
        }
        public string ScreenShot(string fileName)
        {
            string retVal = "";
            if (Form != null) Form.Invoke(new Action(delegate
            {
                retVal = ScreenShotCore(fileName);
            }));
            else
            {
                retVal = ScreenShotCore(fileName);
            }
            return retVal;
        }

        public string GetBrowserCurrentUrl()
        {
            return Browser.Address != null ? Browser.Address : "";
            // //https://stackoverflow.com/questions/44760073/cefsharp-how-to-get-current-url-address-c-sharp
           
        }

        public virtual void LoadUrlCore(string url)
        {
            if (String.IsNullOrEmpty(url)) return;

            var tcs = new TaskCompletionSource<bool>();
            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
               
                if (!args.IsLoading)
                {
                    Browser.LoadingStateChanged -= handler;
                    tcs.TrySetResultAsync(true);
                }
            };
        
            Browser.LoadingStateChanged += handler;
            if (!string.IsNullOrEmpty(url))
            {
                Browser.Load(url);
            }
            

            if (!tcs.Task.Wait(TimeSpan.FromMilliseconds(LoadTimeoutMs)))
                throw new TimeoutException("Load timeout");
        }


        public virtual void Dispose()
        {
            if(Form != null) Form.Invoke(new Action(delegate
            {
                if (Browser != null)
                {
                    Browser.Dispose();

                    Browser = null;
                }
            }));
            else
            {
                if (Browser != null)
                {
                    Browser.Dispose();

                    Browser = null;
                }
            }
            
            // Form?.Dispose();
        }
        public virtual void Scroll(string xpath)
        {
            if(String.IsNullOrEmpty(xpath))
            {
                EvaluateScript("window.scroll(0,100000);");
            }
            else
            {
                EvaluateScript(string.Format("var el = document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null).iterateNext(); el.scrollIntoView();", xpath.Replace('\'', '\"')),
                    0);
            }
            

        }
        public virtual void SetValue(string xpath, string value)
        {
            EvaluateScript(string.Format("document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null).iterateNext().value='{1}';", xpath.Replace('\'', '\"'), value), 
                1000);
        }
        public virtual void Click(string xpath)
        {
            EvaluateScript(string.Format("document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null).iterateNext().click();", xpath.Replace('\'', '\"')));        
        }
        public virtual bool ElementExists(string xpath)
        {
            var result = Browser.EvaluateScriptAsync(string.Format("document.evaluate('{0}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerHTML;", xpath.Replace('\'', '\"')));
            result.Wait();

            if (!result.Result.Success) return true;
            return false;
        }

        public virtual string ScreenShotCore(string fileName)
        {

            if (String.IsNullOrEmpty(fileName)) fileName = Guid.NewGuid().ToString() + ".jpg";
            int width = 1280;
            int height = 1480;

            string jsString = "Math.max(document.body.scrollHeight, " +
                              "document.documentElement.scrollHeight, document.body.offsetHeight, " +
                              "document.documentElement.offsetHeight, document.body.clientHeight, " +
                              "document.documentElement.clientHeight);";


            var executedScript = Browser.EvaluateScriptAsync(jsString).Result.Result;

            height = Convert.ToInt32(executedScript);

            var size = new Size(width, height);

            SetBrowserSize(size);
            //

            //Thread.Sleep(500);
            // Wait for the screenshot to be taken.
            var bitmap = BrowserScreenshot(); 
            bitmap.Save(fileName,ImageFormat.Bmp);
            return fileName;
        }

        protected abstract Bitmap BrowserScreenshot();

        protected abstract void SetBrowserSize(Size size);

        public virtual void EvaluateScriptCore(string script, int timeOutMs = 0)
        {
            //if form !=null
            var result = Browser.EvaluateScriptAsync(script);
            result.Wait();
            Thread.Sleep(timeOutMs);
            if (!result.Result.Success) throw new Exception(result.Result.Message);
        }

        public virtual void EvaluateScript(string script, int timeOutMs = 0)
        {
            if (Form != null) Form.Invoke(new Action(delegate
            {
                EvaluateScriptCore(script, timeOutMs);
            }));
            else
            {
                EvaluateScriptCore(script, timeOutMs);
            }
        }

        protected virtual string GetHtmlCore()
        {
            return Browser.GetSourceAsync().Result;
        }

        public virtual string GetHtml()
        {
            string retVal = "";
            if (Form != null) Form.Invoke(new Action(delegate
            {
                retVal = GetHtmlCore();
            }));
            else
            {
                retVal = GetHtmlCore();
            }

            return retVal;
        }

        public void CloseBrowser()
        {
            throw new NotImplementedException();
        }

        protected virtual void SendKeyCore(string xpath, long charCode)
        {
            throw new NotImplementedException();
            
        }

      
        public virtual void SendKey(string xpath, long charCode)

        {
            if (Form != null) Form.Invoke(new Action(delegate
            {
                SendKeyCore(xpath, charCode);
            }));
            else
            {
                SendKeyCore(xpath, charCode);
            }
        }

        public virtual void SelectListOption()
        {
            
        }

        public virtual int GetHtmlElementCount(string xPath)
        {
            int retVal = 0;
            if (Form != null) Form.Invoke(new Action(delegate
            {
                retVal = GetHtmlElementCountCore(xPath);
            }));
            else
            {
                retVal = GetHtmlElementCountCore(xPath);
            }
            return retVal;
        }
        protected virtual int GetHtmlElementCountCore(string xPath)
        {

            int retVal = 0;
            var script =
                "function foo(){let xPathResult = document.evaluate('" + xPath.Replace('\'', '\"') + "', document, null, XPathResult.ANY_TYPE, null);" +
                "let nodes = [];  let node = xPathResult.iterateNext();" +
                "while (node) { nodes.push(node); node = xPathResult.iterateNext();} return nodes.length;}foo()";
            //var script = "function foo(){const nodes = [];return nodes;}foo();";
            Task<JavascriptResponse> result = Browser.EvaluateScriptAsync(script);
            result.Wait();
            if (result.Result.Success && result.Result.Result != null)
                retVal = Convert.ToInt32(result.Result.Result);
            return retVal;


        }
    }
}
