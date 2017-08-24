using Meteor.database;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Meteor.content
{
    class NameplateObject
    {

        private readonly string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

        public int nameplate_id;
        public int character_id;
        int workspace_id;

        String nameplate_path;
        public String full_path;
        public String filename;

        MeteorDatabase meteorDatabase;

        public NameplateObject(int id,int char_id,int work_id)
        {
            meteorDatabase = new MeteorDatabase();
            nameplate_id = id;
            character_id = char_id;
            workspace_id = work_id;

            nameplate_path = app_path + "/filebank/nameplates/" + nameplate_id + "/";
            full_path = nameplate_path + "chrn_11_" + meteorDatabase.Characters.First(c => c.Id == character_id).csp_folder + "_XX.nut";
            filename = new FileInfo(full_path).Name;

        }

        public NameplateObject(String path, int char_id, int work_id)
        {
            meteorDatabase = new MeteorDatabase();
            character_id = char_id;
            workspace_id = work_id;

            get_nameplate(path);

        }

        public Boolean get_nameplate(String path)
        {
            try { 
            Nameplate nameplate = new Nameplate()
            {
                character_id = character_id,
                name = "new Nameplate"
            };
            meteorDatabase.Nameplates.Add(nameplate);
            meteorDatabase.SaveChanges();

                nameplate_path = app_path + "/filebank/nameplates/" + nameplate.Id + "/";
                full_path = nameplate_path + "chrn_11_" + nameplate.character_id + "_XX.nut";
                filename = new FileInfo(full_path).Name;
                if (!Directory.Exists(nameplate_path))
                {
                    Directory.CreateDirectory(nameplate_path);
                }
                File.Copy(path, full_path, true);
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
            return true;
        }

    }
}
