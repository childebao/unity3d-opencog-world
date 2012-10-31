using UnityEngine;
using System.Collections;

public class VerticalSizeCalculator {
	// This is a utility class that takes a gameObject and will determine how high above it's
	// transform position the objects stops. Depending on the type of object, certain colliders and
	// other things that have "bounds" should be ignored.

	static public float getHeight(Transform t, Transform ignoredTransform) {
		float maxHeight = 0.0f;
		var meshes = t.GetComponentsInChildren<MeshRenderer>();
		var skinnedMesh = t.GetComponentInChildren<SkinnedMeshRenderer>();
		
		foreach (MeshRenderer m in meshes) {
			// don't include the the size of the spinner - we do that above.
			if (m.transform.parent == ignoredTransform) continue;
			Debug.Log("mesh for object " + m.gameObject + ", size is " + m.bounds);
			// the center of the bounds might not be centered on the parents position.
			float newY = (m.bounds.center - t.position).y + m.bounds.extents.y;
			//float newY = m.bounds.center.y + m.bounds.size.y;
			if (newY > maxHeight) maxHeight = newY;
		}
		if (skinnedMesh != null) {
			// for some reason a skinned mesh renderer doesn't get detected, which is what the avatars use
			var m = skinnedMesh;
			Debug.Log("mesh for object " + m.gameObject + ", size is " + m.bounds);
			// the center of the bounds might not be centered on the parents position.
			float newY = (m.bounds.center - t.position).y + m.bounds.extents.y;
			//float newY = m.bounds.center.y + m.bounds.size.y;
			if (newY > maxHeight) maxHeight = newY;
		}
		return maxHeight;
	}	
	
		
	
}
