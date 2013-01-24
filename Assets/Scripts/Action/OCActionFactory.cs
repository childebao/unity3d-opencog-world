using UnityEngine;
using System.Collections;

/// <summary>
/// OpenCog Action Factory.  Creates OCActions according to an xml or protobuf protocol.
/// Assigns animations to actions and estimates the time the action will take
/// depending on certain factors (such as the animation's play time, or the motor's speed).
/// Also registers available OCActions in the RegisteredActions list.
/// </summary>
public class OCActionFactory : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	/// <summary>
	/// Creates the action using a protobuf schema.  Not sure of the design for now.
	/// </summary>
	/// <returns>
	/// The action.
	/// </returns>
	/// <param name='serializer'>
	/// Serializer.
	/// </param>
	/// <param name='adjustCoordinate'>
	/// Adjust coordinate.
	/// </param>
	public OCAction CreateAction(ProtoBuf.Serializer serializer, bool adjustCoordinate = false)
	{
		OCAction action = new OCAction();
		
		
		
		return action;
	}
	
	/// <summary>
	/// Creates the action from an xml schema.  For backwards compatibility.
	/// </summary>
	/// <returns>
	/// The action.
	/// </returns>
	/// <param name='element'>
	/// The XML element corresponding to the action.
	/// </param>
	/// <param name='adjustCoordinate'>
	/// Whether to adjust coordinate. (not sure about this yet)
	/// </param>
	public OCAction CreateAction(XmlElement element, bool adjustCoordinate = false)
	{
		OCAction action = new OCAction();
		
		
		
		return action;
	}
}

