# FTP Manager
Simple FTP Framework for upload, download, rename, test and create. Very easy to use.

UPLOAD
<code>
using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
{
  	if (ftp.ValidConnection)
  	{
     		// Uploads the file to the root directory
     		ftp.UploadFile(@"C:\Users\User\Desktop\NinjaReport.txt");
     		// Uploads the file to the specified directory
     		ftp.UploadFile(@"C:\Users\User\Desktop\NinjaReport.txt", "test\test1");
  	}
}
  </code>

DOWNLOAD
<code>
using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
{
  	if (ftp.ValidConnection)
  	{
      		// Downloads the file to the specified client directory
      		ftp.DownloadFile(@"test\NinjaReport.txt", @"C:\Users\User\Desktop\NinjaReport.txt");
  	}
}
  </code>

<code>
RENAME
using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
{
   	if (ftp.ValidConnection)
   	{
       		// Renames the specified file
       		ftp.RenameFile(@"test\NinjaReport.txt", "SamuraiReport.txt");
   	}
}
  </code>

CREATE
<code>
using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
{
	if (ftp.ValidConnection)
	{
		// Creates the specified directory
		ftp.CreateDirectory(@"test\test2\test3");
   	}
}
  </code>

DELETE
<code>
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
  </code>

INFORMATIONS
<code>
using (var ftp = new FtpManager("ftp.yourcompany.com", "ftpUser", "ftpPassword"))
{
    	if (ftp.ValidConnection)
    	{
        	// Checks if the specified directory exists
        	var exist = ftp.DirectoryExists(@"test\test2");
        	// Gets the files and subfolders of the root directory
        	var fileList = ftp.GetDirectoryContent();
        	// Gets the files and subfolders of the specified directory
        	fileList = ftp.GetDirectoryContent(@"test\test2\");
    	}
}
  </code>
