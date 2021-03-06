﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PedalBuilds
{
    class CreateDb
    {
        public static void Create()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataFolder = Path.Combine(appDataFolder, "Pedal Builder");

            try
            {
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                if (!File.Exists(Path.Combine(dataFolder, "Pedals.sqlite")))
                {
                    SQLiteConnection.CreateFile(Path.Combine(dataFolder, "Pedals.sqlite"));
                }

                SQLiteConnection con = new SQLiteConnection(@"Data Source=" + dataFolder + "/Pedals.sqlite;Version=3");
                con.Open();

                string pedalSql = "CREATE TABLE IF NOT EXISTS pedals (" +
                                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                    "name TEXT NOT NULL," +
                                    "builds INTEGER," +
                                    "notes TEXT)";

                string componentSql = "CREATE TABLE IF NOT EXISTS components (" +
                                        "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                        "type TEXT," +
                                        "value TEXT," +
                                        "notes TEXT," +
                                        "url TEXT," +
                                        "price REAL)";

            string partListSql = "CREATE TABLE IF NOT EXISTS partlist (" +
                                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                    "partname TEXT NOT NULL," +
                                    "component_id INTEGER NOT NULL," +
                                    "pedal_id INTEGER NOT NULL)";

                SQLiteCommand cmd = new SQLiteCommand(pedalSql, con);
                cmd.ExecuteNonQuery();

                cmd.CommandText = componentSql;
                cmd.ExecuteNonQuery();

                cmd.CommandText = partListSql;
                cmd.ExecuteNonQuery();

                con.Close();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }
    }
}