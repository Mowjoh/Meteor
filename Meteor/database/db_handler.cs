using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

namespace Meteor.database
{
    public class db_handler
    {
        //Variables
        private readonly SQLiteConnection db_connection;

        //Constructor
        public db_handler()
        {
            db_connection = new SQLiteConnection("Data Source=Library.sqlite;Version=3;");
            db_connection.Open();
        }

        //----------------------Library------------------------------------

        public void reorder_workspace()
        {
            var sql = "select slot from workspaces";

            var sqLiteCommand = new SQLiteCommand(sql, db_connection);
            var executeReader = sqLiteCommand.ExecuteReader();
            var i = 1;
            while (executeReader.Read())
            {
                var slot = executeReader.GetInt32(0);
                if (i != slot)
                {
                    var sql2 = "update workspaces set slot = @i where slot = @slot";
                    sqLiteCommand = new SQLiteCommand(sql2, db_connection);
                    sqLiteCommand.Parameters.AddWithValue("i", i);
                    sqLiteCommand.Parameters.AddWithValue("slot", slot);
                    sqLiteCommand.ExecuteNonQuery();
                }
                i++;
            }
        }

        public void reorder_skins(int character_id, int workspace_id)
        {
            var sql =
                "select slot from skin_library where character_id = @character_id and workspace_id = @workspace_id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            var reader = command.ExecuteReader();
            var countslot = 1;
            while (reader.Read())
            {
                var slot = reader.GetInt32(0);
                if (countslot != slot)
                {
                    var sql2 =
                        "update skin_library set slot = @countslot where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
                    command = new SQLiteCommand(sql2, db_connection);
                    command.Parameters.AddWithValue("countslot", countslot);
                    command.Parameters.AddWithValue("slot", slot);
                    command.Parameters.AddWithValue("character_id", character_id);
                    command.Parameters.AddWithValue("workspace_id", workspace_id);
                    command.ExecuteNonQuery();
                }
                countslot++;
            }
        }

        public void restore_default(int slot, int char_id, string workspace)
        {
            var sql =
                "select skin_id from skin_library where slot = @slot and character_id = @char_id and workspace_id = 1";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("char_id", char_id);
            var reader = command.ExecuteReader();
            var id = 0;
            while (reader.Read())
                id = reader.GetInt32(0);

            sql =
                "update skin_library set skin_id = @id where slot = @slot and character_id = @char_id and workspace_id = @workspace";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("workspace", workspace);
            command.ExecuteNonQuery();
        }

