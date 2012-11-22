using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OCEmotionalExpression : MonoBehaviour {

    // Don't show any emotions if intensity of the dominant feeling is lower than this threshold
    public float showEmotionThreshold = 0.5f; 

    // The path for texture of facial expressions.
    private static string facialTexturePath = "Assets/Models/smallrobot/Materials/";
    // The texture of facial expressions stored in resources should be named by this prefix plus feeling name
    private static string facialTexturePrefix = "smallrobot_texture_";
    private static string facialTextureExt = ".TGA";

    private Dictionary<string, Texture2D> emotionTextureMap = new Dictionary<string, Texture2D>();

    private Transform face = null;

	// Use this for initialization
	void Start () {
        face = transform.Find("robot/robotG/mainG/head_GEO");
        if (!face)
        {
            Debug.LogError("Face of the robot is not found");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void showEmotionExpression(Dictionary<string, float> feelingValueMap)
    { 
		/* TODO: uncomment this function after new robot expressions are done
        string dominant_feeling = ""; 
        float dominant_feeling_value = 0; 

        // Find dominant feeling and its value
        lock (feelingValueMap)
        {
            foreach (string feeling in feelingValueMap.Keys)
            {
                float value = feelingValueMap[feeling];

                if (dominant_feeling_value <= value) {
                    dominant_feeling = feeling;
                    dominant_feeling_value = value;
                }
            }   
        }// lock  


        if (dominant_feeling_value < showEmotionThreshold)
            dominant_feeling = "normal"; 

        // Get corresponding facial texture, if fails try to load it from resources
        Texture2D tex = null;
        string textureName = facialTexturePrefix + dominant_feeling + facialTextureExt;
        string textureFullPath = facialTexturePath + textureName;

        if (this.emotionTextureMap.ContainsKey(dominant_feeling))
            tex = this.emotionTextureMap[dominant_feeling];
        else {
            tex = (Texture2D)Resources.LoadAssetAtPath(textureFullPath, typeof(Texture2D));
            if (tex)
            {
                this.emotionTextureMap[dominant_feeling] = tex;
                Debug.Log("Texture for " + dominant_feeling + " loaded.");
            }
        } 
    
        // Set facial texture
        if (tex)
            face.gameObject.renderer.material.mainTexture = tex;
        else
            Debug.LogError("Failed to get texture named: " + textureName);          
		  */
    }
		

}
