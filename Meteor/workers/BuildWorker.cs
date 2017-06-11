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

        }

        protected internal void Launch(builder build)
        {
            _build = build;
            _worker.RunWorkerAsync();
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            var activeWorkspace = int.Parse(dbHandler.get_property("workspace"));

            //Making sure it's the proper ID
            _build.set_workspace_id(activeWorkspace);

            //Cleaning folders
            _build.clean_workspace();

            //Building folders
            _build.build();
        }

        
    }
}
