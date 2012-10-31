using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using System.IO;

[Serializable]
public class Chunk
{
    private Block[,,] m_Blocks;
    //private Block[] m_Blocks;

    // Chunk location in the chunk array
    private int m_ArrayX;
    private int m_ArrayY;
    private int m_ArrayZ;
    public readonly WorldData m_WorldData;

    private List<int> m_Indices = new List<int>();
    private List<Vector2> m_Uvs = new List<Vector2>();
    private List<Vector3> m_Vertices = new List<Vector3>();
    private List<Color> m_Colors = new List<Color>();
    private static int m_Id;

    // Location in Unity space
    private Vector3i m_Position;
    private Object m_ChunkTransform;
    private TQueue<GameObjectCreationData> m_GameObjectCreationQueue = new TQueue<GameObjectCreationData>();

    /// <summary>
    /// Some decorations only consider topsoil. Let's cache these, for quicker evaluation.
    /// </summary>
    public readonly List<Vector3i> TopSoilBlocks = new List<Vector3i>();


    public Chunk(int arrayX, int arrayY, int arrayZ, WorldData worldData)
    {
		if(worldData == null)
			Debug.Log("WorldData is Null in Chunk Constructor!");
        m_WorldData = worldData;
        ArrayX = arrayX;
        ArrayY = arrayY;
        ArrayZ = arrayZ;
        m_Id++;
        Id = m_Id;
    }

    private int Id { get; set; }

    public void InitializeBlocks()
    {
		//Debug.Log("InitializeBlocks");
        m_Blocks = new Block[WorldData.ChunkBlockWidth, WorldData.ChunkBlockHeight, WorldData.ChunkBlockDepth];
        //m_Blocks = new Block[(WorldData.ChunkBlockWidth 
        //    + 1) * (WorldData.ChunkBlockHeight + 1) * (WorldData.ChunkBlockDepth + 1)];
    }

    static Chunk()
    {
        WorldChunkYOffset = 0;
    }

    public static Chunk CreateChunk(Vector3i location, WorldData worldData)
    {
        Chunk chunk = new Chunk(location.X, location.Y, location.Z, worldData);
        worldData.Chunks[location.X, location.Y, location.Z] = chunk;
        chunk.InitializeBlocks();
        return chunk;
    }

    public void Initialize(Vector3i location)
    {
        ArrayX = location.X;
        ArrayY = location.Y;
        ArrayZ = location.Z;
        Id++;
        WorldData.Chunks[location.X, location.Y, location.Z] = this;
        InitializeBlocks();
    }

    public int ArrayX
    {
        get { return m_ArrayX; }
        set
        {
            m_ArrayX = value;
            m_Position.X = value * WorldData.ChunkBlockWidth + WorldData.MapChunkOffset.X * WorldData.ChunkBlockWidth;
        }
    }

    public int ArrayY
    {
        get { return m_ArrayY; }
        set
        {
            m_ArrayY = value;
            m_Position.Y = value * WorldData.ChunkBlockHeight + WorldData.MapChunkOffset.Y * WorldData.ChunkBlockHeight;
        }
    }

    public int ArrayZ
    {
        get { return m_ArrayZ; }
        set
        {
            m_ArrayZ = value;
            m_Position.Z = value + (WorldData.MapChunkOffset.Z * WorldData.ChunkBlockDepth);
        }
    }

    public Vector3i WorldPosition
    {
        get { return new Vector3i(m_ArrayX + WorldData.MapChunkOffset.X, m_ArrayY + WorldData.MapChunkOffset.Y, m_ArrayZ); }
    }

    //public Block[,,] Blocks
    //{
    //    get { return m_Blocks; }
    //    set { m_Blocks = value; }
    //}

    public Block GetBlock(int x, int y, int z)
    {
//		if(m_WorldData == null)
//			Debug.Log("WorldData is Null in GetBlock!");
		
//        int index0 = z * m_WorldData.ChunkBlockWidth * m_WorldData.ChunkBlockHeight + y * m_WorldData.ChunkBlockHeight + x;
//        return
//            m_Blocks[
//                index0];
        //x * m_WorldData.ChunkBlockWidth + y * m_WorldData.ChunkBlockHeight + z * m_WorldData.ChunkBlockDepth];
		
		Block ret = new Block();
		
		try 
		{
			ret = m_Blocks[x,y,z];
			
		} 
		catch (Exception ex) 
		{
			Debug.LogException(ex);
			Debug.LogError("x(max): " + x + "(" + m_Blocks.GetUpperBound(0) + ") y(max): " + y + "(" + m_Blocks.GetUpperBound(1) + ") z(max): " + z + "(" + m_Blocks.GetUpperBound(2) + ")");
		}
		
		return ret;
    }

    public void SetBlockLight(int x, int y, int z, byte lightAmount)
    {
//		if(m_WorldData == null)
//			Debug.Log("WorldData is Null in SetBlockLight!");
		
//        m_Blocks[
//           z * m_WorldData.ChunkBlockWidth * m_WorldData.ChunkBlockHeight + y * m_WorldData.ChunkBlockHeight + x].
//            LightAmount = lightAmount;
		m_Blocks[x,y,z].LightAmount = lightAmount;
    }

