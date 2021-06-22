using CefBrowserWrapper;
using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btCreateAnotherBrowser.Enabled = false;

            UniCefBrowserWrapperFactory.CefGlobalInit();
        }
        CefBrowserWrapperFactoryBase _factory;
        private void btCreateFormHostingBrowsers_Click(object sender, EventArgs e)
        {
            btCreateAnotherBrowser.Enabled = true;
            btCreateFormHostingBrowsers.Enabled = false;

            Task.Run(
                            () =>
                            {
                                
                                _factory = new UniCefBrowserWrapperFactory(true);
                            });
        }


        private void btCreateAnotherBrowser_Click(object sender, EventArgs e)
        {
            new Thread(delegate ()
            {
                Task.Run(
                             () =>
                             {
                                 //while (true)
                                 //{
                                     var browser2 = _factory.Create(true, true, 10000, new SingleBrowserInfo("", ""));
                                 browser2.LoadUrl("http://webasyst.synoparser.ru/index.php?ukey=product&productID=4460");
                                 // browser2.LoadUrl("chrome://inducebrowsercrashforrealz");
                                 //}
                                 //Thread.Sleep(3000);
                             });
            }).Start();
            

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _factory.Dispose();

            Cef.Shutdown();
        }
    }
}
