using Meteor.database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Meteor.content
{
    class Nameplate
    {

        private readonly string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

        public int nameplate_id;
        public int character_id;
        int workspace_id;

        String nameplate_path;
        public String full_path;
        public String filename;

        db_handler db;

        public Nameplate(int id,int char_id,int work_id, db_handler dbs)
        {
            db = dbs;
            nameplate_id = id;
            character_id = char_id;
            workspace_id = work_id;

            nameplate_path = app_path + "/filebank/nameplates/" + nameplate_id + "/";
            full_path = nameplate_path + "chrn_11_" + db.get_character_cspfolder(character_id) + "_XX.nut";
            filename = new FileInfo(full_path).Name;

        }

        public Nameplate(String path, int char_id, int work_id, db_handler dbs)
        {
            db = dbs;
            character_id = char_id;
            workspace_id = work_id;

            get_nameplate(path);

            
            
        }

        public Boolean get_nameplate(String path)
        {
            try
            {
                long id = db.insert_nameplate(character_id);
                nameplate_id = Convert.ToInt32(id);
                nameplate_path = app_path + "/filebank/nameplates/" + nameplate_id + "/";
                full_path = nameplate_path + "chrn_11_" + db.get_character_cspfolder(character_id) + "_XX.nut";
                filename = new FileInfo(full_path).Name;
                if (!Directory.Exists(nameplate_path))
                {
                    Directory.CreateDirectory(nameplate_path);
                }
                File.Copy(path, full_path);


                return true;
            }
            catch
            {
                return false;
            }
            
        }

    }
}
