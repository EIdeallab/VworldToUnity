using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Terrain Render class. need idx, idy, resolution input.
/// </summary>
public class TerrainRenderer
{
    private int minIdx;
    private int minIdy;
    private int maxIdx;
    private int maxIdy;

    private int resolution;
    private int terrainHeight;
    
    private Terrain[,] terrrains;

    /// <summary>
    /// Initialize render class
    /// </summary>
    public void Init (int minX, int minY, int maxX, int maxY, int res, int height)
    {
        minIdx = minX;
        minIdy = minY;
        maxIdx = maxX;
        maxIdy = maxY;
        resolution = res;
        terrainHeight = height;
    }

    /// <summary>
    /// Main render function
    /// </summary>
    public void Run ()
    {
        // File check process
        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "\\DEM raw\\");
        if (info == null)
            return;

        for (int y = minIdy; y < maxIdy; y++)
        {
            for (int x = minIdx; x < maxIdx; x++)
            {
                String assetName = Application.dataPath + "\\DEM raw\\" + "terrain file_" + x + "_" + y + ".raw";
                if (!File.Exists(assetName))
                {
                    Debug.Log("terrain file_" + x + "_" + y + ".raw" + " not exist. Build failed.");
                    return;
                }
            }
        }
        
        // If all file exist, start render process.
        int mx = maxIdx - minIdx;
        int my = maxIdy - minIdy;
        for (int y = 0; y <= my; y++)
        {
            for (int x = 0; x <= mx; x++)
            {
                // Load dds Textures
                byte[] bytes = File.ReadAllBytes(@"Assets\DEM dds\tile_" + (x + minIdx) + "_" + (y + minIdy) + ".dds");
                Texture2D ddsTexture = LoadTextureDXT(bytes, TextureFormat.DXT1);
                ddsTexture.filterMode = FilterMode.Bilinear;
                SplatPrototype[] tex = new SplatPrototype[1];
                tex[0] = new SplatPrototype();
                tex[0].texture = ddsTexture;
                tex[0].tileSize = new Vector2(resolution, resolution);

                // Create Terrain Data
                TerrainData terrainData = new TerrainData();
                terrainData.splatPrototypes = tex;
                terrainData.heightmapResolution = resolution;
                terrainData.size = new Vector3(resolution, resolution, resolution);
                float[,] heightMap = LoadHeigtmap(x + minIdx, y + minIdy);
                terrainData.SetHeights(0, 0, heightMap);

                // Create Game Object
                GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                terrainObject.name = "terrain_" + x + "_" + y;
                terrainObject.transform.position = new Vector3(x * resolution, 0, -y * resolution);
            }
        }
    }

    /// <summary>
    /// Read DDS and return unity Texture2D
    /// </summary>
    public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
    {
        if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
            throw new System.Exception("invaild Texture Format");

        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new System.Exception("Invaild DDS DXT Texture");

        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        int DDS_HEADER_SIZE = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = new Texture2D(width, height, textureFormat, false);
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Read RAW file to Rendering
    /// </summary>
    public float[,] LoadHeigtmap(int idx, int idy)
    {
        float[,] data = new float[resolution, resolution];
        using (var file = File.OpenRead("Assets/DEM raw/terrain file_" + idx + "_" + idy + ".raw"))
        using (var reader = new BinaryReader(file))
        {
            // Vworld texture is always 65 x 65
            for (int y = 0; y < 65; y++)
            {
                for (int x = 0; x < 65; x++)
                {
                    float v = (float)reader.ReadUInt16() / 0xFFFF;
                    data[y, x] = v * terrainHeight;
                }
            }
        }
        return data;
    }
}

