using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using Meteor.content;
using Meteor.database;
using SharpCompress.Common;
using SharpCompress.Reader;
using newpath = System.IO.Path;

namespace Meteor.workers
{
    public class DownloadWorker : Worker
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        private String _downloadUrl;

        //Constructor
        public DownloadWorker(db_handler databaseHandler) : base(databaseHandler)
        {
            _worker.WorkerReportsProgress = true;
            Name = "DownloadWorker";
        }

        protected internal void Launch(String url)
        {
            _downloadUrl = url;
            _worker.RunWorkerAsync();
            Status = 1;
            Style = 1;
            Message = "Downloading file";
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            base.WorkerDowork(sender, e);

            if (!Directory.Exists(AppPath + "/downloads/"))
            {
                Directory.CreateDirectory(AppPath + "/downloads/");
            }
            else
            {
                try
                {
                    Directory.Delete(AppPath + "/downloads/", true);
                }
                catch
                {
                    CheckFolderDeletion(AppPath + "/downloads/");
                }
                Directory.CreateDirectory(AppPath + "/downloads/");
            }

            var link = _downloadUrl.Substring(7, _downloadUrl.Length - 7);
            var extension = _downloadUrl.Split('.')[_downloadUrl.Split('.').Length - 1];

            WebClient webby = new WebClient();
            Uri ur = new Uri(link);
            webby.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webby.DownloadFileCompleted += WebClientDownloadCompleted;
            webby.DownloadFileAsync(ur, AppPath + "/downloads/archive." + extension);

            void WebClientDownloadProgressChanged(object senders, DownloadProgressChangedEventArgs es)
            {
                try
                {
                    Status = 2;
                    _worker.ReportProgress(es.ProgressPercentage);
                }
                catch
                {
                    
                }
                
            }

            void WebClientDownloadCompleted(object sender2, AsyncCompletedEventArgs e2)
            {
                
            }

            while (webby.IsBusy)
            {
            }

        }

        //Folder Code
        private void CheckFolderDeletion(String path)
        {
            if (Directory.Exists(path))
            {
                setAttributesNormal(new DirectoryInfo(path));
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {

            }
        }

        private void setAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                setAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }

        
    }
}
