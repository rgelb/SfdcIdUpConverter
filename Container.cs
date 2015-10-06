using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Microsoft.VisualBasic;

namespace SfdcIdUpConverter
{
    public partial class Container : Form
    {
        #region Private Variables
        private enum ConnectionStatus
        {
            Connecting,
            Connected,
            Error
        }

        ClipboardMonitor clipboardMonitor;
        ConnectionStatus sfConnectionStatus = ConnectionStatus.Connecting;
        bool resultsShowing;
        string lastError = string.Empty;
        PartnerSoap.SforceService svcPartnerSoap;
        ApexSvcSoap.ApexService svcApexSoap;
        readonly FileSystemWatcher settingsWatch = new FileSystemWatcher();
        readonly Results frmResults = new Results();
        private string lastClipboardText = "";
        private DateTime lastClipboardOn = DateTime.MinValue;        
        #endregion

        #region Constructors
        public Container()
        {
            InitializeComponent();
        } 
        #endregion

        #region Private Methods
        private void ConnectToSf()
        {
            IndicateSfConnection();
            LoadWebServices();
            IndicateSfConnection();
        }

        private void SubscribeToSettingsChanges()
        {
            FileInfo fi = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            settingsWatch.Path = fi.DirectoryName;
            settingsWatch.Filter = fi.Name;
            settingsWatch.NotifyFilter = NotifyFilters.LastWrite;

            settingsWatch.Changed += SettingsChanged;
            settingsWatch.EnableRaisingEvents = true;
        }

        private void SubscribeToResultFormEvents()
        {
            frmResults.FormShown += (sender, e) => { resultsShowing = true; Debug.WriteLine("Results Shown"); };
            frmResults.FormHidden += (sender, e) => { resultsShowing = false; Debug.WriteLine("Results Hidden"); };

        }

        private void IndicateSfConnection()
        {
            Bitmap bmp;
            switch (sfConnectionStatus)
            {
                case ConnectionStatus.Connecting:
                    bmp = SfdcIdUpConverter.Properties.Resources.connectionInactive;
                    trayIconApp.Text = "Connecting to Salesforce...";
                    break;
                case ConnectionStatus.Connected:
                    bmp = SfdcIdUpConverter.Properties.Resources.connectionActive;
                    trayIconApp.Text = "SfdcId Up Converter - Connected";
                    break;
                case ConnectionStatus.Error:
                    bmp = SfdcIdUpConverter.Properties.Resources.connectionError;
                    trayIconApp.Text = ("Connection Error -  " + lastError).Left(63);
                    break;
                default:
                    MessageBox.Show("Invalid connection status");
                    return;
            }

            trayIconApp.Icon = Icon.FromHandle(bmp.GetHicon());
        }

        private void LoadWebServices()
        {
            try
            {
                svcPartnerSoap = new PartnerSoap.SforceService();
                svcPartnerSoap.AdjustUrl();

                PartnerSoap.LoginResult loginResult = svcPartnerSoap.login(AppSettings.UserName, AppSettings.Password);

                var SessionID = loginResult.sessionId;
                var SessionURL = loginResult.serverUrl;

                svcPartnerSoap.SessionHeaderValue = new PartnerSoap.SessionHeader { sessionId = loginResult.sessionId };
                svcPartnerSoap.Url = SessionURL;

                svcApexSoap = new ApexSvcSoap.ApexService();
                svcApexSoap.SessionHeaderValue = new ApexSvcSoap.SessionHeader { sessionId = SessionID };
                svcApexSoap.AdjustUrl(SessionURL);

                // set debugging headers
                ApexSvcSoap.LogInfo infoAll = new ApexSvcSoap.LogInfo();
                infoAll.category = ApexSvcSoap.LogCategory.All;
                infoAll.level = ApexSvcSoap.LogCategoryLevel.Debug;

                var infoApex = new ApexSvcSoap.LogInfo();
                infoApex.category = ApexSvcSoap.LogCategory.Apex_code;
                infoApex.level = ApexSvcSoap.LogCategoryLevel.Debug;

                var infoProfiling = new ApexSvcSoap.LogInfo();
                infoProfiling.category = ApexSvcSoap.LogCategory.Apex_profiling;
                infoProfiling.level = ApexSvcSoap.LogCategoryLevel.Debug;

                var infoDB = new ApexSvcSoap.LogInfo();
                infoDB.category = (ApexSvcSoap.LogCategory.Db);
                infoDB.level = (ApexSvcSoap.LogCategoryLevel.Debug);

                ApexSvcSoap.DebuggingHeader debugHeader = new ApexSvcSoap.DebuggingHeader();
                debugHeader.debugLevel = ApexSvcSoap.LogType.Debugonly;
                debugHeader.categories = new ApexSvcSoap.LogInfo[] { infoAll, infoApex, infoDB, infoProfiling };

                svcApexSoap.DebuggingHeaderValue = debugHeader;

                sfConnectionStatus = ConnectionStatus.Connected;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                sfConnectionStatus = ConnectionStatus.Error;
            }
        }

