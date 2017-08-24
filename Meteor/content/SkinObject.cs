using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using Meteor.database;

namespace Meteor.content
{
    internal class SkinObject
    {
        #region Constructor

        public SkinObject(int slot, int skin_id, int char_id, int workspace_id, MeteorDatabase meteorDatabase)
        {
            this.slot = slot;
            this.skin_id = skin_id;
            this.char_id = char_id;
            this.workspace_id = workspace_id;
            this.meteorDatabase = meteorDatabase;

            model_path = app_path + "/filebank/skins/" + skin_id + "/models/";
            csp_path = app_path + "/filebank/skins/" + skin_id + "/csps/";

            dlc = meteorDatabase.Characters.First(c => c.Id == this.char_id).dlc;
        }

        #endregion

        #region Variables

        private MeteorDatabase meteorDatabase;

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

        public void AddModelsFromPath(string path)
        {
            var foldername = Path.GetFileName(path);
            if (cXX.IsMatch(foldername) | lXX.IsMatch(foldername))
            {
                //Work the file as body
                AddModelFolder(path, "body");
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
                        AddModelFolder(dir, parent);
                    }
                }
            }
        }

        public void AddCspsFromPath(string path)
        {
            var csps = Directory.GetFiles(path, "*.nut", SearchOption.AllDirectories);
            foreach (var csp in csps)
            {
                var filename = Path.GetFileName(csp);
                if (cspr.IsMatch(filename))
                    AddCspFile(csp);
            }
        }

        public void AddCspFile(string filepath)
        {
            //Filename
            var filename = Path.GetFileName(filepath);

            //Type
            var csptype = filename.Split('_')[0] + "_" + filename.Split('_')[1];

            //Setting destination
            var destination = csp_path + filename.Split('_')[0] + "_" + filename.Split('_')[1] + "_" +
                              filename.Split('_')[2] + "_XX.nut";
            //Creating desitnation
            if (!Directory.Exists(csp_path))
                Directory.CreateDirectory(csp_path);

            //Getting file hash
            var hash = GetSha1Hash(filepath);

            //Getting DB Skin
            Skin hashSkin = meteorDatabase.Skins.FirstOrDefault(
                s => s.csp_hash_1 == hash || s.csp_hash_2 == hash || s.csp_hash_3 == hash || s.csp_hash_4 == hash);

            Skin skin = meteorDatabase.Skins.First(s => s.Id == skin_id);

            File.Copy(filepath, destination, true);

            if (skin.skin_csps != "")
            {
                string[] csps = skin.skin_csps.Split(';');

                List<string> cspList = new List<string>();
                Boolean present = false;

                if (!csps.Contains(csptype))
                {
                    cspList = csps.ToList();
                    cspList.Add(csptype);
                    cspList.Sort(StringComparer.InvariantCulture);
                    skin.skin_csps = string.Join(";", cspList.ToArray());
                }
            }
            else
            {
                skin.skin_csps = csptype;
            }

            switch (csptype)
            {
                case "chr_00":
                    skin.csp_hash_1 = hash;
                    break;
                case "chr_11":
                    skin.csp_hash_2 = hash;
                    break;
                case "chr_13":
                    skin.csp_hash_3 = hash;
                    break;
                case "stock_90":
                    break;
            }
            
            meteorDatabase.SaveChanges();

        }

        public void AddModelFolder(string path, string parent)
        {
            var foldermatch = Path.GetFileName(path);
            var name = cXX.IsMatch(foldermatch) ? "cXX" : "lXX";
            var destination = model_path + parent + "/" + name + "/";
            var modelName = parent + "/" + name;

            var modelpath_1 = path + "/model.nud";
            var modelpath_2 = path + "/model.nut";

            var hash = "nohash";
            var hash2 = "nohash";

            if (File.Exists(modelpath_1))
                hash = GetSha1Hash(modelpath_1);
            if (File.Exists(modelpath_2))
                hash2 = GetSha1Hash(modelpath_2);

            Skin skin = meteorDatabase.Skins.First(s => s.Id == skin_id);
            Skin hashSkin = meteorDatabase.Skins.FirstOrDefault(s => s.model_hash_1 == hash || s.model_hash_2 == hash);

            if (skin.skin_models != "")
            {
                string[] models = skin.skin_models.Split(';');

                List<string> modelList = new List<string>();
                Boolean present = false;

                if (!models.Contains(modelName))
                {
                    modelList = models.ToList();
                    modelList.Add(modelName);
                    modelList.Sort(StringComparer.InvariantCulture);
                    skin.skin_models = string.Join(";", modelList.ToArray());
                }

            }
            else
            {
                skin.skin_models = modelName;
            }

            skin.model_hash_1 = hash;
            skin.model_hash_2 = hash2;

            meteorDatabase.SaveChanges();

            CopyFolder(path, destination);
            
        }

        public void RemoveCspFile(string csp_name)
        {
            Skin skin = meteorDatabase.Skins.First(s => s.Id == skin_id);

            if (skin.skin_csps.Split(';').Contains(csp_name))
            {
                List<string> list = skin.skin_csps.Split(';').ToList();

                list.Remove(csp_name);

                skin.skin_csps = list.Count == 0 ? "" : string.Join(";", list.ToArray());
            }

            meteorDatabase.SaveChanges();

            var files = Directory.GetFiles(csp_path, csp_name + "*");
            if (files.Length > 0)
                File.Delete(files[0]);

        }

        public void RemoveModel(string model_name)
        {
            Skin skin = meteorDatabase.Skins.First(s => s.Id == skin_id);

            if (skin.skin_models.Split(';').Contains(model_name))
            {
                List<string> list = skin.skin_models.Split(';').ToList();

                list.Remove(model_name);

                skin.skin_models = list.Count == 0 ? "" : string.Join(";", list.ToArray());
            }
            meteorDatabase.SaveChanges();

            var destination = model_path + model_name.Split('/')[0];
            if (Directory.Exists(destination))
                Directory.Delete(destination, true);
        }

        public void CopyFolder(string source, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
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