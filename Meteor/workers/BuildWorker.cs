using System.ComponentModel;
using Meteor.content;
using Meteor.database;

namespace Meteor.workers
{
    public class BuildWorker : Worker
    {
        private builder _build;

        public BuildWorker(db_handler databaseHandler) : base(databaseHandler)
        {
            Name = "BuildWorker";
        }

        protected internal override void Launch()
        {
            _build = new builder(int.Parse(DbHandler.get_property("workspace")),int.Parse(DbHandler.get_property("region")), int.Parse(DbHandler.get_property("language")),DbHandler);
            Message = "Building Workspace";
            _worker.RunWorkerAsync();
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            Status = 1;

            var activeWorkspace = int.Parse(DbHandler.get_property("workspace"));

            //Making sure it's the proper ID
            _build.set_workspace_id(activeWorkspace);

            //Cleaning folders
            _build.clean_workspace();

            //Building folders
            _build.build();

        }

        
    }
}
