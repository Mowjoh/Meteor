using System;
using System.ComponentModel;
using Meteor.database;

namespace Meteor.workers
{
    public class ClearWorkspaceWorker : Worker
    {
        private int _selectedWorkspace;

        public ClearWorkspaceWorker(db_handler databaseHandler) : base(databaseHandler)
        {
        }

        public void Launch(int selectedWorkspace)
        {
            Message = "Clearing Workspace";
            Status = 1;
            _selectedWorkspace = selectedWorkspace;
            _worker.RunWorkerAsync();
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            base.WorkerDowork(sender, e);
            var id = DbHandler.get_workspace_id(_selectedWorkspace);
            DbHandler.clear_workspace(id);
            DbHandler.add_default_skins(Convert.ToInt64(id));
        }
    }
}