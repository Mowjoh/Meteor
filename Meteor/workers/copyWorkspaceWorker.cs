using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meteor.database;

namespace Meteor.workers
{
    public class copyWorkspaceWorker : Worker
    {
        private int _selectedWorkspace;


        //Constructor
        public copyWorkspaceWorker(db_handler databaseHandler) : base(databaseHandler)
        {

        }

        //Launch command
        public void Launch(int selectedWorkspace)
        {

            Status = 1;
            Message = "Copying Workspace";
            _selectedWorkspace = selectedWorkspace;
            _worker.RunWorkerAsync();
        }


        //Async worker functions
        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            //Getting active workspace ID
            int activeWorkspace = int.Parse(DbHandler.get_property("workspace"));

            //Launching the copy process
            DbHandler.copy_skins(activeWorkspace, _selectedWorkspace);
        }

    }
}


