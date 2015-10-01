using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SfdcIdUpConverter
{
    public partial class Results : Form
    {
        public event EventHandler FormHidden = delegate { };
        public event EventHandler FormShown = delegate { };

        public Results()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //PopulateResultLabel();

            // now size it
            // btnCopy.Left = lblIdInfo.Width + OFFSET;
            // this.Size = new Size(lblIdInfo.Width + OFFSET + btnCopy.Width, btnCopy.Height);
            const int OFFSET = 3;
            int left = Screen.PrimaryScreen.WorkingArea.Width - this.Width - OFFSET;
            int top = Screen.PrimaryScreen.WorkingArea.Height - this.Height - OFFSET;
            this.Location = new Point(left, top);

            //// initialize timers
            //InitializeTimers();

            

            base.OnLoad(e);
        }

        private void PopulateResultLabel()
        {
            lblIdInfo.Text = string.Format("{0}\n{1}", ObjectType, ObjectId);
        }

        private void DisposeTimers()
        {
            tmrHide.Stop();
            tmrFade.Stop();
        }

        private void InitializeTimers()
        {
            tmrHide.Start();
        }

        public string ObjectType { get; set; }
        public string ObjectId { get; set; }


        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ObjectId);
            this.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DisposeTimers();
            HideForm();
        }

        private void tmrFade_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.05;

            if (this.Opacity <= 0.1)
            {
                tmrFade.Stop();
                HideForm();               
            }
        }

        private void tmrHide_Tick(object sender, EventArgs e)
        {
            // turn off this timer
            tmrHide.Stop();

            // start the fade
            tmrFade.Start();
        }

        public void UpdateResults(bool withAnimation = true)
        {
            DisposeTimers();

            // animate the form to go offscreen, the back on
            if (withAnimation)
                AnimateFormOffScreenThenOn();

            PopulateResultLabel();
            InitializeTimers();

            FormShown.RaiseEvent(this, EventArgs.Empty);
            this.Focus();
        }

        private void AnimateFormOffScreenThenOn()
        {
            const int OFFSET = 10;  // to make sure the form is not visible
            const int STEP = 5;     // pixels to move for each step
            int pixelsToMove = this.Height + OFFSET;
            int initialTopPos = this.Top;
            int pixelsMoved = 0;

            // move off screen to the bottom
            while (pixelsMoved < pixelsToMove)
            {
                pixelsMoved += STEP;
                this.Top = initialTopPos + pixelsMoved;

                Thread.Sleep(10);
            }

            // move back on the screen
            while (pixelsMoved > 0)
            {
                pixelsMoved -= STEP;
                this.Top = initialTopPos + pixelsMoved;

                Thread.Sleep(10);
            }
        }

        private void HideForm()
        {
            this.Hide();
            FormHidden.RaiseEvent(this, EventArgs.Empty);
        }


    }
}
