using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Meteor.database;

namespace Meteor.content
{
    internal class Skin
    {
        #region Constructor

        public Skin(int slot, int skin_id, int char_id, int workspace_id, db_handler db)
        {
            this.db = db;
            this.slot = slot;
            this.skin_id = skin_id;
            this.char_id = char_id;
            this.workspace_id = workspace_id;

            model_path = app_path + "/filebank/skins/" + skin_id + "/models/";
            csp_path = app_path + "/filebank/skins/" + skin_id + "/csps/";

            dlc = db.get_character_dlc(char_id);
        }

        #endregion

        #region Variables

        private readonly db_handler db;

        private readonly int slot;
        public readonly int skin_id;
        public readonly int char_id;
        public bool dlc;
        private readonly int workspace_id;

        private readonly string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public string model_path;
        public string csp_path;

        private readonly Regex cXX = new Regex("^[c]([0-9]{2}|xx|[0-9]x|x[0-9])$", RegexOptions.IgnoreCase);
        private readonly Regex lXX = new Regex("^[l]([0-9]{2}|xx|[0-9]x|x[0-9])$", RegexOptions.IgnoreCase);

        private readonly Regex cspr = new Regex(
            "^((?:chrn|chr|stock)_[0-9][0-9])_([a-zA-Z]+)_([0-9]{2}|xx|[0-9]x|x[0-9]).nut$", RegexOptions.IgnoreCase);

        private string[] exceptions = {"chrn_11"};

        #endregion

        #region File Management

        public void get_models(string path)
        {
            var foldername = Path.GetFileName(path);
            if (cXX.IsMatch(foldername) | lXX.IsMatch(foldername))
            {
                //Work the file as body
                add_model_folder(path, "body");
            }
            else
            {
                var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                foreach (var dir in directories)
                {
                    foldername = Path.GetFileName(dir);
                    if (cXX.IsMatch(foldername) | lXX.IsMatch(foldername))
                    {
                        //Work the file with the parent
                        var parent = Path.GetFileName(Directory.GetParent(dir).FullName);
                        add_model_folder(dir, parent);
                    }
                }
            }
        }

        public void get_csps(string path)
        {
            var csps = Directory.GetFiles(path, "*.nut", SearchOption.AllDirectories);
            foreach (var csp in csps)
            {
                var filename = Path.GetFileName(csp);
                if (cspr.IsMatch(filename))
                    add_csp_file(csp);
            }
        }

        public void add_csp_file(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            var csptype = filename.Split('_')[0] + "_" + filename.Split('_')[1];
            var destination = csp_path + filename.Split('_')[0] + "_" + filename.Split('_')[1] + "_" +
                              filename.Split('_')[2] + "_XX.nut";
            if (!Directory.Exists(csp_path))
                Directory.CreateDirectory(csp_path);
            var hash = GetSha1Hash(filepath);
            if (db.get_skin_id_hash(hash) != 0)
            {
                if (db.get_skin_id_hash(hash) != skin_id)
                {
                    var newid = db.get_skin_id_hash(hash);
                    db.replace_skin(newid, char_id, slot, workspace_id);
                    db.delete_skin(skin_id);
                }
                else
                {
                    File.Copy(filepath, destination, true);
                    db.add_csp(skin_id, csptype);
                    switch (csptype)
                    {
                        case "chr_00":
                            db.set_skin_hash(skin_id, 2, hash);
                            break;
                        case "chr_11":
                            db.set_skin_hash(skin_id, 3, hash);
                            break;
                        case "chr_13":
                            db.set_skin_hash(skin_id, 4, hash);
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
                        db.set_skin_hash(skin_id, 2, hash);
                        break;
                    case "chr_11":
                        db.set_skin_hash(skin_id, 3, hash);
                        break;
                    case "chr_13":
                        db.set_skin_hash(skin_id, 4, hash);
                        break;
                    case "stock_90":
                        break;
                }
            }
        }

        public void add_model_folder(string path, string parent)
        {
            var foldermatch = Path.GetFileName(path);
            var name = cXX.IsMatch(foldermatch) ? "cXX" : "lXX";
            var destination = model_path + parent + "/" + name + "/";

            var modelpath_1 = path + "/model.nud";
            var modelpath_2 = path + "/model.nut";
            var hash = "nohash";
            var hash2 = "nohash";
            if (File.Exists(modelpath_1))
                hash = GetSha1Hash(modelpath_1);
            if (File.Exists(modelpath_2))
                hash2 = GetSha1Hash(modelpath_2);

            if (db.get_skin_id_hash(hash) != 0 && db.get_skin_id_hash(hash2) != 0)
            {
                var newid = db.get_skin_id_hash(hash);
                if (skin_id != newid)
                {
                    //Skin is a duplicate
                    db.replace_skin(newid, char_id, slot, workspace_id);
                    db.delete_skin(skin_id);
                }
                else
                {
                    //Skin isn't a duplicate
                    copy_folder(path, destination);
                    db.add_model(skin_id, parent + "/" + name);
                    db.set_skin_hash(skin_id, 0, hash);
                    db.set_skin_hash(skin_id, 1, hash2);
                }
            }
            else
            {
                //Skin isn't a duplicate
                copy_folder(path, destination);
                db.add_model(skin_id, parent + "/" + name);
                db.set_skin_hash(skin_id, 0, hash);
                db.set_skin_hash(skin_id, 1, hash2);
            }
        }

        public void copy_folder(string source, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

        public void remove_csp_file(string csp_name)
        {
            Console.WriteLine(csp_name);
            var files = Directory.GetFiles(csp_path, csp_name + "*");
            if (files.Length > 0)
                File.Delete(files[0]);
            db.remove_csp(skin_id, csp_name);
        }

        public void remove_model(string model_name)
        {
            var destination = model_path + model_name;
            if (Directory.Exists(destination))
                Directory.Delete(destination, true);
            db.remove_model(skin_id, model_name);
        }

        #endregion

        #region Duplicates

        //Duplicates
        private void check_duplicate_skin(string filehash)
        {
        }

        public string GetSha1Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return string.Join("-", md5.ComputeHash(stream));
                }
            }
        }

        #endregion
    }
}