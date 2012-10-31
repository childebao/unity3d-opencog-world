using System;
using System.Collections.Generic;
using UnityEngine;

// record the all the states of objects that will be automatically reported to opencog when its value changes
namespace Embodiment
{
	public struct StateInfo
	{
		public GameObject gameObject;
		public Behaviour behaviour;
		public String stateName;
		//public System.Object stateVariable; // the object reference to the state variable
		
	}
	
	public class StateChangesRegister
	{
		public static List<StateInfo> StateList = new List<StateInfo>();
		
		// register a state of a gameobject to be automatically reported to opencog when its value changes 
		public static void RegisterState(GameObject go, Behaviour bh, String stateName)
		{
			System.Diagnostics.Debug.Assert(go != null && bh != null && stateName != null);
			
			StateInfo aInfo = new StateInfo();
			aInfo.gameObject = go;
			aInfo.behaviour = bh;
			aInfo.stateName = stateName;
			
			StateList.Add(aInfo);
			
			 
			GameObject[] OCAs = GameObject.FindGameObjectsWithTag("OCA");
			foreach (GameObject OCA in OCAs)
			{
				OCPerceptionCollector pCollector = OCA.GetComponent<OCPerceptionCollector>() as OCPerceptionCollector;
				if (pCollector != null)
				{
					pCollector.addNewState(aInfo);
				}
			}
			
		}
		
		public static void UnregisterState(StateInfo aInfo)
		{
			if ( StateList.Contains(aInfo) )
				StateList.Remove(aInfo);
		}
		
	}
	

}

