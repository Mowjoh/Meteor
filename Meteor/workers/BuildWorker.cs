using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Meteor.content;
using Meteor.database;

namespace Meteor.workers
{
    public class BuildWorker : Worker
    {
        private ContentBuilder _build;
        private int activeWorkspace;

        public BuildWorker(MeteorDatabase meteorDatabase) : base(meteorDatabase)
        {
            Name = "BuildWorker";
        }

        protected internal override void Launch()
        {
            activeWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value);
            int region = int.Parse(meteorDatabase.Configurations.First(c => c.property == "region").value);
            int language = int.Parse(meteorDatabase.Configurations.First(c => c.property == "language").value);
            _build = new ContentBuilder(activeWorkspace, region,language,meteorDatabase);
            Message = "Building Workspace";
            _worker.RunWorkerAsync();
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            meteorDatabase = new MeteorDatabase();
            
            Status = 1;

            //Making sure it's the proper ID
            _build.set_workspace_id(activeWorkspace);

            //Cleaning folders
            _build.clean_workspace();

            //Building folders
            _build.build();

            switch (meteorDatabase.Configurations.First(c => c.property == "AutoSmashExplorerRelaunch").value)
            {
                case "1":
                    LaunchS4E();
                    break;

                case "2":
                    KillS4E();
                    LaunchS4E();
                    break;
            }
        }

        private void LaunchS4E()
        {
            var path = meteorDatabase.Configurations.First(c => c.property == "smashExplorerExe").value;
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value + "/"
            };
            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                MeteorCode.WriteToConsole("Sm4sh Explorer couldn't be launched. Is it setup in config?", 2);
            }
        }

        private void KillS4E()
        {
            foreach (var process in Process.GetProcessesByName("Sm4shFileExplorer"))
            {
                process.Kill();
            }
        }
    }
}
