using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Spawn manager.
/// </summary>
public class SpawnManager : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Classes
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Object cache.
	/// </summary>
	[Serializable]
	public class ObjectCache
	{
		//////////////////////////////////////////////////
		
		#region Public Member Data
		
		//////////////////////////////////////////////////
		
		/// <summary>
		/// The prefab.
		/// </summary>
		public GameObject prefab;
		
		/// <summary>
		/// The size of the cache.
		/// </summary>
		public int cacheSize = 10;
		
		//////////////////////////////////////////////////
		
		#endregion
		
		//////////////////////////////////////////////////
			
		#region Private Member Data
		
		//////////////////////////////////////////////////
		
		/// <summary>
		/// The objects.
		/// </summary>
		private GameObject[] objects;
		
		/// <summary>
		/// The index of the cache.
		/// </summary>
		private int cacheIndex = 0;
		
		//////////////////////////////////////////////////
		
		#endregion
		
		//////////////////////////////////////////////////
		
		#region Public Member Functions
		
		//////////////////////////////////////////////////
		
		/// <summary>
		/// Initialize this instance.
		/// </summary>
		public void Initialize () 
		{
			objects = new GameObject[cacheSize];
			
			// Instantiate the objects in the array and set them to be inactive
			for (var i = 0; i < cacheSize; i++) {
				objects[i] = MonoBehaviour.Instantiate (prefab) as GameObject;
				objects[i].SetActiveRecursively (false);
				objects[i].name = objects[i].name + i;
			}
		}
		
		/// <summary>
		/// Gets the next object in cache.
		/// </summary>
		/// <returns>
		/// The next object in cache.
		/// </returns>
		public GameObject GetNextObjectInCache ()
		{
			GameObject obj = null;
			
			// The cacheIndex starts out at the position of the object created
			// the longest time ago, so that one is usually free,
			// but in case not, loop through the cache until we find a free one.
			for (int i = 0; i < cacheSize; i++) 
			{
				obj = objects[cacheIndex];
				
				// If we found an inactive object in the cache, use that.
				if (!obj.active)
					break;
				
				// If not, increment index and make it loop around
				// if it exceeds the size of the cache
				cacheIndex = (cacheIndex + 1) % cacheSize;
			}
			
			// The object should be inactive. If it's not, log a warning and use
			// the object created the longest ago even though it's still active.
			if (obj.active) 
			{
				Debug.LogWarning (
					"Spawn of " + prefab.name +
					" exceeds cache size of " + cacheSize +
					"! Reusing already active object.", obj);
				SpawnManager.Destroy (obj);
			}
			
			// Increment index and make it loop around
			// if it exceeds the size of the cache
			cacheIndex = (cacheIndex + 1) % cacheSize;
			
			return obj;
		}
		
		//////////////////////////////////////////////////
		
		#endregion
		
		//////////////////////////////////////////////////
		
		#region Private Member Functions
		
		//////////////////////////////////////////////////
		
		//////////////////////////////////////////////////
		
		#endregion
		
		//////////////////////////////////////////////////
	};
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The spawnManager.
	/// @NOTE: Is this a singleton?
	/// </summary>
	static public SpawnManager spawnManager;
	
	/// <summary>
	/// The caches.
	/// </summary>
	public ObjectCache[] caches;
	
	public Hashtable activeCachedObjects;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SpawnManager"/> class.
	/// </summary>
	public SpawnManager ()
	{
	}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake () 
	{
		// Set the global variable
		spawnManager = this;	
		
		// Total number of cached objects
		int amount  = 0;
		
		// Loop through the caches
		for (var i = 0; i < caches.GetLength(0); i++) {
			// Initialize each cache
			caches[i].Initialize ();
			
			// Count
			amount += caches[i].cacheSize;
		}
		
		// Create a hashtable with the capacity set to the amount of cached objects specified
		activeCachedObjects = new Hashtable (amount);
	}
	
	/// <summary>
	/// Spawn the specified prefab, position and rotation.
	/// </summary>
	/// <param name='prefab'>
	/// Prefab.
	/// </param>
	/// <param name='position'>
	/// Position.
	/// </param>
	/// <param name='rotation'>
	/// Rotation.
	/// </param>
	static public GameObject Spawn (GameObject prefab, Vector3 position, Quaternion rotation) 
	{
		ObjectCache cache = null;
		
		// Find the cache for the specified prefab
		if (spawnManager) 
		{
			for (var i = 0; i < spawnManager.caches.GetLength(0); i++) 
			{
				if (spawnManager.caches[i].prefab == prefab) {
					cache = spawnManager.caches[i];
				}
			}
		}
		
		// If there's no cache for this prefab type, just instantiate normally
		if (cache == null) {
			return Instantiate (prefab, position, rotation) as GameObject;
		}
		
		// Find the next object in the cache
		GameObject obj = cache.GetNextObjectInCache ();
		
		// Set the position and rotation of the object
		obj.transform.position = position;
		obj.transform.rotation = rotation;
		
		// Set the object to be active
		obj.SetActiveRecursively (true);
		spawnManager.activeCachedObjects[obj] = true;
		
		return obj;
	}
	
	/// <summary>
	/// Destroy the specified objectToDestroy.
	/// </summary>
	/// <param name='objectToDestroy'>
	/// Object to destroy.
	/// </param>
	static public void Destroy (GameObject objectToDestroy) 
	{
		if (spawnManager && spawnManager.activeCachedObjects.ContainsKey (objectToDestroy)) 
		{
			objectToDestroy.SetActiveRecursively (false);
			spawnManager.activeCachedObjects[objectToDestroy] = false;
		}
		else 
		{
			GameObject.Destroy (objectToDestroy);
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


