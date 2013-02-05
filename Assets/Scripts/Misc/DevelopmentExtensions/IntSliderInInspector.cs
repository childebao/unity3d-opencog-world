using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class IntSliderInInspector : System.Attribute {
	
	public int MinValue;
	public int MaxValue;
	
	public IntSliderInInspector(int minValue, int maxValue)
	{
		this.MinValue = minValue;
		this.MaxValue = maxValue;
	}


}
