using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace FtpManager
{
    public class FtpManager : IDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FtpManager"/> class.
        /// </summary>
        /// <param name="ftpServer">The adress.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        public FtpManager(string ftpServer, string user, string password)
        {
            this.Server = ftpServer;
            this.User = user;
            this.Password = password;
            this.CheckConnection();
        }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the server/host.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="FtpManager"/> has a valid connection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the connection is valid, otherwise <c>false</c>.
        /// </value>
        public bool ValidConnection { get; private set; }

        /// <summary>
        ///     Creates the specified directory.
        /// </summary>
        /// <param name="newDirectory">The new directory. Write @"test\test2\test3" for example.</param>
        public void CreateDirectory(string newDirectory)
        {
            string rootDirectory = string.Empty;

            foreach (var direcotry in newDirectory.Split('\\'))
            {
                try
                {
                    if (this.DirectoryExists(rootDirectory + '\\' + direcotry + '\\'))
                    {
                        rootDirectory = string.Concat(rootDirectory, '\\', direcotry + '\\');
                        continue;
                    }

                    var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/" + rootDirectory + "/" + direcotry));
                    rootDirectory = string.Concat(rootDirectory, "/", direcotry);
                    ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                    ftpWebRequest.UseBinary = true;
                    ftpWebRequest.Proxy = null;
                    ftpWebRequest.KeepAlive = false;
                    ftpWebRequest.UsePassive = true;
                    ftpWebRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                    ftpWebRequest.GetResponse();
                }
                catch (WebException ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        ///     Deletes the whole directory with all files and subfolders.
        /// </summary>
        /// <param name="directory">The directory to delete. Write @"test\test2\test3" for example.</param>
        public void DeleteDirectory(string directory)
        {
            foreach (var file in this.GetFileList(directory))
                this.DeleteFile(directory + '\\' + file.Split('/')[file.Split('/').Length - 1]);

            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/" + directory));
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                var response = ftpWebRequest.GetResponse();
                response.Close();
                ftpWebRequest = null;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Deletes the file.
        /// </summary>
        /// <param name="ftpFilePath">The file path. Write @"test\NinjaReport.txt" for example.</param>
        public void DeleteFile(string ftpFilePath)
        {
            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/" + ftpFilePath));
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                var response = ftpWebRequest.GetResponse();
                response.Close();
                ftpWebRequest = null;
            }
            catch
            {
                DeleteDirectory(ftpFilePath);
            }
        }

        /// <summary>
        ///     Indicates if the specified directory exists. This function returns true
        ///     if a directory existing with the given name.
        /// </summary>
        /// <param name="directory">Directory to test. Write @"test\test2\" for example. The "\" at the end is important.</param>
        /// <returns></returns>
        public bool DirectoryExists(string directory)
        {
            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + this.Server + "/" + directory);
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                using (var response = ftpWebRequest.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            this.Server = null;
            this.User = null;
            this.Password = null;
        }

        /// <summary>
        ///     Downloads the specified file.
        /// </summary>
        /// <param name="ftpFilePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        /// <param name="destinationFilePath">The destination file path. Write @"C:\Users\User\Desktop\NinjaReport.txt" for example.</param>
        public void Download(string ftpFilePath, string destinationFilePath)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(this.User, this.Password);

                var data = webClient.DownloadData(new Uri("ftp://" + this.Server + "/" + ftpFilePath));
                var fileStream = File.Create(destinationFilePath);

                fileStream.Write(data, 0, data.Length);
                fileStream.Close();
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Gets the file list from specific directory.
        /// </summary>
        /// <param name="directory">The directory. Write @"test\" for example. The "\" at the end can be important.</param>
        /// <returns></returns>
        public List<string> GetFileList(string directory)
        {
            var ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + this.Server + "/" + directory);
            ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
            ftpWebRequest.UseBinary = true;
            ftpWebRequest.Proxy = null;
            ftpWebRequest.KeepAlive = false;
            ftpWebRequest.UsePassive = true;
            ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            var files = new List<string>();
            using (var response = ftpWebRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF7))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        files.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            return files;
        }

        /// <summary>
        ///     Gets the file list from root directory.
        /// </summary>
        /// <returns></returns>
        public List<string> GetFileList()
        {
            return this.GetFileList(string.Empty);
        }

        /// <summary>
        ///     Renames the specified file.
        /// </summary>
        /// <param name="ftpFilePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        /// <param name="newFileName">The new filename. Write "SamuraiReport.txt" for example.</param>
        public void Rename(string ftpFilePath, string newFileName)
        {
            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/" + ftpFilePath));
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.Rename;
                ftpWebRequest.RenameTo = newFileName;
                var response = ftpWebRequest.GetResponse();
                response.Close();
                ftpWebRequest = null;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Uploads the specified file.
        /// </summary>
        /// <param name="filPath">The client file path. Write @"C:\Users\User\Desktop\SamuraiReport.txt" for example.</param>
        /// <param name="direcotry">The ftp direcotry. Write @"test\test2" for example.</param>
        public void Upload(string filePath, string directory)
        {
            if (directory != string.Empty)
                this.CreateDirectory(directory);

            var fileName = filePath.Split('\\')[filePath.Split('\\').Length - 1];

            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/" + directory + fileName));
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;

                var ftpStream = ftpWebRequest.GetRequestStream();
                var fileStream = File.OpenRead(filePath);
                var length = 1024;
                var buffer = new byte[length];
                var bytesRead = 0;

                do
                {
                    bytesRead = fileStream.Read(buffer, 0, length);
                    ftpStream.Write(buffer, 0, bytesRead);
                }
                while (bytesRead != 0);

                fileStream.Close();
                ftpStream.Close();
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Uploads the specified file.
        /// </summary>
        /// <param name="filePath">The file path. Write @"C:\Users\User\Desktop\SamuraiReport.txt" for example.</param>
        public void Upload(string filePath)
        {
            this.Upload(filePath, string.Empty);
        }

        /// <summary>
        ///     Checks the connection.
        /// </summary>
        private void CheckConnection()
        {
            try
            {
                WebRequest.DefaultWebProxy = null;
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.Server + "/"));
                ftpWebRequest.Credentials = new NetworkCredential(this.User, this.Password);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Proxy = null;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                var webResponse = ftpWebRequest.GetResponse();
                webResponse.Dispose();
            }
            catch (Exception)
            {
                this.ValidConnection = false;
                return;
            }

            this.ValidConnection = true;
        }
    }
}