﻿using Database.Models;
using System;
using System.Collections;
using System.Data.SQLite;

namespace Database
{
    public class Servers : SQLiteDataController
    {
        public Servers()
        {
        }

        public ArrayList ArrayListServers { get; set; } = new ArrayList();

        public void Read()
        {
            string sql = "SELECT * FROM Servers";

            //using (SQLiteConnection conn = new SQLiteConnection())
            //{

            //}

            string result = base.ExecuteQuery(sql, null);

            this.ArrayListServers.Clear();

            if (result == string.Empty)
            {
                if (base.Database.Reader.HasRows)
                {
                    while (base.Database.Reader.Read())
                    {
                        var pwd = (new Func<string>(() =>
                        {
                            string pword = base.Database.Reader["password"].ToString();
                            if (pword != string.Empty)
                            {
                                pword = RijndaelSettings.Decrypt(pword);
                            }

                            return pword;
                        }).Invoke());

                        var host = new Host(Database.Reader["server"].ToString(), int.Parse(base.Database.Reader["port"].ToString()));
                        var creds = new Credentials(Database.Reader["domain"].ToString(),
                            Database.Reader["username"].ToString(), pwd);
                        Model_ServerDetails sd = new Model_ServerDetails(host, creds)
                        {
                            UID = base.Database.Reader["uid"].ToString(),
                            GroupID = int.Parse(base.Database.Reader["groupid"].ToString()),
                            ServerName = base.Database.Reader["servername"].ToString(),

                            Description = base.Database.Reader["description"].ToString(),
                            ColorDepth = int.Parse(base.Database.Reader["colordepth"].ToString()),
                            DesktopWidth = int.Parse(base.Database.Reader["desktopwidth"].ToString()),
                            DesktopHeight = int.Parse(base.Database.Reader["desktopheight"].ToString()),
                            Fullscreen = int.Parse(base.Database.Reader["fullscreen"].ToString()) == 1 ? true : false
                        };

                        this.ArrayListServers.Add(sd);
                    }
                }
            }
            else
            {
                base.Database.CloseConnection();
                System.Diagnostics.Debug.WriteLine(result);
                throw new Exception(result);
            }

            base.Database.CloseConnection();
        }

        public void Save(bool isNew, Model_ServerDetails server_details)
        {
            if (isNew)
            {
                Save(server_details);
            }
            else
            {
                Update(server_details);
            }
        }

        private void Save(Model_ServerDetails server_details)
        {
            #region sql
            string sql = "INSERT INTO Servers(uid, groupid, servername, server, domain, port, username, password, description, colordepth, desktopwidth, desktopheight, fullscreen) ";
            sql += "VALUES(@uid, @gid, @sname, @server, @domain, @port, @uname, @pword, @desc, @cdepth, @dwidth, @dheight, @fscreen)";
            #endregion

            #region params

            var pwd = server_details.Login.Password;
            if (pwd != string.Empty) { pwd = RijndaelSettings.Encrypt(pwd); }

            SQLiteParameter[] parameters = {
                                               new SQLiteParameter("@uid", server_details.UID),
                                               new SQLiteParameter("@gid", server_details.GroupID),
                                               new SQLiteParameter("@sname", server_details.ServerName),
                                               new SQLiteParameter("@server", server_details.Host.Name),
                                               new SQLiteParameter("@domain", server_details.Login.Domain),
                                               new SQLiteParameter("@port", server_details.Host.Port),
                                               new SQLiteParameter("@uname", server_details.Login.UserName),
                                               new SQLiteParameter("@pword", pwd),
                                               new SQLiteParameter("@desc", server_details.Description),
                                               new SQLiteParameter("@cdepth", server_details.ColorDepth),
                                               new SQLiteParameter("@dwidth", server_details.DesktopWidth),
                                               new SQLiteParameter("@dheight", server_details.DesktopHeight),
                                               new SQLiteParameter("@fscreen", server_details.Fullscreen)
                                           };
            #endregion

            string result = base.ExecuteNonQuery(sql, parameters);

            if (result == string.Empty)
            {
            }
            else
            {
                base.Database.CloseConnection();
                System.Diagnostics.Debug.WriteLine(result);

                if (result.Contains("Abort due to constraint violation"))
                {
                    throw new DatabaseException(DatabaseException.ExceptionTypes.DUPLICATE_ENTRY);
                }
                else
                {
                    throw new Exception(result);
                }
            }

            base.Database.CloseConnection();
        }

        private void Update(Model_ServerDetails server_details)
        {
            #region sql
            string sql = @"
UPDATE 
    Servers 
SET 
    uid=@uid, 
    groupid=@gid,
    servername=@sname, 
    server=@server,
    domain=@domain,
    port=@port,
    username=@uname,
    password=@pword,
    description=@desc,
    colordepth=@cdepth, 
    desktopwidth=@dwidth,
    desktopheight=@dheight,
    fullscreen=@fscreen
WHERE
    uid=@uid";
            #endregion

            #region params
            var pwd = server_details.Login.Password;
            if (pwd != string.Empty) { pwd = RijndaelSettings.Encrypt(pwd); }

            SQLiteParameter[] parameters = {
                                               new SQLiteParameter("@uid", server_details.UID),
                                               new SQLiteParameter("@gid", server_details.GroupID),
                                               new SQLiteParameter("@sname", server_details.ServerName),
                                               new SQLiteParameter("@server", server_details.Host.Name),
                                               new SQLiteParameter("@domain", server_details.Login.Domain),
                                               new SQLiteParameter("@port", server_details.Host.Port),
                                               new SQLiteParameter("@uname", server_details.Login.UserName),
                                               new SQLiteParameter("@pword", pwd),
                                               new SQLiteParameter("@desc", server_details.Description),
                                               new SQLiteParameter("@cdepth", server_details.ColorDepth),
                                               new SQLiteParameter("@dwidth", server_details.DesktopWidth),
                                               new SQLiteParameter("@dheight", server_details.DesktopHeight),
                                               new SQLiteParameter("@fscreen", server_details.Fullscreen)
                                           };
            #endregion

            string result = base.ExecuteNonQuery(sql, parameters);

            if (result == string.Empty)
            {
            }
            else
            {
                base.Database.CloseConnection();
                System.Diagnostics.Debug.WriteLine(result);

                if (result.Contains("Abort due to constraint violation"))
                {
                    throw new DatabaseException(DatabaseException.ExceptionTypes.DUPLICATE_ENTRY);
                }
                else
                {
                    throw new Exception(result);
                }
            }

            base.Database.CloseConnection();
        }

        public void UpdateGroupIdByID(string id, int newGroupID)
        {
            string sql = "UPDATE Servers SET groupid = @gid WHERE Servers.uid = @uid";
            SQLiteParameter[] parameters = {
                                               new SQLiteParameter("@gid", newGroupID),
                                               new SQLiteParameter("@uid", id)
                                           };

            string result = base.ExecuteNonQuery(sql, parameters);

            if (result == string.Empty)
            {

            }
            else
            {
                base.Database.CloseConnection();
                throw new Exception(result);
            }

            base.Database.CloseConnection();
        }

        public void DeleteByID(string id)
        {
            string sql = "DELETE FROM Servers WHERE Servers.uid=@uid";

            SQLiteParameter[] parameters = {
                                               new SQLiteParameter("@uid", id)
                                           };

            string result = base.ExecuteNonQuery(sql, parameters);

            if (result == string.Empty)
            {

            }
            else
            {
                base.Database.CloseConnection();
                System.Diagnostics.Debug.WriteLine(result);
                throw new Exception(result);
            }

            base.Database.CloseConnection();
        }
    }
}
