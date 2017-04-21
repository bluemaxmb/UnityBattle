using UnityEngine;
using System.Collections.Generic;

public class OnScreenLog : MonoBehaviour
{
    static int msgCount = 0;
    static List<string> log = new List<string>();
    static List<string> warnings = new List<string>();
    static public int maxLines = 16;

    public int fontSize = 24;

    // Set this to hard code the offset of your log
    // NOTE: safe zone needs to be considered here.
    public int startx = 70;
    public int starty = 70;

    // Use this to set startx and starty using a percentage of
    // the screen.
    public float safeZonePercent = 0;

    // Set this to hard code the actual width of your on screen
    // log.
    int width = 0;

    // Use this to offset your widget at a percent of the screen height.
    public int startAtPercentOfScreenHeight = 0;

    // Use this to offset your widget at a percent of the screen width.
    public int startAtPercentOfScreenWidth = 0;

    // Set this to control a percentage of the screen you wish your
    // on screen log to consume.
    public int percentOfScreenWidth = 0;

    void Start()
    {
#if !DEBUG_MODE
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
#endif

		int safex = 0;
        int safey = 0;

        if (safeZonePercent != 0)
        {
            safex = (int)(Screen.width * (safeZonePercent / 100.0f));
            safey = (int)(Screen.height * (safeZonePercent / 100.0f));
        }
        if (startAtPercentOfScreenHeight != 0)
        {
            starty = (int)(Screen.height * startAtPercentOfScreenHeight / 100.0f) + safey;
        }
        if (startAtPercentOfScreenWidth != 0)
        {
            startx = (int)(Screen.width * startAtPercentOfScreenWidth / 100.0f) + safex;
        }
        if (percentOfScreenWidth != 0)
        {
            width = (int)((Screen.width - safex * 2) * percentOfScreenWidth / 100.0f);
        }
        else
        {
            width = Screen.width - startx * 2;
        }
    }

    void Update()
    {
    }

    void OnGUI()
    {
        GUIStyle style = GUI.skin.GetStyle("Label");
        style.fontSize = fontSize;
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = false;

        float height = 0;
        string logText = "";
        for (int i = 0; i < log.Count; ++i)
        {
            logText += " " + log[i];
            logText += "\n";
            height += style.lineHeight;
        }
        height += 6;
        Rect boundingBox = new Rect(startx, starty, width, height);
        GUI.Box(boundingBox, "Log");
        GUI.Label(boundingBox, logText, style);
    }

    static public void Add(string msg)
    {
        string cleaned = msg.Replace("\r", " ");
        cleaned = cleaned.Replace("\n", " ");

#if UNITY_WSA
        System.Diagnostics.Debug.WriteLine("[APP] " + cleaned);
#else
        System.Console.WriteLine("[APP] " + cleaned);
#endif

        log.Add(cleaned);
        msgCount++;

        if (msgCount > maxLines)
        {
            log.RemoveAt(0);
        }
    }

    static public void AddWarning(string msg)
    {
        string cleaned = msg.Replace("\r", " ");
        cleaned = cleaned.Replace("\n", " ");

#if UNITY_WSA
        System.Diagnostics.Debug.WriteLine("[APP] " + cleaned);
#else
        System.Console.WriteLine("[APP] " + cleaned);
#endif

        warnings.Add(cleaned);
        msgCount++;

        if (msgCount > maxLines)
        {
            warnings.RemoveAt(0);
        }
    }

    static public void Clear()
    {
        log.Clear();
    }
}
