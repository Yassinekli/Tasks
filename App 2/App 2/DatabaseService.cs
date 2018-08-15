using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_2
{
    class DatabaseService
    {
        static FilesUploadedDBEntities database = new FilesUploadedDBEntities();

        static public List<Uploaded_Files> GetUploadedFiles()
        {
            return database.Uploaded_Files.ToList();
        }

        static public void AddNewFile(Uploaded_Files newUploadedFile)
        {
            database.Uploaded_Files.Add(newUploadedFile);
            database.SaveChanges();
        }

        static public void UpdateFile(Uploaded_Files updatedFile)
        {
            database.Uploaded_Files.Find(updatedFile.File_ID).File_Path = updatedFile.File_Path;
            database.SaveChanges();
        }

        static public void DeleteFile(int FileID)
        {
            database.Uploaded_Files.Remove(database.Uploaded_Files.Find(FileID));
            database.SaveChanges();
        }

        static public bool FileNotExists(string filePath) 
        {
            return database.Uploaded_Files.Where(file => file.File_Path.Contains(filePath)).Count() == 0;
        }
    }
}
