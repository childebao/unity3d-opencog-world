using System.Collections.Generic;

public class WorldMover
{
    private readonly WorldData m_WorldData;
    private readonly TerrainGenerator m_TerrainGenerator;
    private readonly MeshGenerator m_MeshGenerator;
    private BatchProcessor<Chunk> m_BatchProcessor = new BatchProcessor<Chunk>();
    public WorldMover(WorldData worldData, TerrainGenerator terrainGenerator, MeshGenerator meshGenerator)
    {
        m_WorldData = worldData;
        m_TerrainGenerator = terrainGenerator;
        m_MeshGenerator = meshGenerator;
    }

    /// <summary>
    /// Increment meaning the direction the map will be shifted
    /// </summary>
    /// <param name="xIncrement"></param>
    /// <param name="yIncrement"></param>
    public void ShiftAllWorldChunks(int xIncrement, int yIncrement)
    {
        List<Chunk> chunksNeedingTerrainGen = new List<Chunk>();
        List<Chunk> chunksNeedingToBeDestroyed = new List<Chunk>();
        List<Chunk> chunksNeedingMeshGeneration = new List<Chunk>();

        for (int x = m_WorldData.LeftChunkBorderColumn; x <= m_WorldData.RightChunkBorderColumn; x++)
        {
            for (int y = m_WorldData.BottomChunkBorderRow; y <= m_WorldData.TopChunkBorderRow; y++)
            {
                //Chunk.Move(m_World, x + xIncrement, y + yIncrement, x, y, chunksNeedingTerrainGen,
                //           chunksNeedingToBeDestroyed,
                //           chunksNeedingMeshGeneration);
            }
        }

        // Now actually replace the original chunks with the shifted chunks
		//Debug.LogError("In ShiftAllWorldChunks");
        for (int x = m_WorldData.LeftChunkBorderColumn; x <= m_WorldData.RightChunkBorderColumn; x++)
        {
            for (int y = m_WorldData.BottomChunkBorderRow; y <= m_WorldData.TopChunkBorderRow; y++)
            {
                if (m_WorldData.Chunks[x, y, 0].ReplacementChunk != null)
                {
                    m_WorldData.Chunks[x, y, 0] = m_WorldData.Chunks[x, y, 0].ReplacementChunk;
                    m_WorldData.Chunks[x, y, 0].ArrayX = x;
                    m_WorldData.Chunks[x, y, 0].ArrayY = y;
                }
            }
        }

        foreach (Chunk chunk in chunksNeedingToBeDestroyed)
        {
            World.DestroyChunk(chunk);
        }

        m_BatchProcessor.Process(chunksNeedingTerrainGen, m_TerrainGenerator.GenerateTerrain, true );
        m_BatchProcessor.Process(chunksNeedingMeshGeneration, m_MeshGenerator.GenerateMesh, true );
    }
}