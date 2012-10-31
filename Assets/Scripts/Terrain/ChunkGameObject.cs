using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObject : MonoBehaviour, IGameObject
{
    public Shader shader;// = Shader.Find ("Diffuse");

    private MeshFilter m_MeshFilter;
    private MeshCollider m_MeshCollider;
    private MeshRenderer m_MeshRenderer;
    private static int m_ChunksDrawn;

    public Texture Texture;

    /// <summary>
    /// Actually create the mesh now for the chunk.
    /// </summary>
    public void Start()
    {
    }

    public void update()
    {
        CheckIfMovedOutsideTheWorld();
    }

    public void CheckIfMovedOutsideTheWorld()
    {
        if (transform.position == world.getGroundBelowPoint(transform.position))
        {
			Debug.Log("In CheckIfMovedOutsideTheWorld, destroying...");
            Destroy();
        }
    }

    public void CreateFromChunk(Chunk chunk, Dictionary<string, Transform> decoratorPrefabs)
    {
        CreateChunkGameObjectMesh(chunk);
        CreateGameObjectDecorations(chunk, decoratorPrefabs);
        m_ChunksDrawn++;
        CheckIfInitialWorldChunksHaveBeenDrawn(chunk);
    }

    private static void CheckIfInitialWorldChunksHaveBeenDrawn(Chunk chunk)
    {
        m_ChunksDrawn++;
        if (m_ChunksDrawn >= chunk.WorldData.TotalChunks)
        {
            chunk.WorldData.WorldIsReady = true;
        }
    }

    /// <summary>
    /// Create all Unity GameObject decorations for this chunk
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="decoratorPrefabs">All possible decorations, by their name</param>
    private static void CreateGameObjectDecorations(Chunk chunk, Dictionary<string, Transform> decoratorPrefabs)
    {
		//@TODO: Move more of the decoration code from Chunk.cs here to fix this...
//        while (chunk.GameObjectCreationQueue.Count > 0)
//        {
//            GameObjectCreationData creationData = chunk.GameObjectCreationQueue.Dequeue();
//            Instantiate(decoratorPrefabs[creationData.Name], creationData.GlobalUnityPosition,
//                        Quaternion.LookRotation(creationData.Rotation));
//        }
    }

    private void CreateChunkGameObjectMesh(Chunk chunk)
    {
        m_MeshFilter = GetOrCreateComponent<MeshFilter>();
        m_MeshCollider = GetOrCreateComponent<MeshCollider>();
        m_MeshRenderer = GetOrCreateComponent<MeshRenderer>();

        m_MeshRenderer.material.shader = shader;
        m_MeshRenderer.material.mainTexture = Texture;
        transform.position = new Vector3(chunk.Position.X, chunk.Position.Z, chunk.Position.Y);

        m_MeshFilter.mesh.Clear();
        m_MeshFilter.mesh.vertices = chunk.Vertices.ToArray();
        m_MeshFilter.mesh.uv = chunk.Uvs.ToArray();
        m_MeshFilter.mesh.colors = chunk.Colors.ToArray();
        m_MeshFilter.mesh.triangles = chunk.Indices.ToArray();
        m_MeshCollider.sharedMesh = null;
        m_MeshCollider.sharedMesh = m_MeshFilter.mesh;
		m_MeshFilter.mesh.Optimize();
		m_MeshFilter.mesh.RecalculateNormals();

        chunk.Vertices = new List<Vector3>();
        chunk.Uvs = new List<Vector2>();
        chunk.Colors = new List<Color>();
        chunk.Indices = new List<int>();
    }

    public T GetOrCreateComponent<T>() where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public WorldGameObject world { get; set; }

    public void Destroy()
    {
        Debug.Log("Destroy ChunkGameObject " + name);
//        if (world.ChunkPrefab != null)
//        {
//            MeshFilter meshFilter = world.ChunkPrefab.GetComponent<MeshFilter>();
//            meshFilter.mesh.Clear();
//            Destroy(meshFilter.mesh);
//            meshFilter.mesh = null;
//            Destroy(meshFilter);
//            Destroy(world.ChunkPrefab.gameObject);
//            world.ChunkPrefab = null;
//        }
        //world = null;

        m_MeshFilter.mesh.Clear();
        Destroy(m_MeshFilter.mesh);
        m_MeshFilter.mesh = null;
        Destroy(m_MeshFilter);
        Destroy();
    }
}