using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Vworld 3D object loader
/// </summary>
class ObjectLoader : Loader
{
    // Load exist file names to avoid unnecessary downloading.
    private List<string> fileExistBil;
    private List<string> fileExistRaw;
    private List<string> fileNamesDds;

    // Vworld api url, Vworld api key.
    public ObjectLoader(string apikey)
    {
        storageDirectory = Application.dataPath;
        apiKey = apikey;
    }

    public ObjectLoader(float latitude, float longitude, int radius, int level)
    {
        storageDirectory = Application.dataPath;
        Init(latitude, longitude, radius, level);
    }
    
	
}
