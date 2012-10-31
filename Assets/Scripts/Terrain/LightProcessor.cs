using System;
using System.Collections.Generic;
using UnityEngine;

public interface ILightProcessor
{
    void LightChunks(List<Chunk> allVisibleChunks);
    void SetLightingAroundBlock(int x, int y, int z, int lightIndex);
    void RecalculateLightingAroundBlock(int x, int y, int z);

    /// <summary>
    /// Identifies all sunlit blocks, then lights all surrounding blocks
    /// </summary>
    /// <param name="chunk"></param>
    void LightChunk(Chunk chunk);
}

public class LightProcessor : ILightProcessor
{
    private readonly IBatchProcessor<Chunk> m_BatchProcessor;
    private readonly WorldData m_WorldData;
    //private readonly ChunkProcessor m_ChunkProcessor;

    public LightProcessor(IBatchProcessor<Chunk> batchProcessor, WorldData worldData, ChunkProcessor chunkProcessor)
    {
        m_BatchProcessor = batchProcessor;
        m_WorldData = worldData;
        //m_ChunkProcessor = chunkProcessor;
    }


    public void LightChunks(List<Chunk> chunks)
    {
        m_BatchProcessor.Process(chunks, LightChunk, true);
    }


    /// <summary>
    /// Identifies all sunlit blocks, then lights all surrounding blocks
    /// </summary>
    /// <param name="chunk"></param>
    public void LightChunk(Chunk chunk)
    {
		//Debug.Log("In LightChunk...");
        if (chunk.IsOnTheBorder)
        {
            return;
        }

        LightSunlitBlocksInChunk(chunk);

        byte sunlight = Sunlight();
        int chunkWorldX = chunk.ArrayX * m_WorldData.ChunkBlockWidth;
        int chunkWorldY = chunk.ArrayY * m_WorldData.ChunkBlockHeight;

        for (int x = 0; x < m_WorldData.ChunkBlockWidth; x++)
        {
            int blockX = chunkWorldX + x;
            for (int y = 0; y < m_WorldData.ChunkBlockHeight; y++)
            {
                int blockY = chunkWorldY + y;

                for (int z = m_WorldData.ChunkBlockDepth - 1; z >= 0; z--)
                {
                    Block block = chunk.GetBlock(x, y, z);
                    // Only light blocks that surround a sunlit block (for now)
                    if (block.Type == BlockType.Air && block.LightAmount == sunlight)
                    {
                        SetLightingAroundBlock(blockX, blockY, z, 1);
                    }
                }
            }
        }
    }

    private byte Sunlight()
    {
        return m_WorldData.ShadesOfLight[0];
    }

    /// <summary>
    /// This pass marks all blocks as sunlit that are open to sunlight directly above.
    /// </summary>
    /// <param name="chunk"></param>
    private void LightSunlitBlocksInChunk(Chunk chunk)
    {
        byte sunlight = m_WorldData.ShadesOfLight[0];
        for (int x = 0; x < m_WorldData.ChunkBlockWidth; x++)
        {
            for (int y = 0; y < m_WorldData.ChunkBlockHeight; y++)
            {
                for (int z = m_WorldData.ChunkBlockDepth -1; z >=0; z--)
                {
                    // Starting at the top of the chunk, work our way down marking
                    // blocks as sunlit until we hit the bottom, or find a block.
                    Block block = chunk.GetBlock(x, y, z);
                    if (block.Type != BlockType.Air)
                    {
                        break;
                    }
                    chunk.SetBlockLight(x, y, z, sunlight);
                }
            }
        }
    }

