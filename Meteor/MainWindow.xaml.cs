using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Meteor.database;
using Meteor.sections;
using Meteor.sections.filebank;
using Meteor.updates;
using Meteor.workers;

namespace Meteor
{
    public partial class MainWindow
    {

        #region Variables

        private readonly string _appPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        private Mutex _mutex;

        //Handlers
        private readonly db_handler _dbHandler;

        //Workers
        private readonly BackgroundWorker _urlWorker = new BackgroundWorker();
        private readonly BackgroundWorker _controllerWorker = new BackgroundWorker();
        public addWorkspaceWorker AddWorkspaceWorker;
        public copyWorkspaceWorker CopyWorkspaceWorker;
        public MslWorkspaceWorker MslWorkspaceWorker;
        public ClearWorkspaceWorker ClearWorkspaceWorker;
        public BuildWorker BuildWorker;
        public DownloadWorker DownloadWorker;
        public ContentInstallerWorker ContentInstallerWorker;

        private Worker _currentWorker;

        //Controller
        private int _controllerPercent;
        private string _controllerMessage = "";
        private int _controllerStyle;
        private bool _controllerLock;

        #endregion

        public MainWindow()
        {
            Directory.SetCurrentDirectory(_appPath);
            Mutexcheck(Environment.GetCommandLineArgs());

            InitializeComponent();

            //Init functions
            #region Inits

            try
            {
                _dbHandler = new db_handler();
                MeteorCode.WriteToConsole("The connection to the database was successful.", 3);
                DatabaseUpdater updooter = new DatabaseUpdater();
                if (updooter.UpdootDatabase())
                {
                    MeteorCode.WriteToConsole("The database update was succesfull.", 0);
                }
                else
                {
                    if(File.Exists(_appPath + "/command.txt"))
                    {
                        MeteorCode.WriteToConsole("There was an issue with the database update", 2);
                    }
                }
            }
            catch
            {
                MeteorCode.WriteToConsole(
                "The connection to the database was unsuccessful. Please check that the Library is there.", 2);
            }

            Console.Text = "";
            ActiveWorkspaceTextBox.Text = _dbHandler.GetWorkspaceName(int.Parse(_dbHandler.get_property("workspace")));

            SetupWorkers();
            UpdateBackgroundColor();
            InitializeMeteor();
            MeteorCode.WriteToConsole("Welcome to Meteor !", 0);

            var updater = new Updater(_appPath);

            var args = Environment.GetCommandLineArgs();
            if (args.Length != 2) return;
            MeteorCode.WriteToConsole(args[1], 0);
            DownloadWorker.Launch(args[1]);

            #endregion
        }

        //Init procedures
        private void Mutexcheck(string[] args)
        {
            Mutex mt;
            if (Mutex.TryOpenExisting("meteor_mutex", out mt))
            {
                if (!Directory.Exists(_appPath + "/downloads/"))
                {
                    Directory.CreateDirectory(_appPath + "/downloads/");
                }

                File.WriteAllLines(_appPath + "/downloads/url.txt", args);

                var test = new Mutex(true, "meteor_url");
                Application.Current.Shutdown();
            }
            else
            {
                _mutex = new Mutex(true, "meteor_mutex");
            }
        }


        //Interface functions
        private void LockFrames()
        {
            SkinsFrame.IsEnabled = false;
            StagesFrame.IsEnabled = false;
            ConfigurationFrame.IsEnabled = false;
            WorkspaceFrame.IsEnabled = false;
            FilebankFrame.IsEnabled = false;

            _controllerLock = true;
        }

        private void UnlockFrames()
        {
            SkinsFrame.IsEnabled = true;
            StagesFrame.IsEnabled = true;
            ConfigurationFrame.IsEnabled = true;
            WorkspaceFrame.IsEnabled = true;
            FilebankFrame.IsEnabled = true;

            _controllerLock = false;
        }

        private void UpdateBackgroundColor()
        {
            var color = _dbHandler.get_property("background");

            if (color == "")
                color = "FF4764EA";

            var lgb = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };


            var gradientColor1 = ColorConverter.ConvertFromString("#FF2E2E2E");
            if (gradientColor1 != null)
            {
                var gs1 = new GradientStop
                {
                    Color = (Color)gradientColor1,
                    Offset = 0.12
                };

                var gradientColor2 = ColorConverter.ConvertFromString(color);
                if (gradientColor2 != null)
                {
                    var gs2 = new GradientStop
                    {
                        Color = (Color)gradientColor2,
                        Offset = 0.927
                    };

                    lgb.GradientStops.Add(gs1);
                    lgb.GradientStops.Add(gs2);
                }
            }

