using System;
using System.Configuration;
using System.IO;
using Renci.SshNet;




class Program
{
    static void Main()
    {
        string sftpPath = ConfigurationManager.AppSettings["UploadFile"];
        string localfilePath = ConfigurationManager.AppSettings["DownloadFile"];



        using (var client = GetSftpClient())
        {
            client.Connect();



            UploadFiles(client, sftpPath, localfilePath);
            DownloadFiles(client, sftpPath, localfilePath);



            client.Disconnect();
        }



        Console.WriteLine("Done.");
    }



    static void UploadFiles(SftpClient client, string sftpPath, string localfilePath)
    {
        var directory = new DirectoryInfo(localfilePath);
        foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
        {
            var relativePath = file.FullName.Substring(localfilePath.Length + 1); // remove the local file path prefix
            var remotePath = sftpPath + "/" + relativePath.Replace("\\", "/"); // convert path separators and add the SFTP directory path prefix

            using (var fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                client.UploadFile(fileStream, remotePath);
            }
        }
    }





    static void DownloadFiles(SftpClient client, string sftpPath, string localfilePath)
    {
        var directory = new DirectoryInfo(localfilePath);
        foreach (var file in client.ListDirectory(sftpPath))
        {
            if (!file.IsDirectory)
            {
                using (var fileStream = File.Create(Path.Combine(localfilePath, file.Name)))
                {
                    client.DownloadFile(sftpPath + "/" + file.Name, fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }
    }




    static SftpClient GetSftpClient()
    {
        var host = ConfigurationManager.AppSettings["SftpHost"];
        var port = int.Parse(ConfigurationManager.AppSettings["SftpPort"]);
        var username = ConfigurationManager.AppSettings["SftpUsername"];
        var password = ConfigurationManager.AppSettings["SftpPassword"];



        return new SftpClient(host, port, username, password);
    }
}