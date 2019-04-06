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

    [MenuItem("Background/Background Creator")]
    private static void Init()
    {
        lat = 0;
        lon = 0;
        rad = 30000;
        level = 8;
        
        var window = (BKGGeneratorUI)GetWindow(typeof(BKGGeneratorUI));
        generator = new BKGGenerator(lat, lon, rad, level);
        window.Show();
    }

    private void OnGUI()
    {
        lat = EditorGUILayout.FloatField("Latitude", lat);
        lon = EditorGUILayout.FloatField("Longitude", lon);
        rad = EditorGUILayout.IntSlider("Radius", rad, 1000, 60000);
        level = EditorGUILayout.IntSlider("Level", level, 7, 15);

        if (GUILayout.Button("Load"))
        {
            
        }
    }

    // Use this for initialization
    void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