            Background = lgb;
        }
        
        //Process Status Functions
        private void ChangeProgressBarStyle(int mode)
        {
            switch (mode)
            {
                case 0:
                    statusbar.IsIndeterminate = true;
                    break;
                case 1:
                    statusbar.IsIndeterminate = false;
                    statusbar.Value = 0;
                    break;
                case 2:
                    statusbar.IsIndeterminate = false;
                    statusbar.Value = 100;
                    break;
            }

        }

        private void ChangeStatusValue(string text)
        {
            app_status_text.Content = text;
        }

        private void UpdateProgressBarValue(int val)
        {
            statusbar.Value = val;
        }

        //First boot
        private void InitializeMeteor()
        {
            //Check for first boot
            if (_dbHandler.get_property("initialized") == "0")
            {
                _dbHandler.SetWorkspaceDate(DateTime.Now.ToLongDateString(),int.Parse(_dbHandler.get_property("workspace")));
                _dbHandler.set_property_value("1", "initialized");
            }
        }


        //Section actions
        private void ChangeSection(object sender, SelectionChangedEventArgs e)
        {
            var li = (ListBoxItem)SectionList.SelectedItem;
            var val = li.Content.ToString();

            switch (val)
            {
                case "Skins":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[0];
                    MeteorCode.WriteToConsole("Section changed to Skins", 3);
                    Skins skinsPage = (Skins)SkinsFrame.Content;
                    skinsPage.LoadSkinList(skinsPage.SelectedCharacterName);
                    skinsPage.SkinsListBox.SelectedIndex = skinsPage.SelectedSlot-1;
                    break;
                case "Stages":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[1];
                                MeteorCode.WriteToConsole("Section changed to Stages", 3);
                    break;
                case "Interface":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[2];
                                MeteorCode.WriteToConsole("Section changed to Interface", 3);
                    break;
                case "FileBank":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[3];
                                MeteorCode.WriteToConsole("Section changed to Filebank", 3);
                    Filebank FilebankPage = (Filebank) FilebankFrame.Content;

                    FilebankNameplates FilebankNameplatesPage =
                        (FilebankNameplates) FilebankPage.FilebankNameplateFrame.Content;
                    FilebankNameplatesPage.ReloadNameplates();

                    FilebankSkins FilebankSkinsPage =
                        (FilebankSkins) FilebankPage.FilebankSkinFrame.Content;
                    FilebankSkinsPage.ReloadSkins();

                    FilebankPacker FilebankPackerPage =
                        (FilebankPacker) FilebankPage.FilebankPackerFrame.Content;
                    FilebankPackerPage.Reload();
                    break;
                case "Workspace":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[4];
                                MeteorCode.WriteToConsole("Section changed to Workspace", 3);
                    Workspace workspacePage = (Workspace) WorkspaceFrame.Content;
                    workspacePage.LoadWorkspaceStats();
                    break;
                case "Configuration":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[5];
                                MeteorCode.WriteToConsole("Section changed to Configuration", 3);
                    break;
                case "About":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[6];
                                MeteorCode.WriteToConsole("Section changed to About", 3);
                    break;
            }
        }

        //Workers
        private void SetupWorkers()
        {
            //Setting up Workers
            AddWorkspaceWorker = new addWorkspaceWorker(_dbHandler);
            CopyWorkspaceWorker = new copyWorkspaceWorker(_dbHandler);
            MslWorkspaceWorker = new MslWorkspaceWorker(_dbHandler);
            ClearWorkspaceWorker = new ClearWorkspaceWorker(_dbHandler);
            BuildWorker = new BuildWorker(_dbHandler);
            DownloadWorker = new DownloadWorker(_dbHandler);
            ContentInstallerWorker = new ContentInstallerWorker(_dbHandler);

            //Setting up BackgroundWorkers
            _urlWorker.DoWork += UrlWorkerDoWork;
            _urlWorker.ProgressChanged += UrlWorkerReport;
            _urlWorker.WorkerReportsProgress = true;
            _controllerWorker.DoWork += ControllerWorkerDoWork;
            _controllerWorker.ProgressChanged += ControllerWorkerProgress;
            _controllerWorker.WorkerReportsProgress = true;
            _controllerWorker.RunWorkerCompleted += ControllerWorkerCompleted;

            //Launching BackgroundWorkers
            _controllerWorker.RunWorkerAsync();
            _urlWorker.RunWorkerAsync();

        }

