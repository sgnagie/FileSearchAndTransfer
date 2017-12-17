using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace JournalFileTransfer
{
    public partial class Form1 : Form
    {
        string file = "";                                       //input file in csv format with no headers
        string saveDest = "";                                   //Dir we will be saving too
        string SearchDir = "";                                  //Dir we will be searching
        List<string> filesToFind = new List<string>();          //list of files we will be searching for including both journal ID an payment ID
        List<string> paymentFilesList = new List<string>();     //list to compare to to make sure all files are found
        List<string> foundFilesList = new List<string>();       //list of all found files

        public Form1()
        {
            InitializeComponent();
            SetMyButtonIcon();
            //textBox2.Text = "D:\\temp";
            //textBox3.Text = "D:\\temp";
            saveDest = textBox2.Text;
            SearchDir = textBox3.Text;
        }

        private void SetMyButtonIcon()
        {
            // Assign an image to the button.
            Image img = Properties.Resources.folder_32x32;
            button1.Image = img;
            button2.Image = img;
            button3.Image = img;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV Files | *.csv";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Select a Cursor File";

            // Show the Dialog.  
            // If the user clicked OK in the dialog  
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.  
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        //sets the save destination where all folders will be created and files saved
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;
                    saveDest = textBox2.Text;
                }
            }
        }

        // Sets the search directory and all subfolders
        private void button3_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox3.Text = fbd.SelectedPath;
                    SearchDir = textBox3.Text;
                }
            }
        }

        private void btBeginImport_Click(object sender, EventArgs e)
        {
            
            if(String.IsNullOrEmpty(textBox1.Text) | String.IsNullOrEmpty(textBox2.Text) | String.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Please select an Import file, Save Destination and Search Directory.");
            }
            else
            {
                file = textBox1.Text;
                ParseFile(file);
                CreateFoldersAndCopyFiles(filesToFind);
            }
        }

        private List<string> ParseFile(string file)
        {
            try
            {
                using (var reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');
                        filesToFind.Add(values[0]);
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Unable to access file. It may be in use by another process.");
            }
            return filesToFind;
        }

        // Method to check for folders existance and create if not. 
        private void CreateFoldersAndCopyFiles(List<string> listA)
        {
            foreach(string t in listA)
            {
                string[] val = t.Split(',');
                string journalID = val[0];
                string paymentID = val[1];
                string SaveFilePath = saveDest + "\\" + journalID;
                paymentFilesList.Add(paymentID);
                //next create folder to save to in saveDest
                try
                {
                    if (!Directory.Exists(SaveFilePath))
                    {
                        Directory.CreateDirectory(SaveFilePath);
                    }

                    SearchFilesAndCopy(paymentID, SearchDir, SaveFilePath);

                }
                catch (Exception ex)
                {
                    // handle them here
                    lbMessage.Text = "Unable to create and save to this destination \r\n" + ex.Message;
                    lbMessage.Refresh();
                }

            }

            List<string> FilesNotFound = new List<string>();

                    foreach (var str in paymentFilesList)
                    {
                        // FirstOrDefault finds first match or returns default (null for string) if not found.
                        var match = foundFilesList.FirstOrDefault(s => s.StartsWith(str));
                        if (match == null)
                        {
                            FilesNotFound.Add(str);
                        }
                    }

            if(FilesNotFound.Count > 0)
            {
                TextWriter tw = new StreamWriter(saveDest + "\\FilesNotFound.txt");
                foreach (String s in FilesNotFound)
                    tw.WriteLine(s);
                tw.Close();
                MessageBox.Show(FilesNotFound.Count + " files were not found.\n\r " + "Check the \"FilesNotFound.txt\" file for this list:");
            }

            MessageBox.Show("Complete!");
            CloseProgram();
        }

        private void SearchFilesAndCopy(string paymentID, string SearchDir, string SaveDir)
        {
            string partialName = paymentID;
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(SearchDir);
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + partialName + "*.*", SearchOption.AllDirectories);

            foreach (FileInfo foundFile in filesInDir)
            {
                lbMessage.Text = "Found and copying file - " + foundFile;
                lbMessage.Refresh();
                foundFilesList.Add(Path.GetFileNameWithoutExtension(foundFile.ToString()));
                string fullName = foundFile.FullName;
                string toDest = SaveDir + "\\" + foundFile;
                File.Copy(fullName, toDest, true);
                System.Threading.Thread.Sleep(250);
            }
        }

        private void CloseProgram()
        {
            this.Close();
            Application.Exit();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
