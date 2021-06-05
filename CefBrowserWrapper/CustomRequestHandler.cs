using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;

namespace CefBrowserWrapper
{
    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {

        string _userAgent;
        public CustomResourceRequestHandler(string userAgent)
        {
            _userAgent = userAgent;

        }
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            //my logic gonna live here
            request.SetHeaderByName("user-agent", _userAgent, true);
            return CefReturnValue.Continue;
        }
    }

    public class CustomRequestHandler : RequestHandler
    {

        string _proxyLogin;
        string _proxyPassword;
        string _userAgent;
        public CustomRequestHandler(string proxyLogin, string proxyPassword,string userAgent)
        {
            _proxyLogin = proxyLogin;
            _proxyPassword = proxyPassword;
            _userAgent = userAgent;
        }
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler(_userAgent);
        }
        

        protected override bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if (!String.IsNullOrEmpty(_proxyLogin))
            {
                callback.Continue(_proxyLogin, _proxyPassword);
                return true;
            }

            return false;
        }

       
    }
}