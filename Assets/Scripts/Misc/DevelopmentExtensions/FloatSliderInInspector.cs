using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class FloatSliderInInspector : System.Attribute {
	
	public float MinValue;
	public float MaxValue;
	
	public FloatSliderInInspector(float minValue, float maxValue)
	{
		this.MinValue = minValue;
		this.MaxValue = maxValue;
	}
}
