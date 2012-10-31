using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class DialogueEntry
{
    public string longText;
    public Texture2D img;

    // Maybe we can design several styles of dialog window to be
    // shown on the screen, which depends on pos.
    public int pos;
    public DialogueEntry(string s)
    {
        this.longText = s;
        this.pos = 0;
    }

	public DialogueEntry() 
    {
		this.longText = "New entry";
        this.pos = 0;
	}
}

class DialogInstance : MonoBehaviour
{
    public GUISkin dialogSkin;
    //public RenderTexture avatarFaceTexture;
	public Camera avatarCamera;
    public float textSpeed = 40f;
    public Dictionary<string, DialogueEntry> DialogSamples = new Dictionary<string,DialogueEntry>();

    private List<string> parsedText = new List<string>();
    private int lineCount = 0;
    private float timeStart = 0.0f;

    //private Player player;
	private HUD theHUD;
    private bool showDialog = false;

    private DialogueEntry displayEntry = null;

    #region GUI related
    // Current dialog content
    private GUIContent curContent = new GUIContent("");
    #endregion

    private void IncreaseLineCount()
    {
        timeStart = Time.time;
        lineCount++;
    }

    private void DecreaseLineCount()
    {
        timeStart = Time.time;
        lineCount--;
    }

    void Start()
    {
        //player = GameObject.Find("Player").GetComponent<Player>() as Player;
		theHUD = GameObject.Find("HUD").GetComponent<HUD>() as HUD;
        string text = "I need energy... Could you please help me to get a battery?|Thank you very much!";
        DialogSamples.Add("require-help", new DialogueEntry(text));
        //LoadDialog("require-help");
    }

    void Update()
    {
        // Simulate the printer effect
        if (lineCount >= parsedText.Count)
        {
            parsedText.Clear();
            showDialog = false;
            return;
        }
        
        string text = parsedText[lineCount];
        int charNum = (int)((Time.time - timeStart) * textSpeed);
        if (charNum < text.Length) text = text.Substring(0, charNum);
        curContent = new GUIContent(text);
    }

    void Restart()
    {
    }

    /**
     * Load builtin dialog entry through given key.
     */
    public void LoadDialog(string content)
    {
        /*
        if (DialogSamples.ContainsKey(key))
        {
            displayEntry = DialogSamples[key];
        }
        else
        {
            displayEntry = null;
            return;
        }
        
        ParseText(displayEntry.longText);
        */
        ParseText(content);

        DisplayDialog();
    }

    public void LoadDialog(DialogueEntry entry)
    {
        if (entry == null) return;
        displayEntry = entry;
        lineCount = 0;
        ParseText(displayEntry.longText);

        DisplayDialog();
    }

    /**
     * Parse the long text in dialog entry with '|' as a separator.
     */
    public void ParseText(string s)
    {
        char[] separator = { '|' };
        parsedText = s.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private void DisplayDialog()
    {
        showDialog = true;
        theHUD.setCharacterControl(false);
    }

    private void HideDialog()
    {
        showDialog = false;
		avatarCamera.enabled = false;
        theHUD.setCharacterControl(true);
    }

    void OnGUI()
    {
        if (!showDialog) return;
        if (curContent == null) return;
        if (dialogSkin != null) GUI.skin = dialogSkin;
		
		float bottomBorder = 4.0f;
		float dialogHeight = 100.0f;
		float cameraSquareSize = dialogHeight;
		float dialogWidth = (Screen.width / 3.0f) + cameraSquareSize;
		
		float topEdge = Screen.height - bottomBorder - dialogHeight;
		float bottomEdge = Screen.height - bottomBorder;
		float leftEdge = (Screen.width / 2.0f) - (dialogWidth / 2.0f);
		float rightEdge = leftEdge + dialogWidth;
		
		float arrowPositionX = rightEdge - 30;
		float arrowPositionY = bottomEdge - 80.0f;

        GUILayout.BeginHorizontal();
        // If an avatar view is given, draw it on the screen.
        //if (avatarFaceTexture != null)
        //    GUI.DrawTexture(new Rect(Screen.width / 4.5f, Screen.height * 4 / 5, 128, 128), avatarFaceTexture);
		if (avatarCamera != null) {
			avatarCamera.enabled = true;
            avatarCamera.depth = 1;
			avatarCamera.rect = new Rect((leftEdge)/Screen.width, bottomBorder / Screen.height,
				                        cameraSquareSize/Screen.width, cameraSquareSize/Screen.height);
		}
        GUI.Box(new Rect(leftEdge, topEdge, dialogWidth, dialogHeight), curContent, "textboxplayer");
        if (GUI.Button(new Rect(rightEdge - 20, topEdge - 20, 32, 32), "", "exit"))
        {
            HideDialog();
        }
        if (lineCount > 0)
        {
            if (GUI.Button(new Rect(arrowPositionX - 30, arrowPositionY, 24, 24), "", "prevItem"))
            {
                DecreaseLineCount();
            }
        }
        if (lineCount < parsedText.Count - 1)
        {
            if (GUI.Button(new Rect(arrowPositionX, arrowPositionY, 24, 24), "", "nextItem"))
            {
                IncreaseLineCount();
            }
        }
        GUILayout.EndHorizontal();
    }
}