    private int LightIndexOf(byte shadeOfLight)
    {
        for (int i = 0; i < m_WorldData.ShadesOfLight.Length; i++)
        {
            if (m_WorldData.ShadesOfLight[i] == shadeOfLight)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// When a block in a chunk is added or removed, we have to recalculate
    /// lighting around that block.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void RecalculateLightingAroundBlock(int x, int y, int z)
    {
        SetLightingAroundBlockRecursively(x - 1, y, z);
        SetLightingAroundBlockRecursively(x + 1, y, z);
        SetLightingAroundBlockRecursively(x, y + 1, z);
        SetLightingAroundBlockRecursively(x, y - 1, z);
        SetLightingAroundBlockRecursively(x, y, z + 1);
        SetLightingAroundBlockRecursively(x, y, z - 1);
    }

    private void SetLightingAroundBlockRecursively(int x, int y, int z)
    {
        byte currentShade = m_WorldData.GetBlockLight(x, y, z);
        if (currentShade == 0)
        {
            return;
        }

        int shadeIndex = LightIndexOf(currentShade);
        if (shadeIndex == m_WorldData.NumberOfLightShades - 1)
        {
            return;
        }

        SetLightingAroundBlock(x, y, z, shadeIndex + 1);
    }

    /// <summary>
    /// Lights the 6 blocks around the block, recursively
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="lightIndex"></param>
    public void SetLightingAroundBlock(int x, int y, int z, int lightIndex)
    {
        SetLightingAroundBlockRecursively(x - 1, y, z, lightIndex);
        SetLightingAroundBlockRecursively(x + 1, y, z, lightIndex);
        SetLightingAroundBlockRecursively(x, y + 1, z, lightIndex);
        SetLightingAroundBlockRecursively(x, y - 1, z, lightIndex);
        SetLightingAroundBlockRecursively(x, y, z + 1, lightIndex);
        SetLightingAroundBlockRecursively(x, y, z - 1, lightIndex);
    }

    private void SetLightingAroundBlockRecursively(int x, int y, int z, int lightIndex)
    {
        // The only reason we should need this check is if we have a lot of shades of light.
        // For example, if chunks are 32 blocks wide, and we have 40 shades of light,
        // lighting a chunk beside a border chunk could easily extend across the border chunk and 
        // out of the world bounds.
        if (x < 0 || y < 0 || x >= m_WorldData.WidthInBlocks || y >= m_WorldData.HeightInBlocks ||
            z >= m_WorldData.DepthInBlocks ||
            z < 0)
        {
            return;
        }

        int chunkX = x / m_WorldData.ChunkBlockWidth;
        int chunkY = y / m_WorldData.ChunkBlockHeight;
        int chunkZ = z / m_WorldData.ChunkBlockDepth;
        int blockX = x % m_WorldData.ChunkBlockWidth;
        int blockY = y % m_WorldData.ChunkBlockHeight;
        int blockZ = z % m_WorldData.ChunkBlockDepth;
        Chunk chunk = m_WorldData.Chunks[chunkX, chunkY, chunkZ];
        Block block = chunk.GetBlock(blockX, blockY, blockZ);

        // Solid blocks don't get lit
        if (block.Type != BlockType.Air)
        {
            return;
        }

        byte lightAmount = m_WorldData.ShadesOfLight[lightIndex];

        // If it's already as bright or brighter than the shade we are working on, leave,
        // the lighting here is done.
        if (block.LightAmount >= lightAmount)
        {
            return;
        }

        // Set the new block light amount
        chunk.SetBlockLight(blockX, blockY, blockZ, lightAmount);

        // This chunk needs to be relit and redrawn now.
        if (!chunk.NeedsRegeneration)
        {
            chunk.NeedsRegeneration = true;
            //Debug.Log(chunk + " needs regen.");
        }

        // The next block will be drawn slightly darker, unless it would have no light at all.
        int nextLightIndex = lightIndex + 1;
        if (nextLightIndex == m_WorldData.NumberOfLightShades)
        {
            return;
        }

        SetLightingAroundBlockRecursively(x - 1, y, z, nextLightIndex);
        SetLightingAroundBlockRecursively(x + 1, y, z, nextLightIndex);
        SetLightingAroundBlockRecursively(x, y + 1, z, nextLightIndex);
        SetLightingAroundBlockRecursively(x, y - 1, z, nextLightIndex);
        SetLightingAroundBlockRecursively(x, y, z + 1, nextLightIndex);
        SetLightingAroundBlockRecursively(x, y, z - 1, nextLightIndex);
        
    }
}