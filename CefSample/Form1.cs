using System;
using System.Windows.Forms;

namespace CefSample
{
    public partial class Form1 : Form
    {
        private CefBrowserWrapper.BrowserForm browserForm;

        public Form1()
        {
            Load += FormLoad;
            
            InitializeComponent();
            
            btCreateAnotherBrowser.Enabled = false;
        }

        private void FormLoad(object sender, EventArgs e)
        {
            Location = new System.Drawing.Point(0, 0);
        }

        private void btCreateFormHostingBrowsers_Click(object sender, EventArgs e)
        {
            btCreateAnotherBrowser.Enabled = true;
            btCreateFormHostingBrowsers.Enabled = false;

            browserForm = new CefBrowserWrapper.BrowserForm();
            browserForm.StartPosition = FormStartPosition.CenterScreen;

            browserForm.Show();
        }


        private void btCreateAnotherBrowser_Click(object sender, EventArgs e)
        {

            browserForm.AddBrowserTab();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
