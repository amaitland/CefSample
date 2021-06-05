using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefBrowserWrapper
{
    public class HtmlElementInfo
    {
        public string OuterHTML { get; set; } = "";

        public HtmlElementInfo(string outerHTML)
        {
            OuterHTML = outerHTML;
        }
    }
}