        private Worker CheckWorkerStatus()
        {

            List<Worker> workers = new List<Worker> { AddWorkspaceWorker, CopyWorkspaceWorker, MslWorkspaceWorker, ClearWorkspaceWorker, BuildWorker, DownloadWorker, ContentInstallerWorker };

            foreach (var worker in workers)
            {
                var workerStatus = worker.Status;
               
                if (workerStatus != 0)
                {
                    if (workerStatus == 3)
                    {
                        RefreshWorker(worker);
                        return worker;
                    }
                    RefreshWorker(worker);
                }
            }

            return null;
        }

        private void RefreshWorker(Worker worker)
        {
            switch (worker.Status)
            {
                //Default
                default:
                    _controllerStyle = worker.Style;
                    _controllerMessage = "default message";
                    _controllerWorker.ReportProgress(-1);
                    break;
                //Worker has started
                case 1:
                    if (!_controllerLock)
                    {
                        _controllerStyle = worker.Style;
                        _controllerMessage = worker.Message;
                        _controllerWorker.ReportProgress(1);

                    }
                    break;

                //Worker updates it's progress
                case 2:
                    _controllerMessage = worker.Message;
                    _controllerPercent = worker.Completion;
                    _controllerWorker.ReportProgress(2);
                    break;

                //Worker has completed the task
                case 3:
                    _controllerMessage = worker.Message;
                    _controllerWorker.ReportProgress(3);
                    worker.Status = 0;
                    break;
            }
        }

        private void PostWork(Worker worker)
        {
            Workspace workspace = (Workspace)WorkspaceFrame.Content;
            Skins skins = (Skins) SkinsFrame.Content;
            switch (worker?.Name)
            {
                case "addWorkspaceWorker":
                    workspace.ReloadWorkspacesList();
                    workspace.LoadWorkspaceStats();
                    MeteorCode.WriteToConsole("Workspace was successfully added", 0);
                    break;

                case "ClearWorkspaceWorker":
                    workspace.LoadWorkspaceStats();
                    MeteorCode.WriteToConsole("Workspace was successfully cleared", 0);
                    break;
                case "copyWorkspaceWorker":
                    workspace.LoadWorkspaceStats();
                    MeteorCode.WriteToConsole("Workspace was successfully synced from the active workspace", 0);
                    break;
                case "BuildWorker":
                    _dbHandler.upBuildCount(int.Parse(_dbHandler.get_property("workspace")));
                    workspace.LoadWorkspaceStats();
                    MeteorCode.WriteToConsole("Workspace built!",0);
                    break;
                case "DownloadWorker":
                    ContentInstallerWorker.Launch();
                    MeteorCode.WriteToConsole("Download successful", 0);
                    break;
                case "ContentInstallerWorker":
                    skins.LoadSkinList(skins.SelectedCharacterName);
                    MeteorCode.WriteToConsole("Content added!", 0);
                    break;

            }
        }

        private void UrlWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Mutex meteorUrl;
            var y = 1;
            while (true)
                if (Mutex.TryOpenExisting("meteor_url", out meteorUrl))
                {
                    meteorUrl.Dispose();
                    _urlWorker.ReportProgress(y);
                    y = y == 99 ? 1 : y;
                }
            // ReSharper disable once FunctionNeverReturns
        }

        private void UrlWorkerReport(object sender, ProgressChangedEventArgs e)
        {
            if (File.Exists(_appPath + "/downloads/url.txt"))
            {
                var lines = File.ReadAllLines(_appPath + "/downloads/url.txt");

                            MeteorCode.WriteToConsole("Url Detected", 0);
                DownloadWorker.Launch(lines[1]);

                File.Delete(_appPath + "/downloads/url.txt");
            }
        }

        private void ControllerWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var workerFinished = false;
            while (!workerFinished)
            {
                Thread.Sleep(75);

                var current = CheckWorkerStatus();
                _currentWorker = current;

                if (_currentWorker == null) return;
                workerFinished = true;

            }
        }

        private void ControllerWorkerProgress(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    LockFrames();
                    
                    ChangeStatusValue(_controllerMessage);

                    if (_controllerStyle == 0)
                    {
                        ChangeProgressBarStyle(0);
                    }
                    if (_controllerStyle == 1)
                    {
                        ChangeProgressBarStyle(1);
                    }
                    break;

                case 2:
                    UpdateProgressBarValue(_controllerPercent);
                    ChangeStatusValue(_controllerMessage);
                    break;

                case 3:
                    ChangeStatusValue(_controllerMessage);
                    ChangeProgressBarStyle(2);
                    UnlockFrames();
                    break;

            }
        }

        private void ControllerWorkerCompleted(object sender, AsyncCompletedEventArgs e)
        {
            PostWork(_currentWorker);
            _controllerWorker.RunWorkerAsync();
        }

    }
}