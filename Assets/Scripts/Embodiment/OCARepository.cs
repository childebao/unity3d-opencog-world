using UnityEngine;

using System.Collections.Generic;
using System.Collections;

public class OCARepository : MonoBehaviour
{
	public static Transform myTransform;
	
	void Start(){
		myTransform = transform;
	}
	
	public static bool isOCAExist (int id){
		foreach(Transform child in myTransform)
		{
			if(child.gameObject.GetInstanceID() == id)
			{
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Add oc avatar to repository 
	/// </summary>
	/// <param name="Target">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <returns>
	/// If avatar with a given name already exists, return false.
	/// </returns>
	public static bool AddOCA(GameObject Target){
        if(isOCAExist(Target.GetInstanceID()))
		{
			Debug.Log("Avatar's name has already existed!! It should be unique!!");
			return false;
		}
		else
		{
			Target.transform.parent = myTransform;
			return true;
		}
    }

    public static void RemoveOCA(GameObject Target){
		if(isOCAExist(Target.GetInstanceID()))
		{
			Target.transform.parent = myTransform.root;
			Target.tag = "Untagged";
			
			// Remove the object out of sight.
			Destroy(Target);
		}
        else
		{
			Debug.Log("Avatar \""+Target.name+"\" doesn't exist!!");
		}
    }	
	
    public static void RemoveOCA(int id){
        RemoveOCA(myTransform.GetChild(id).gameObject);
    }
	
	public static GameObject GetOCA(string Name){
        foreach(Transform child in myTransform)
		{
			if(child.gameObject.name == Name)
			{
				return child.gameObject;
			}
		}
		Debug.Log("Avatar \""+Name+"\" doesn't exist!!");
        return null;
    }
	
    public static GameObject GetOCA(int id){
		if(myTransform.childCount > 0)
		{
	        foreach(Transform child in myTransform)
			{
				if(child.gameObject.GetInstanceID() == id)
				{
					return child.gameObject;
				}
			}
			Debug.Log("Avatar "+id.ToString()+" doesn't exist!!");
		    return null;
		}
		else
		{
			Debug.Log("OCA Repository has no child!!");
			return null;
		}
    }

    public static List<GameObject> GetAllOCA(){
        List<GameObject> result = new List<GameObject>();
        foreach(Transform child in myTransform) {
            result.Add(child.gameObject);
        }
        return result;
    }

    public static List<GameObject> GetOCAByTag(string Tag)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in myTransform) {
            if(child.tag == Tag)
                result.Add(child.gameObject);
        }
        if(result.Count == 0)
            Debug.Log("No avatar is tagged with \"" + Tag+ "\"");
        return result;
    }
}
