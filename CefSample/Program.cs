using CefBrowserWrapper;
using CefSharp;
using System;
using System.IO;
using System.Windows.Forms;

namespace CefSample
{
    static class Program
    {
        public const string CefSharpCacheLocalPath = @"Cefsharp\Cache";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), CefSharpCacheLocalPath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var settings = new CefSharp.WinForms.CefSettings()
            {
                RootCachePath = path
            };

            Cef.Initialize(settings);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Cef.Shutdown();
        }
    }
}
