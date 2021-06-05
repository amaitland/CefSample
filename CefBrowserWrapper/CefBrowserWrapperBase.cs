
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
                        Thread.Sleep(1000);
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
                //try
                //{
                //    Console.WriteLine("handler");
                //    //var script = "function foo(){return document.readyState;}foo()";
                   
                //    //Task<JavascriptResponse> result = Browser.EvaluateScriptAsync(script);
                //    //result.Wait();
                //    //if (result.Result.Success && result.Result.Result != null)
                //    //    Console.WriteLine(result.Result.Result.ToString());
                //}
                //catch 
                //{
                    
                //}
                
                //Console.WriteLine());
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
            // Gecko imp,ementation is below
            // can try just sending js event
            // smth like this: https://stackoverflow.com/questions/596481/is-it-possible-to-simulate-key-press-events-programmatically 
            // Swyat feedback on popularity : http://joxi.ru/n2YwyRLIezYqPr
            //  очень редко, например при выборе значения в SELECT иногда только кликом получается выбрать значения и потом чтобы закрыть select для продолжения сценария нажимаю enter, иногда кнопка "Поиска" не нажимается и после заполнения формы ENTER нажимаю.
            //nsIDOMWindowUtils utils = Xpcom.QueryInterface<nsIDOMWindowUtils>(browser.Window.DomWindow);
            //using (nsAString type = new nsAString("keypress"))
            //{
            //    //https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/nsIDOMWindowUtils
            //    long modifiers = 0;
            //    string modifstr = string.Empty;
            //    if ((browserActionSetting as SendKeyBrowserActionSetting).ControlKeyOn)
            //    {
            //        modifiers += nsIDOMWindowUtilsConsts.MODIFIER_CONTROL;
            //        modifstr = "Control";
            //    }
            //    if ((browserActionSetting as SendKeyBrowserActionSetting).AltKeyOn)
            //    {
            //        modifiers += nsIDOMWindowUtilsConsts.MODIFIER_ALT;
            //        modifstr = "Alt";
            //    }
            //    if ((browserActionSetting as SendKeyBrowserActionSetting).ShiftKeyOn)
            //    {
            //        modifiers += nsIDOMWindowUtilsConsts.MODIFIER_SHIFT;
            //        modifstr = "Shift";
            //    }

            //    int charKeyCode = KeyToCharConverter.getCharByKeyName(
            //            (browserActionSetting as SendKeyBrowserActionSetting).CharCode.ToString());

            //    if ((browserActionSetting as SendKeyBrowserActionSetting).CharCode.ToString().Length > 1)
            //    {
            //        using (
            //    nsAString characters = new nsAString("keypress"),
            //        unmodifiedCharaters = new nsAString(modifstr))
            //        {
            //            utils.SendNativeKeyEvent(0, charKeyCode, (int)modifiers,
            //                characters, unmodifiedCharaters, null);
            //        }

            //    }
            //    else
            //    {
            //        using (
            //   nsAString characters = new nsAString("keypress"),
            //       unmodifiedCharaters = new nsAString(modifstr))
            //        {
            //            utils.SendNativeKeyEvent(0, charKeyCode, (int)modifiers,
            //                      characters, unmodifiedCharaters, null);
            //        }
            //    }


            //}
        }

        //protected abstract CefSettingsBase CreateCefSettings();
        //public virtual void SetProxy(string proxy)
        //{

        //    if (!string.IsNullOrEmpty(proxy))
        //    {
        //        string login = string.Empty;
        //        string password = string.Empty;
        //        string address = string.Empty;
        //        string port = string.Empty;

        //        string[] proxyStringSeparated = proxy.Split(':');
        //        if (proxyStringSeparated.Length == 2)
        //        {
        //            address = proxyStringSeparated[0].Trim();
        //            port = proxyStringSeparated[1].Trim();

        //            Browser.RequestHandler = new RequestHandler();
        //        }
        //        else if (proxyStringSeparated.Length == 4)
        //        {
        //            address = proxyStringSeparated[0].Trim();
        //            port = proxyStringSeparated[1].Trim();
        //            login = proxyStringSeparated[2];
        //            password = proxyStringSeparated[3];

        //            Browser.RequestHandler = new ProxyAuthRequestHandler(login, password);
        //        }
        //        //Browser.RequestContext.SetPreference();
        //        if (!Cef.IsInitialized) Cef.Initialize(CreateCefSettings());
        //        Cef.UIThreadTaskFactory.StartNew(delegate
        //        {
        //            var rc = Browser.GetBrowser().GetHost().RequestContext;
        //            var v = new Dictionary<string, object>();
        //            v["mode"] = "fixed_servers";
        //            v["server"] = string.Format("http://{0}:{1}", address, port);
        //            //v["server"] = string.Format("{0}:{1}", address, port);
        //            string error;
        //            bool success = Browser.RequestContext.SetPreference("proxy", v, out error);
        //            if (!success)
        //            {
        //                throw new WebException("Îøèáêà ïðîêñè: " + error);
        //            }
        //        });
        //    }
        //}
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
            //string valueToSelect = prepareValue((browserActionSetting as SelectListOptionBrowserActionSetting).TargetValue);

            //#region Find element

            //GeckoHtmlElement element = getDomObjectByXpath((browserActionSetting as ElementRelatedBrowserActionSettingBase));

            //#endregion

            //string originalValue = "";

            //try
            //{
            //    originalValue = (element as GeckoSelectElement).Value;
            //}
            //catch
            //{

            //}

            //#region Select option

            //#region By index

            //if ((browserActionSetting as SelectListOptionBrowserActionSetting).TargetIndex > -1)
            //{
            //    for (uint i = 0; i < (element as GeckoSelectElement).Options.Length; i++)
            //    {
            //        GeckoOptionElement item = (element as GeckoSelectElement).Options.Item(i);

            //        if (item.Index == Convert.ToInt32(
            //            (browserActionSetting as SelectListOptionBrowserActionSetting).TargetIndex))
            //        {
            //            item.Selected = true;
            //            break;
            //        }
            //    }
            //}
            //#endregion

            //#region By value

            //else
            //{
            //    ComparerFactory comparerFactory = new ComparerFactory();
            //    ComparerBase comparer = comparerFactory.getInstance(
            //        (browserActionSetting as SelectListOptionBrowserActionSetting).RegexCompare
            //        );

            //    for (uint i = 0; i < (element as GeckoSelectElement).Options.Length; i++)
            //    {
            //        GeckoOptionElement item = (element as GeckoSelectElement).Options.Item(i);

            //        if (comparer.compare(item.Text,
            //            valueToSelect))
            //        {
            //            item.Selected = true;
            //            break;
            //        }
            //    }

            //}

            //#endregion

            //#endregion

            //#region Send event


            ////nsAStringBase changeEvent = new nsAString("change");
            //// DomEventArgs eventArgs = browser.Document.CreateEvent("MouseEvent");
            //// eventArgs.DomEvent.InitEvent(changeEvent, true, true);
            //// element.GetEventTarget().DispatchEvent(eventArgs);

            //#endregion


            //#region Проверим если исходное и текущее значение одинаковые, причем они отличаются от того, которое нужно было установить, выдадим предупреждение
            //if ((element as GeckoSelectElement).Value == originalValue)
            ////&&
            ////!comparer.compare(originalValue,
            ////    valueToSelect))
            //{
            //    logger.AddEventToLog(LogLevel.Warning,
            //        Common.Action_ + browserActionSetting.Name.ToString() + Common.semicolon_SelectedValueWasNotChanged, currentInput);
            //}
            //#endregion

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

        //public virtual int GetHtmlElementCountCore(string xPath)
        //{

        //    //HtmlElementInfo[] results;
        //    //let query = document.evaluate(xpath, parent || document,
        //    //    null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
        //    //for (let i = 0, length = query.snapshotLength; i < length; ++i)
        //    //{
        //    //    results.push(query.snapshotItem(i));
        //    //}
        //    //return results;

        //    //EvaluateScript(string.Format("var el = document.evaluate('{0}', document, null, XPathResult.ANY_TYPE, null).iterateNext(); el.scrollIntoView();", xpath.Replace('\'', '\"')),
        //    //    1000); ORDERED_NODE_SNAPSHOT_TYPE

        //    //" + xPath.Replace('\'', '\"') +
        //    int retVal = 0;
        //    var script =
        //        "function foo(){let xPathResult = document.evaluate('" + xPath.Replace('\'', '\"') + "', document, null, XPathResult.ANY_TYPE, null);" +
        //        "let nodes = [];  let node = xPathResult.iterateNext();" +
        //        "while (node) { nodes.push(node); node = xPathResult.iterateNext();} return nodes.map(x => ({outerHTML: x.outerHTML}));}foo()";

        //    //var script = "function foo(){const nodes = [];return nodes;}foo();";

        //    Task<JavascriptResponse> result = Browser.EvaluateScriptAsync(script);
        //    result.Wait();
        //    //if (result.Result.Success && result.Result.Result != null)
        //    //    retVal = Convert.ToInt32(result.Result.Result);

        //    List<object> abc = (List<object>)result.Result.Result;
        //    foreach (System.Dynamic.ExpandoObject item in abc)
        //    {
        //        // Console.WriteLine(item._data.ToString());

        //        IDictionary<string, object> propertyValues = item;

        //        foreach (var property in propertyValues.Keys)
        //        {
        //            //retVal.Add(new HtmlElementInfo(propertyValues[property].ToString()));
        //            break;
        //            //Console.WriteLine(String.Format("{0} : {1}", property, propertyValues[property]));
        //        }
        //    }



        //    return retVal;
        //}
    }
}
