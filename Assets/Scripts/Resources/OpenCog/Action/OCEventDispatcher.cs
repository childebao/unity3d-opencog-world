using UnityEngine;
using System.Collections;
using System;

public class OCEventDispatcher : MonoBehaviour
{
	public EventArgs e = null;
	
	//EVENT: ResetLevel
	public delegate void mResetLevelDelegate(OCEventDispatcher ED, EventArgs e);
	//Event
	public event mResetLevelDelegate mResetLevelEvent;
	//Riser
	public void RiseResetLevelEvent()
	{
		if(mResetLevelEvent!=null) mResetLevelEvent(this,e);
	}
	
//	//EVENT: LoadLevel
//	public delegate void mLoadLevelDelegate(OCEventDispatcher ED, CustomArg myArg);
//	public event mLoadLevelDelegate mLoadLevelEvent;
//	public void RiseLoadLevelEvent(int n)
//	{
//	        CustomArg _myArg = new CustomArg(n);
//	        if(mLoadLevelEvent!=null) mLoadLevelEvent(this,_myArg);
//	}
	
}

