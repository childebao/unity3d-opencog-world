using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class ShowInInspectorIfEnum : System.Attribute {
	
	public string EnumField;
	public object EnumValue;
	
	public ShowInInspectorIfEnum(string enumField, object enumValue)
	{
		this.EnumField = enumField;
		this.EnumValue = enumValue;		
	}
}