        private void ProcessClipboard(string contents)
        {
            // we should be looking 15 digit value
            if (contents.Length != 15) return;

            // sometimes clipboard event fires twice...guard against that
            if (lastClipboardText == contents && DateTime.Now.Subtract(lastClipboardOn).TotalMilliseconds < 2000)
                return;

            lastClipboardText = contents;
            lastClipboardOn = DateTime.Now;

            string f = @"private String IdToObject(Id someId) {
                            Schema.SObjectType objectType = someId.getSObjectType();
                            return String.valueOf(objectType + '/' + Id.valueOf(someId));
                        }

                        System.debug(IdToObject('{0}'));
                        ";
            string apexCode = f.Replace("{0}", contents);

            ApexSvcSoap.ExecuteAnonymousResult result = svcApexSoap.executeAnonymous(apexCode);

            if (svcApexSoap.DebuggingInfoValue != null)
            {
                Tuple<string, string> returnSet = ParseApexLog(svcApexSoap.DebuggingInfoValue.debugLog);
                ShowResults(returnSet.Item1, returnSet.Item2);

            }
        }

        private void ShowResults(string objectType, string objectId)
        {
            frmResults.Opacity = 1;
            frmResults.ObjectType = objectType;
            frmResults.ObjectId = objectId;


            // several conditions

            //  1.  Form never shown before (resultsShowing = false)
            //  2.  Form shown              (resultsShowing = true)
            //  3.  Form hidden             (resultsShowing = false)

            frmResults.Show();
            frmResults.UpdateResults(resultsShowing);
        }

        private Tuple<string, string> ParseApexLog(string log)
        {
            // Debug.WriteLine(log);

            string[] lines = log.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains("|USER_DEBUG|") && line.Contains("|DEBUG|"))
                {
                    // this line is it
                    string[] elements = line.Split("|".ToCharArray());

                    // get last element
                    string returnValue = elements[elements.Length - 1];

                    string[] constituentParts = returnValue.Split('/');

                    string objectType = constituentParts[0];
                    string objectId = constituentParts[1];

                    Debug.WriteLine("Object Type = " + objectType);
                    Debug.WriteLine("Object Id = " + objectId);


                    return new Tuple<string, string>(objectType, objectId);
                }
            }

            return null;
        }

        private void AddErrorToLog(Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Trace.WriteLine(ex.ToString());
        } 
        #endregion

        #region Events

        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;

            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += clipboardMonitor_ClipboardChanged;

            ConnectToSf();

            SubscribeToResultFormEvents();
            SubscribeToSettingsChanges();

            base.OnLoad(e);
        }

        private void SettingsChanged(object sender, FileSystemEventArgs e)
        {
            // we turn off, then on Raising events because 
            // the FileSystemWatcher sends multiple events per single change

            settingsWatch.EnableRaisingEvents = false;
            try
            {
                Debug.WriteLine(e.ChangeType + " at " + e.FullPath);

                // set to reconnect
                sfConnectionStatus = ConnectionStatus.Connecting;
                ConnectToSf();
            }
            finally
            {
                settingsWatch.EnableRaisingEvents = true;
            }
        }

        private void clipboardMonitor_ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            try
            {
                // don't process anything if not connected
                if (sfConnectionStatus != ConnectionStatus.Connected) return;

                IDataObject iData = e.DataObject;
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    string contents = (string)iData.GetData(DataFormats.Text);
                    Debug.WriteLine("Clipboard Change Detected: " + contents);

                    ProcessClipboard(contents);
                }

            }
            catch (Exception ex)
            {
                AddErrorToLog(ex);
            }
        }

        private void mnuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuItemAbout_Click(object sender, System.EventArgs e)
        {
            string msg = @"SfdcId 15 to 18 Up Converter" + Environment.NewLine + "Copyright © Robert Gelb 2015";
            MessageBox.Show(msg, "About UpConverter", MessageBoxButtons.OK);
        }

        private void mnuItemSettings_Click(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, null);
        }

        private void tmrRefreshConnection_Tick(object sender, EventArgs e)
        {
            // the token we get from the connection is good for 12 hours...
            // we are gonna refresh every 6 hours just to be sure
            // set to reconnect.  Only do this if we are currently connected

            if (sfConnectionStatus != ConnectionStatus.Connected) return;

            sfConnectionStatus = ConnectionStatus.Connecting;
            ConnectToSf();
        } 
        #endregion
        
    }
}
