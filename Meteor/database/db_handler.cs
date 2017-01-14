using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteor.database
{
    class db_handler
    {
        #region Class Variables
        SQLiteConnection db_connection;
        #endregion

        #region Constructors
        public db_handler()
        {
            
            db_connection = new SQLiteConnection("Data Source=Library.sqlite;Version=3;");
            db_connection.Open();
        }
        #endregion

        #region Get
        //Gets the list of characters by game order or alphabetical order
        public ArrayList get_characters(int mode)
        {
            ArrayList characters = new ArrayList();
            string sql = "";

            switch (mode)
            {
                case 1:
                    sql = "select name from characters";
                    break;
                case 2:
                    sql = "select name from characters order by name asc";
                    break;
            }
             

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                characters.Add(reader.GetString(0));
            }

            return characters;
        }

        //Gets character id
        public int get_character_id(String char_name)
        {
            int id = 0;
            String sql = "Select id from characters where name = '"+char_name+"'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        //Get's a skin id based on char id and slot
        public int get_skin_id(int char_id, int slot)
        {
            String sql = "Select skin_id from skin_library where character_id = "+char_id+" and slot = " + slot;
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        //gets the skin list for a specific character name
        public ArrayList get_character_skins(String char_name)
        {
            ArrayList skins = new ArrayList();

            String sql = "select skins.name from skin_library Join skins on(skins.id = skin_library.skin_id) Join characters on(characters.id = skin_library.character_id) Where characters.name = '"+char_name+"' and skin_library.workspace_id = 1 Order by skin_library.Slot";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skins.Add(reader.GetString(0));
            }

            return skins;
        }

        //Gets the skin info
        public String[] get_skin_info(int slot, String char_name)
        {
            String[] info;

            String sql = "select skins.name, skins.author, skins.models, skins.csps from skin_library Join skins on(skins.id = skin_library.skin_id) Join characters on(characters.id = skin_library.character_id) Where characters.name = '" + char_name + "' and skin_library.Slot = " + slot + " and skin_library.workspace_id = 1 Order by skin_library.Slot";
            String name = "";
            String author = "";
            String models = "";
            String csps = "";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                name = reader.GetString(0);
                author = reader.GetString(1);
                if (!reader.IsDBNull(2))
                {
                    models = reader.GetString(2);
                }

                if (!reader.IsDBNull(3))
                {
                    csps = reader.GetString(3);
                }
                
            }

            info = new String[] { name,author,models,csps };
            return info;
        }

        //Gets a property value
        public String get_property(String property)
        {
            String value = "";
            String sql = "select value from configuration where property = '"+property+"'";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                value = reader.GetString(0);
            }

            return value;
        }
        #endregion

        #region Set
        public void set_skin_name(String name,int id)
        {
            String sql = "update skins set name ='"+name+"' where id = "+id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }
        public void set_skin_author(String author, int id)
        {
            String sql = "update skins set author ='" + author + "' where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        public void set_property_value(String value,String property)
        {
            String sql = "update configuration set value ='" + value + "' where property = '" + property+"'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }
        #endregion

        #region Insert
        public void add_skin(String name,String author,String models, String csps,int character_id)
        {
            String sql = "insert into skin (name,author,models,csps,character_id) values('"+name+ "','" + author + "','" + models + "','" + csps + "'," + character_id + ")";
        }
        #endregion

    }
}
