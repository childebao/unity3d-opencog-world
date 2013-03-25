using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class WorldData
{
    private readonly IChunkProcessor m_ChunkProcessor;
    private int m_ChunksWide = 69;
    private int m_ChunksHigh = 47;
    private int m_ChunksDeep = 1;
	private int m_ChunksWidthOffset = 0;
	private int m_ChunksHeightOffset = 0;
    private int m_ChunkBlockWidth = 16;
    private int m_ChunkBlockHeight = 16;
    private int m_ChunkBlockDepth = 128;
    private int m_NumberOfLightShades = 10;
    private Vector3i m_MapChunkOffset = new Vector3i(0, 0, 0);
	private bool generalCorpus = true;
	private StreamWriter corpusFileWriter ;
	
	private float m_NoiseBlockXOffset = UnityEngine.Random.Range(0,100000);
	
	// If this is true then we expect to have textures for the normals of surfaces
	public bool useNormalMap = false;
	
	// This is the lowest point of the map after generation
	// @warning this is NOT updated later when the characters interacting with the world make blocks disappear.
	private int _floor = 55;
	
	public int floor
	{
        get { return _floor; }
		set { _floor = value; }
    }
	
    private readonly Dictionary<int, BlockUVCoordinates> m_BlockUVCoordinates =
        new Dictionary<int, BlockUVCoordinates>();

    public WorldData(IChunkProcessor chunkProcessor)
    {
        m_ChunkProcessor = chunkProcessor;
        SetShadesOfLight(m_NumberOfLightShades);
		
		// GenerateBlockEntityCorpus
		if (generalCorpus)
			corpusFileWriter = new StreamWriter("ScmCorpus.scm");

    }
	
	public void printOneEntityToCorpus(string entityClass,string entityName)
	{
		if (! generalCorpus)
			return;
		
		lock(corpusFileWriter)
		{
		// add the entity InheritanceLink
		corpusFileWriter.WriteLine("(InheritanceLink (stv 1 0.0012484394) (av 0 1 0)");
		corpusFileWriter.WriteLine("   (BlockEntityNode \"" + entityName + "\" (av 0 1 0))");
		corpusFileWriter.WriteLine("   (ConceptNode \"" + entityClass + "\" (stv 1 0.0012484394))");
		corpusFileWriter.WriteLine("   )");
		corpusFileWriter.WriteLine(")\n");
		}
	}
	
	public void printOneBlockToCorpus(string entityName,
		string blockType, int blockX, int blockY, int blockZ)
	{
		if (! generalCorpus)
			return;

		lock(corpusFileWriter)
		{
		// add material predicate for this block
		corpusFileWriter.WriteLine("(EvaluationLink (stv 1 0.0012484394) (av 0 1 0)");
		corpusFileWriter.WriteLine("   (PredicateNode \"material\" (av 0 1 0))");
		corpusFileWriter.WriteLine("   (ListLink");
		corpusFileWriter.WriteLine("      (StructureNode \"BLOCK_" + blockX + "_" + blockY + "_" + blockZ + "\")");
		corpusFileWriter.WriteLine("      (ConceptNode \"" + blockType + "\")");
		corpusFileWriter.WriteLine("   )");
		corpusFileWriter.WriteLine(")\n");
		
		// add this block part-of this entity
		corpusFileWriter.WriteLine("(EvaluationLink (stv 1 0.0012484394) (av 0 1 0)");
		corpusFileWriter.WriteLine("   (PredicateNode \"Part_Of\" (av 0 1 0))");
		corpusFileWriter.WriteLine("   (ListLink");
		corpusFileWriter.WriteLine("      (StructureNode \"BLOCK_" + blockX + "_" + blockY + "_" + blockZ + "\")");
		corpusFileWriter.WriteLine("      (BlockEntityNode \"" + entityName + "\" (av 1000 0 0))");
		corpusFileWriter.WriteLine("   )");
		corpusFileWriter.WriteLine(")\n");
		}

	}
	
	public void finishGenerateCorpus()
	{
		if (generalCorpus)
			corpusFileWriter.Close();
	}
	
    public void GenerateUVCoordinatesForAllBlocks()
    {
        // Topsoil
        SetBlockUVCoordinates(BlockType.TopSoil, 0, 5, 1);
        SetBlockUVCoordinates(BlockType.Dirt, 1,1,1);
        SetBlockUVCoordinates(BlockType.Light, 2, 2, 2);
        SetBlockUVCoordinates(BlockType.Lava, 3,3,3);
		SetBlockUVCoordinates(BlockType.Leaves, 4,4,4);
        SetBlockUVCoordinates(BlockType.Stone, 6,6,6);
		SetBlockUVCoordinates(BlockType.WoodenDoor, 7,7,7);
		SetBlockUVCoordinates(BlockType.IronDoor, 8,8,8);
		SetBlockUVCoordinates(BlockType.Trapdoor, 9,9,9);
		SetBlockUVCoordinates(BlockType.Cobblestone, 10, 10, 10);
		SetBlockUVCoordinates(BlockType.WoodenPlanks, 11, 11, 11);
		SetBlockUVCoordinates(BlockType.Saplings, 1, 1, 1);
		SetBlockUVCoordinates(BlockType.Bedrock, 6,6,6);
		SetBlockUVCoordinates(BlockType.Water, 12,12,12);
		SetBlockUVCoordinates(BlockType.StationaryWater, 12,12,12);
		SetBlockUVCoordinates(BlockType.StationaryLava, 3,3,3);
		SetBlockUVCoordinates(BlockType.Sand, 13,13,13);
		SetBlockUVCoordinates(BlockType.Gravel, 14,14,14);
		SetBlockUVCoordinates(BlockType.GoldOre, 15,15,15);
		SetBlockUVCoordinates(BlockType.IronOre, 16,16,16);
		SetBlockUVCoordinates(BlockType.CoalOre, 17,17,17);
		SetBlockUVCoordinates(BlockType.Wood, 18,18,18);
		SetBlockUVCoordinates(BlockType.Glass, 19,19,19);
		SetBlockUVCoordinates(BlockType.LapisLazuliBlock, 6,6,6);
		SetBlockUVCoordinates(BlockType.Sandstone, 6,6,6);
		SetBlockUVCoordinates(BlockType.Bed, 20,11,11);
		SetBlockUVCoordinates(BlockType.Cobweb, 21,21,21);
		SetBlockUVCoordinates(BlockType.TallGrass, 0,22,0);
		SetBlockUVCoordinates(BlockType.DeadBrush, 0,22,0);
		SetBlockUVCoordinates(BlockType.Piston, 8,11,8);
		SetBlockUVCoordinates(BlockType.Dandelion, 0,23,23);
		SetBlockUVCoordinates(BlockType.Rose, 0,24,24);
		SetBlockUVCoordinates(BlockType.BrownMushroom, 0,25,25);
		SetBlockUVCoordinates(BlockType.RedMushroom, 0,26,26);
		SetBlockUVCoordinates(BlockType.BlockOfGold, 27,27,27);
		SetBlockUVCoordinates(BlockType.BlockOfIron, 28,28,28);
		SetBlockUVCoordinates(BlockType.Slabs, 29,29,29);
		SetBlockUVCoordinates(BlockType.Bricks, 30,30,30);
		SetBlockUVCoordinates(BlockType.TNT, 31,32,33);
		SetBlockUVCoordinates(BlockType.Bookshelf, 11,34,11);
		SetBlockUVCoordinates(BlockType.MossStone, 6,6,6);
		SetBlockUVCoordinates(BlockType.Obsidian, 6,6,6);
		SetBlockUVCoordinates(BlockType.Torch, 36,35,11);
		SetBlockUVCoordinates(BlockType.Fire, 36,35,11);
		SetBlockUVCoordinates(BlockType.WoodenStairs, 18,18,18);
		SetBlockUVCoordinates(BlockType.Chest, 37,38,37);
		SetBlockUVCoordinates(BlockType.DiamondOre, 39,39,39);
		SetBlockUVCoordinates(BlockType.BlockOfDiamond, 40,40,40);
		SetBlockUVCoordinates(BlockType.CraftingTable, 34,38,37);
		SetBlockUVCoordinates(BlockType.Farmland, 41,1,1);
		SetBlockUVCoordinates(BlockType.Furnace, 42,43,42);
		SetBlockUVCoordinates(BlockType.BurningFurnace, 42,43,42);
		SetBlockUVCoordinates(BlockType.Ladders, 37,44,37);
		SetBlockUVCoordinates(BlockType.Rails, 45,8,45);
		SetBlockUVCoordinates(BlockType.CobblestoneStairs, 42,10,42);
		SetBlockUVCoordinates(BlockType.Lever, 37,46,37);
		SetBlockUVCoordinates(BlockType.Snow, 47,48,1);
		SetBlockUVCoordinates(BlockType.Ice, 49,49,49);
		SetBlockUVCoordinates(BlockType.SnowBlock, 47,47,47);
		SetBlockUVCoordinates(BlockType.ClayBlock, 50,50,50);
		SetBlockUVCoordinates(BlockType.Fence, 51,51,51);
		SetBlockUVCoordinates(BlockType.Portal, 52,52,52);
		SetBlockUVCoordinates(BlockType.LockedChest, 37,38,37);
		SetBlockUVCoordinates(BlockType.StoneBricks, 53,53,53);
		SetBlockUVCoordinates(BlockType.IronBars, 51,51,51);
		SetBlockUVCoordinates(BlockType.GlassPane, 19,19,19);
		SetBlockUVCoordinates(BlockType.FenceGate, 51,51,51);
		SetBlockUVCoordinates(BlockType.BrickStairs, 30,30,30);
		SetBlockUVCoordinates(BlockType.StoneBrickStairs, 53,53,53);
		SetBlockUVCoordinates(BlockType.Cauldron, 42,43,42);
		SetBlockUVCoordinates(BlockType.EndPortal, 52,52,52);
		SetBlockUVCoordinates(BlockType.Wheel, 52, 52, 52);
    }

    /// <summary>
    /// Sets the UV coordinate rectangles for the top texture, side texture, and bottom texture
    /// given the index of each in the list of textures assigned to the World gameobject in the hierarchy.
    /// </summary>
    /// <param name="blockType"></param>
    /// <param name="topIndex"></param>
    /// <param name="sideIndex"></param>
    /// <param name="bottomIndex"></param>
    private void SetBlockUVCoordinates(BlockType blockType, int topIndex, int sideIndex, int bottomIndex)
    {
		// Do we need to also set UV coordinates for normals? We shouldn't because they should be the same...
        BlockUvCoordinates.Add((int) (blockType),
                               new BlockUVCoordinates(WorldTextureAtlasUvs[topIndex], WorldTextureAtlasUvs[sideIndex],
                                                      WorldTextureAtlasUvs[bottomIndex]));
    }

    public int ChunksWide
    {
        get { return m_ChunksWide; }
        set { m_ChunksWide = value; }
    }

    public int ChunksHigh
    {
        get { return m_ChunksHigh; }
        set { m_ChunksHigh = value; }
    }

    public int ChunksDeep
    {
        get { return m_ChunksDeep; }
        set { m_ChunksDeep = value; }
    }
	
	public int ChunksWidthOffset
	{
		get {return m_ChunksWidthOffset;}
		set { m_ChunksWidthOffset = value; }
	}
	
	public int ChunksHeightOffset
	{
		get {return m_ChunksHeightOffset;}
		set { m_ChunksHeightOffset = value; }
	}

    public int ChunkBlockWidth
    {
        get { return m_ChunkBlockWidth; }
        set { m_ChunkBlockWidth = value; }
    }


    public int ChunkBlockHeight
    {
        get { return m_ChunkBlockHeight; }
        set { m_ChunkBlockHeight = value; }
    }

    /// <summary>
    /// How many blocks deep a chunk is
    /// </summary>
    public int ChunkBlockDepth
    {
        get { return m_ChunkBlockDepth; }
        set { m_ChunkBlockDepth = value; }
    }

    public int NumberOfLightShades
    {
        get { return m_NumberOfLightShades; }
        set { m_NumberOfLightShades = value; }
    }

    public void SetShadesOfLight(int numberOfShadesOfLight)
    {
        byte[] shadesOfLight = new byte[numberOfShadesOfLight];
        m_NextLowerShadesOfLight = new byte[numberOfShadesOfLight];
        byte lightReduction = (byte) (255 / numberOfShadesOfLight);
        byte shadeOfLight = 255;
        for (int i = 0; i < numberOfShadesOfLight; i++)
        {
            shadesOfLight[i] = shadeOfLight;
            if (i > 0)
            {
                m_NextLowerShadesOfLight[i - 1] = shadeOfLight;
            }
            shadeOfLight -= lightReduction;
        }
        m_NumberOfLightShades = numberOfShadesOfLight;

        m_ShadesOfLight = shadesOfLight;
    }


    public byte[] NextLowerShadesOfLight
    {
        get { return m_NextLowerShadesOfLight; }
    }

    private byte[] m_ShadesOfLight;
    private byte[] m_NextLowerShadesOfLight;


    public byte[] ShadesOfLight
    {
        get { return m_ShadesOfLight; }
    }

    public int WidthInBlocks
    {
        get { return m_ChunksWide * m_ChunkBlockWidth; }
    }

    public int HeightInBlocks
    {
        get { return m_ChunksHigh * m_ChunkBlockHeight; }
    }

    public int DepthInBlocks
    {
        get { return m_ChunksDeep * m_ChunkBlockDepth; }
    }

    public int BottomChunkBorderRow
	{
		get { return m_ChunksHeightOffset; }
	}
	
    public int LeftChunkBorderColumn
	{
		get { return m_ChunksWidthOffset; }
	}

    public int BottomVisibleChunkRow
	{
		get { return BottomChunkBorderRow + 1; }
	}
	
    public int LeftVisibleChunkColumn 
	{
		get { return LeftChunkBorderColumn + 1; }
	}

    public int TopChunkBorderRow
    {
        get { return m_ChunksHigh - 1; }
    }

    public int RightChunkBorderColumn
    {
        get { return m_ChunksWide - 1; }
    }

    public int TopVisibleChunkRow
    {
        get { return m_ChunksHigh - 2; }
    }

    public int RightVisibleChunkColumn
    {
        get { return ChunksWide - 2; }
    }
	
	public Bounds bounds
    {
        get {
			Bounds b = new Bounds();
			Vector3 min = new Vector3(LeftVisibleChunkColumn * m_ChunkBlockWidth,0,BottomVisibleChunkRow * m_ChunkBlockHeight);
			Vector3 max = new Vector3(RightChunkBorderColumn * m_ChunkBlockWidth,
			                          m_ChunksDeep*m_ChunkBlockDepth,
			                          TopChunkBorderRow * m_ChunkBlockHeight);
			b.SetMinMax(min, max);
			return b;
		}
    }

    public int TotalChunks
    {
        get { return m_ChunksWide * m_ChunksHigh; }
    }

    public int CenterChunkX
    {
        get { return m_ChunksWide / 2; }
    }

    public int CenterChunkY
    {
        get { return m_ChunksHigh / 2; }
    }

    public void SetDimensions(int chunksWide, int chunksHigh, int chunksDeep, int chunkBlockWidth, int chunkBlockHeight,
                              int chunkBlockDepth)
    {
        ChunksWide = chunksWide;
        ChunksHigh = chunksHigh;
        ChunksDeep = chunksDeep;
        ChunkBlockWidth = chunkBlockWidth;
        ChunkBlockHeight = chunkBlockHeight;
        ChunkBlockDepth = chunkBlockDepth;
        InitializeGridChunks();
    }

    // Our horizontal movement offset for noise sampling

    private float m_GlobalXOffset;
    private float m_GlobalZOffset;
    public Chunk[,,] Chunks;

    public void InitializeGridChunks()
    {
//		Debug.Log("In InitializeGridChunks...");
		//@TODO: Dude, standardize the Depth vs. Height!!!!
        Chunks = new Chunk[m_ChunksWide, m_ChunksHigh, m_ChunksDeep];
        m_ChunkProcessor.ChunksAreBeingAdded = true;
        List<Chunk> newChunksToProcess = new List<Chunk>();
        // Add all world chunks to the batch for processing
        for (int x = LeftChunkBorderColumn; x <= RightChunkBorderColumn; x++)
        {
            for (int y = BottomChunkBorderRow; y <= TopChunkBorderRow; y++)
            {
                for (int z = 0; z < m_ChunksDeep; z++)
                {
                    Chunks[x, y, z] = new Chunk(x, y, z, this);
                    Chunks[x, y, z].InitializeBlocks();
					
					//if( (x > m_ChunksWide / 2 - 1) && (x < m_ChunksWide / 2 + 1) )
					{
	                    newChunksToProcess.Add(Chunks[x, y, z]);
	                    m_ChunkProcessor.AddChunkToTerrainQueue(Chunks[x, y, z]);
					}
                }
            }
        }
		Debug.Log("In WorldData, InitializeGridChunks: size of newChunksToProcess: " + newChunksToProcess.Count);
        m_ChunkProcessor.AddBatchOfChunks(newChunksToProcess, BatchType.TerrainGeneration);
        m_ChunkProcessor.AddBatchOfChunks(newChunksToProcess, BatchType.Decoration);
        m_ChunkProcessor.ChunksAreBeingAdded = false;
    }

    public Block GetBlock(int x, int y, int z)
    {
        int chunkX = x / ChunkBlockWidth;
        int chunkY = y / ChunkBlockHeight;
        int chunkZ = z / ChunkBlockDepth;
        int blockX = x % ChunkBlockWidth;
        int blockY = y % ChunkBlockHeight;
        int blockZ = z % ChunkBlockDepth;

        return Chunks[chunkX, chunkY, chunkZ].GetBlock(blockX, blockY, blockZ);
    }
	
	public bool DoesBlockExist(int x, int y, int z)
	{
        int chunkX = x / ChunkBlockWidth;
        int chunkY = y / ChunkBlockHeight;
        int chunkZ = z / ChunkBlockDepth;
        int blockX = x % ChunkBlockWidth;
        int blockY = y % ChunkBlockHeight;
        int blockZ = z % ChunkBlockDepth;
		
		bool xBounds = false;
		bool yBounds = false;
		bool zBounds = false;
		
		xBounds = chunkX >= 0 && chunkX < Chunks.GetLength(0);
		yBounds = chunkY >= 0 && chunkY < Chunks.GetLength(1);
		zBounds = chunkZ >= 0 && chunkZ < Chunks.GetLength(2);
		
		if(xBounds && yBounds && zBounds) xBounds = blockX >= 0 && blockX < Chunks[chunkX, chunkY, chunkZ].Blocks.GetLength(0);
		if(xBounds && yBounds && zBounds) yBounds = blockY >= 0 && blockY < Chunks[chunkX, chunkY, chunkZ].Blocks.GetLength(1);
		if(xBounds && yBounds && zBounds) zBounds = blockZ >= 0 && blockZ < Chunks[chunkX, chunkY, chunkZ].Blocks.GetLength(2);
		
		return xBounds && yBounds && zBounds;
	}

    public void SetBlockTypeWithRegeneration(int x, int y, int z, BlockType blockType)
    {
        int chunkX = x / ChunkBlockWidth;
        int chunkY = y / ChunkBlockHeight;
        int chunkZ = z / ChunkBlockDepth;
        int blockX = x % ChunkBlockWidth;
        int blockY = y % ChunkBlockHeight;
        int blockZ = z % ChunkBlockDepth;
		
        Chunks[chunkX, chunkY, 0].SetBlockType(blockX, blockY, blockZ, blockType);
        Chunks[chunkX, chunkY, chunkZ].NeedsRegeneration = true;

        // If we change a block on the border of a chunk, the adjacent chunk
        // needs to be regenerated also.
        if (DoesBlockExist(blockX + 1, blockY, blockZ) && blockX == ChunkBlockWidth - 1)
        {
            Chunks[chunkX + 1, chunkY, chunkZ].NeedsRegeneration = true;
        }
        else if (DoesBlockExist(blockX - 1, blockY, blockZ) && blockX == 0)
        {
            Chunks[chunkX - 1, chunkY, chunkZ].NeedsRegeneration = true;
        }

        if (DoesBlockExist(blockX, blockY - 1, blockZ) && blockY == 0)
        {
            Chunks[chunkX, chunkY - 1, chunkZ].NeedsRegeneration = true;
        }
        else if (DoesBlockExist(blockX, blockY + 1, blockZ) && blockY == ChunkBlockHeight - 1)
        {
            Chunks[chunkX, chunkY + 1, chunkZ].NeedsRegeneration = true;
        }
    }

    /// <summary>
    /// Sets a block value, given the WORLD (global) map coordinates and type.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="blockType"></param>
    public void SetBlockType(int x, int y, int z, BlockType blockType)
    {
        int localX = (x - m_MapChunkOffset.X);
        int localY = (y - m_MapChunkOffset.Y);
        int localZ = (z - m_MapChunkOffset.Z);

        int chunkX = localX / ChunkBlockWidth;
        int chunkY = localY / ChunkBlockHeight;
        int chunkZ = localZ / ChunkBlockDepth;
        int blockX = localX % ChunkBlockWidth;
        int blockY = localY % ChunkBlockHeight;
        int blockZ = localZ % ChunkBlockDepth;

        Chunks[chunkX, chunkY, 0].SetBlockType(blockX, blockY, blockZ,blockType);
    }

    public void SetBlockLightWithRegeneration(int x, int y, int z, byte lightAmount)
    {
        int chunkX = x / ChunkBlockWidth;
        int chunkY = y / ChunkBlockHeight;
        int chunkZ = z / ChunkBlockDepth;
        int blockX = x % ChunkBlockWidth;
        int blockY = y % ChunkBlockHeight;
        int blockZ = z % ChunkBlockDepth;

        Chunks[chunkX, chunkY, chunkZ].SetBlockType(blockX, blockY, blockZ,BlockType.Air);
        Chunks[chunkX, chunkY, chunkZ].SetBlockLight(blockX, blockY, blockZ, lightAmount);
        Chunks[chunkX, chunkY, chunkZ].NeedsRegeneration = true;
    }

    public byte GetBlockLight(int x, int y, int z)
    {
        int chunkX = x / ChunkBlockWidth;
        int chunkY = y / ChunkBlockHeight;
        int chunkZ = z / ChunkBlockDepth;
        int blockX = x % ChunkBlockWidth;
        int blockY = y % ChunkBlockHeight;
        int blockZ = z % ChunkBlockDepth;

        return Chunks[chunkX, chunkY, 0].GetBlock(blockX, blockY, blockZ).LightAmount;
    }

    public List<Chunk> ChunksNeedingRegeneration
    {
        get
        {
            List<Chunk> chunks = new List<Chunk>();
            // Add all world chunks to the batch for processing
            for (int x = LeftChunkBorderColumn; x <= RightChunkBorderColumn; x++)
            {
                for (int y = BottomChunkBorderRow; y <= TopChunkBorderRow; y++)
                {
                    for (int z = 0; z < m_ChunksDeep; z++)
                    {
                        if (Chunks[x, y, z].NeedsRegeneration)
                        {
                            chunks.Add(Chunks[x, y, z]);
                        }
                    }
                }
            }
            return chunks;
        }
    }

    //private readonly TQueue<Chunk> m_FinishedChunks = new TQueue<Chunk>();


    public Rect[] WorldTextureAtlasUvs { get; set; }

    public Dictionary<int, BlockUVCoordinates> BlockUvCoordinates
    {
        get { return m_BlockUVCoordinates; }
    }

    public Vector3i MapBlockOffset { get; set; }

    public Vector3i MapChunkOffset
    {
        get { return m_MapChunkOffset; }
        set
        {
            m_MapChunkOffset = value;
            MapBlockOffset = new Vector3i(m_MapChunkOffset.X * m_ChunkBlockWidth,
                                          m_MapChunkOffset.Y * m_ChunkBlockHeight, m_MapChunkOffset.Z * ChunkBlockDepth);
        }
    }

    public bool WorldIsReady { get; set; }

    public List<Chunk> VisibleChunks
    {
        get
        {
            List<Chunk> chunks = new List<Chunk>();
            for (int x = LeftChunkBorderColumn; x <= RightChunkBorderColumn; x++)
            {
                for (int y = BottomChunkBorderRow; y <= TopChunkBorderRow; y++)
                {
                    for (int z = 0; z < m_ChunksDeep; z++)
                    {
                        chunks.Add(Chunks[x, y, z]);
                    }
                }
            }
            return chunks;
        }
    }

    public List<Chunk> GetChunksNeedingRegeneration()
    {
        List<Chunk> chunks = new List<Chunk>();
        for (int x = LeftChunkBorderColumn; x <= RightChunkBorderColumn; x++)
        {
            for (int y = BottomChunkBorderRow; y <= TopChunkBorderRow; y++)
            {
                for (int z = 0; z < m_ChunksDeep; z++)
                {
                    Chunk chunk = Chunks[x, y, z];
                    if (chunk.NeedsRegeneration)
                    {
                        chunk.NeedsRegeneration = false;
                        chunks.Add(chunk);
                    }
                }
            }
        }
        return chunks;
    }
	
	public void AddFinishedChunk(Chunk chunk)
    {
        FinishedChunks.Enqueue(chunk);
    }

    public TQueue<Chunk> FinishedChunks = new TQueue<Chunk>();


    public Chunk GetFinishedChunk()
    {
        if (FinishedChunks.Count == 0)
        {
            return null;
        }

        return FinishedChunks.Dequeue();
    }
	
	string mapDir = FileTerrainMethod.mapDir; //"Assets/Maps";
	string mapExt = FileTerrainMethod.mapExt; //".blockmap";
	
	public bool saveData(string filename)
	{
		
		System.IO.Directory.CreateDirectory(mapDir);
		filename = System.IO.Path.Combine(mapDir, filename) + mapExt;
		using (StreamWriter sw = new StreamWriter(filename))
		{
			// Save the metadata.
			sw.WriteLine("Chunks wide:" + ChunksWide);
			sw.WriteLine("Chunks high:" + ChunksHigh);
			sw.WriteLine("Chunks deep:" + ChunksDeep);
			
			sw.WriteLine("Chunk width:" + ChunkBlockWidth);
			sw.WriteLine("Chunk height:" + ChunkBlockHeight);
			sw.WriteLine("Chunk depth:" + ChunkBlockDepth);
			// Then save all the chunks.
	        for (int x = LeftChunkBorderColumn; x <= RightChunkBorderColumn; x++)
	        {
	            for (int y = BottomChunkBorderRow; y <= TopChunkBorderRow; y++)
	            {
	                for (int z = 0; z < m_ChunksDeep; z++)
	                {
	                    Chunks[x, y, z].saveData(sw);
	                }
	            }
	        }
		}
		return true;
	}
	
}
