using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using System;
using System.Windows.Forms;

namespace CefBrowserWrapper
{
    public partial class BrowserForm : Form
    {
        public BrowserForm()
        {
            InitializeComponent();
        }

        public void AddBrowserTab()
        {
            //var browser = new CefSharp.WinForms.ChromiumWebBrowser("https://google.com");
            var tb = new TextBox { Text = "Testing", Dock = DockStyle.Fill };

            AddTab(tb);

            tabControl1.SelectedIndex = tabControl1.TabCount - 1;

            //if (browser.Parent != null)
            //{ 
            //    tabControl1.SelectedTab = (browser.Parent as TabPage);
            //    (browser.Parent as TabPage).Text = "Loading...";
            //}
        }    
  

        //to avoid form stealing focus from the main form
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private TabPage AddTab(Control control)
        {
            var tp = new TabPage("Browser");

            tp.Controls.Add(control);

            tp.Location = new System.Drawing.Point(4, 25);
            tp.Margin = new Padding(4, 4, 4, 4);
            tp.Padding = new Padding(4, 4, 4, 4);
            tp.Size = new System.Drawing.Size(1061, 681);
            tp.TabIndex = 0;

            
            control.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left
            | AnchorStyles.Right);
            control.Dock = DockStyle.Fill;
            control.Location = new System.Drawing.Point(8, 32);
            control.Margin = new Padding(4, 4, 4, 4);
            control.MinimumSize = new System.Drawing.Size(27, 25);
            control.Size = new System.Drawing.Size(1043, 601);
            control.TabIndex = 34;

            tabControl1.TabPages.Add(tp);

            return tp;
        }
    }
}
