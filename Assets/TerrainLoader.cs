using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class TerrainLoader : MonoBehaviour
{
    private int minIdx;
    private int minIdy;
    private int maxIdx;
    private int maxIdy;

    private int resolution;
    
    private Terrain[,] terrrains;

    public void Init (int minX, int minY, int maxX, int maxY, int res)
    {
        minIdx = minX;
        minIdy = minY;
        maxIdx = maxX;
        maxIdy = maxY;
        resolution = res;
    }

    public void Run ()
    {
        terrrains = new Terrain[maxIdy - minIdy, maxIdx - minIdx];
        for (int y = minIdy; y < maxIdy; y++)
        {
            for (int x = minIdx; x < maxIdx; x++)
            {
                byte[] bytes = File.ReadAllBytes(@"Assets\DEM dds\tile_" + x + "_" + y + ".dds");
                Texture2D ddsTexture = LoadTextureDXT(bytes, TextureFormat.DXT1);
                ddsTexture.filterMode = FilterMode.Bilinear;
                SplatPrototype[] tex = new SplatPrototype[1];
                tex[0] = new SplatPrototype();
                tex[0].texture = ddsTexture;
                tex[0].tileSize = new Vector2(resolution, resolution);

                terrrains[y ,x] = GetComponent<Terrain>();
                terrrains[y, x].terrainData.splatPrototypes = tex;
            }
        }
    }

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
}

