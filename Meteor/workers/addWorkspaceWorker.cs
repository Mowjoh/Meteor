﻿using System.ComponentModel;
using System.Windows;
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

            var id = DbHandler.add_workspace("New Workspace");
            DbHandler.add_default_skins(id);
           
        }

        protected internal override void PostWork()
        {
            MeteorCode.WriteToConsole("Workspace was successfully added",0);
        }


    }
}


