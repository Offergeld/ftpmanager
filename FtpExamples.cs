namespace FtpManager
{
    public static class FtpExamples
    {
        // UPLOAD
        public static void Upload()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Uploads the file to the root directory
                    ftp.Upload(@"C:\Users\User\Desktop\NinjaReport.txt");

                    // Uploads the file to the specified directory
                    ftp.Upload(@"C:\Users\User\Desktop\NinjaReport.txt", "test\test1");
                }
            }
        }

        // DOWNLOAD
        public static void Download()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Downloads the file to the specified client directory
                    ftp.Download(@"test\NinjaReport.txt", @"C:\Users\User\Desktop\NinjaReport.txt");
                }
            }
        }

        // RENAME
        public static void Rename()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Renames the specified file
                    ftp.Rename(@"test\NinjaReport.txt", "SamuraiReport.txt");
                }
            }
        }

        // CREATE
        public static void CreateDirectory()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Creates the specified directory
                    ftp.CreateDirectory(@"test\test2\test3");
                }
            }
        }

        // DELETE
        public static void Delete()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Deletes the specified file
                    ftp.DeleteFile(@"test\NinjaReport.txt");

                    // Deletes the whole directory with all files and subfolders
                    ftp.DeleteDirectory(@"test\test2");
                }
            }
        }

        // INFORMATIONS
        public static void Informations()
        {
            using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
            {
                if (ftp.ValidConnection)
                {
                    // Checks if the specified directory exists
                    var exist = ftp.DirectoryExists(@"test\test2");

                    // Gets the files and subfolders of the root directory
                    var fileList = ftp.GetFileList();

                    // Gets the files and subfolders of the specified directory
                    fileList = ftp.GetFileList(@"test\test2\");
                }
            }
        }
    }
}