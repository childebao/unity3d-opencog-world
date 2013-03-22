using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Embodiment;
using ProtoBuf;

namespace oldOpenCog
{

public enum PropertyType {
    STRING,
    INT,
    FLOAT,
    BOOL
}

[System.Serializable]
[ProtoContract]
public class OCProperty {
    public bool enabled;
    [ProtoMember(1)]
    public string Key
    {
        get { return key; }
        set { key = value; }
    }
    public string key;

    [ProtoMember(2)]
    public string value;
    public PropertyType valueType;

    public OCProperty(string inputKey, string inputValue, PropertyType inputType = PropertyType.STRING, bool enabled = false) {
        this.key = inputKey;
        this.value = inputValue.ToString();
        this.valueType = inputType;
        this.enabled = enabled;
    }

    public OCProperty()
    {
        Key = "null";
    }
}

public class OCPropertyManager : MonoBehaviour {

    /**
     * A property list for an opencog game object that is serializable 
     * in unity editor, so that enables developer 
     * to add / delete / modify the properties via editor.
     */
    public List<OCProperty> propertyList = new List<OCProperty>();
	
	private Config config = Config.getInstance();
	
	public string getValue(string key) {
		foreach (OCProperty ocp in propertyList) {
			if (ocp.key == key)
				return ocp.value;
		}
		return "";	
	}
	
	public PropertyType getType(string key) {
		foreach (OCProperty ocp in propertyList) {
			if (ocp.key == key)
				return ocp.valueType;
		}
		return PropertyType.STRING;
	}
	
	public OCProperty getProperty(string key) {
		foreach (OCProperty ocp in propertyList) {
			if (ocp.key == key)
				return ocp;
		}
		return null;
	}
	
	public bool checkExist(string key) {
		foreach (OCProperty ocp in propertyList) {
			if (ocp.key == key)
				return true;
		}
		return false;
	}
}

}// namespace oldOpenCog
