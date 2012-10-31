using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(WorldGameObject))]
class VisualPathDebugger : MonoBehaviour
{
    public List<Transform> lastPathMarks = new List<Transform>();
    // For coordinates in the path.
    public Transform landMark;
    // Reference to the world data.
    private WorldData worldData;

    void Awake()
    {
        // Get the world game object.
        WorldGameObject world = GameObject.Find("World").GetComponent<WorldGameObject>() as WorldGameObject;
        // Get the chunks data.
        worldData = world.WorldData;
    }

    public void DrawPath(List<Vector3> path)
    {
        clearLandmarks();

        string pathStr = "Path";
        foreach (Vector3 location in path)
        {
            pathStr += " -> " + location.ToString();
            addLandmark(location);
        }
        Debug.LogWarning(pathStr);
    }

    void addLandmark(Vector3 location)
    {
        Transform t = (Transform)Instantiate(landMark);
        t.position = new Vector3(location.x, location.y - 0.5f, location.z );
        t.name = location.ToString();
        this.lastPathMarks.Add(t);
    }

    void clearLandmarks()
    {
        foreach (Transform t in this.lastPathMarks)
        {
            GameObject.Destroy(t.gameObject);
        }
        this.lastPathMarks.Clear();
    }
}
