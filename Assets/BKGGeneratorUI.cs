using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BKGGeneratorUI))]
[CanEditMultipleObjects]
public class BKGGeneratorUI : EditorWindow
{
    private static float lat;
    private static float lon;
    private static int rad;
    private static int level;
    private static float progress;

    private static BKGGenerator generator;

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

    [MenuItem("Background/Background Creator")]
    private static void Init()
    {
        lat = 0;
        lon = 0;
        rad = 30000;
        level = 8;
        
        var window = (BKGGeneratorUI)GetWindow(typeof(BKGGeneratorUI));
        

        window.Show();
    }

    private void OnGUI()
    {
        lat = EditorGUILayout.FloatField("Latitude", lat);
        lon = EditorGUILayout.FloatField("Longitude", lon);
        rad = EditorGUILayout.IntSlider("Radius", rad, 1000, 60000);
        level = EditorGUILayout.IntSlider("Level", level, 7, 15);
        
        generator = new BKGGenerator();

        if (GUILayout.Button("Load"))
        {
            generator.Init(lat, lon, rad, level);
            generator.Generate();
        }
        
        EditorGUI.ProgressBar(new Rect(3, 100, position.width - 6, 20), Progress / 100, (Progress*100).ToString());
    }
    
	// Update is called once per frame
	void Update ()
    {
        if(generator != null)
            Progress = generator.GetProgressStatus();
    }
}
