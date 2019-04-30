using UnityEngine;
using UnityEditor;

/// <summary>
/// This Class provides Load and Render user interfaces. 
/// </summary>
public class LoaderUI : EditorWindow
{
    private static string apiKey;
    private static float lat;
    private static float lon;
    private static int rad;
    private static int level;

    private static Vector2Int minIndex;
    private static Vector2Int maxIndex;
    private static int resolution;
    private static int height;

    private static float progress;

    private Rect LoadDataArea;
    private Rect RenderDataArea;

    private static LoaderUI window;
    private static TerrainLoader generator;
    private static TerrainRenderer renderer;

    public float Lat
    {
        get { return lat; }
        set { lat = Mathf.Clamp(value, -90.0f, 90.0f); }
    }
    public float Lon
    {
        get { return lon; }
        set { lon = Mathf.Clamp(value, -180.0f, 180.0f); }
    }
    public float Rad
    {
        get { return rad; }
        set { lon = Mathf.Clamp(value, 0, 60000); }
    }
    private int Level
    {
        get { return level; }
        set { level = Mathf.Clamp(value, 7, 15); }
    }
    private float Progress
    {
        get { return progress; }
        set { progress = Mathf.Clamp01(value); }
    }
    private int Resolution
    {
        get { return resolution; }
        set { resolution = Mathf.NextPowerOfTwo(value - 1) + 1; }
    }
    private int Height
    {
        get { return height; }
        set { height = value; }
    }

    /// <summary>
    /// Main ui initialize function
    /// </summary>
    [MenuItem("V-world Api/Loader")]
    private static void Init()
    {
        apiKey = "";
        lat = 0;
        lon = 0;
        rad = 30000;
        level = 8;

        resolution = 64;
        height = 50;

        window = GetWindow<LoaderUI>();
        window.maxSize = window.minSize = new Vector2(300, 400);
        window.Show();
    }

    /// <summary>
    /// Implementation of user interface.
    /// </summary>
    private void OnGUI()
    {
        int offset = 0;
        LoadDataArea = new Rect(3, offset += 0, position.width - 6, 120);
        RenderDataArea = new Rect(3, offset += 120, position.width - 6, 130);

        // fixed window size
        GUILayout.ExpandHeight(false);
        GUILayout.ExpandWidth(false);

        #region Load Data
        GUILayout.BeginArea(LoadDataArea);
        apiKey = EditorGUILayout.TextField("Api Key", apiKey);
        lat = EditorGUILayout.FloatField("Latitude", lat);
        lon = EditorGUILayout.FloatField("Longitude", lon);
        rad = EditorGUILayout.IntSlider("Radius", rad, 1000, 60000);
        level = EditorGUILayout.IntSlider("Level", level, 7, 15);
        if (GUILayout.Button("Load Data"))
        {
            generator = new TerrainLoader(apiKey);
            generator.Init(lat, lon, rad, level);
            generator.Generate();
        }
        if (generator != null)
            Progress = generator.GetProgressStatus();
        else
            Progress = 0;
        GUILayout.EndArea();
        #endregion

        #region Render Data
        GUILayout.BeginArea(RenderDataArea);
        minIndex = EditorGUILayout.Vector2IntField("MinIndex", minIndex);
        maxIndex = EditorGUILayout.Vector2IntField("MaxIndex", maxIndex);
        Resolution = EditorGUILayout.IntSlider("Resolution", Resolution, 32, 1024);
        Height = EditorGUILayout.IntSlider("Height", Height, 1, 100);
        if (GUILayout.Button("Render Data"))
        {
            renderer = new TerrainRenderer();
            renderer.Init(minIndex.x, minIndex.y, maxIndex.x, maxIndex.y, resolution, Height);
            renderer.Run();
        }
        GUILayout.EndArea();
        #endregion

        EditorGUI.ProgressBar(new Rect(3, position.height - 24, position.width - 6, 20), Progress, (Progress * 100).ToString());
    }
}
