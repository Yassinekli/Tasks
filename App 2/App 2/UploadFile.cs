using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace App_2
{
    public partial class UploadFile : UserControl
    {
        private string fileName;

        public UploadFile()
        {
            InitializeComponent();
        }

        private void UploadFile_Load(object sender, EventArgs e)
        {
            // Get all uploaded files from database.
            dgvUploadedFiles.DataSource = DatabaseService.GetUploadedFiles();
            // Check if all imported files exist in their locations.
            if(!AllFilesExist() && ((List<Uploaded_Files>)dgvUploadedFiles.DataSource).Count() != 0)
            {
                if(MessageBox.Show("Some files have been missed.\nWould you like to delete them from database ?", "Missing files", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    foreach (var item in DatabaseService.GetUploadedFiles().Where(file => !File.Exists(file.File_Path)))
                        DatabaseService.DeleteFile(item.File_ID);
                    dgvUploadedFiles.DataSource = DatabaseService.GetUploadedFiles();
                }
            }
            // Configure DataGridView UI.
            ConfigDGVUploadedFiles();
        }

        private bool AllFilesExist()
        {
            return ((List<Uploaded_Files>)dgvUploadedFiles.DataSource).Where(file => !File.Exists(file.File_Path)).Count() == 0;
        }

        private void ConfigDGVUploadedFiles()
        {
            // Make column [File ID] invisible,
            // Under the hood, column [File ID] will stay working to retreive key value for the selected row.
            dgvUploadedFiles.Columns[2].Visible = false;
            // Change position of [Update] and [Delete] buttons, this code will not change indexes of these two columns, it will only change position in DataGridView.
            dgvUploadedFiles.Columns[0].DisplayIndex = 5;
            dgvUploadedFiles.Columns[1].DisplayIndex = 5;
            // Change header text for column File Path
            dgvUploadedFiles.Columns[5].HeaderText = "File Path";
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            // Check if file's path, title and description exists.
            if(!tbTitle.Text.Trim().Equals("") && !lblFilePath.Text.Equals("No imported file") && !tbDescription.Text.Trim().Equals(""))
            {
                // Make sure that user didn't change location of imported file.
                if(File.Exists(lblFilePath.Text))
                {
                    // Check if the file already exists in database.
                    if(DatabaseService.FileNotExists(fileName))
                    {
                        // Create file that I will copy files into it if it doesn't exist
                        if (!Directory.Exists("../../../Uploaded Files"))
                            Directory.CreateDirectory("../../../Uploaded Files");
                        // Get destination full path for [Uploaded Files] folder,
                        // and concatenate the destination full path with the selected file name.
                        string destFileName = Path.Combine(Path.GetFullPath("../../../Uploaded Files"), fileName);

                        try
                        {
                            // Copy the selected file into [Uploaded files] folder, and set third parameter to true to overwrite the file.
                            File.Copy(lblFilePath.Text, destFileName, true);

                            // Add file's path to database.
                            DatabaseService.AddNewFile(new Uploaded_Files
                            {
                                Title = tbTitle.Text.Trim(),
                                Description = tbDescription.Text.Trim(),
                                File_Path = destFileName
                            });

                            //// Update form's controles.
                            UpdateControles();
                        }
                        catch (Exception exception)
                        {
                            CatchError(exception, "The selected file has too long path.\n Change imported file's location to short path to avoid this error.");
                        }
                    }
                    else
                        MessageBox.Show("The imported file already exists in database !", "File already exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                    MessageBox.Show("Imported file doesn't exist !", "Not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("To upload a file, you must import a file and set title and description to it.", "Invalid upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateControles()
        {
            tbTitle.Text = "";
            tbDescription.Text = "";
            lblFilePath.Text = "No imported file";
            dgvUploadedFiles.DataSource = DatabaseService.GetUploadedFiles();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Allow user to select only one file.
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get full path for selected file.
                    lblFilePath.Text = openFileDialog.FileName;
                    // Get the name of selected file.
                    fileName = openFileDialog.SafeFileName;
                }
            }
        }

        private void dgvUploadedFiles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Get path and ID of the selected file.
            string selectedFilePath = dgvUploadedFiles.Rows[e.RowIndex].Cells[5].Value.ToString();
            int fileID = (int)dgvUploadedFiles.Rows[e.RowIndex].Cells[2].Value;
            // check if it is Update or Delete event.
            // If it is Update event.
            if (e.ColumnIndex == 0)
            {
                // If the selected file exists.
                if (File.Exists(selectedFilePath))
                {
                    using(SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                        // Get file's name only.
                        saveFileDialog.FileName = selectedFilePath.Substring(selectedFilePath.LastIndexOf('\\') + 1);
                        // Get file's extension
                        saveFileDialog.DefaultExt = selectedFilePath.Substring(selectedFilePath.LastIndexOf('.'));
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            try 
                            {
                                File.Move(selectedFilePath, saveFileDialog.FileName);
                                DatabaseService.UpdateFile(new Uploaded_Files
                                {
                                    File_ID = fileID,
                                    File_Path = saveFileDialog.FileName
                                });
                                dgvUploadedFiles.Refresh();
                            }
                            catch (Exception exception){
                                CatchError(exception, "The selected file has too long path.\n Update file's location to short path to avoid this error.");
                            }
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show("The selected file doesn't exists !\nClick YES to delete it and get new data from database.", "File not found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DatabaseService.DeleteFile((int)dgvUploadedFiles.Rows[e.RowIndex].Cells[2].Value);
                        dgvUploadedFiles.DataSource = DatabaseService.GetUploadedFiles();
                    }
                }
            }
            // If it is Delete event
            if(e.ColumnIndex == 1)
            {
                if (MessageBox.Show("Are you sure you want to delete ?", "Deleting file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try {
                        File.Delete(selectedFilePath);
                        DatabaseService.DeleteFile(fileID);
                        dgvUploadedFiles.DataSource = DatabaseService.GetUploadedFiles();
                    }
                    catch (Exception exception)
                    {
                        CatchError(exception, "The selected file has too long path.\n Update file's location to short path to avoid this error.");
                    }
                }
            }
        }

        private void CatchError(Exception exception, string message) {
            if (exception.GetType().ToString().Equals("System.IO.PathTooLongException"))
                MessageBox.Show(message, "Too Long Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Something went wrong with the imported file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
