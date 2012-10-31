using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Health.
/// </summary>
public class Health : MonoBehaviour
{
	
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The max health.
	/// </summary>
	public float maxHealth = 100.0f;
	
	/// <summary>
	/// The health.
	/// </summary>
	public float health = 100.0f;
	
	/// <summary>
	/// The regenerate speed.
	/// </summary>
	public float regenerateSpeed = 0.0f;
	
	/// <summary>
	/// The invincible.
	/// </summary>
	public bool invincible = false;
	
	/// <summary>
	/// The dead.
	/// </summary>
	public bool dead = false;
	
	/// <summary>
	/// The damage prefab.
	/// </summary>
	public GameObject damagePrefab;
	
	/// <summary>
	/// The damage effect transform.
	/// </summary>
	public Transform damageEffectTransform;
	
	/// <summary>
	/// The damage effect multiplier.
	/// </summary>
	public float damageEffectMultiplier = 1.0f;
	
	/// <summary>
	/// The damage effect centered.
	/// </summary>
	public bool damageEffectCentered = true;
	
	/// <summary>
	/// The scorch mark prefab.
	/// </summary>
	public GameObject scorchMarkPrefab = null;
	
	/// <summary>
	/// The damage signals.
	/// </summary>
	public SignalSender damageSignals;
	
	/// <summary>
	/// The die signals.
	/// </summary>
	public SignalSender dieSignals;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The last damage time.
	/// </summary>
	private float lastDamageTime = 0;
	
	/// <summary>
	/// The damage effect.
	/// </summary>
	private ParticleEmitter damageEffect;
	
	/// <summary>
	/// The damage effect center Y offset.
	/// </summary>
	private float damageEffectCenterYOffset;
	
	/// <summary>
	/// The collider radius heuristic.
	/// </summary>
	private float colliderRadiusHeuristic = 1.0f;
	
	/// <summary>
	/// The scorch mark.
	/// </summary>
	private GameObject scorchMark = null;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Health"/> class.
	/// </summary>
//	public Health ()
//	{
//	}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake()
	{
		enabled = false;
		if (damagePrefab) 
		{
			if (damageEffectTransform == null)
				damageEffectTransform = transform;
			GameObject effect = SpawnManager.Spawn (damagePrefab, Vector3.zero, Quaternion.identity);
			effect.transform.parent = damageEffectTransform;
			effect.transform.localPosition = Vector3.zero;
			damageEffect = effect.particleEmitter;
			Vector2 tempSize = new Vector2(collider.bounds.extents.x,collider.bounds.extents.z);
			colliderRadiusHeuristic = tempSize.magnitude * 0.5f;
			damageEffectCenterYOffset = collider.bounds.extents.y;
			
		}
		if (scorchMarkPrefab) 
		{
			scorchMark = (GameObject)GameObject.Instantiate(scorchMarkPrefab, Vector3.zero, Quaternion.identity);
			scorchMark.active = false;
		}
	}
	
	/// <summary>
	/// Raises the damage event.
	/// </summary>
	/// <param name='amount'>
	/// Amount.
	/// </param>
	/// <param name='fromDirection'>
	/// From direction.
	/// </param>
	public void OnDamage(float amount, Vector3 fromDirection)
	{
		// Take no damage if invincible, dead, or if the damage is zero
		if(invincible)
			return;
		if (dead)
			return;
		if (amount <= 0)
			return;
		
		// Decrease health by damage and send damage signals
		
		// @HACK: this hack will be removed for the final game
		//  but makes playing and showing certain areas in the
		//  game a lot easier
		/*	
		#if !UNITY_IPHONE && !UNITY_ANDROID
		if(gameObject.tag != "Player")
			amount *= 10.0;
		#endif
		*/
		
		health -= amount;
		damageSignals.SendSignals (this);
		lastDamageTime = Time.time;
		
		// Enable so the Update function will be called
		// if regeneration is enabled
		if (regenerateSpeed > 0)
			enabled = true;
		
		// Show damage effect if there is one
		if (damageEffect) {
			damageEffect.transform.rotation = Quaternion.LookRotation (fromDirection, Vector3.up);
			if(!damageEffectCentered) {
				Vector3 dir = fromDirection;
				dir.y = 0.0f;
				damageEffect.transform.position = (transform.position + Vector3.up * damageEffectCenterYOffset) + colliderRadiusHeuristic * dir;
			}
			// @NOTE: due to popular demand (ethan, storm) we decided
			// to make the amount damage independent ...
			//var particleAmount = Random.Range (damageEffect.minEmission, damageEffect.maxEmission + 1);
			//particleAmount = particleAmount * amount * damageEffectMultiplier;
			damageEffect.Emit();// (particleAmount);
		}
		
		// Die if no health left
		if (health <= 0)
		{
			GameScore.RegisterDeath (gameObject);
			
			health = 0;
			dead = true;
			dieSignals.SendSignals (this);
			enabled = false;
			
			// scorch marks
			if (scorchMark) {
				scorchMark.active = true;
				// @NOTE: maybe we can justify a raycast here so we can place the mark
				// on slopes with proper normal alignments
				// @TODO: spawn a yield Sub() to handle placement, as we can
				// spread calculations over several frames => cheap in total
				Vector3 scorchPosition  = collider.ClosestPointOnBounds (transform.position - Vector3.up * 100);
				scorchMark.transform.position = scorchPosition + Vector3.up * 0.1f;
				float y = (float) new Random().RandomRange(0, 90);
				scorchMark.transform.eulerAngles.Set(scorchMark.transform.eulerAngles.x, y, scorchMark.transform.eulerAngles.z);
			}
		}
	}
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	public void OnEnable()
	{
		Regenerate();
	}
	
	/// <summary>
	/// Regenerate this instance.
	/// </summary>
	public IEnumerator Regenerate()
	{
		if (regenerateSpeed > 0.0f) 
		{
			while (enabled) 
			{
				if (Time.time > lastDamageTime + 3) 
				{
					health += regenerateSpeed;
					
					yield break;
					
					if (health >= maxHealth) 
					{
						health = maxHealth;
						enabled = false;
					}
				}
				yield return new WaitForSeconds (1.0f);
			}
		}
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	

}


