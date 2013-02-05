using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
public class CustomDrawMethod : System.Attribute  {

	public string DrawMethod;
	public object[] Parameters;
	
	public CustomDrawMethod(string drawMethod)
	{
		this.DrawMethod = drawMethod;
		Parameters = new object[0];
	}
	
	public CustomDrawMethod(string drawMethod,params object[] parameters)
	{
		this.DrawMethod = drawMethod;
		this.Parameters = parameters;
	}
	
	public string ParametersToString()
	{
		string parametersString = "";
		for(int i= 0; i < Parameters.Length;i++)
		{
			parametersString+= ", "+Parameters[i].ToString();
		}
		return parametersString;
	}
}