    public void SetBlockType(int x, int y, int z, BlockType blockType)
    {
//		if(m_WorldData == null)
//			Debug.Log("WorldData is Null in SetBlockType!");
			
//        m_Blocks[
//            z * m_WorldData.ChunkBlockWidth * m_WorldData.ChunkBlockHeight + y * m_WorldData.ChunkBlockHeight + x].Type =
//            blockType;
//		if(m_Blocks == null || m_Blocks[x,y,z] == null)
//			Debug.Log("Block or Block list is null in SetBlockType!   " + x.ToString() + " " + m_Blocks.GetUpperBound(0).ToString() + "," + y.ToString() + m_Blocks.GetUpperBound(1).ToString() + "," + z.ToString() + m_Blocks.GetUpperBound(2).ToString());
		m_Blocks[x,y,z].Type = blockType;
    }

    public List<int> Indices
    {
        get { return m_Indices; }
        set { m_Indices = value; }
    }

    public List<Vector2> Uvs
    {
        get { return m_Uvs; }
        set { m_Uvs = value; }
    }

    public List<Vector3> Vertices
    {
        get { return m_Vertices; }
        set { m_Vertices = value; }
    }

    public List<Color> Colors
    {
        get { return m_Colors; }
        set { m_Colors = value; }
    }


    public static int WorldChunkYOffset { get; set; }

    public Object ChunkTransform
    {
        get { return m_ChunkTransform; }
        set { m_ChunkTransform = value; }
    }

    public override string ToString()
    {
        return String.Format("Chunk_{3}_{0},{1},{2}_{4}", m_ArrayX, m_ArrayY, m_ArrayZ, Id, WorldPosition);
    }
	
	public string ToStringOld()
	{
		return String.Format("Chunk_{0},{1},{2}", m_ArrayX, m_ArrayY, m_ArrayZ);
	}
	
	public Chunk ReplacementChunk { get; set; }

    /// <summary>
    /// Marks a chunk as needing to be relit and redrawn.
    /// </summary>
    public bool NeedsRegeneration { get; set; }

    public Vector3i Position
    {
        get { return m_Position; }
        set { m_Position = value; }
    }

    // TODO: When we make the world multiple chunks deep, this will have to handle ArrayZ values. 

    public bool IsOnTheBorder
    {
        get
        {
            return m_ArrayX == 0 || m_ArrayY == 0 || m_ArrayX == WorldData.RightChunkBorderColumn ||
                   m_ArrayY == WorldData.TopChunkBorderRow;
        }
    }

    public bool IsInsideTheBorder
    {
        get
        {
            return m_ArrayX > 0 && m_ArrayY > 0 && m_ArrayX < WorldData.RightChunkBorderColumn &&
                   m_ArrayY < WorldData.TopChunkBorderRow;
        }
    }

    public TQueue<GameObjectCreationData> GameObjectCreationQueue
    {
        get { return m_GameObjectCreationQueue; }
        set { m_GameObjectCreationQueue = value; }
    }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public Block[,,] Blocks
    {
        get { return m_Blocks; }
    }

    public void AddGameObjectCreationData(GameObjectCreationData gameObjectCreationData)
    {
        m_GameObjectCreationQueue.Enqueue(gameObjectCreationData);
    }
	
	public bool saveData (StreamWriter sw)
	{
		
		// This uses simple run length encoding
		// col x,y: [count]x[BlockType] ... for each block in column
		sw.WriteLine(this.ToString());
		for (int x = 0; x < WorldData.ChunkBlockWidth; x++) {
        	for (int y = 0; y < WorldData.ChunkBlockHeight; y++) {
				sw.Write("col " + x + "," + y + ":");
				BlockType bt = BlockType.Air;
				int counter = 0;
				for (int z = 0; z < WorldData.ChunkBlockDepth; z++) {
					BlockType currentBlock = GetBlock(x,y,z).Type;
					if (bt != currentBlock && counter > 0) {
						sw.Write(counter + "x" + ((byte)bt) + " ");
						counter = 0;
					}
					bt = currentBlock;
					counter += 1;
				}
				if (counter > 0)
					sw.Write(counter + "x" + ((byte)bt));
				sw.WriteLine();
			}
		}
		return true;
	}
	
		
	public int getChunkFloorHeight(WorldData worldData) {
		int[] depthTally = new int[worldData.ChunkBlockDepth];
		for (int z = 0; z < worldData.ChunkBlockDepth; z++) depthTally[z] = 0;
		
		// We are now in the right place to start reading the chunk data
		for (int x = 0; x < worldData.ChunkBlockWidth; x++) {
	        for (int y = 0; y < worldData.ChunkBlockHeight; y++) {
				BlockType LastType = BlockType.Stone;
				for (int z = 0; z < worldData.ChunkBlockDepth; z++) {
//					if(GetBlock(x,y,z) == null)
//					{
//						SetBlockType(x,y,z, BlockType.Air);
//						SetBlockLight(x,y,z, 255);
//					}
					BlockType bt = (BlockType) GetBlock(x,y,z).Type;
					// If this run is for beginning of some air, then we add the previous
					// block as the height in the depth tally. This is used to determine
					// the "floor" of the world.
					if (bt != LastType) {
						if (bt == BlockType.Air && z != 0) {
							depthTally[z-1] += 1;
							continue;
						}
						LastType = bt;
					}
				}
            }
        }
		
		// Determine world floor height, for use by the OCPerceptionCollector
		int maxZ=0;
		for (int z = 0; z < worldData.ChunkBlockDepth; z++)
			if (depthTally[z] > depthTally[maxZ]) maxZ = z;
		
		return maxZ;
	}
}