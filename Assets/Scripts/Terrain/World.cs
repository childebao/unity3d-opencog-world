using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWorld
{
    void SetLightAt(Vector3i localMapPosition);
}

public class World : IWorld
{
    private readonly ITerrainGenerator m_TerrainGenerator;
    private readonly WorldData m_WorldData;
    private readonly ILightProcessor m_LightProcessor;
    private readonly IMeshDataGenerator m_MeshDataGenerator;
    private readonly IWorldDecorator m_WorldDecorator;
    private readonly IChunkProcessor m_ChunkProcessor;
    private Thread m_ProcessingThread;

    private bool m_Processing;


    public World(WorldData worldData, ITerrainGenerator terrainGenerator, ILightProcessor lightProcessor,
                 IMeshDataGenerator meshDataGenerator, IWorldDecorator worldDecorator, IChunkProcessor chunkProcessor)
    {
        m_WorldData = worldData;
        m_LightProcessor = lightProcessor;
        m_MeshDataGenerator = meshDataGenerator;
        m_WorldDecorator = worldDecorator;
        m_ChunkProcessor = chunkProcessor;
        m_TerrainGenerator = terrainGenerator;
        ContinueProcessingChunks = true;
    }

    private ChunkBatch m_CurrentBatchBeingProcessed;

    private int m_BatchCount;

    /// <summary>
    /// This is the main method which processes the batches of chunks.
    /// It is responsible for calling the terrain generation, decorating, lighting,
    /// and mesh generation.
    /// It's running in a seperate thread, and never stops checking for more work to do.
    /// </summary>
    private void ProcessChunks()
    {
		Debug.Log("In Process Chunks...0");
        while (ContinueProcessingChunks)
        {
            // Complete each batch before moving onto the next
            if (m_CurrentBatchBeingProcessed!= null || m_ChunkProcessor.ChunksAreBeingAdded)
            {
                continue;
            }
			
			//Debug.Log("In Process Chunks...1");
			
            ChunkBatch batch = m_ChunkProcessor.GetBatchOfChunksToProcess();

            if (batch == null)
            {
                continue;
            }

            m_CurrentBatchBeingProcessed = batch;
            
            DateTime start = DateTime.Now;
			
			Debug.Log("In Process Chunks...2: " + batch.Chunks.Count);
            if (batch.BatchType == BatchType.TerrainGeneration)
            {
                m_TerrainGenerator.GenerateTerrain(batch.Chunks);
                m_CurrentBatchBeingProcessed = null;
                continue;
            }
            if (batch.BatchType != BatchType.Lighting && m_WorldDecorator != null)
            {
                m_WorldDecorator.Decorate(batch.Chunks);
            }
			
			Debug.Log("In Process Chunks...3");
            m_LightProcessor.LightChunks(batch.Chunks);
            m_MeshDataGenerator.GenerateMeshData(batch.Chunks);
            m_CurrentBatchBeingProcessed = null;

            if (batch.Chunks.Count > 0)
            {
                Debug.Log("Total Time: " + (DateTime.Now - start));
            }

            Thread.Sleep(1);
			Debug.Log("In Process Chunks...4");
        }
    }

    /// <summary>
    /// When set to false, stops the chunk processing thread
    /// </summary>
    public bool ContinueProcessingChunks { get; set; }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public IChunkProcessor ChunkProcessor
    {
        get { return m_ChunkProcessor; }
    }

    public void InitializeGridChunks()
    {
        WorldData.InitializeGridChunks();
    }


