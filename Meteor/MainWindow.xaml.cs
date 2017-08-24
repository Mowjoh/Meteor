using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        //Database
        public MeteorDatabase meteorDatabase = new MeteorDatabase();

        //Workers
        private readonly BackgroundWorker _urlWorker = new BackgroundWorker();
        private readonly BackgroundWorker _controllerWorker = new BackgroundWorker();
        public BuildWorker BuildWorker;
        public DownloadWorker DownloadWorker;

        private Worker _currentWorker;

        //Controller
        private int _controllerPercent;
        private string _controllerMessage = "";
        private int _controllerStyle;
        private bool _controllerLock;

        private int clearCount = 0;

        private int activeSection = 0;
        private bool _loading = false;
        

        #endregion

        public MainWindow()
        {
            Directory.SetCurrentDirectory(_appPath);
            Mutexcheck(Environment.GetCommandLineArgs());

            InitializeComponent();

            SetupWorkers();

            UpdateBackgroundColor();
            InitializeMeteor();

            LoadWorkspaces();

            var updater = new Updater(_appPath);


            var args = Environment.GetCommandLineArgs();
            if (args.Length != 2) return;
            MeteorCode.WriteToConsole(args[1], 0);
            DownloadWorker.Launch(args[1]);

            MeteorCode.ChangeStatus("Build Complete");
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
            var color = meteorDatabase.Configurations.First(c => c.property == "background").value;
            
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
                    StatusBar.IsIndeterminate = true;
                    break;
                case 1:
                    StatusBar.IsIndeterminate = false;
                    StatusBar.Value = 0;
                    break;
                case 2:
                    StatusBar.IsIndeterminate = false;
                    StatusBar.Value = 100;
                    break;
            }

        }

        private void ChangeStatusValue(string text)
        {
             StatusMessage.Text= text;
        }

        private void UpdateProgressBarValue(int val)
        {
            StatusBar.Value = val;
        }

        //First boot
        private void InitializeMeteor()
        {
            //Check for first boot
            if (meteorDatabase.Configurations.First(c => c.property == "firstLaunch").value == "1")
            {
                
                // _dbHandler.SetWorkspaceDate(DateTime.Now.ToLongDateString(),int.Parse(_dbHandler.get_property("workspace")));
                //_dbHandler.set_property_value("1", "initialized");
            }
        }

        //Section actions
        private void ChangeSection(object sender, RoutedEventArgs e)
        {
            Button butt = (Button)sender;
            var val = butt.Name;
            switch (val)
            {
                case "SkinsSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[0];
                    MeteorCode.WriteToConsole("Section changed to Skins", 3);
                    SkinsSection skinsSectionPage = (SkinsSection)SkinsFrame.Content;
                    skinsSectionPage.LoadLists();
                    skinsSectionPage.SkinsListBox.SelectedIndex = skinsSectionPage.SelectedSlot - 1;
                    activeSection = 0;
                    break;
                case "StagesSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[1];
                    MeteorCode.WriteToConsole("Section changed to Stages", 3);
                    activeSection = 1;
                    break;
                case "InterfaceSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[2];
                    MeteorCode.WriteToConsole("Section changed to Interface", 3);
                    activeSection = 2;
                    break;
                case "FileBankSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[3];
                    MeteorCode.WriteToConsole("Section changed to Filebank", 3);
                    FilebankSection FilebankSectionPage = (FilebankSection)FilebankFrame.Content;

                    FilebankNameplates FilebankNameplatesPage =
                        (FilebankNameplates)FilebankSectionPage.FilebankNameplateFrame.Content;
                    FilebankNameplatesPage.ReloadNameplates();

                    FilebankSkins FilebankSkinsPage =
                        (FilebankSkins)FilebankSectionPage.FilebankSkinFrame.Content;
                    FilebankSkinsPage.ReloadSkins();

                    FilebankPacker FilebankPackerPage =
                        (FilebankPacker)FilebankSectionPage.FilebankPackerFrame.Content;
                    FilebankPackerPage.Reload();
                    activeSection = 3;
                    break;
                case "WorkspaceSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[4];
                    MeteorCode.WriteToConsole("Section changed to Workspace", 3);
                    WorkspaceSection workspacePage = (WorkspaceSection)WorkspaceFrame.Content;
                    workspacePage.LoadWorkspaceStats();
                    activeSection = 4;
                    break;
                case "ConfigurationSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[5];
                    MeteorCode.WriteToConsole("Section changed to Configuration", 3);
                    activeSection = 5;
                    break;
                case "AboutSectionButton":
                    SectionsTabControl.SelectedItem = SectionsTabControl.Items[6];
                    MeteorCode.WriteToConsole("Section changed to About", 3);
                    activeSection = 6;
                    break;
            }
        }

        private void ActivateWorkspace(int index)
        {
            Workspace workspace = meteorDatabase.Workspaces.First(w => w.slot == index + 1);

            //Assigning values
            meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value = workspace.Id.ToString();

            //Checking S4E configuration
            if (meteorDatabase.Configurations.First(c => c.property == "smashExplorerExe").value != "")
            {
                MeteorCode.SetS4EWorkspacePath(workspace.Id, meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value);
            }
            meteorDatabase.SaveChanges();

            MeteorCode.Message("Workspace '"+ workspace.name+"' activated!");
        }

        //Workers
        private void SetupWorkers()
        {
            //Setting up Workers
            BuildWorker = new BuildWorker(meteorDatabase);
            DownloadWorker = new DownloadWorker(meteorDatabase);

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

            List<Worker> workers = new List<Worker> { BuildWorker, DownloadWorker };

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
            WorkspaceSection workspace = (WorkspaceSection)WorkspaceFrame.Content;
            SkinsSection skinsSection = (SkinsSection) SkinsFrame.Content;
            switch (worker?.Name)
            {
                case "BuildWorker":
                    int id = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value);
                    Workspace workspaceitem = meteorDatabase.Workspaces.First(
                        w => w.Id == id
                        );
                    workspaceitem.buildcount++;
                    meteorDatabase.SaveChanges();
                    workspace.LoadWorkspaceStats();
                    MeteorCode.WriteToConsole("Workspace built!",0);
                    break;

                case "DownloadWorker":
                    MeteorCode.WriteToConsole("Installation successful", 0);
                    switch (activeSection)
                    {
                        case 0:
                            SectionsTabControl.SelectedItem = SectionsTabControl.Items[0];
                            SkinsSection skinsSectionPage = (SkinsSection)SkinsFrame.Content;
                            skinsSectionPage.LoadLists();
                            skinsSectionPage.SkinsListBox.SelectedIndex = skinsSectionPage.SelectedSlot - 1;
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            SectionsTabControl.SelectedItem = SectionsTabControl.Items[3];
                            FilebankSection FilebankSectionPage = (FilebankSection)FilebankFrame.Content;

                            FilebankNameplates FilebankNameplatesPage =
                                (FilebankNameplates)FilebankSectionPage.FilebankNameplateFrame.Content;
                            FilebankNameplatesPage.ReloadNameplates();

                            FilebankSkins FilebankSkinsPage =
                                (FilebankSkins)FilebankSectionPage.FilebankSkinFrame.Content;
                            FilebankSkinsPage.ReloadSkins();

                            FilebankPacker FilebankPackerPage =
                                (FilebankPacker)FilebankSectionPage.FilebankPackerFrame.Content;
                            FilebankPackerPage.Reload();
                            break;
                        case 4:
                            WorkspaceSection workspacePage = (WorkspaceSection)WorkspaceFrame.Content;
                            workspacePage.ReloadWorkspaceScreen();
                            break;
                        case 5:
                            break;
                        case 6:
                            break;
                    }
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
                Thread.Sleep(15);

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
                    if (_controllerStyle == 0)
                    {
                        ChangeProgressBarStyle(0);
                    }
                    if (_controllerStyle == 1)
                    {
                        ChangeProgressBarStyle(1);
                        UpdateProgressBarValue(_controllerPercent);
                    }
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

        private void ClearCountUp(object sender, MouseButtonEventArgs e)
        {
            if (clearCount == 4)
            {
                var result = MessageBox.Show("Reset Everything?", "Segtendo WARNING",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    meteorDatabase.ResetEverything();
                    Directory.Delete(MeteorCode.AppPath + "/workspaces/",true);
                    Directory.Delete(MeteorCode.AppPath + "/filebank/", true);
                    Directory.Delete(MeteorCode.AppPath + "/downloads/", true);
                    Directory.CreateDirectory(MeteorCode.AppPath + "/workspaces/");
                    Directory.CreateDirectory(MeteorCode.AppPath + "/filebank/");
                    Directory.CreateDirectory(MeteorCode.AppPath + "/downloads/");
                    Application.Current.Shutdown();
                }
                clearCount = 0;
            }
            else
            {
                clearCount++;
            }
        }


        //Loads the workspaces in the top TabControl
        public void LoadWorkspaces()
        {
            meteorDatabase = new MeteorDatabase();
            _loading = true;
            var itemList = new List<TabItem>();

            TabControl WorkspaceTab = ((MainWindow)Application.Current.MainWindow).WorkspaceTabControl;
            int workspaceId = int.Parse(meteorDatabase.Configurations
                .First(c => c.property == "activeWorkspace").value);
            int index = meteorDatabase.Workspaces.First(w => w.Id == workspaceId).slot -1;
            var selected = WorkspaceTab.SelectedIndex;

            foreach (Workspace workspace in meteorDatabase.Workspaces.Where(w => w.slot > 0))
            {
                var TabItem = new TabItem()
                {
                    Header = workspace.name
                };
                itemList.Add(TabItem);
            }

            WorkspaceTab.ItemsSource = itemList;

            if (index + 1 <= itemList.Count)
            {
                WorkspaceTab.SelectedIndex = index;
            }
            else
            {
                WorkspaceTab.SelectedIndex = WorkspaceTab.Items.Count - 1;
            }
                

            _loading = false;
        }

        private void WorkspaceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
            {
                ActivateWorkspace(WorkspaceTabControl.SelectedIndex);
                
                switch (activeSection)
                {
                    case 0:
                        SectionsTabControl.SelectedItem = SectionsTabControl.Items[0];
                        SkinsSection skinsSectionPage = (SkinsSection)SkinsFrame.Content;
                        skinsSectionPage.LoadLists();
                        skinsSectionPage.SkinsListBox.SelectedIndex = skinsSectionPage.SelectedSlot - 1;
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        SectionsTabControl.SelectedItem = SectionsTabControl.Items[3];
                        FilebankSection FilebankSectionPage = (FilebankSection)FilebankFrame.Content;

                        FilebankNameplates FilebankNameplatesPage =
                            (FilebankNameplates)FilebankSectionPage.FilebankNameplateFrame.Content;
                        FilebankNameplatesPage.ReloadNameplates();

                        FilebankSkins FilebankSkinsPage =
                            (FilebankSkins)FilebankSectionPage.FilebankSkinFrame.Content;
                        FilebankSkinsPage.ReloadSkins();

                        FilebankPacker FilebankPackerPage =
                            (FilebankPacker)FilebankSectionPage.FilebankPackerFrame.Content;
                        FilebankPackerPage.Reload();
                        break;
                    case 4:
                        WorkspaceSection workspacePage = (WorkspaceSection)WorkspaceFrame.Content;
                        workspacePage.ReloadWorkspaceScreen();
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                }
            }
        }
    }
}