        public void replace_skin(int new_id, int char_id, int slot, int workspace_id)
        {
            var sql =
                "update skin_library set  skin_id = @skin_id where character_id = @character_id and slot = @slot and workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", new_id);
            command.Parameters.AddWithValue("character_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }

        //----------------------Characters----------------------------------

        public ArrayList get_characters(int mode)
        {
            var characters = new ArrayList();
            var sql = "";

            switch (mode)
            {
                case 1:
                    sql = "select id, name from characters order by id";
                    break;
                case 0:
                    sql = "select id, name from characters order by name asc";
                    break;
            }


            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                characters.Add(new string[] {reader.GetInt32(0).ToString(), reader.GetString(1) });

            return characters;
        }

        public int get_character_id(string char_name)
        {
            var id = 0;
            var sql = "Select id from characters where name = @name";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", char_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public int get_character_id(int skin_id)
        {
            var id = 0;
            var sql = "Select character_id from skins where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public String get_character_name(int skin_id)
        {
            var name = "";
            var sql = "Select characters.name from characters join skins on(characters.id = skins.character_id) where skins.id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                name = reader.GetString(0);

            return name;
        }

        public String get_character_name_nameplate(int nameplate_id)
        {
            var name = "";
            var sql = "Select characters.name from characters join nameplates on(characters.id = nameplates.character_id) where nameplates.id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", nameplate_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                name = reader.GetString(0);

            return name;
        }

        public int get_character_id_msl(string msl_name)
        {
            var id = 0;
            var sql = "Select id from characters where msl_name = @name";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", msl_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public int get_character_id_cspfoldername(string msl_name)
        {
            var id = 0;
            var sql = "Select id from characters where csp_folder = @name";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", msl_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public String get_character_cspfolder(int char_id)
        {
            String folder = "";
            var sql = "Select csp_folder from characters where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", char_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                folder = reader.GetString(0);

            return folder;
        }

        public int get_character_uichar_position(int char_id)
        {
            var id = 0;
            var sql = "Select ui_char_db_id from characters where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", char_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public bool check_msl_character_name(string foldername)
        {
            var result = false;

            var sql = "Select id from characters where msl_name = @foldername";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("foldername", foldername);
            var reader = command.ExecuteReader();
            while (reader.Read())
                result = true;

            return result;
        }

        public bool get_character_dlc(int char_id)
        {
            var test = false;

            var sql = "select dlc from characters where id = @char_id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var val = reader.GetInt32(0);
                if (val == 1)
                    test = true;
            }

            return test;
        }

        public String get_character_value(String select,String field,String value)
        {
            String result = "";

            var sql = "Select " + select + " from characters where " + field + " = @value";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("value", value);
            var reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetString(0);

            return result;
        }


        //----------------------Skins----------------------------------------

        public ArrayList get_character_skins(string char_name, string workspace_id)
        {
            var skins = new ArrayList();

            var sql =
                "select skins.id,skins.name from skin_library Join skins on(skins.id = skin_library.skin_id) Join characters on(characters.id = skin_library.character_id) Where characters.name = @char_name and skin_library.workspace_id = @workspace_id Order by skin_library.Slot";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            command.Parameters.AddWithValue("workspace_id", int.Parse(workspace_id));
            var reader = command.ExecuteReader();
            while (reader.Read())
                skins.Add(new string[] { reader.GetInt32(0).ToString() , reader.GetString(1) });

            return skins;
        }

        public int get_character_skin_count(int character_id, int workspace_id)
        {
            var id = 0;
            var sql =
                "Select count(*) from skin_library where character_id = @character_id and workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public ArrayList get_character_custom_skins(string char_name, string workspace_id)
        {
            var skins = new ArrayList();

            var sql =
                "select skins.name from skins Join characters on(characters.id = skins.character_id) Where characters.name = @char_name and skins.locked = 0 Order by skins.id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                skins.Add(reader.GetString(0));

            return skins;
        }

        public ArrayList get_character_custom_skins_id(string char_name, string workspace_id)
        {
            var skins = new ArrayList();

            var sql =
                "select skins.id from skins Join characters on(characters.id = skins.character_id) Where characters.name = @char_name and skins.locked = 0 Order by skins.id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                skins.Add(reader.GetInt32(0));

            return skins;
        }

        public int get_skin_id(int char_id, int slot, int workspace_id)
        {
            var sql =
                "Select skin_id from skin_library where character_id = @char_id and slot = @slot and workspace_id = @workspace_id";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public int get_skin_gb_id(int skin_id)
        {
            var sql = "Select gb_id from skins where id = @skin_id";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public int get_skin_gb_uid(int skin_id)
        {
            var sql = "Select gb_uid from skins where id = @skin_id";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public string[] get_skin_info(int skin_id)
        {
            string[] info;

            var sql = "select skins.name, skins.author, skins.models, skins.csps,characters.name from skins join characters on (skins.character_id = characters.id) where skins.id = @skin_id";
            var name = "";
            var author = "";
            var models = "";
            var csps = "";
            var character = "";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                name = reader.GetString(0);
                author = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    models = reader.GetString(2);

                if (!reader.IsDBNull(3))
                    csps = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    character = reader.GetString(4);
            }

            info = new[] { name, author, models, csps,character };
            return info;
        }

        public bool skin_locked(int skin_id)
        {
            var test = false;

            var sql = "Select locked from skins where id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                test = reader.GetInt32(0) == 1 ? true : false;

            return test;
        }

        public bool skin_locked(int slot, int character_id)
        {
            var test = false;

            var sql =
                "Select locked from skin_library where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", get_property("workspace"));
            var reader = command.ExecuteReader();
            while (reader.Read())
                test = reader.GetInt32(0) == 1 ? true : false;

            return test;
        }

        public DataSet get_custom_skins()
        {
            var ds = new DataSet();

            var sql =
                "Select name as 'Skin Name', author as 'Skin Author',models as 'Model Files',csps as 'Csp files',gb_uid as 'Gamebanana User Id', id as 'Skin ID' from skins where locked = 0";
            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();

            var sqlda = new SQLiteDataAdapter(sql, db_connection);
            sqlda.Fill(ds);

            return ds;
        }

        public ArrayList get_custom_skins_id()
        {
            var skins = new ArrayList();

            var sql =
                "select skins.id from skins Where skins.locked = 0 Order by skins.character_id";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                skins.Add(reader.GetInt32(0));

            return skins;
        }

        public ArrayList GetCustomSkins()
        {
            var skins = new ArrayList();

            var sql =
                "select skins.name from skins Where skins.locked = 0 Order by skins.character_id";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                skins.Add(reader.GetString(0));

            return skins;
        }

        public ArrayList get_character_builder_skins_id(int workspace_id)
        {
            var skins = new ArrayList();

            var sql =
                "select skins.id, skin_library.slot,skins.character_id from skins Join skin_library on(skin_library.skin_id = skins.id) Where skins.locked = 0 and skin_library.workspace_id = @workspace_id Order by skins.id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] val = { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) };
                skins.Add(val);
            }

            return skins;
        }

        internal void remove_csp(int skin_id, string csp_type)
        {
            var csps = get_skin_csps(skin_id);
            var count = 0;
            var newcsps = "";
            foreach (var s in csps)
            {
                if (s != csp_type)
                    if (count == 0)
                        newcsps += s;
                    else
                        newcsps += ";" + s;
                count++;
            }


            var sql = "update skins set csps = @csps where id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("csps", newcsps);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.ExecuteNonQuery();
        }

        internal void remove_model(int skin_id, string model_name)
        {
            var models = get_skin_models(skin_id);
            var count = 0;
            var newmodels = "";
            foreach (var s in models)
            {
                if (s != model_name)
                    if (count == 0)
                        newmodels += s;
                    else
                        newmodels += ";" + s;
                count++;
            }


            var sql = "update skins set models = @models where id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("models", newmodels);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.ExecuteNonQuery();
        }

        public string[] get_skin_models(int skin_id)
        {
            string[] skin_models;
            var sql = "select models from skins where id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            var var = "";
            while (reader.Read())
                var = reader.GetString(0);
            skin_models = var.Split(';');
            return skin_models;
        }

        public string[] get_skin_csps(int skin_id)
        {
            string[] skin_csps;
            var sql = "select csps from skins where id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            var var = "";
            while (reader.Read())
                var = reader.GetString(0);
            skin_csps = var.Split(';');
            return skin_csps;
        }

        public bool check_skin_in_library(int skin_id)
        {
            var test = false;

            var sql = "Select slot from skin_library where skin_id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                test = reader.GetInt32(0) > 0 ? true : false;

            return test;
        }

        public string get_skin_modelfolder(int skin_id)
        {
            var folder = "";


            var sql =
                "Select characters.model_folder from characters join skins on(skins.character_id = characters.id) where skins.id = @skin_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                folder = reader.GetString(0);

            return folder;
        }

        public int get_skin_id_hash(string skin_hash)
        {
            var sql =
                "Select id from skins where model_hash_1 = @skin_hash or model_hash_2 = @skin_hash or csp_hash_1 = @skin_hash or csp_hash_2 = @skin_hash or csp_hash_3 = @skin_hash or csp_hash_4 = @skin_hash ";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_hash", skin_hash);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public void set_skin_name(string name, int id)
        {
            var sql = "update skins set name = @name where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_skin_author(string author, int id)
        {
            var sql = "update skins set author = @author where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_skin_gb_uid(int val, int skin_id)
        {
            var sql = "update skins set gb_uid = @gb_uid  where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("gb_uid", val);
            command.Parameters.AddWithValue("id", skin_id);
            command.ExecuteNonQuery();
        }

        public int add_skin(string name, string author, string models, string csps, int character_id, int slot)
        {
            var sql =
                "insert into skins (name,author,models,csps,character_id,locked,gb_uid) values(@name,@author,@models,@csps,@character_id,0,0)";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("models", models);
            command.Parameters.AddWithValue("csps", csps);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();

            var id = db_connection.LastInsertRowId;

            sql = "insert into skin_library (workspace_id,character_id,skin_id,slot,locked) values (" +
                  get_property("workspace") + "," + character_id + "," + id + "," + slot + ",0)";
            command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();

            return Convert.ToInt32(id);
        }

        public void insert_skin(int skin_id, int workspace_id, int char_id, int slot)
        {
            var sql =
                "insert into skin_library (workspace_id,character_id,skin_id,slot,locked) values (@workspace_id,@char_id,@skin_id,@slot,0)";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();
        }

        public void convert_skin(string name, string author, string models, string csps, int character_id, int slot)
        {
            var sql =
                "insert into skins (name,author,models,csps,character_id,locked,gb_uid) values(@name,@author,@models,@csps,@character_id,0,0)";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("models", models);
            command.Parameters.AddWithValue("csps", csps);
            command.Parameters.AddWithValue("character_id", character_id);
            command.ExecuteNonQuery();

            var id = db_connection.LastInsertRowId;

            sql =
                "update skin_library set skin_id = @skin_id where slot = @slot and workspace_id = @workspace_id and character_id = @character_id";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", id);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", get_property("workspace"));

            command.ExecuteNonQuery();
        }

        public void set_skin_hash(int id, int hash_id, string value)
        {
            var hashname = "";
            switch (hash_id)
            {
                case 0:
                    hashname = "model_hash_1";
                    break;
                case 1:
                    hashname = "model_hash_2";
                    break;
                case 2:
                    hashname = "csp_hash_1";
                    break;
                case 3:
                    hashname = "csp_hash_2";
                    break;
                case 4:
                    hashname = "csp_hash_3";
                    break;
                case 5:
                    hashname = "csp_hash_4";
                    break;
            }
            var sql = "update skins set " + hashname + " = @value where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("value", value);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        internal void remove_skin(int slot, int character_id, string selected_workspace)
        {
            var sql =
                "delete from skin_library where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", selected_workspace);
            command.ExecuteNonQuery();
        }

        internal void delete_skin(int skin_id)
        {
            var sql = "delete from skins where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", skin_id);
            command.ExecuteNonQuery();
        }

        public void add_model(int skin_id, string model_name)
        {
            var models = get_skin_models(skin_id);
            var present = false;
            var newmodels = "";
            foreach (var m in models)
            {
                if (m != "")
                    newmodels += m + ";";

                if (m == model_name)
                    present = true;
            }

            if (!present)
            {
                newmodels += model_name;
                var sql = "update skins set models = @newmodels where id = @skin_id";
                var command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("newmodels", newmodels);
                command.Parameters.AddWithValue("skin_id", skin_id);
                command.ExecuteNonQuery();
            }
        }

        public void add_csp(int skin_id, string csp_type)
        {
            var csps = get_skin_csps(skin_id);
            var present = false;
            var newcsps = "";
            foreach (var c in csps)
            {
                if (c != "")
                    newcsps += c + ";";

                if (c == csp_type)
                    present = true;
            }

            if (!present)
            {
                newcsps += csp_type;
                var sql = "update skins set csps = @newcsps where id = @skin_id";
                var command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("newcsps", newcsps);
                command.Parameters.AddWithValue("skin_id", skin_id);
                command.ExecuteNonQuery();
            }
        }

        public void set_skin_nameplate(int char_id,int slot,int nameplate_id, int workspace)
        {
            var sql =
               "update skin_library set nameplate_id = @nameplate_id where character_id = @char_id and slot = @slot and workspace_id = @workspace";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace", workspace);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("nameplate_id", nameplate_id);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();
        }


        //----------------------Nameplates-----------------------------------

        public string[] get_nameplate_info(int nameplate_id)
        {
            string[] info;

            var sql = "select nameplates.name, nameplates.author, characters.name from nameplates join characters on(characters.id = nameplates.character_id) where nameplates.id = @nameplate_id";
            var name = "";
            var author = "";
            var character = "";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("nameplate_id", nameplate_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                name = reader.GetString(0);
                author = reader.GetString(1);
                character = reader.GetString(2);
            }

            info = new[] { name, author,character };
            return info;
        }

        public ArrayList get_character_nameplates(int char_id)
        {
            ArrayList al = new ArrayList();
            var sql =
                "Select name from nameplates where character_id = @char_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                String s = reader.GetString(0);
                al.Add(s);
            }

            return al;
        }

        public ArrayList get_character_nameplates_id(int char_id)
        {
            ArrayList al = new ArrayList();
            var sql =
                "Select id from nameplates where character_id = @char_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int s = reader.GetInt32(0);
                al.Add(s);
            }

            return al;
        }

        public String get_skin_nameplate(int slot, int character_id, int workspace)
        {
            String name = "";
            var sql = "Select nameplates.name from nameplates join skin_library on(nameplates.id = skin_library.nameplate_id) where skin_library.character_id = @id and skin_library.slot = @slot and skin_library.workspace_id = @workspace";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", character_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace", workspace);
            var reader = command.ExecuteReader();
            while (reader.Read())
                name = reader.GetString(0);

            return name;
        }

        public ArrayList get_character_custom_nameplates(string char_name, string workspace_id)
        {
            var nameplates = new ArrayList();

            var sql =
                "select nameplates.name from nameplates Join characters on(characters.id = nameplates.character_id) Where characters.name = @char_name Order by nameplates.id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                nameplates.Add(reader.GetString(0));

            return nameplates;
        }

        public ArrayList get_character_custom_nameplates_id(string char_name, string workspace_id)
        {
            var nameplates = new ArrayList();

            var sql =
                "select nameplates.id from nameplates Join characters on(characters.id = nameplates.character_id) Where characters.name = @char_name  Order by nameplates.id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            var reader = command.ExecuteReader();
            while (reader.Read())
                nameplates.Add(reader.GetInt32(0));

            return nameplates;
        }

        public int get_character_id_nameplate(int nameplate_id)
        {
            var sql =
               "Select character_id from nameplates where id = @id";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", nameplate_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }

        public ArrayList GetCustomNameplates()
        {
            var nameplates = new ArrayList();

            var sql =
                "select name from nameplates order by id";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                nameplates.Add(reader.GetString(0));

            return nameplates;
        }

        public ArrayList get_custom_nameplates_id()
        {
            var nameplates_id = new ArrayList();

            var sql =
                "select id from nameplates Order by id";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                nameplates_id.Add(reader.GetInt32(0));

            return nameplates_id;
        }

        public void set_nameplate_name(string name, int id)
        {
            var sql = "update nameplates set name = @name where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_nameplate_author(string name, int id)
        {
            var sql = "update nameplates set author = @name where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public long insert_nameplate(int char_id)
        {
            var sql = "insert into nameplates (name,character_id,author,hash) values (@name,@char_id,'','')";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", "New Nameplate");
            command.Parameters.AddWithValue("char_id", char_id);
            command.ExecuteNonQuery();

            return db_connection.LastInsertRowId;
        }

        internal void delete_nameplate(int skin_id)
        {
            var sql = "delete from nameplates where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", skin_id);
            command.ExecuteNonQuery();
        }

        public int get_character_nameplate_count(int char_id)
        {
            var sql =
              "Select nameplate_count from characters where id = @id";
            var id = 0;
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", char_id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);

            return id;
        }


        public ArrayList get_custom_nameplates()
        {
            ArrayList nameplates = new ArrayList();

            var sql = "select nameplates.id, nameplates.character_id, skin_library.Slot, characters.csp_folder from nameplates join skin_library on (nameplates.id = skin_library.nameplate_id) join characters on (nameplates.character_id = characters.id) order by nameplates.id";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] values = new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) };
                nameplates.Add(values);
            }
           

            return nameplates;
        }

        //----------------------Workspace-----------------------------------

        public ArrayList get_workspaces()
        {
            var workspaces = new ArrayList();
            var sql = "select name from workspaces where slot != 1";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                workspaces.Add(reader.GetString(0));

            return workspaces;
        }

        public int get_workspace_count()
        {
            var id = 0;
            var sql = "select count(*) from workspaces";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);
            return id;
        }

        public bool workspace_default(int slot)
        {
            var result = false;
            var sql = "select locked from workspaces where slot = @slot";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            var reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt32(0) == 1 ? true : false;

            return result;
        }

        public String[] get_workspace_stats(int slot)
        {
            string[] stats;
            var skinCount = 0;
            var nameplateCount = 0;
            var buildcount = 0;
            var sql =
                "select count(*) from skin_library join workspaces on (workspaces.id = skin_library.workspace_id) where skin_library.locked = 0 and  workspaces.slot = @slot";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            var reader = command.ExecuteReader();
            while (reader.Read())
                skinCount = reader.GetInt32(0);

            sql =
                "select count(*) from skin_library join workspaces on (workspaces.id = skin_library.workspace_id) where nameplate_id != 0 and  workspaces.slot = @slot";

            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            reader = command.ExecuteReader();
            while (reader.Read())
                nameplateCount = reader.GetInt32(0);

            sql =
                "select date from workspaces where workspaces.slot = @slot";

            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            reader = command.ExecuteReader();
            string date = "";
            while (reader.Read())
                date = reader.GetString(0);

            sql =
                "select builds from workspaces where workspaces.slot = @slot";

            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            reader = command.ExecuteReader();
            while (reader.Read())
                buildcount = reader.GetInt32(0);

            sql =
                "select charaname,max(cnt) from ( select count(*) as cnt,characters.name as charaname from skin_library join characters on (characters.id = skin_library.character_id) join workspaces on (skin_library.workspace_id = workspaces.id) where workspaces.slot = @slot and skin_library.locked = 0 group by skin_library.character_id)";

            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            reader = command.ExecuteReader();
            string mostskin = "";
            try
            {
                while (reader.Read())
                    mostskin = reader.GetString(0);
            }
            catch
            {
                
            }
            

            stats = new[] { skinCount.ToString(), nameplateCount.ToString(), date, buildcount.ToString(), mostskin };
            return stats;
        }

        public int get_workspace_id(int slot)
        {
            var id = 0;
            var sql = "select id from workspaces where slot = @slot";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            var reader = command.ExecuteReader();
            while (reader.Read())
                id = reader.GetInt32(0);
            return id;
        }

        public int get_workspace_slot(int id)
        {
            var slot = 0;
            var sql = "select slot from workspaces where id = @id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                slot = reader.GetInt32(0);
            return slot;
        }

        public string GetWorkspaceName(int id)
        {
            var name = "";
            var sql = "select name from workspaces where id = @id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            var reader = command.ExecuteReader();
            while (reader.Read())
                name = reader.GetString(0);
            return name;
        }

        public void set_workspace_name(string name, int workspaceid)
        {
            var sql = "update workspaces set name = @name where id = @id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("id", workspaceid);
            command.ExecuteNonQuery();
        }

        internal void clear_workspace(int workspace_id)
        {
            var sql = "delete from skin_library where workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }

        internal void delete_workspace(int workspace_id)
        {
            var sql = "delete from skin_library where workspace_id = @workspace_id";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();

            sql = "delete from workspaces where id = @workspace_id";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }

        public void add_default_skins(long id)
        {
            var lines = new ArrayList();
            var sql = "select character_id,skin_id,slot from skin_library Where skin_library.workspace_id = 1";

            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] line = { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) };
                lines.Add(line);
            }