    private static void ClearRegenerationStatus(IEnumerable<Chunk> chunks)
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.NeedsRegeneration = false;
        }
    }


    // Regenerates the target chunk first, followed by any others that need regeneration.

    public void RegenerateChunks(int chunkX, int chunkY, int chunkZ)
    {
		Debug.Log("In RegenerateChunks...");
        m_ChunkProcessor.AddBatchOfChunks(new List<Chunk>(){m_WorldData.Chunks[chunkX, chunkY, chunkZ]}, BatchType.Lighting );
        //List<Chunk> chunksNeedingRegeneration = WorldData.ChunksNeedingRegeneration;
        //if (chunksNeedingRegeneration.Count == 0)
        //{
        //    return;
        //}

        //Chunk targetChunk = WorldData.Chunks[chunkX, chunkY, chunkZ];

        ////Put our target chunk as the first in the list.
        //if (chunksNeedingRegeneration.Contains(targetChunk))
        //{
        //    chunksNeedingRegeneration.Remove(targetChunk);
        //    chunksNeedingRegeneration.Insert(0, targetChunk);
        //}

        //RegenerateChunks(chunksNeedingRegeneration);
    }

    public void RegenerateChunks()
    {
        List<Chunk> chunksNeedingRegeneration = WorldData.ChunksNeedingRegeneration;
        if (chunksNeedingRegeneration.Count == 0)
        {
            return;
        }

        RegenerateChunks(chunksNeedingRegeneration);
    }

    public void RegenerateChunks(List<Chunk> chunksNeedingRegeneration)
    {
        m_ChunkProcessor.AddChunksToLightingQueue(chunksNeedingRegeneration);
        //m_LightProcessor.LightChunks(chunksNeedingRegeneration);
        //m_MeshDataGenerator.GenerateMeshData(chunksNeedingRegeneration);
        ClearRegenerationStatus(chunksNeedingRegeneration);
    }

    /// <summary>
    /// Generic block removal. 
    /// </summary>
    /// <param name="localMapPosition"></param>
    public void RemoveBlockAt(Vector3i localMapPosition)
    {
        //DateTime start = DateTime.Now;
        WorldData.SetBlockTypeWithRegeneration(localMapPosition.X, localMapPosition.Y, localMapPosition.Z, BlockType.Air);
        m_LightProcessor.RecalculateLightingAroundBlock(localMapPosition.X, localMapPosition.Y, localMapPosition.Z);
        List<Chunk> chunksNeedingRegeneration = m_WorldData.ChunksNeedingRegeneration;
        m_ChunkProcessor.AddBatchOfChunks(chunksNeedingRegeneration, BatchType.Lighting);
        ClearRegenerationStatus(chunksNeedingRegeneration);
    }

    public void SetLightAt(Vector3i localMapPosition)
    {
        DateTime start = DateTime.Now;
        Debug.Log("Setting block light");
        WorldData.SetBlockLightWithRegeneration(localMapPosition.X, localMapPosition.Y, localMapPosition.Z, 255);
        Debug.Log("Relighting");
        m_LightProcessor.RecalculateLightingAroundBlock(localMapPosition.X, localMapPosition.Y, localMapPosition.Z);
        Debug.Log("Adding batch");
        List<Chunk> chunksNeedingRegeneration = m_WorldData.ChunksNeedingRegeneration;
        m_ChunkProcessor.AddBatchOfChunks(chunksNeedingRegeneration, BatchType.Lighting);
        ClearRegenerationStatus(chunksNeedingRegeneration);
        Debug.Log("RemoveBlock: " + (DateTime.Now - start)); 
    }

    public void GenerateBlockAt(IntVect buildPoint, BlockType bt = BlockType.Stone)
	{
		Console.print("In GenerateBlockAt...");
		try
		{
			m_WorldData.SetBlockTypeWithRegeneration(buildPoint.X, buildPoint.Y, buildPoint.Z, bt);
			RegenerateChunks(buildPoint.X / m_WorldData.ChunkBlockWidth,
			                 buildPoint.Y / m_WorldData.ChunkBlockHeight,
			                 buildPoint.Z / m_WorldData.ChunkBlockDepth);
		}
		catch (Exception e)
            {
                Debug.LogError("An error occurred in generating a block: " + e.ToString());
            }
	}
	
	public void FireNukeAt(IntVect hitPoint, Ray ray)
    {
        Debug.Log(hitPoint + " - " + ray);
        float xInc = hitPoint.X;
        float yInc = hitPoint.Y;
        float zInc = hitPoint.Z;
        for (int distance = 0; distance <= 10; distance++)
        {
            xInc += ray.direction.x;
            yInc += ray.direction.y;
            zInc += ray.direction.z;
            for (int numBlocks = 0; numBlocks < 10; numBlocks++)
            {
                int blockX = (int) (UnityEngine.Random.insideUnitSphere.x * 3 + xInc);
                int blockY = (int) (UnityEngine.Random.insideUnitSphere.y * 3 + yInc);
                int blockZ = (int) (UnityEngine.Random.insideUnitSphere.z * 3 + zInc);
                m_WorldData.SetBlockTypeWithRegeneration(blockX, blockY, blockZ, BlockType.Air);
            }
        }
    }

    private int m_DiggingAmount = 100;

    private Vector3i m_DiggingLocation;

    private DateTime m_LastDigTime;

    private readonly TimeSpan m_DigDuration = TimeSpan.FromSeconds(0.25);

    public readonly Queue<Vector3> Diggings = new Queue<Vector3>();

    public bool ThreadisAlive
    {
        get { return m_ProcessingThread.IsAlive; }
    }
	
	// Used when the girl burns a block with her left arm
	public void Burn(IntVect targetLocation, Vector3 hitPoint)
	{
		//Console.print("In Burn");
		Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
		
		FireNukeAt(targetLocation, ray);
	}
	
	// Used when the girl freezes a block with her right arm
	public void Freeze(IntVect targetLocation, Vector3 hitPoint)
	{
		//Console.print("In Freeze");
	}
	
	// Used when the girl steams a block with both of her arms
	public void Steams(IntVect targetLocation, Vector3 hitPoint)
	{
		//Console.print("In Steams");
	}
	
	// Used when the robot lifts a block with his right arm
	public void Lift(IntVect targetLocation, Vector3 hitPoint)
	{
		//Console.print("In Lift");
	}
	
	// Used when the robot places a block with his right arm
	public void Place(IntVect targetLocation, Vector3 hitPoint)
	{
		//Console.print("In Place");
	}

    /// <summary>
    /// Begins digging at the targetLocation. This is just simple digging now, it 
    /// doesn't know about different block types.
    /// </summary>
    /// <param name="localMapPosition">The local map position of the block to dig in.</param>
    /// <param name="globalPosition">The exact dig point, in Unity coordinates.</param>
    public void Dig(Vector3i localMapPosition, Vector3 globalPosition)
    {
        DateTime currentDigTime = DateTime.Now;

        // If we are digging but shift to a different block, we lose the digging amount at the original
        // block and start over.
        if (localMapPosition != m_DiggingLocation)
        {
            // Three hits will remove the block
            m_DiggingAmount = 20;
            // Save the current digging location
            m_DiggingLocation = localMapPosition;
            m_LastDigTime = currentDigTime;
            // Let the sparks and awesome digging sound fly
            Diggings.Enqueue(globalPosition);
        }
        else
        {
            // Have we been working on this block long enough?
            if (currentDigTime - m_LastDigTime > m_DigDuration)
            {
                // Eventually, we'll just spawn these like the gameobject decorations
                Diggings.Enqueue(globalPosition);
                // Knock off 10 whacks from the block
                m_DiggingAmount = m_DiggingAmount - 10;
                m_LastDigTime = currentDigTime;

                // All gone?
                if (m_DiggingAmount <= 0)
                {
					//fDebug.Log("Digging...?");
                    RemoveBlockAt(localMapPosition);
                    m_DiggingAmount = 100;
                }
            }
        }
    }

    /// <summary>
    /// Given a 'global' position in the entire world, get the local position
    /// of the block on the map. 
    /// </summary>
    /// <param name="targetLocation"></param>
    /// <returns></returns>
    private Vector3i GetBlockMapPosition(Vector3i targetLocation)
    {
        return targetLocation - m_WorldData.MapBlockOffset;
    }

    /// <summary>
    /// Should only be called when the application quits
    /// </summary>
    /// <param name="chunk"></param>
    public static void DestroyChunk(Chunk chunk)
    {
        GameObject chunkPrefab = chunk.ChunkTransform as GameObject;
        if (chunkPrefab != null)
        {
            Transform transform = (chunkPrefab.transform);
            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            Object.Destroy(meshFilter.mesh);
            meshFilter.mesh = null;
            Object.Destroy(meshFilter);
            Object.Destroy(transform);
            chunk.ChunkTransform = null;
        }
        chunk = null;
    }


    //public void GenerateTerrainMeshData(DateTime start)
    //{
    //    List<Chunk> chunksForMeshDataGeneration = ChunkProcessor.GetChunksForMeshDataGeneration();
    //    if (chunksForMeshDataGeneration.Count == 0)
    //    {
    //        return;
    //    }

    //    Debug.Log(" Total Time: " + (DateTime.Now - start));
    //    m_MeshDataGenerator.GenerateMeshData(chunksForMeshDataGeneration);
    //}

    //public void LightTerrain()
    //{
    //    m_LightProcessor.LightChunks(ChunkProcessor.GetChunksForLighting());
    //}

    //public void DecorateTerrain()
    //{
    //    m_WorldDecorator.Decorate(ChunkProcessor.GetChunksForDecoration());
    //}

    //public void GenerateNewTerrain()
    //{
    //    m_TerrainGenerator.CreateTerrainData(ChunkProcessor.GetChunksForTerrainGeneration());
    //}


    public Block GetBlock(Vector3i blockLocation)
    {
        return m_WorldData.GetBlock(blockLocation.X, blockLocation.Y, blockLocation.Z);
    }

    public void StopProcessing()
    {
        m_ProcessingThread.Abort();
    }

    public void StartProcessingThread()
    {
        m_ProcessingThread = new Thread(ProcessChunks);
        m_ProcessingThread.Start();
    }

    /// <summary>
    /// Convert a block position in the global map position (from Unity space)
    /// to the local map visible to the player.
    /// </summary>
    /// <param name="globalBlockPosition"></param>
    /// <returns></returns>
    public Vector3i GlobalToLocalMapBlockPosition(Vector3i globalBlockPosition)
    {
        return globalBlockPosition - m_WorldData.MapBlockOffset;
    }
}