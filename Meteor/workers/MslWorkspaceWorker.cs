using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml;
using Meteor.database;
using winforms = System.Windows.Forms;

namespace Meteor.workers
{
    public class MslWorkspaceWorker : Worker
    {

        private int _progressStyle = 1;

        private String mslPath = "";

        public MslWorkspaceWorker(db_handler dbHandler) :base (dbHandler)
        {
            
        }

        protected internal override void Launch()
        {
            Interlocked.Exchange(ref _status, 1);

            //Dialog
            var openFileDialog = new winforms.OpenFileDialog();
            openFileDialog.Title = "Select Meteor Skin Library.exe";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = "MSL Executable (Meteor Skin Library.exe)|Meteor Skin Library.exe";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                mslPath = openFileDialog.FileName;

            }
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            var id = DbHandler.add_workspace("MSL Import");
            DbHandler.add_default_skins(id);

            var workspacepath = new FileInfo(mslPath).DirectoryName;
            var libraryPath = workspacepath + "/mmsl_config/Library.xml";
            ArrayList contents = parseLibraryContents(libraryPath);
        }

        private ArrayList parseLibraryContents(String path)
        {
            ArrayList contents = new ArrayList();


            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList nodes = xml.SelectNodes("/Filebank/Character[attribute::name='" + "fullname" + "']/skins/skin");

            foreach (XmlElement node in nodes)
            {

            }

            return contents;

        }
    }
}
