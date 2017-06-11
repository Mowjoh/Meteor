using System.ComponentModel;
using Meteor.database;

namespace Meteor.workers
{
    public class addWorkspaceWorker : Worker
    {

        //Constructor
        public addWorkspaceWorker(db_handler dbHandler) : base(dbHandler)
        {
            
        }


        //Async worker functions
        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            Message = "Creating Workspace";

            var id = dbHandler.add_workspace("New Workspace");
            dbHandler.add_default_skins(id);
           
        }

    }
}


