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
        ClipboardMonitor clipboardMonitor;
        bool connectedToSf = false;
        bool resultsShowing = false;

        EnterpriseSoap.SforceService svcEntepriseSoap;
        ApexSvcSoap.ApexService svcApexSoap;

        FileSystemWatcher settingsWatch = new FileSystemWatcher();

        Results frmResults = new Results();

        public Container()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;

            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += clipboardMonitor_ClipboardChanged;

            IndicateSfConnection();
            LoadWebServices();
            IndicateSfConnection();

            SubscribeToResultFormEvents();
            SubscribeToSettingsChanges();

            base.OnLoad(e);
        }

        private void SubscribeToSettingsChanges()
        {
            FileInfo fi = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            settingsWatch.Path = fi.DirectoryName;
            settingsWatch.Filter = "*.config";
            settingsWatch.NotifyFilter = NotifyFilters.LastWrite;

            settingsWatch.Changed += new FileSystemEventHandler(SettingsChanged);
            settingsWatch.EnableRaisingEvents = true;
        }

        private void SettingsChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine(e.ChangeType + " at " + e.FullPath);
        }

        private void SubscribeToResultFormEvents()
        {
            frmResults.FormShown += (object sender, EventArgs e) => { resultsShowing = true; Debug.WriteLine("Results Shown"); };
            frmResults.FormHidden += (object sender, EventArgs e) => { resultsShowing = false;  Debug.WriteLine("Results Hidden"); };

        }

        private void IndicateSfConnection()
        {
            Bitmap bmp = connectedToSf ? 
                SfdcIdUpConverter.Properties.Resources.connectionActive :
                SfdcIdUpConverter.Properties.Resources.connectionInactive;

            trayIconApp.Icon = Icon.FromHandle(bmp.GetHicon());
        }

        void clipboardMonitor_ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            try
            {
                // don't process anything if not connected
                if (!connectedToSf) return;

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

        private void LoadWebServices()
        {
            try
            {

                svcEntepriseSoap = new EnterpriseSoap.SforceService();
                EnterpriseSoap.LoginResult loginResult = svcEntepriseSoap.login("robertgelb@iheartmedia.com", "Robdogg2");
                var SessionID = loginResult.sessionId;
                var SessionURL = loginResult.serverUrl;

                svcEntepriseSoap.SessionHeaderValue = new EnterpriseSoap.SessionHeader { sessionId = loginResult.sessionId };
                svcEntepriseSoap.Url = SessionURL;

                svcApexSoap = new ApexSvcSoap.ApexService();
                svcApexSoap.SessionHeaderValue = new ApexSvcSoap.SessionHeader { sessionId = SessionID };

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
                
                connectedToSf = true;
            }
            catch (Exception ex)
            {
                connectedToSf = false;
            }
        }

        private string lastClipboardText = "";
        private DateTime lastClipboardOn = DateTime.MinValue;

        private void ProcessClipboard(string contents)
        {
            // we should be looking 15 digit value
            if (contents.Length != 15) return;

            if (lastClipboardText == contents && DateTime.Now.Subtract(lastClipboardOn).TotalMilliseconds < 500)
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

            string[] lines = log.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

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


        private void mnuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuItemAbout_Click(object sender, System.EventArgs e)
        {
            string msg = @"SfdcId Up Converter";
            MessageBox.Show(msg, "About UpConverter", MessageBoxButtons.OK);
        }

        private void mnuItemSettings_Click(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, null);
        }
        
    }
}
