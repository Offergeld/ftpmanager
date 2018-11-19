using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Offergeld.FtpManager
{
    public class FtpManager : IDisposable
    {
        public readonly FtpRegistration Registration = null;

        /// <summary>
        ///     Gets a value indicating whether this <see cref="FtpManager"/> has a valid connection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the connection is valid, otherwise <c>false</c>.
        /// </value>
        public bool ValidConnection { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FtpManager"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        public FtpManager(FtpRegistration registration)
        {
            Registration = registration;
            ValidConnection = ConnectionIsValid();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FtpManager"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        public FtpManager(string server, string user, string password)
        {
            Registration = new FtpRegistration
            {
                Server = server,
                User = user,
                Password = password
            };
            ValidConnection = ConnectionIsValid();
        }

        /// <summary>
        ///     Changes the server.
        /// </summary>
        /// <param name="server">The server.</param>
        public void ChangeServer(string server)
        {
            Registration.Server = server;
            ValidConnection = ConnectionIsValid();
        }

        /// <summary>
        ///     Changes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        public void ChangeUser(string user)
        {
            Registration.User = user;
            ValidConnection = ConnectionIsValid();
        }

        /// <summary>
        ///     Changes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        public void ChangePassword(string password)
        {
            Registration.Password = password;
            ValidConnection = ConnectionIsValid();
        }

        /// <summary>
        ///     Creates the specified directory.
        /// </summary>
        /// <param name="directory">The new directory. Write @"test\test2\test3" for example.</param>
        public void CreateDirectory(string directory)
        {
            string rootDirectory = string.Empty;

            foreach (var direcotry in directory.Split('\\'))
            {
                try
                {
                    if (this.DirectoryExists(rootDirectory + '\\' + direcotry + '\\'))
                    {
                        rootDirectory = string.Concat(rootDirectory, '\\', direcotry + '\\');
                        continue;
                    }

                    var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/" + rootDirectory + "/" + direcotry));
                    rootDirectory = string.Concat(rootDirectory, "/", direcotry);
                    ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
            foreach (var file in GetDirectoryContent(directory))
                DeleteFile(directory + '\\' + file.Split('/')[file.Split('/').Length - 1]);

            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/" + directory));
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
        /// <param name="filePath">The file path. Write @"test\NinjaReport.txt" for example.</param>
        public void DeleteFile(string filePath)
        {
            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/" + filePath));
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
                DeleteDirectory(filePath);
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
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Registration.Server + "/" + directory);
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
            Registration.Server = null;
            Registration.User = null;
            Registration.Password = null;
        }

        /// <summary>
        ///     Gets the specified file.
        /// </summary>
        /// <param name="filePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        public byte[] GetFile(string filePath)
        {
            try
            {
                var webClient = new WebClient
                {
                    Credentials = new NetworkCredential(Registration.User, Registration.Password)
                };

                return webClient.DownloadData(new Uri("ftp://" + Registration.Server + "/" + filePath));
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Downloads the specified file.
        /// </summary>
        /// <param name="filePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        public byte[] DownloadFile(string filePath)
        {
            try
            {
                var webClient = new WebClient
                {
                    Credentials = new NetworkCredential(Registration.User, Registration.Password)
                };

                return webClient.DownloadData(new Uri("ftp://" + Registration.Server + "/" + filePath));
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Downloads the specified file.
        /// </summary>
        /// <param name="filePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        /// <param name="destinationFilePath">The destination file path. Write @"C:\Users\User\Desktop\NinjaReport.txt" for example.</param>
        public void DownloadFile(string filePath, string destinationFilePath)
        {
            try
            {
                var webClient = new WebClient
                {
                    Credentials = new NetworkCredential(Registration.User, Registration.Password)
                };

                var data = webClient.DownloadData(new Uri("ftp://" + Registration.Server + "/" + filePath));
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
        ///     Gets content list from specific directory.
        /// </summary>
        /// <param name="directory">The directory. Write @"test\" for example. The "\" at the end can be important.</param>
        /// <returns></returns>
        public List<string> GetDirectoryContent(string directory)
        {
            var ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Registration.Server + "/" + directory);
            ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
        ///     Gets content list from root directory.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectoryContent()
        {
            return GetDirectoryContent(string.Empty);
        }

        /// <summary>
        ///     Renames the specified file.
        /// </summary>
        /// <param name="filePath">The ftp file path. Write @"test\NinjaReport.txt" for example.</param>
        /// <param name="newFileName">The new filename. Write "SamuraiReport.txt" for example.</param>
        public void RenameFile(string filePath, string newFileName)
        {
            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/" + filePath));
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
        public void UploadFile(string filePath, string directory)
        {
            if (directory != string.Empty)
                CreateDirectory(directory);

            var fileName = filePath.Split('\\')[filePath.Split('\\').Length - 1];

            try
            {
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/" + directory + fileName));
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
        public void UploadFile(string filePath)
        {
            UploadFile(filePath, string.Empty);
        }

        /// <summary>
        ///     Checks the connection.
        /// </summary>
        private bool ConnectionIsValid()
        {
            try
            {
                WebRequest.DefaultWebProxy = null;
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + Registration.Server + "/"));
                ftpWebRequest.Credentials = new NetworkCredential(Registration.User, Registration.Password);
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
                return false;
            }

            return true;
        }
    }
}
