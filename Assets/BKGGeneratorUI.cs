using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(BKGGeneratorUI))]
//[CanEditMultipleObjects]
public class BKGGeneratorUI : EditorWindow
{
    private static float lat;
    private static float lon;
    private static int rad;
    private static int level;

    private static Vector2Int minIndex;
    private static Vector2Int maxIndex;
    private static int resolution;

    private static float progress;

    private Rect LoadDataArea;
    private Rect RenderDataArea;

    private static BKGGeneratorUI window;
    private static BKGGenerator generator;
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
        set { resolution = Mathf.NextPowerOfTwo(value); }
    }

    [MenuItem("Background/Background Creator")]
    private static void Init()
    {
        lat = 0;
        lon = 0;
        rad = 30000;
        level = 8;
        
        window = GetWindow<BKGGeneratorUI>();
        window.maxSize = window.minSize = new Vector2(300, 400);
        window.Show();
    }

    private void OnGUI()
    {
        int offset = 0;
        LoadDataArea = new Rect(3, offset += 0, position.width-6, 100);
        RenderDataArea = new Rect(3, offset += 100, position.width-6, 120);

        GUILayout.ExpandHeight(false);
        GUILayout.ExpandWidth(false);

        #region Load Data
        GUILayout.BeginArea(LoadDataArea);
        lat = EditorGUILayout.FloatField("Latitude", lat);
        lon = EditorGUILayout.FloatField("Longitude", lon);
        rad = EditorGUILayout.IntSlider("Radius", rad, 1000, 60000);
        level = EditorGUILayout.IntSlider("Level", level, 7, 15);
        if (GUILayout.Button("Load Data"))
        {
            generator = new BKGGenerator();
            generator.Init(lat, lon, rad, level);
            generator.Generate();
        }
        if(generator != null)
            Progress = generator.GetProgressStatus();
        else
            Progress = 0;
        GUILayout.EndArea();
        #endregion

        #region Render Data
        GUILayout.BeginArea(RenderDataArea);
        minIndex = EditorGUILayout.Vector2IntField("MinIndex", minIndex);
        maxIndex = EditorGUILayout.Vector2IntField("MaxIndex", maxIndex);
        resolution = EditorGUILayout.IntSlider(resolution, 32, 1024);
        if (GUILayout.Button("Render Data"))
        {
            renderer = new TerrainRenderer();
            renderer.Init(minIndex.x, minIndex.y, maxIndex.x, maxIndex.y, resolution);
            renderer.Run();
        }
        GUILayout.EndArea();
        #endregion

        EditorGUI.ProgressBar(new Rect(3, position.height - 24, position.width - 6, 20), Progress, (Progress * 100).ToString());
    }
}
