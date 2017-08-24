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
        protected readonly BackgroundWorker _worker = new BackgroundWorker();

        protected MeteorDatabase meteorDatabase;
        protected int _completion;
        protected String _message;
        private int _progressStyle = 0;

        public string Name { get; set; }

        //Status Code. -1 for error, 0 for stopped, 1 for success, + for other codes
        protected int _status;

        //Constructor
        protected Worker(MeteorDatabase meteorDatabase)
        {
            this.meteorDatabase = meteorDatabase;

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

        //Post Work command
        protected internal virtual void PostWork()
        {
            Status = 0;
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

        public int Style
        {
            get => _progressStyle;
            set => _progressStyle = value;
        }
    }
}
