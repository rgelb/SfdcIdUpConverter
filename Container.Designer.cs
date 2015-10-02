namespace SfdcIdUpConverter
{
    partial class Container
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Container));
            this.trayIconApp = new System.Windows.Forms.NotifyIcon(this.components);
            this.mnuApp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.tmrRefreshConnection = new System.Windows.Forms.Timer(this.components);
            this.mnuApp.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayIconApp
            // 
            this.trayIconApp.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.trayIconApp.BalloonTipText = "SfdcId Up Converter";
            this.trayIconApp.BalloonTipTitle = "Adris";
            this.trayIconApp.ContextMenuStrip = this.mnuApp;
            this.trayIconApp.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIconApp.Icon")));
            this.trayIconApp.Text = "SfdcId Up Converter";
            this.trayIconApp.Visible = true;
            // 
            // mnuApp
            // 
            this.mnuApp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuItemSettings,
            this.mnuItemExit,
            this.toolStripSeparator2,
            this.mnuItemAbout});
            this.mnuApp.Name = "mnuApp";
            this.mnuApp.Size = new System.Drawing.Size(201, 76);
            // 
            // mnuItemSettings
            // 
            this.mnuItemSettings.Name = "mnuItemSettings";
            this.mnuItemSettings.Size = new System.Drawing.Size(200, 22);
            this.mnuItemSettings.Text = "Settings";
            this.mnuItemSettings.Click += new System.EventHandler(this.mnuItemSettings_Click);
            // 
            // mnuItemExit
            // 
            this.mnuItemExit.Name = "mnuItemExit";
            this.mnuItemExit.Size = new System.Drawing.Size(200, 22);
            this.mnuItemExit.Text = "Exit";
            this.mnuItemExit.Click += new System.EventHandler(this.mnuItemExit_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(197, 6);
            // 
            // mnuItemAbout
            // 
            this.mnuItemAbout.Name = "mnuItemAbout";
            this.mnuItemAbout.Size = new System.Drawing.Size(200, 22);
            this.mnuItemAbout.Text = "About SfdcUpConverter";
            this.mnuItemAbout.Click += new System.EventHandler(this.mnuItemAbout_Click);
            // 
            // tmrRefreshConnection
            // 
            this.tmrRefreshConnection.Enabled = true;
            this.tmrRefreshConnection.Interval = 21600000;
            this.tmrRefreshConnection.Tick += new System.EventHandler(this.tmrRefreshConnection_Tick);
            // 
            // Container
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Container";
            this.Text = "SfdcId Up Converter";
            this.mnuApp.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.NotifyIcon trayIconApp;
        private System.Windows.Forms.ContextMenuStrip mnuApp;
        private System.Windows.Forms.ToolStripMenuItem mnuItemExit;
        private System.Windows.Forms.ToolStripMenuItem mnuItemAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuItemSettings;
        private System.Windows.Forms.Timer tmrRefreshConnection;
    }
}

