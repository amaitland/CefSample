using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefBrowserWrapper
{
   
    public class SingleBrowserInfo
    {
        public ProxyInfo Proxy { get; set; }

        public string UserAgent { get; set; }

        public SingleBrowserInfo(string rawProxyString, string userAgent)
        {
            Proxy = new ProxyInfo(rawProxyString.Trim());
            UserAgent = userAgent;
        }


    }

    public class ProxyInfo
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Scheme { get; set; }

        public ProxyInfo(string rawProxyString)
        {
            Ip = "";
            Port = 0;
            Login = "";
            Password = "";
            Scheme = "http";

            string[] proxyStringSeparated = rawProxyString.Split(':');

            if (proxyStringSeparated.Length >= 2)//Предполагается формат адрес:порт
            {
                Ip = proxyStringSeparated[0].Trim();

                int port;
                bool resultOfConvertion = Int32.TryParse(proxyStringSeparated[1], out port);
                if (resultOfConvertion)
                {
                    Port = port;
                }
                else
                {
                    throw new Exception("Proxy port is not recognized");
                }
            }

            if (proxyStringSeparated.Length >= 4)//Предполагается формат адрес:порт:логин:пароль
            {
                Login = proxyStringSeparated[2];
                Password = proxyStringSeparated[3];
            }

            if (proxyStringSeparated.Length == 5)
                Scheme = proxyStringSeparated[4];
        }
    }

}
