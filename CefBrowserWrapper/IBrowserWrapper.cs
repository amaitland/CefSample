using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefBrowserWrapper
{
    public interface IBrowserWrapper : IDisposable
    {
        void LoadUrl(string url);

        string ScreenShot(string fileName);

        void Scroll(string xpath);
        void SetValue(string xpath, string value);
        void Click(string xpath);
        bool ElementExists(string xpath);

        string ScreenShotCore(string fileName);

        void EvaluateScriptCore(string script, int timeOutMs = 0);

        void EvaluateScript(string script, int timeOutMs = 0);


        string GetHtml();

        void CloseBrowser();

        void SendKey(string xpath, long charCode);

        void SelectListOption();

        int GetHtmlElementCount(string xpath);
    }
}
