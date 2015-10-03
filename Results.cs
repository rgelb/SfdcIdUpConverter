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
        #region Private Variables

        private Color originalBackColor;
        private Color clickedBackColor;

        #endregion

        #region Constructors
        public Results()
        {
            InitializeComponent();
        } 
        #endregion

        #region Public Properties
        public event EventHandler FormHidden = delegate { };
        public event EventHandler FormShown = delegate { };

        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        #endregion

        #region Events
        protected override void OnLoad(EventArgs e)
        {
            const int OFFSET = 3;
            int left = Screen.PrimaryScreen.WorkingArea.Width - this.Width - OFFSET;
            int top = Screen.PrimaryScreen.WorkingArea.Height - this.Height - OFFSET;
            this.Location = new Point(left, top);

            // subscribe to all the mouse move events
            this.MouseMove += HandleFormMouseMove;
            lblIdInfo.MouseMove += HandleFormMouseMove;
            btnClose.MouseMove += HandleFormMouseMove;
            btnCopy.MouseMove += HandleFormMouseMove;

            originalBackColor = this.BackColor;
            clickedBackColor = Color.AntiqueWhite;

            base.OnLoad(e);
        }
        private void Results_Paint(object sender, PaintEventArgs e)
        {
            // draw a border around the form
            try
            {
                const int THICKNESS = 2;
                using (Graphics g = this.CreateGraphics())
                {
                    Pen pen = new Pen(Color.DarkBlue, THICKNESS);
                    Rectangle rect = e.ClipRectangle;

                    // increment X/Y so that drawing starts at 0,0 (instead of -1,-1)
                    rect.X++;
                    rect.Y++;

                    // reduce rect by thickness of the pen, so it draws on form, not outside of it
                    rect.Width -= THICKNESS;
                    rect.Height -= THICKNESS;

                    g.DrawRectangle(pen, rect);
                    pen.Dispose();
                }
            }
            catch 
            {
                // ignore
            }
        }


        private void HandleFormMouseMove(object sender, MouseEventArgs e)
        {
            // stop all the fading or whatever else since user moved the mouse over
            DisposeTimers();
            this.Opacity = 1;
            InitializeTimers();
        }

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

        private void btnCopy_MouseDown(object sender, MouseEventArgs e)
        {
            // indicate that button is clicked
            this.BackColor = clickedBackColor;
        }

        private void btnCopy_MouseUp(object sender, MouseEventArgs e)
        {
            // revert to original color
            this.BackColor = originalBackColor;
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

        #endregion

        #region Public Methods 
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
        #endregion

        #region Private Methods
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

        private void AnimateFormOffScreenThenOn()
        {
            const int OFFSET = 10;  // to make sure the form is not visible
            const int STEP = 5;     // pixels to move for each step
            int pixelsToMove = this.Height + OFFSET;
            int initialTopPos = this.Top;
            int pixelsMoved = 0;

            try
            {
                // turn off painting for the duration animation
                this.Paint -= Results_Paint;

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
            finally
            {
                this.Paint += Results_Paint;
                this.Invalidate();  // force repaint
            }
        }

        private void HideForm()
        {
            this.Hide();
            FormHidden.RaiseEvent(this, EventArgs.Empty);
        }


        #endregion
    }
}
