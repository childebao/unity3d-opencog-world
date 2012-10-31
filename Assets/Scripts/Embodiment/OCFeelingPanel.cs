using System;
using System.Collections.Generic;
using UnityEngine;
using Embodiment;

/**
 * @class
 * Show a panel of OCAvatar's feelings on the game GUI.
 */
[RequireComponent(typeof(OCConnector))]
class OCFeelingPanel : MonoBehaviour
{
    // the skin the console will use
    public GUISkin panelSkin;
    // style for label
    private GUIStyle boxStyle;
    // A map from feeling names to textures. The texture needs to be created dynamically
    // whenever a new feeling is added.
    private Dictionary<string, Texture2D> feelingTextureMap;
    // We need to initialize the feeling to texture map at the first time of obtaining the
    // feeling information.
    private bool isFeelingTextureMapInit = false;

    private OCConnector OCCon;

    private bool showPanel = true;
    private Rect panel;
    private Vector2 scrollPosition;
    private float feelingBarWidth;

    public void ShowPanel()
    {
        showPanel = true;
    }

    public void HidePanel()
    {
        showPanel = false;
    }

    private void FeelingMonitorPanel(int id)
    {
        Dictionary<string, float> feelingValueMap = OCCon.FeelingValueMap;
        if (feelingValueMap.Count == 0) return;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        feelingBarWidth = Screen.width * 0.3f;

        boxStyle = panelSkin.box;
        lock (feelingValueMap)
        {
            int topOffset = 5;
            foreach (string feeling in feelingValueMap.Keys)
            {
                if (!isFeelingTextureMapInit)
                {
                    float r = UnityEngine.Random.value;
                    float g = UnityEngine.Random.value;
                    float b = UnityEngine.Random.value;
                    Color c = new Color(r, g, b, 0.6f);
                    Texture2D t = new Texture2D(1, 1);
                    t.SetPixel(0, 0, c);
                    t.Apply();
                    this.feelingTextureMap[feeling] = t;
                }
                float value = feelingValueMap[feeling];

                // Set the texture of background.
                boxStyle.normal.background = feelingTextureMap[feeling];
                GUILayout.BeginHorizontal();
                GUILayout.Label(feeling + ": ", panelSkin.label, GUILayout.MaxWidth(panel.width * 0.3f));
                GUILayout.Box("", boxStyle, GUILayout.Width(feelingBarWidth * value), GUILayout.Height(16));
                GUILayout.EndHorizontal();
                topOffset += 15;
            }
            // We only need to initialize the map at the first time.
            if (!isFeelingTextureMapInit) isFeelingTextureMapInit = true;
        }

        GUILayout.EndScrollView();
    }

    void Start()
    {
        OCCon = GetComponent<OCConnector>() as OCConnector;
        feelingTextureMap = new Dictionary<string, Texture2D>();
    }

    void OnGUI()
    {
        if (panelSkin != null)
            GUI.skin = panelSkin;

        if (showPanel)
        {
            panel = new Rect(Screen.width * 0.65f, Screen.height * 0.7f, Screen.width * 0.35f, Screen.height * 0.3f);
            panel = GUI.Window(2, panel, FeelingMonitorPanel, gameObject.name + " Feeling Panel"); 
        }
    }
}

