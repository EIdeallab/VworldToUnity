using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Loader interface
/// </summary>
public class Loader
{
    protected static float lat = 0;
    protected static float lon = 0;
    protected static int rad = 0;
    protected static int lv = 0;
    protected static float unit = 0;

    protected static int minIdx;
    protected static int minIdy;
    protected static int maxIdx;
    protected static int maxIdy;

    // Vworld api url, Vworld api key.
    protected static string url3 = @"http://xdworld.vworld.kr:8080/XDServer/requestLayerNode?APIKey=";
    protected static string url4 = @"http://xdworld.vworld.kr:8080/XDServer/requestLayerObject?APIKey=";

    protected string apiKey;

    // File count. need to show downloading progress.
    protected static int totalTask = 0;
    protected static int progress = 0;

    // File Storage folder.
    protected static string storageDirectory;
    protected static int fileSize;

    /// <summary>
    /// Each functions need to be redefined.
    /// </summary>
    public virtual void Init(float latitude, float longitude, int radius, int level) { }
    public virtual void Generate() { }
    public virtual void Run() { }

    /// <summary>
    /// Get current download progress. 
    /// </summary>
    public virtual float GetProgressStatus()
    {
        return (totalTask != 0) ? (float)progress / totalTask : 0;
    }

    /// <summary>
    /// Create sub folders when there are no folders exist
    /// </summary>
    public virtual void MakeSubFolders(string fileLocation, string[] subfolders)
    {
        string[] files = Directory.GetDirectories(fileLocation);
        foreach (string subfolder in subfolders)
        {
            bool isExist = false;
            if (files != null)
            {
                for (int j = 0; j < files.Length; j++)
                {
                    if (files[j].Equals(subfolder))
                    {
                        isExist = true; // If folder exists then skip
                        break;
                    }
                }
            }
            if (!isExist)
            {
                DirectoryInfo newDir = Directory.CreateDirectory(fileLocation + subfolder);
                newDir.Create();
            }
        }
    }

    /// <summary>
    /// Send Http request and download files into storage folder
    /// </summary>
    public virtual long RequestFile(string address, string fileName)
    {
        long size = 0;
        string url = address;
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (FileStream fileStream = File.OpenWrite(storageDirectory + fileName))
            {
                stream.CopyTo(fileStream);
                size = stream.Length;
            }
        }
        catch (Exception)
        {
            return -1;
        }
        return size;
    }

    /// <summary>
    /// Returns the names of files with a particular extension.
    /// </summary>
    public virtual List<string> GetFileNames(string fileLocation, string extension)
    {
        List<string> fileNames = new List<string>();
        string[] files = Directory.GetFiles(fileLocation);

        // If directory is not empty
        if (!(files.Length <= 0))
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(extension))
                {
                    fileNames.Add(files[i]);
                }
            }
        }
        return fileNames;
    }
}
