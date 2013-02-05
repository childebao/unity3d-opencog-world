using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class InspectorTooltip : System.Attribute
{
	public string Tooltip{ get; set; }
	
	public 	InspectorTooltip (string tooltip)
	{
		this.Tooltip = tooltip;
	}
}
