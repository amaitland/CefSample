using CefSharp;
using CefSharp.WinForms;
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
using System.Runtime.InteropServices;



namespace CefBrowserWrapper
{
    

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        public event EventHandler<ChromiumWebBrowser> ProceedButtonClicked;
        //private bool _formShown = false;
        ManualResetEvent _manualResetEvent;
        public Form1(ManualResetEvent manualResetEvent)
        {
            _manualResetEvent = manualResetEvent;
            InitializeComponent();
            //tabControl1.TabStop = false;
        //    this.WindowState = FormWindowState.Maximized;
        }

        public void ShowBrowser(ChromiumWebBrowser browser)
        {
            if(!this.Visible)
            {
                this.Show();
            }
            //if (!_formShown) this.Show();
            if(browser.Parent==null)
            {
                TabPage tp = AddAnotherTabLikeThis(browser);//AddAnotherTabLikeThis(browser);
                //TabPage tp = new TabPage();
                //tabControl1.TabPages.Add(tp);

                //tp.Controls.Add(browser);
                //browser.Dock = DockStyle.Fill;
            }
            else
            {
                if(!tabControl1.TabPages.Contains((browser.Parent as TabPage)))
                {
                    
                    tabControl1.TabPages.Add((browser.Parent as TabPage));
                }
                    
            }
            if (browser.Parent != null) { 
                tabControl1.SelectedTab = (browser.Parent as TabPage);
                (browser.Parent as TabPage).Text = "Loading...";
            }

        }
        public void SetTabName(ChromiumWebBrowser browser, string name)
        {
            if(name.Length>15)
                name = name.Substring(0, 15)+"...";
            if (browser.Parent != null)
            {
                (browser.Parent as TabPage).Text = name;
            }
        }
        public void HideBrowser(ChromiumWebBrowser browser)
        {
            TabPage tp = (TabPage)browser.Parent;
            tabControl1.TabPages.Remove(tp);

            if (tabControl1.TabPages.Count == 0) Hide();
        }
  

        //to avoid form stealing focus from the main form
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public bool ShowFormFlag = false;
        public bool HideFormFlag = false;
       
        private void Form1_Shown(object sender, EventArgs e)
        {
            //_formShown = true;
            _manualResetEvent.Set();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private TabPage AddAnotherTabLikeThis(ChromiumWebBrowser wb)
        {

            #region Generate controls and put them into the tab

            TabPage tp = new TabPage();
            
            //System.Windows.Forms.TextBox tb;
            //System.Windows.Forms.ProgressBar pb;
            System.Windows.Forms.Button bt;
           

            //wb = new System.Windows.Forms.WebBrowser();
            bt = new System.Windows.Forms.Button();
            //pb = new System.Windows.Forms.ProgressBar();
            //tb = new System.Windows.Forms.TextBox();


            //this.tabControl1.SuspendLayout();
            //tp.SuspendLayout();
            //this.SuspendLayout();

            tabControl1.TabPages.Add(tp);


            tp.Controls.Add(wb);
            tp.Controls.Add(bt);
            //tp.Controls.Add(pb);
            //tp.Controls.Add(tb);

            tp.Location = new System.Drawing.Point(4, 25);
            tp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            //tp.Name = "tabPage2";
            tp.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            tp.Size = new System.Drawing.Size(1061, 681);
            tp.TabIndex = 0;
            //tp.Text = "tabPage2";
            //tp.BackColor = Color.Green;
            //tp.UseVisualStyleBackColor = true;
            
            // 
            // webBrowser1
            // 
            wb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            wb.Dock = DockStyle.Fill;
            wb.Location = new System.Drawing.Point(8, 32);
            wb.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            wb.MinimumSize = new System.Drawing.Size(27, 25);
            //wb.Name = "webBrowser1";
            wb.Size = new System.Drawing.Size(1043, 601);
            wb.TabIndex = 34;
            // 
            // tbUrl
            // 
            //tb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //| System.Windows.Forms.AnchorStyles.Left)
            //| System.Windows.Forms.AnchorStyles.Right)));
            //tb.BackColor = System.Drawing.SystemColors.ControlLight;
            //tb.Location = new System.Drawing.Point(0, 0);
            //tb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            //tb.Name = "tbUrl";
            //tb.ReadOnly = true;
            //tb.Size = new System.Drawing.Size(895, 22);
            //tb.TabIndex = 2;

            // 
            // btUserMessage
            // 
            bt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            bt.Dock = System.Windows.Forms.DockStyle.Bottom;
            bt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            bt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            bt.Location = new System.Drawing.Point(4, 641);
            bt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            bt.Name = "btUserMessage";
            bt.Size = new System.Drawing.Size(1053, 36);
            bt.TabIndex = 33;
            bt.Text = "btUserMessage";
            bt.UseVisualStyleBackColor = true;
            bt.Visible = false;
            // 
            // pbPageLoad
            // 
            //pb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            //pb.Location = new System.Drawing.Point(901, 1);
            //pb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            //pb.Name = "pbPageLoad";
            //pb.Size = new System.Drawing.Size(153, 23);
            //pb.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            //pb.TabIndex = 31;

            //this.tabControl1.ResumeLayout(false);
            //tp.ResumeLayout(false);
            //tp.PerformLayout();
            //this.ResumeLayout(false);
            #endregion
            return tp;
        }


        private void Bt_Click(object sender, EventArgs e)
        {
            


            ChromiumWebBrowser wb = null;
            TabPage tp = (TabPage)(sender as Button).Parent;
            foreach (Control control in tp.Controls)
            {
                if(typeof(ChromiumWebBrowser)==control.GetType())
                {
                    ProceedButtonClicked?.Invoke(this, (control as ChromiumWebBrowser));
                }
                else if (typeof(Button) == control.GetType())
                {
                    control.Visible = false;
                }
            }

            //form.btUserMessage.Visible = false;
            //form.btUserMessage.Text = "";
            //form.timer1.Stop();
            //form.timer1.Enabled = false;
            

        }

        public void ShowLoadProgressBar(ChromiumWebBrowser chromiumWebBrowser, bool show)
        {
            foreach (Control control in chromiumWebBrowser.Parent.Controls)
            {
                if (typeof(ProgressBar) == control.GetType())
                {
                    control.Visible = show;
                }
            }
        }

        internal void ActivateUserInputForBrowser(ChromiumWebBrowser chromiumWebBrowser)
        {
            ShowBrowser(chromiumWebBrowser);
            foreach (Control control in chromiumWebBrowser.Parent.Controls)
            {
                if (typeof(Button) == control.GetType())
                {
                    control.Visible = true;
                }
            }
            //form.btUserMessage.Text = "Осуществите действия и нажмите эту кнопку для продолжения";
            //form.btUserMessage.Visible = true;
            //form.timer1.Interval = 400;
            //form.timer1.Start();
            //form.timer1.Enabled = true;
            //waitingInput = true;
        }

        internal void SetForeground()
        {
            SetForegroundWindow(this.Handle);
        }
    }
}