            foreach (int[] line in lines)
            {
                sql =
                    "insert into skin_library (character_id,skin_id,slot,locked,workspace_id) values (@character_id,@skin_id,@slot,1,@id)";
                command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("character_id", line[0]);
                command.Parameters.AddWithValue("skin_id", line[1]);
                command.Parameters.AddWithValue("slot", line[2]);
                command.Parameters.AddWithValue("id", id);
                command.ExecuteNonQuery();
            }
        }

        public Boolean copy_skins(long id_source, long id_dest)
        {
            Boolean result = true;

            try
            {
                clear_workspace(Convert.ToInt32(id_dest));

                var lines = new ArrayList();
                var sql = "select character_id,skin_id,slot,locked from skin_library Where skin_library.workspace_id = " + id_source;

                var command = new SQLiteCommand(sql, db_connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int[] line = { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3) };
                    lines.Add(line);
                }

                foreach (int[] line in lines)
                {
                    sql =
                        "insert into skin_library (character_id,skin_id,slot,locked,workspace_id) values (@character_id,@skin_id,@slot,1,@id)";
                    command = new SQLiteCommand(sql, db_connection);
                    command.Parameters.AddWithValue("character_id", line[0]);
                    command.Parameters.AddWithValue("skin_id", line[1]);
                    command.Parameters.AddWithValue("slot", line[2]);
                    command.Parameters.AddWithValue("locked", line[3]);
                    command.Parameters.AddWithValue("id", id_dest);
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public long add_workspace(string name)
        {
            reorder_workspace();
            int newSlot = get_workspace_count() +1;

            var sql = "insert into workspaces (name,slot,locked) values (@name,@slot,0)";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("slot", newSlot);
            command.ExecuteNonQuery();

            return db_connection.LastInsertRowId;
        }

        public void SetWorkspaceDate(string date, int workspaceId)
        {
            var sql =
                "update workspaces set date = @date where id = @workspace_id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("date", date);
            command.Parameters.AddWithValue("workspace_id", workspaceId);
            command.ExecuteNonQuery();
        }

        public void upBuildCount(int workspaceId)
        {
            int builds = getWorkspaceBuilds(workspaceId) +1;
            var sql =
                "update workspaces set builds = @builds where id = @workspace_id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("builds", builds.ToString());
            command.Parameters.AddWithValue("workspace_id", workspaceId);
            command.ExecuteNonQuery();
        }

        public int getWorkspaceBuilds(int workspaceId)
        {
            var builds = 0;
            var sql = "select builds from workspaces where id = @id";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", workspaceId);
            var reader = command.ExecuteReader();
            while (reader.Read())
                builds = reader.GetInt32(0);
            return builds;
        }

        //----------------------Packer----------------------------------------

        public DataSet get_packer_content()
        {
            ArrayList results = new ArrayList();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add("Content Type");
            dt.Columns.Add("Name");
            dt.Columns.Add("Installed on");

            var sql =
                "Select content_type, content_id from packer";
            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int[] ids = new int[] { reader.GetInt32(0), reader.GetInt32(1) };
                String type = "";
                String name = "";
                String parent = "";

                switch (ids[0])
                {
                    //Skins
                    case 0:
                        type = "Skins";
                        name = get_skin_info(ids[1])[0];
                        parent = get_character_name(ids[1]);
                        break;
                    case 1:
                        type = "Nameplate";
                        name = get_nameplate_info(ids[1])[0];
                        parent = get_character_name(ids[1]);
                        break;
                }
                DataRow dr = dt.NewRow();
                dr[0] = type;
                dr[1] = name;
                dr[2] = parent;
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);

            return ds;
        }

        public ArrayList get_packer_ids()
        {
            ArrayList al = new ArrayList();
            var sql =
                "Select content_type, content_id from packer";
            var command = new SQLiteCommand(sql, db_connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int[] ids = new int[] { reader.GetInt32(0), reader.GetInt32(1) };
                al.Add(ids);
            }

            return al;
        }

        public void add_packer_item(int type, int id)
        {
            var sql = "insert into packer (content_type,content_id) values (@type,@id)";
            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("type", type);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();

        }

        public void clear_packer_content()
        {
            var sql = "delete from packer";
            var command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        //----------------------Config----------------------------------------

        public string get_property(string property)
        {
            var value = "";
            var sql = "select value from configuration where property = @property";

            var command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("property", property);
            var reader = command.ExecuteReader();

            while (reader.Read())
                value = reader.GetString(0);

            return value;
        }


        public void set_property_value(string value, string property)
        {
            var test = get_property(property);
            if (test != "")
            {
                var sql = "update configuration set value = @value where property =  @property";
                var command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("value", value);
                command.Parameters.AddWithValue("property", property);
                command.ExecuteNonQuery();
            }
            else
            {
                var sql = "insert into configuration ('property','value') values (@property,@value)";
                var command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("value", value);
                command.Parameters.AddWithValue("property", property);
                command.ExecuteNonQuery();
            }
        }

        public Boolean UpdateDatabase(String query)
        {
            try
            {
                var command = new SQLiteCommand(query, db_connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }

        }


    }
}