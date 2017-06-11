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
    public class Worker
    {
        //Class variables
        protected BackgroundWorker _worker = new BackgroundWorker();

        protected db_handler dbHandler;
        protected int _completion;
        protected String _message;
        private int _progressStyle = 0;

        //Status Code. -1 for error, 0 for stopped, 1 for success, + for other codes
        protected int _status;

        //Constructor
        protected Worker(db_handler databaseHandler)
        {
            this.dbHandler = databaseHandler;

            _worker.DoWork += WorkerDowork;
            _worker.RunWorkerCompleted += WorkerCompleted;
            _worker.ProgressChanged += WorkerReport;

        }

        //Launch command
        protected internal virtual void Launch()
        {
            Status = 1;
            _worker.RunWorkerAsync();
        }

        //Async worker functions-----------------------------------------------------------
        protected virtual void WorkerDowork(object sender, DoWorkEventArgs e)
        {
        }

        protected void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Status = -1;
            }
            else
            {
                Message = "Task Completed";
                Status = 3;
            }
        }

        protected void WorkerReport(object sender, ProgressChangedEventArgs e)
        {
           Completion = e.ProgressPercentage;
           Status = 2;
        }

        //Getters and Setters-------------------------------------------------------------
        public int Completion
        {
           get => Interlocked.CompareExchange(ref _completion, 0, 0);
            set => Interlocked.Exchange(ref _completion,value);
        }

        public int Status
        {
            get => _status;
            set => Interlocked.Exchange(ref _status, value);
        }

        public String Message
        {
            get => _message;
            set => _message = value;
        }

        public int Style => _progressStyle;
    }
}
