using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

/// <summary>
/// This Class Provide DEM and DDS datas from Vworld. It Need API key.
/// </summary>
public class BKGGenerator
{
    private static float lat = 0;
    private static float lon = 0;
    private static int rad = 0;
    private static int lv = 0;
    private static float unit = 0;

    // 데이터를 가져올 인덱스
    private static int minIdx;
    private static int minIdy;
    private static int maxIdx;
    private static int maxIdy;

    // 중복 다운로드를 피하기위한 파일 목록
    private List<string> fileExistBil;
    private List<string> fileExistRaw;
    private List<string> fileNamesDds;

    // 요청 url과 api키
    private string url3 = @"http://xdworld.vworld.kr:8080/XDServer/requestLayerNode?APIKey=";
    private string apiKey = "";

    // 요청 파일의 갯수와 확인용 변수
    static int totalTask = 0;
    static int progress = 0;

    //중복 다운이나 변환하지 않도록 저장할 폴더
    private static string storageDirectory;
    private static int fileSize;

    public BKGGenerator() {
        storageDirectory = Application.dataPath;
    }

    public BKGGenerator(float latitude, float longitude, int radius, int level)
    {
        storageDirectory = Application.dataPath;
        lat = latitude;
        lon = longitude;
        rad = radius;
        lv = level;
        unit = 360 / (Mathf.Pow(2, lv) * 10);
    }

    public void Init(float latitude, float longitude, int radius, int level)
    {
        lat = latitude;
        lon = longitude;
        rad = radius;
        lv = level;
        unit = 360 / (Mathf.Pow(2, lv) * 10);
    }

    public float GetProgressStatus()
    {
        return (totalTask != 0) ? (float)progress/ totalTask : 0;
    }

    // Use this for initialization
    public void Generate()
    {

        // 필요한 subfolder를 만든다. 이미 있으면 건너뛴다.
        string[] folders1 = { "\\DEM bil", "\\DEM raw", "\\DEM dds" };
        MakeSubFolders(storageDirectory, folders1);

        // 중복 다운로드를 피하기 위해 현재 있는 파일들 목록을 구한다.
        fileExistBil = GetFileNames(storageDirectory + "\\DEM bil\\", ".bil");
        fileExistRaw = GetFileNames(storageDirectory + "\\DEM raw\\", ".raw");
        fileNamesDds = GetFileNames(storageDirectory + "\\DEM dds\\", ".dds");

        float minLon = lon - (float)rad / 111000; //경도
        float minLat = lat - (float)rad / 88000; //위도	 
        float maxLon = lon + (float)rad / 111000;
        float maxLat = lat + (float)rad / 88000;

        // idx와 idy를 받는 1단계 단계를 생략하고 여기서 직접 계산한다.
        minIdx = (int)Mathf.Floor((minLon + 180) / unit);
        minIdy = (int)Mathf.Floor((minLat + 90)  / unit);
        maxIdx = (int)Mathf.Floor((maxLon + 180) / unit);
        maxIdy = (int)Mathf.Floor((maxLat + 90)  / unit);

        totalTask = (maxIdx - minIdx) * (maxIdy - minIdy);

        Thread t1 = new Thread(() =>
        {
            Run();
        });
        t1.Start();
    }

    private void MakeSubFolders(string fileLocation, string[] subfolders)
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
                        isExist = true; //같은 이름이 있으면 스킵
                        break;
                    }
                } //for
            }
            if (!isExist)
            {
                DirectoryInfo newDir = Directory.CreateDirectory(fileLocation + subfolder);
                newDir.Create();
            }
        }
    }

    public void Run()
    {
        string layerName = "dem";
        string layerName2 = "tile";

        // 단위 구역들을 차례차례 처리한다.
        for (int i = minIdx; i <= maxIdx; i++)
        {
            for (int j = minIdy; j <= maxIdy; j++)
            {

                //tile 이미지를 받아온다.
                string fileNameDds = "tile_" + i + "_" + j + ".dds";

                long size = 0;
                if (!fileNamesDds.Contains(fileNameDds))
                {
                    string address3_1 = url3 + apiKey + "&Layer=" + layerName2 + "&Level=" + lv + "&IDX=" + i + "&IDY=" + j;
                    size = RequestFile(address3_1, "\\DEM dds\\" + fileNameDds);
                }

                //만약 이미 bil 파일이 존재하면 건너뛴다.
                string fileNameBil = "terrain file_" + i + "_" + j + ".bil";
                if (!fileExistBil.Contains(fileNameBil))
                {
                    //존재하지 않으면 다운받는다.
                    string address3 = url3 + apiKey + "&Layer=" + layerName + "&Level=" + lv + "&IDX=" + i + "&IDY=" + j;
                    size = RequestFile(address3, "\\DEM bil\\" + fileNameBil);   //IDX와 IDY 및 nodeLevel을 보내서 bil목록들을 받아 bil에 저장한다.
                    if (size < 16900)
                    {
                        //제대로 된 파일이 아니면
                        continue;
                    }
                }

                string fileNameParsedRaw = "terrain file_" + i + "_" + j + ".raw";
                if (!fileExistRaw.Contains(fileNameParsedRaw))
                {
                    BilParser(fileNameBil, fileNameParsedRaw); //bil파일을 다시 읽고 16비트 raw파일로 저장한다.
                }
                
                progress++;
            }
        }
    }

    private void BilParser(string bilFileName, string rawFileName)
    {
        using (FileStream inFileStream = File.OpenRead(storageDirectory + "\\DEM bil\\" + bilFileName))
        using (BinaryReader inputStream = new BinaryReader(inFileStream))
        using (FileStream outFileStream = File.OpenWrite(storageDirectory + "\\DEM raw\\" + rawFileName))
        using (BinaryWriter outputStream = new BinaryWriter(outFileStream))
        {
            //terrain height
            //vworld에서 제공하는 DEM이 65x65개의 점으로 되어 있다.
            for (int ny = 64; ny >= 0; ny--)
            {
                for (int nx = 0; nx < 65; nx++)
                {
                    //65x65의 격자를 short 형태로 저장한다.
                    float height = inputStream.ReadSingle();
                    short nHeight = Convert.ToInt16(height);
                    outputStream.Write(nHeight);
                }
            }
        }
    }

    /*
	 * httpRequest를 보내고 바이너리 파일을 받아 저장한다.
	 * @param address
	 * @param fileName
	 */
    private long RequestFile(string address, string fileName)
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

    private List<string> GetFileNames(string fileLocation, string extension)
    {
        List<string> fileNames = new List<string>();
        string[] files = Directory.GetFiles(fileLocation);

        // 디렉토리가 비어 있지 않다면 
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
