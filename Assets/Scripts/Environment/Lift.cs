using UnityEngine;
using System.Collections;
using Embodiment;

public class Lift : MonoBehaviour {
	
	public bool isActive = false;
	public Material unactiveMaterial;
	public Material activeMaterial;	
	public GameObject theLiftEffect = null; // the particle system, whose prefab is LiftParticleSystem
	public GameObject prefabLiftEffect = null;
	
	// a lift can only move between the startPos and destPos
	public Vector3 startPos;
	public Vector3 destPos;
	
	
	// Use this for initialization
	void Start () {

		if (isActive)
		{
			gameObject.renderer.material = activeMaterial;
			GenerateLiftEffect();
			
		}
		else
			gameObject.renderer.material = unactiveMaterial;
		
		
		StateChangesRegister.RegisterState(gameObject, this,"isActive" );
	}
	
	void GenerateLiftEffect()
	{
		if (theLiftEffect == null)
		{
			theLiftEffect = Instantiate(prefabLiftEffect) as GameObject;
			theLiftEffect.transform.parent = gameObject.transform;
			theLiftEffect.transform.localPosition = new Vector3(0.0f,0.0f,0.0f);
		}
	}
	
	void RemoveLiftEffect()
	{
		if (theLiftEffect != null)
			DestroyObject(theLiftEffect);
	}
			
	IEnumerator MoveUp(float delayTime = 2.0f)
	{
		if (!isActive)
		{
			RemoveLiftEffect();
			gameObject.renderer.material = unactiveMaterial;
			yield break;
		}
		iTween.MoveTo(gameObject , iTween.Hash("position",destPos,
		                                 	   "speed",1,
		                                 	   "easetype",iTween.EaseType.easeInSine,
		                                       "delay",delayTime,
		                                       "oncomplete","MoveDown",
		                                       "oncompleteparams",2.0f
		                                       ));
		yield return new WaitForSeconds(delayTime);

	}
	
	IEnumerator MoveDown(float delayTime = 2.0f)
	{	
		iTween.MoveTo(gameObject , iTween.Hash("position",startPos,
		                                 	   "speed",1,
		                                 	   "easetype",iTween.EaseType.easeInSine,
		                                       "delay",delayTime,
		                                       "oncomplete","MoveUp",
		                                       "oncompleteparams",2.0f
		                                       ));
		yield return new WaitForSeconds(delayTime);

	}
		
	
	public void RestartTheLift()
	{
		isActive = true;
		gameObject.renderer.material = activeMaterial;
		GenerateLiftEffect();
		StartCoroutine(MoveUp()); 
	}
	
	public void StopTheLift()
	{
		isActive = false;
	}
	
}
