using Meteor.database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Meteor.content
{
    class Skin
    {
        #region Variables
        db_handler db;

        int slot;
        int skin_id;
        int char_id;
        Boolean dlc;
        int workspace_id;

        String app_path = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public String model_path;
        public String csp_path;

        String model_list ="";

        Regex cXX = new Regex("^[c]([0-9]{2}|xx|[0-9]x|x[0-9])$", RegexOptions.IgnoreCase);
        Regex lXX = new Regex("^[l]([0-9]{2}|xx|[0-9]x|x[0-9])$", RegexOptions.IgnoreCase);

        Regex cspr = new Regex("^((?:chrn|chr|stock)_[0-9][0-9])_([a-zA-Z]+)_([0-9]{2}|xx|[0-9]x|x[0-9]).nut$", RegexOptions.IgnoreCase);

        String[] exceptions = new String[] { "chrn_11" };

        #endregion

        #region Constructor
        public Skin(int slot,int skin_id,int char_id,int workspace_id,db_handler db)
        {
            this.db = db;
            this.slot = slot;
            this.skin_id = skin_id;
            this.char_id = char_id;
            this.workspace_id = workspace_id;

            this.model_path = app_path + "/filebank/skins/" + skin_id + "/models/";
            this.csp_path = app_path + "/filebank/skins/" + skin_id + "/csps/";

            this.dlc = db.get_character_dlc(char_id);
        }
        #endregion

        #region File Management
        public void get_models(String path)
        {
            String foldername = System.IO.Path.GetFileName(path);
            if (cXX.IsMatch(foldername) | lXX.IsMatch(foldername))
            {
                //Work the file as body
                add_model_folder(path, "body");
            }
            else
            {

                String[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                foreach (String dir in directories)
                {
                    foldername = System.IO.Path.GetFileName(dir);
                    if (cXX.IsMatch(foldername) | lXX.IsMatch(foldername))
                    {
                        //Work the file with the parent
                        String parent = System.IO.Path.GetFileName(Directory.GetParent(dir).FullName);
                        add_model_folder(dir, parent);
                    }
                }
            }
        }

        public void get_csps(String path)
        {
            String[] csps = Directory.GetFiles(path, "*.nut", SearchOption.AllDirectories);
            foreach(String csp in csps)
            {
                String filename = System.IO.Path.GetFileName(csp);
                if(cspr.IsMatch(filename))
                {
                    add_csp_file(csp);
                    
                }
            }
        }

        public void add_csp_file(String filepath)
        {
            String filename = System.IO.Path.GetFileName(filepath);
            String csptype = filename.Split('_')[0] + "_" + filename.Split('_')[1];
            String destination = csp_path + filename.Split('_')[0] + "_" + filename.Split('_')[1] + "_" + filename.Split('_')[2]+"_XX.nut";
            if (!Directory.Exists(csp_path))
            {
                Directory.CreateDirectory(csp_path);
            }
            String hash = GetSha1Hash(filepath);
            if (db.get_skin_id_hash(hash) != 0)
            {
                if(db.get_skin_id_hash(hash) != this.skin_id)
                {
                    int newid = db.get_skin_id_hash(hash);
                    db.replace_skin(newid, this.char_id, this.slot, this.workspace_id);
                    db.delete_skin(skin_id);
                }else
                {
                    File.Copy(filepath, destination, true);
                    db.add_csp(skin_id, csptype);
                    switch (csptype)
                    {
                        case "chr_00":
                            db.set_skin_hash(this.skin_id, 2, hash);
                            break;
                        case "chr_11":
                            db.set_skin_hash(this.skin_id, 3, hash);
                            break;
                        case "chr_13":
                            db.set_skin_hash(this.skin_id, 4, hash);
                            break;
                        case "stock_90":
                            break;
                    }
                }
            }
            else
            {
                File.Copy(filepath, destination, true);
                db.add_csp(skin_id, csptype);
                switch (csptype)
                {
                    case "chr_00":
                        db.set_skin_hash(this.skin_id, 2, hash);
                        break;
                    case "chr_11":
                        db.set_skin_hash(this.skin_id, 3, hash);
                        break;
                    case "chr_13":
                        db.set_skin_hash(this.skin_id, 4, hash);
                        break;
                    case "stock_90":
                        break;
                }
            }
            
        }

        public void add_model_folder(String path,String parent)
        {
            String foldermatch = System.IO.Path.GetFileName(path);
            String name = cXX.IsMatch(foldermatch) ? "cXX":"lXX";
            String destination = model_path + parent + "/" + name+"/";

            String modelpath_1 = path + "/model.nud";
            String modelpath_2 = path + "/model.nut";
            String hash = "nohash";
            String hash2 = "nohash";
            if (File.Exists(modelpath_1))
            {
                hash = GetSha1Hash(modelpath_1);
            }
            if (File.Exists(modelpath_2))
            {
                hash2 = GetSha1Hash(modelpath_2);
            }

            if(db.get_skin_id_hash(hash) != 0 && db.get_skin_id_hash(hash2) != 0)
            {
                
                
                int newid = db.get_skin_id_hash(hash);
                if (this.skin_id != newid)
                {
                    //Skin is a duplicate
                    db.replace_skin(newid, this.char_id, this.slot, this.workspace_id);
                    db.delete_skin(skin_id);

                }else
                {
                    //Skin isn't a duplicate
                    copy_folder(path, destination);
                    db.add_model(skin_id, parent + "/" + name);
                    db.set_skin_hash(this.skin_id, 0, hash);
                    db.set_skin_hash(this.skin_id, 1, hash2);
                }
                

            }
            else
            {
                //Skin isn't a duplicate
                copy_folder(path, destination);
                db.add_model(skin_id, parent + "/" + name);
                db.set_skin_hash(this.skin_id, 0, hash);
                db.set_skin_hash(this.skin_id, 1, hash2);
            }


            
        }

        public void copy_folder(String source, String destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (string dirPath in Directory.GetDirectories(source, "*",SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(source, "*.*",SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

        public void remove_csp_file(String csp_name)
        {
            Console.WriteLine(csp_name);
            String[] files = Directory.GetFiles(csp_path, csp_name + "*");
            if(files.Length> 0)
            {
                File.Delete(files[0]);
            }
            db.remove_csp(this.skin_id, csp_name);                                    
        }

        public void remove_model(String model_name)
        {
            String destination = model_path + model_name;
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, true);
            }
            db.remove_model(this.skin_id, model_name);
        }
        #endregion

        #region Duplicates
        //Duplicates
        private void check_duplicate_skin(String filehash)
        {

        }

        public string GetSha1Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return String.Join("-",md5.ComputeHash(stream));
                }
            }
        }
        #endregion
    }
}
