using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

/// <summary>
/// This Class Provide DEM and DDS datas from Vworld. It Need API key.
/// </summary>
public class TerrainLoader : Loader
{
    // Load exist file names to avoid unnecessary downloading.
    private List<string> fileExistBil;
    private List<string> fileExistRaw;
    private List<string> fileNamesDds;

    // Vworld api url, Vworld api key.
    public TerrainLoader(string apikey) {
        storageDirectory = Application.dataPath;
        apiKey = apikey;
    }

    public TerrainLoader(float latitude, float longitude, int radius, int level)
    {
        storageDirectory = Application.dataPath;
        Init(latitude, longitude, radius, level);
    }

    /// <summary>
    /// Initialize data api
    /// </summary>
    public override void Init(float latitude, float longitude, int radius, int level)
    {
        lat = latitude;
        lon = longitude;
        rad = radius;
        lv = level;
        unit = 360 / (Mathf.Pow(2, lv) * 10);
    }

    /// <summary>
    /// Core load funtion. Always call this after the init()
    /// </summary>
    public override void Generate()
    {
        // Create Sub folders. bil(vworld dem data), raw(dem image), dds(dem texture image)
        string[] folders1 = { "\\DEM bil", "\\DEM raw", "\\DEM dds" };
        MakeSubFolders(storageDirectory, folders1);

        fileExistBil = GetFileNames(storageDirectory + "\\DEM bil\\", ".bil");
        fileExistRaw = GetFileNames(storageDirectory + "\\DEM raw\\", ".raw");
        fileNamesDds = GetFileNames(storageDirectory + "\\DEM dds\\", ".dds");

        // Calculate the minimum maximum range by converting the metric units into longitude units.
        float minLon = lon - (float)rad / 111000; 
        float minLat = lat - (float)rad / 88000; 
        float maxLon = lon + (float)rad / 111000;
        float maxLat = lat + (float)rad / 88000;

        // Caluculate idx, idy(vworld api unit)
        minIdx = (int)Mathf.Floor((minLon + 180) / unit);
        minIdy = (int)Mathf.Floor((minLat + 90)  / unit);
        maxIdx = (int)Mathf.Floor((maxLon + 180) / unit);
        maxIdy = (int)Mathf.Floor((maxLat + 90)  / unit);

        totalTask = (maxIdx - minIdx) * (maxIdy - minIdy);

        Thread t1 = new Thread(() =>
        {
            // Load datas from Vworld
            Run();
        });
        t1.Start();
    }

    /// <summary>
    /// Thread run funtion to load data.
    /// </summary>
    public override void Run()
    {
        string layerName = "dem";
        string layerName2 = "tile";

        for (int i = minIdx; i <= maxIdx; i++)
        {
            for (int j = minIdy; j <= maxIdy; j++)
            {

                // Get texture image
                string fileNameDds = "tile_" + i + "_" + j + ".dds";

                long size = 0;
                if (!fileNamesDds.Contains(fileNameDds))
                {
                    string address3_1 = url3 + apiKey + "&Layer=" + layerName2 + "&Level=" + lv + "&IDX=" + i + "&IDY=" + j;
                    size = RequestFile(address3_1, "\\DEM dds\\" + fileNameDds);
                }

                // if exists then skip
                string fileNameBil = "terrain file_" + i + "_" + j + ".bil";
                if (!fileExistBil.Contains(fileNameBil))
                {
                    // if not exists then download
                    string address3 = url3 + apiKey + "&Layer=" + layerName + "&Level=" + lv + "&IDX=" + i + "&IDY=" + j;
                    size = RequestFile(address3, "\\DEM bil\\" + fileNameBil);   // Send IDX IDY and nodeLevel for download bil files.
                    if (size < 16900)
                    {
                        // Download fail
                        continue;
                    }
                }

                // Convert bil to raw.
                string fileNameParsedRaw = "terrain file_" + i + "_" + j + ".raw";
                if (!fileExistRaw.Contains(fileNameParsedRaw))
                {
                    //Read bil file and convert 16bit raw file(unity style)
                    BilParser(fileNameBil, fileNameParsedRaw); 
                }
                
                progress++;
            }
        }
    }

    /// <summary>
    /// Thread run funtion to load data.
    /// </summary>
    private void BilParser(string bilFileName, string rawFileName)
    {
        using (FileStream inFileStream = File.OpenRead(storageDirectory + "\\DEM bil\\" + bilFileName))
        using (BinaryReader inputStream = new BinaryReader(inFileStream))
        using (FileStream outFileStream = File.OpenWrite(storageDirectory + "\\DEM raw\\" + rawFileName))
        using (BinaryWriter outputStream = new BinaryWriter(outFileStream))
        {
            // Terrain height
            // The data of VWorld is 65x65 size.
            for (int ny = 0; ny < 65; ny++)
            {
                for (int nx = 0; nx < 65; nx++)
                {
                    // Convert float data to int16
                    float height = inputStream.ReadSingle();
                    short nHeight = Convert.ToInt16(height);
                    outputStream.Write(nHeight);
                }
            }
        }
    }
}
