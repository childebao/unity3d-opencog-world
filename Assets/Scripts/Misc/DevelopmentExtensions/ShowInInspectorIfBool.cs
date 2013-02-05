using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class ShowInInspectorIfBool : System.Attribute {
	
	public string BooleanField;
	public bool EqualsValue;
	
	public ShowInInspectorIfBool(string booleanField, bool equalsValue)
	{
		this.BooleanField = booleanField;
		this.EqualsValue  = equalsValue;
	}

}