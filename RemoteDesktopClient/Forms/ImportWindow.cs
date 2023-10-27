﻿using RDPFileReader;
using System;
using System.IO;
using System.Windows.Forms;
using Database.Models;

namespace MultiRemoteDesktopClient
{
    public partial class ImportWindow : Form
    {
        OpenFileDialog ofd = null;

        public ImportWindow()
        {
            InitializeComponent();
            InitializeControls();
            InitializeControlEvents();
        }

        public void InitializeControls()
        {
        }

        public void InitializeControlEvents()
        {
            this.Shown += new EventHandler(ImportWindow_Shown);
            this.btnStart.Click += new EventHandler(btnStart_Click);
            this.btnBrowse.Click += new EventHandler(btnBrowse_Click);
        }

        void btnBrowse_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "RDP File|*.rdp";
            ofd.Multiselect = true;
            ofd.Title = "Import RDP File";
            ofd.ShowDialog();

            foreach (string thisFile in ofd.FileNames)
            {
                System.Diagnostics.Debug.WriteLine("reading " + thisFile);

                #region Read RDP File

                RDPFile rdpfile;
                {
                    try
                    {
                        rdpfile = new RDPFile();
                        rdpfile.Read(thisFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occured while reading '" + Path.GetFileName(thisFile) + "' and it will be skipped.\r\n\r\nError Message: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);

                        continue;
                    }
                }
                #endregion

                Model_ServerDetails sd = new Model_ServerDetails(new Host(rdpfile.FullAddress), ReadCredentialsFromRDP(rdpfile));
                sd.UID = DateTime.Now.Ticks.ToString();
                sd.GroupID = 1;
                sd.ServerName = System.IO.Path.GetFileNameWithoutExtension(thisFile);
                
                sd.Description = "Imported from " + thisFile;
                sd.ColorDepth = (int)rdpfile.SessionBPP;
                sd.DesktopWidth = rdpfile.DesktopWidth;
                sd.DesktopHeight = rdpfile.DesktopHeight;
                sd.Fullscreen = false;

                ListViewItem thisItem = new ListViewItem(Path.GetFileNameWithoutExtension(thisFile));
                thisItem.SubItems.Add("OK");
                thisItem.SubItems.Add(thisFile);
                thisItem.Tag = sd;
                thisItem.ImageIndex = 0;

                lvRDPFiles.Items.Add(thisItem);
            }

            foreach (ColumnHeader ch in lvRDPFiles.Columns)
            {
                ch.Width = -1;
            }
        }

        private Credentials ReadCredentialsFromRDP(RDPFile file)
        {
            string RDPPassword;
            #region Try decrypting the password from RDP file
           {
                try
                {
                    //System.Diagnostics.Debug.WriteLine("reading password " + thisFile);

                    RDPPassword = file.Password;
                    if (RDPPassword != string.Empty)
                    {
                        // based on http://www.remkoweijnen.nl/blog/2008/03/02/how-rdp-passwords-are-encrypted-2/
                        // he saids, MSTSC just add a ZERO number at the end of the hashed password.
                        // so let's just removed THAT!
                        RDPPassword = RDPPassword.Substring(0, RDPPassword.Length - 1);
                        // and decrypt it!
                        RDPPassword = DataProtection.DataProtectionForRDPWrapper.Decrypt(RDPPassword);

                        RDPPassword = RDPPassword;
                    }

                    System.Diagnostics.Debug.WriteLine("reading password done");
                }
                catch (Exception Ex)
                {
                    RDPPassword = string.Empty;

                    if (Ex.Message == "Problem converting Hex to Bytes")
                    {
                        MessageBox.Show("This RDP File contains a secured password which is currently unsported by this application.\r\nThe importing can still continue but without the password.\r\nYou can edit the password later by selecting a server in 'All Listed Servers' and click 'Edit Settings' button on the toolbar", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (Ex.Message.Contains("Exception decrypting"))
                    {
                        MessageBox.Show("Failed to decrypt the password", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("An unknown error occured while decrypting the password", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            #endregion

            return new Credentials(file.Domain, file.Username, RDPPassword);
        }

        void btnStart_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem thisItem in lvRDPFiles.Items)
            {
                thisItem.SubItems[1].Text = "Importing...";

                Model_ServerDetails sd = (Model_ServerDetails)thisItem.Tag;

                try
                {
                    GlobalHelper.dbServers.Save(true, sd);
                }
                catch (Database.DatabaseException settingEx)
                {
                    if (settingEx.ExceptionType == Database.DatabaseException.ExceptionTypes.DUPLICATE_ENTRY)
                    {
                        MessageBox.Show("Can't save '" + sd.ServerName + "' due to duplicate entry", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                thisItem.SubItems[1].Text = "Done!";
            }

            foreach (ColumnHeader ch in lvRDPFiles.Columns)
            {
                ch.Width = -1;
            }
        }

        void ImportWindow_Shown(object sender, EventArgs e)
        {
            
        }
    }
}
