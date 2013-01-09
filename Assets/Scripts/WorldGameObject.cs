using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Initializes World, Tracks Player Position, and Streams Chunks into the Game.
/// </summary>
public class WorldGameObject : MonoBehaviour
{
    //////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Our blueprint for terrain chunks, in the Unity Hierarchy
	/// </summary>
	public Transform chunkPrefab; 

	/// <summary>
	/// All possible block textures
	/// </summary>
	public Texture2D[] worldTextures;
	
	/// <summary>
	/// The world texture atlas.
	/// </summary>
	public Texture2D worldTextureAtlas;
	
	/// <summary>
	/// Whether to use normal map.
	/// </summary>
	public bool useNormalMap = false;
	
	/// <summary>
	/// All possible block normals
	/// </summary>
	public Texture2D[] worldNormals;
	
	/// <summary>
	/// Our normals texture atlas
	/// </summary>
	public Texture2D worldNormalAtlas;
	
	/// <summary>
	/// Chunk Dimensions
	/// </summary>
	//[NonSerialized]
	public static int chunkBlocksWidth = 10;
	//[NonSerialized]
	public static int chunkBlocksHeight = 10;
	//[NonSerialized]
	public static int chunkBlocksDepth = 10;
	//[NonSerialized]
	public static int chunksWide = 5;
    //[NonSerialized]
	public static int chunksHigh = 5;
    //[NonSerialized]
	public static int chunksDeep = 1;

	/// <summary>
	/// Our Unity decorator prefabs
	/// </summary>
	public List<Transform> decoratorPrefabs = new List<Transform> ();
	
	/// <summary>
	/// The player start location.
	/// </summary>
	public Vector3i playerStart;
	
	/// <summary>
	/// The current player's location, orientation, & scale.
	/// </summary>
	public Transform playerTransform;
	
	public Transform ghostTransform;
	public Transform girlTransform;
	public Transform robotTransform;
	
	/// <summary>
	/// The transform for the sparks created by digging a block.
	/// </summary>
	public Transform sparksTransform;
	
	/// <summary>
	/// The chunk game objects.
	/// </summary>
	public Transform[,,] chunkGameObjects;
	
	/// <summary>
	/// The world generation.
	/// @NOTE: What is this for?
	/// </summary>
	public GUIText worldGeneration;
	
	/// <summary>
	/// Gets the world data (public accessor).
	/// </summary>
	/// <value>
	/// The world data.
	/// </value>
	public WorldData WorldData {
		get { return m_WorldData; }
	}
	
	/// <summary>
	/// Gets the world.
	/// </summary>
	/// <value>
	/// The world.
	/// </value>
	public World World {
		get { return m_World; }
	}
	
	/// <summary>
	/// The filename to load.
	/// </summary>
	public string loadFromFile;
	
	/// <summary>
	/// The filename to save.
	/// </summary>
	public string saveToFile;
	
	/// <summary>
	/// The world object accessor.
	/// </summary>
	public World world
	{
		get {return m_World;}
	}
	
	/// <summary>
	/// Gets the bounds.
	/// </summary>
	/// <value>
	/// The bounds.
	/// </value>
	public Bounds bounds
	{
		get { return m_WorldData.bounds; }
	}
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions...
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WorldGameObject"/> class.
	/// </summary>
	/// <param name='processAllChunksAtOnce'>
	/// Process all chunks at once.
	/// </param>
	public WorldGameObject (bool processAllChunksAtOnce)
	{
		m_ProcessAllChunksAtOnce = processAllChunksAtOnce;
	}
	
	/// <summary>
	/// This will find the point on the ground directly below another point.
	/// </summary>
	/// <returns>
	/// Returns the argument if there is no ground below the point.
	/// </returns>
	/// <param name='thePoint'>
	/// The point.
	/// </param>
	public Vector3 getGroundBelowPoint(Vector3 thePoint)
	{
        Ray ray = new Ray(thePoint, Vector3.down);
        
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) {
			RaycastHit hit = new RaycastHit();
	        if (c.Raycast (ray, out hit, 3000)) {
				return hit.point;
			}
		}
		return thePoint;
		
	}	
	
	public bool isBlockDirectlyInFront(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position, globalCharacterTransform.forward);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 1)) {
				return true;
			}
		}
		
		return false;
	}
	
	public Vector3 getTheBlockPositionDirectlyInFront(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position, globalCharacterTransform.forward);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 5)) {
				return hit.point  + (ray.direction.normalized * 0.5f);
			}
		}
		
		return Vector3.zero;
	}
	
	public bool isBlockBelow(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position, Vector3.down);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 1)) {
				return true;
			}
		}
		
		return false;
	}
	
	public bool isBlockFarBelowFront(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position + globalCharacterTransform.forward, Vector3.down);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 3000)) {
				return true;
			}
		}
		
		return false;
	}
	
	public bool isBlockBelowFront(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position + globalCharacterTransform.forward, Vector3.down);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 1)) {
				return true;
			}
		}
		
		return false;
	}
	
	public bool isBlockAboveFront(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position + Vector3.up, globalCharacterTransform.forward);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 1)) {
				return true;
			}
		}
		
		return false;
	}
	
	public bool isBlockAbove(Transform globalCharacterTransform)
	{
		Ray ray = new Ray(globalCharacterTransform.position, Vector3.up);
		RaycastHit hit = new RaycastHit();
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
	        if (c.Raycast (ray, out hit, 1)) {
				return true;
			}
		}
		
		return false;
	}
	
	public bool isSpaceToRun(Transform globalCharacterTransform)
	{
		RaycastHit hit = new RaycastHit();
		
		Ray ray1 = new Ray(globalCharacterTransform.position, globalCharacterTransform.forward);
		Ray ray2 = new Ray(globalCharacterTransform.position + globalCharacterTransform.forward, Vector3.down);
		Ray ray3 = new Ray(globalCharacterTransform.position + 2*globalCharacterTransform.forward, Vector3.down);
		Ray ray4 = new Ray(globalCharacterTransform.position + 3*globalCharacterTransform.forward, Vector3.down);
		
		foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
		{
			bool allClear = false;
			
			allClear = !c.Raycast (ray1, out hit, 3);
			allClear = allClear && c.Raycast(ray2, out hit, 3000);
			allClear = allClear && c.Raycast(ray3, out hit, 3000);
			allClear = allClear && c.Raycast(ray4, out hit, 3000);
			
	        if(allClear)
				return true;
		}
		
		return false;
	}
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The world data object.
	/// </summary>
	private WorldData m_WorldData;
	
	/// <summary>
	/// The world object.
	/// </summary>
	private World m_World;
	
	/// <summary>
	/// The current list of world decorations.
	/// </summary>
//	private List<IDecoration> m_WorldDecorations = new List<IDecoration> ();
	
	/// <summary>
	/// The location of the player's current chunk.
	/// </summary>
	private Vector3i m_PlayerChunkPosition;
	
	/// <summary>
	/// Our unity prefabs, by their name
	/// </summary>
	private Dictionary<string, Transform> m_PrefabsByName = new Dictionary<string, Transform> ();
	
	/// <summary>
	/// @TODO: Which chunk's parent transform is this?
	/// </summary>
	private Transform m_ChunksParent;
	
	/// <summary>
	/// The chunk processor we're currently using.
	/// </summary>
	private ChunkProcessor m_ChunkProcessor;
	
	/// <summary>
	/// The chunk mover we're currently using.
	/// </summary>
	private ChunkMover m_ChunkMover;
	
	/// <summary>
	/// @NOTE: Is this specifying the parameters we used for creating this game world?
	/// </summary>
	private readonly TQueue<GameObjectCreationData> m_GameObjectCreationQueue = new TQueue<GameObjectCreationData> ();
	
	/// <summary>
	/// The mesh data generator we're currently using...
	/// </summary>
	private IMeshDataGenerator m_MeshDataGenerator;
	
	/// <summary>
	/// The start time of the game.
	/// </summary>
	private DateTime m_StartTime;
	
	/// <summary>
	/// @TODO: Aren't we already recording the player's last location?
	/// </summary>
	private Vector3 m_LastPlayerLocation;

	/// <summary>
	/// The last time the player tried to dig.
	/// </summary>
	private DateTime m_LastDigTime;
	
	/// <summary>
	/// The time to wait before swings.
	/// @NOTE: What does this mean?
	/// </summary>
	private readonly TimeSpan m_TimeToWaitBeforeSwings = TimeSpan.FromSeconds (0.25f);
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="WorldGameObject"/> enough time has passed to swing again.
	/// </summary>
	/// <value>
	/// <c>true</c> if enough time has passed to swing again; otherwise, <c>false</c>.
	/// </value>
	private bool EnoughTimeHasPassedToSwingAgain {
		get {
			if (m_LastDigTime + m_TimeToWaitBeforeSwings < DateTime.Now) {
				m_LastDigTime = DateTime.Now;
				return true;
			}
			return false;
		}
	}
	
	/// <summary>
	/// Whether to process all chunks at once.
	/// </summary>
	private readonly bool m_ProcessAllChunksAtOnce;
	
	/// <summary>
	/// The last chunk game object creation time.
	/// </summary>
	private DateTime m_LastChunkGameObjectCreationTime = DateTime.Now;
	
	/// <summary>
	/// Whether the player is activated.
	/// </summary>
	private bool m_PlayerIsActivated;
	
	/// <summary>
	/// The finished flag.
	/// </summary>
	private bool m_FinishedFlag = false;
	
	/// <summary>
	/// The player.
	/// </summary>
	//private Player player;
	
	private HUD theHUD;
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////	
	
	/// <summary>
	/// Start this instance by calling various initalization methods.
	/// </summary>
	private void Start ()
	{
		theHUD = GameObject.Find("HUD").GetComponent<HUD>();
		
//		Debug.Log("In WorldGameObject.Start()");
		
		GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player") as GameObject;//GameObject.Find("Player") as GameObject;
		if (playerGameObject == null)
			Debug.LogWarning("No \"Player\" game object found");
		//player = playerGameObject.GetComponent<Player>();
		
		m_PrefabsByName = new Dictionary<string, Transform> (); 
		
		m_ChunkProcessor = new ChunkProcessor ();
		m_WorldData = new WorldData (m_ChunkProcessor);
		m_WorldData.useNormalMap = useNormalMap;
		
		TerrainGenerator t_generator = null;
		if (loadFromFile.Length != 0 ) {
			
			FileTerrainMethod ftm = new FileTerrainMethod(loadFromFile);
			m_WorldData.ChunkBlockWidth  = chunkBlocksWidth  = (int)ftm.chunkWidth;//
			m_WorldData.ChunkBlockHeight = chunkBlocksHeight = (int)ftm.chunkHeight;//
			m_WorldData.ChunkBlockDepth  = chunkBlocksDepth  = (int)ftm.chunkDepth;//
			m_WorldData.ChunksWide = chunksWide = (int)ftm.worldWidth;//
			m_WorldData.ChunksHigh = chunksHigh = (int)ftm.worldHeight;//
			m_WorldData.ChunksDeep = chunksDeep = (int)ftm.worldDepth;//
			//m_WorldData.ChunksWidthOffset = (int)ftm.worldWidthOffset;
			//m_WorldData.ChunksHeightOffset = (int)ftm.worldHeightOffset;
			Debug.Log("In WorldGameObject, Start: dimensions: " + WorldData.ChunksWide + ", " + WorldData.ChunksHigh + ", " + WorldData.ChunksDeep);
			Debug.Log ("   Chunk Dimensions: " + WorldData.ChunkBlockWidth + ", " + WorldData.ChunkBlockHeight + ", " + WorldData.ChunkBlockDepth);
			t_generator = new TerrainGenerator(WorldData, m_ChunkProcessor, new BatchProcessor<Chunk>(),ftm);
		} else {
			InitializeDecoratorPrefabs ();
			m_WorldData.ChunkBlockWidth  =  (int)chunkBlocksWidth;
			m_WorldData.ChunkBlockHeight = (int)chunkBlocksHeight;
			m_WorldData.ChunkBlockDepth  =  (int)chunkBlocksDepth;
			m_WorldData.ChunksWide = (int)chunksWide;
			m_WorldData.ChunksHigh = (int)chunksHigh;
			m_WorldData.ChunksDeep = (int)chunksDeep;
			t_generator = new TerrainGenerator(WorldData, m_ChunkProcessor, new BatchProcessor<Chunk>(), 
                                                 //new PlainTerrainMethod()),
		                                         new DualLayerTerrainWithMediumValleys());
		                                         //new StandardTerrainMediumCaves());
		                    					 //new BasinTerrainMethod());
		}
		
		// t_generator = new TerrainGenerator (WorldData, m_ChunkProcessor, batchProcessor, 
        //                                         new DualLayerTerrainWithMediumValleys ()),
		
		m_ChunksParent = transform.FindChild ("Chunks");
		m_ChunkMover = new ChunkMover (m_WorldData, m_ChunkProcessor);

		// Only the dual layer terrain w/ medium valleys and standard terrain medium caves
		// currently work, I haven't updated the others to return sunlit blocks.
		BatchPoolProcessor<Chunk> batchProcessor = new BatchPoolProcessor<Chunk> ();
		WorldDecorator worldDecorator;
		//if (loadFromFile.Length != 0 ) 
		//	worldDecorator = null;
		//else 
			worldDecorator = new WorldDecorator (WorldData, batchProcessor);
		
		m_MeshDataGenerator = new MeshDataGenerator (batchProcessor, WorldData, m_ChunkProcessor);
		m_World = new World (WorldData,
                            t_generator,
                            new LightProcessor (batchProcessor, WorldData, m_ChunkProcessor),
                            m_MeshDataGenerator,
                            worldDecorator, m_ChunkProcessor);
		
		Debug.Log("In WorldGameObject, Start: Before InitializeGridChunks...");
		
		m_World.InitializeGridChunks ();

		InitializeTextures ();
		playerTransform.transform.position = new Vector3 (WorldData.WidthInBlocks / 2 + 0.5f, 128, WorldData.HeightInBlocks / 2 + 0.5f);
		ghostTransform.transform.position = new Vector3 (WorldData.WidthInBlocks / 2 - 2 + 0.5f, 128, WorldData.HeightInBlocks / 2 + 0.5f);
		girlTransform.transform.position = new Vector3 (WorldData.WidthInBlocks / 2 + 0.5f, 128, WorldData.HeightInBlocks / 2 + 0.5f);
		robotTransform.transform.position = new Vector3 (WorldData.WidthInBlocks / 2 + 2 + 0.5f, 128, WorldData.HeightInBlocks / 2 + 0.5f);
		CreateWorldChunkPrefabs ();
		
		m_World.StartProcessingThread ();
		m_PlayerChunkPosition = CurrentPlayerChunkPosition ();
		m_LastPlayerLocation = playerTransform.position;
	}

	/// <summary>
	/// Add our decorator prefabs assigned in the inspector to a dictionary of 
	/// decorators, by their name. Later, we can just create an instance of the 
	/// prefab in Unity by just referring to the name.
	/// </summary>
	private void InitializeDecoratorPrefabs ()
	{
		foreach (Transform decoratorPrefab in decoratorPrefabs) {
			m_PrefabsByName.Add (decoratorPrefab.name, decoratorPrefab);
		}
	}
	
	/// <summary>
	/// Initializes the world texture atlas.
	/// </summary>
	private void InitializeTextures ()
	{
		worldTextureAtlas = new Texture2D (2048, 2048);
		WorldData.WorldTextureAtlasUvs = worldTextureAtlas.PackTextures (worldTextures, 0);
		worldTextureAtlas.filterMode = FilterMode.Point;
		worldTextureAtlas.anisoLevel = 9;
		worldTextureAtlas.Apply ();

		// Now do normals
		if (useNormalMap) {
			worldNormalAtlas = new Texture2D(2048, 2048);
			worldNormalAtlas.PackTextures(worldNormals, 0);
			worldNormalAtlas.filterMode = FilterMode.Point;
	        worldNormalAtlas.anisoLevel = 9;
	        worldNormalAtlas.Apply();
		}
        
		//foreach (Rect worldTextureAtlasUv in m_World.WorldTextureAtlasUvs)
        //{
        //    Debug.Log(worldTextureAtlasUv);
        //}
		
        WorldData.GenerateUVCoordinatesForAllBlocks();
	}
	
	/**
	 * Highlight the block that the player is looking at. (Doesn't currently work)
	 */
	Vector3 lastHitPoint = Vector3.zero;
	public void HighlightFocusingBlock()
	{
		// Determine what the character is looking at
        Ray ray = Camera.mainCamera.ScreenPointToRay (new Vector3(Screen.width/2.0f, Screen.height / 2.0f, 0));
        RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit, 8.0f))
        {
            if ((int)lastHitPoint.x == (int)hit.point.x &&
			    (int)lastHitPoint.y == (int)hit.point.y &&
			    (int)lastHitPoint.z == (int)hit.point.z) {
				//Debug.Log("same block");
				return;
			}
			lastHitPoint = hit.point;
			if (WorldData.GetBlock((int)hit.point.x, (int)hit.point.z, (int)hit.point.y).Type != BlockType.Air)
			{
				WorldData.SetBlockType((int)hit.point.x, (int)hit.point.z, (int)hit.point.y, BlockType.Lava);
				Debug.Log("block type is " + WorldData.GetBlock((int)hit.point.x, (int)hit.point.z, (int)hit.point.y).Type);
            	m_World.RegenerateChunks();
			} else {
				Debug.Log("air block");
			}
		} else {
			Debug.Log("hit nothing");
		}
	}

    private BlockType storedBlockType = BlockType.Air;
    public Texture2D storedBlockTexture = null;

    public void setStoredBlockType(BlockType bt) {
        if (storedBlockType == bt || bt == BlockType.Air) return;
        storedBlockType = bt;
        // Delete old texture
        Destroy(storedBlockTexture);
                
        Rect uvs = WorldData.BlockUvCoordinates[(int)storedBlockType].BlockFaceUvCoordinates[(int)BlockFace.Top];
        uvs.x = uvs.x * worldTextureAtlas.width;
        uvs.width = uvs.width * worldTextureAtlas.width;
        uvs.y = uvs.y * worldTextureAtlas.height;
        uvs.height = uvs.height * worldTextureAtlas.height;

        Color[] c = worldTextureAtlas.GetPixels((int)uvs.x, (int)uvs.y, (int)uvs.width, (int)uvs.height);
        
        storedBlockTexture = new Texture2D((int)uvs.width, (int)uvs.height);
        storedBlockTexture.SetPixels(c);
        storedBlockTexture.Apply();
    }
	
	/// <summary>
	/// Creates the world chunk prefabs.
	/// </summary>
	private void CreateWorldChunkPrefabs ()
	{
		for (int x = 0; x < WorldData.ChunksWide; x++) {
			for (int y = 0; y < WorldData.ChunksHigh; y++) {
				for (int z = 0; z < WorldData.ChunksDeep; z++) {
					Chunk chunk = WorldData.Chunks [x, y, z];
					CreatePrefabForChunk (chunk);
				}
			}
		}
	}
	
	/// <summary>
	/// Creates the prefab for chunk.
	/// </summary>
	/// <param name='chunk'>
	/// Chunk to make prefab for.
	/// </param>
	private void CreatePrefabForChunk (Chunk chunk)
	{
		Vector3 chunkGameObjectPosition =
            new Vector3 (chunk.Position.X, chunk.Position.Z, chunk.Position.Y);
		Transform chunkGameObject =
            Instantiate (chunkPrefab, chunkGameObjectPosition, Quaternion.identity) as Transform;
		chunkGameObject.parent = m_ChunksParent;
		chunkGameObject.name = chunk.ToString ();
//		chunkGameObjects[x, y, z] = chunkGameObject;
		ChunkGameObject chunkGameObjectScript = chunkGameObject.GetComponent<ChunkGameObject> ();
		chunkGameObjectScript.Texture = worldTextureAtlas;
		chunk.ChunkTransform = chunkGameObject;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update ()
	{
		if(m_World != null)
		{
			ProcessPlayerInput ();
			DisplayDiggings ();
			CheckForWorldMove ();
			CreatePrefabsFromFinishedChunks ();
			//RemoveAnyChunksThatAreOffTheMap ();
			PlayFootsteps ();
		}
	}
	
	/// <summary>
	/// Processes the player input.
	/// </summary>
	private void ProcessPlayerInput ()
	{
		//if(player == null)
		//	return;
		/*
        if (!Input.anyKey)
        {
            return;
        }
        m_ProcessAllChunksAtOnce = true;
		//*/
		
		// If the player interaction menu is visible or the console is active,
		// don't respond to key presses.
		//Debug.Log("The HUD: " + theHUD.ToString());
		//Debug.Log("Console: " + Console.get().ToString());
		if (theHUD.interactMenuVisible || Console.get().isActive()) return;
		
		
//		// When press leftShift , will shift the camera between the First person and Third person view
//		if(Input.GetKeyDown(KeyCode.LeftShift)) 
//		{
//			if (player.isNowFPview)
//			{
//				player.theMainCamera.transform.localPosition =  new Vector3(0.0f, 8.6f, -8.0f);
//				//player.theMainCamera.transform.LookAt(Player);
//				player.isNowFPview = false;
//				Debug.Log("Change to the Third Person view");
//			}
//			else
//			{
//				player.theMainCamera.transform.localPosition = new Vector3(0.0f, 1.6f, 0.2f);
//				player.isNowFPview = true;
//				Debug.Log("Change to the First Person view");
//			}
//		}

        if (Input.GetKeyDown(KeyCode.Q)) {
            RaycastHit hit;
            Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 8.0f)) {
                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
                BlockType NewBlockType = WorldData.GetBlock((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y).Type;
                if (NewBlockType == BlockType.Stone)
                {
                    // Wrap around to the beginning block type...
                    NewBlockType = BlockType.TopSoil;
                } else  {
                    // Otherwise just increment to the next block type.
                    NewBlockType += 1;
                }
                IntVect blockBuildPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
                m_World.GenerateBlockAt(blockBuildPoint, NewBlockType);
            }
        }
		if (Input.GetKeyDown(KeyCode.R)) {
            RaycastHit hit;
            Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 8.0f)) {
                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
                BlockType NewBlockType = BlockType.Air;

                IntVect blockBuildPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
                m_World.GenerateBlockAt(blockBuildPoint, NewBlockType);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RaycastHit hit;
            Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 8.0f))
            {
                // go INTO the block we are looking at
                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
                setStoredBlockType(WorldData.GetBlock((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y).Type);
                
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            RaycastHit hit;
            Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 20.0f)) // increase the lenght of ray from 8.0 to 20 for the third person view
            {
                WorldData.SetBlockLightWithRegeneration((int)hit.point.x, (int)hit.point.z, (int)hit.point.y, 255);
                m_World.RegenerateChunks();
            }
        }
		
		if (Input.GetKeyDown(KeyCode.C))
		{
			RaycastHit hit;
            Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 20.0f))
            {
                Vector3 hitPoint = hit.point - (ray.direction.normalized * 0.01f);
                IntVect blockBuildPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
                m_World.GenerateBlockAt(blockBuildPoint, storedBlockType);
			}
		}
		
//		if(Input.GetKeyDown(KeyCode.P))
//		{
//			player.characterType++;
//			if(player.characterType > Player.CharacterType.robot)
//				player.characterType = 0;
//			string characterType = player.characterType.ToString();
//			Console.print("Player Character Type: " + characterType);
//		}
        
        // Ask the robot to recognize a structure built by blocks
/*		if (Input.GetKeyDown(KeyCode.I))
		{
			Ray ray = new Ray(globalCharacterTransform.position, globalCharacterTransform.forward);
			RaycastHit hit = new RaycastHit();
			IntVect blockhitPoint =null;
			foreach (MeshCollider c in transform.GetComponentsInChildren<MeshCollider>()) 
			{
		        if (c.Raycast (ray, out hit, 3)) 
				{
				    blockhitPoint = new IntVect((int)hit.point.x, (int)hit.point.z, (int)hit.point.y);
					break;
				}
			}
			
			if (blockhitPoint == null)
				return;
            //Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
            
            Transform allAvatars = GameObject.Find("Avatars").transform;
			foreach (Transform child in allAvatars)
	        {
	            if (child.gameObject.tag != "OCA") continue;
	            OCConnector con = child.gameObject.GetComponent<OCConnector>() as OCConnector;
				if (con != null)
				{
					con.sendBlockStructure(blockhitPoint,true);
				}
	        }
			
		}
		*/
		// save current scene into the saveToFile mapfile
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.S))
		{
			if (saveToFile.Length == 0)
			{
				saveToFile = "DefaultSavedMap";
			}
			m_WorldData.saveData(saveToFile);
			Debug.Log("Saved the current map to " + saveToFile);
		}		

	}

    private void CreateFinishedChunk()
    {
        while (WorldData.FinishedChunks.Count > 0)
        {
            if (!m_ReadyToActivatePlayer)
            {
                m_ReadyToActivatePlayer = true;
                playerTransform.gameObject.SetActiveRecursively(true);
            }
            Chunk chunk = WorldData.GetFinishedChunk();
            if (chunk == null)
            {
                return;
            }
            ChunkGameObject chunkGameObjectScript =
                chunkGameObjects[chunk.ArrayX, chunk.ArrayY, chunk.ArrayZ].GetComponent<ChunkGameObject>();

            chunkGameObjectScript.CreateFromChunk(chunk, m_PrefabsByName);
            if (!m_ProcessAllChunksAtOnce)
            {
                return;
            }
        }
    }


    private bool m_ReadyToActivatePlayer;

	/// <summary>
	/// Displays the diggings.
	/// </summary>
	private void DisplayDiggings ()
	{
		if (m_World != null && m_World.Diggings.Count == 0) {
			return;
		}
		Vector3 diggingsLocation = m_World.Diggings.Dequeue ();
		Instantiate (sparksTransform, diggingsLocation, Quaternion.identity);
	}
	
	/// <summary>
	/// If the player has moved into a different chunk, we need to generate
	/// new world terrain
	/// </summary>
	private void CheckForWorldMove ()
	{
		Vector3i newPlayerChunkPosition = CurrentPlayerChunkPosition ();
		if (newPlayerChunkPosition != m_PlayerChunkPosition) {
			Vector3i direction = (m_PlayerChunkPosition - newPlayerChunkPosition);
			m_PlayerChunkPosition = newPlayerChunkPosition;
			m_ChunkMover.ShiftWorldChunks (direction);
		}
	}
	
	/// <summary>
	/// Creates the prefabs from finished chunks.
	/// </summary>
	private void CreatePrefabsFromFinishedChunks ()
	{
		if (!m_PlayerIsActivated) 
		{
			ActivateThePlayer ();
		}
		
		while (m_ChunkProcessor.MeshCreationQueue.Count > 0) 
		{

			// Don't freeze everything by drawing them every frame
			if (m_LastChunkGameObjectCreationTime + TimeSpan.FromSeconds (0.001) > DateTime.Now) {
				return;
			}
			m_LastChunkGameObjectCreationTime = DateTime.Now;
			Chunk chunk = m_ChunkProcessor.MeshCreationQueue.Dequeue ();

			if (chunk == null) {
				return;
			}

			if (chunk.ChunkTransform == null) {
				CreatePrefabForChunk (chunk);
			}

			Transform chunkTransform = (Transform)chunk.ChunkTransform;
			ChunkGameObject chunkGameObjectScript = chunkTransform.GetComponent<ChunkGameObject> ();
			((IGameObject)chunkGameObjectScript).world = this;
			
			//@TODO: What's up with ChunkGameObjectScript and decorator prefabs?
			chunkGameObjectScript.CreateFromChunk (chunk, m_PrefabsByName);

			if (!m_ProcessAllChunksAtOnce) {
				return;
			}
		}
	}
	
	/// <summary>
	/// Removes any chunks that are off the map.
	/// </summary>
	private void RemoveAnyChunksThatAreOffTheMap ()
	{
		if (m_ChunkProcessor.PrefabRemovalQueue.Count > 0) 
		{
		    Chunk chunkToRemove = m_ChunkProcessor.PrefabRemovalQueue.Dequeue();
		    Transform chunkTransform = (Transform)chunkToRemove.ChunkTransform;
		    ChunkGameObject chunkGameObjectScript = chunkTransform.GetComponent<ChunkGameObject>();
		    //@TODO: Replace this method?
			//chunkGameObjectScript.Destroy();
		}
	}
	
	/// <summary>
	/// Plays the footsteps.
	/// </summary>
	private void PlayFootsteps ()
	{
		if (playerTransform.position == m_LastPlayerLocation) {
			return;
		}

		float distance = Vector3.Distance (playerTransform.position, m_LastPlayerLocation);
		if (distance >= 1) {
			m_LastPlayerLocation = playerTransform.position;
		}
	}

	/// <summary>
	/// The current chunk that the player is in.
	/// </summary>
	/// <returns></returns>
	private Vector3i CurrentPlayerChunkPosition ()
	{
//		Debug.Log("Chunk Block Width:" + m_WorldData.ChunkBlockWidth.ToString());
//		Debug.Log("Chunk Block Height:" + m_WorldData.ChunkBlockHeight.ToString());
		return new Vector3i ((int)playerTransform.position.x / m_WorldData.ChunkBlockWidth,
                            (int)playerTransform.position.z / m_WorldData.ChunkBlockHeight, 0);
	}

	/// <summary>
	/// Activates the player.
	/// </summary>
	private void ActivateThePlayer ()
	{
		// If the world is not ready...no playing yet.
		if (!m_WorldData.WorldIsReady) {
			//Debug.Log("World is not ready...");
			return;
		}
		else
		{
			//Debug.Log("World is now ready...");
		}

		m_PlayerIsActivated = true;
		//playerTransform.GetComponentInChildren<PlayerMoveController> ().enabled = true;
		m_PlayerChunkPosition = CurrentPlayerChunkPosition ();
		GameObject.Find ("GeneratingWorld").active = false;
	}

	/// <summary>
	/// When we quit the app, be sure to shut down the threads and
	/// destory our chunks
	/// </summary>
	private void OnApplicationQuit ()
	{
		Debug.Log ("Exiting!");
		if(m_World != null) m_World.ContinueProcessingChunks = false;
		for (int x = 0; x < WorldData.ChunksWide; x++) {
			for (int y = 0; y < WorldData.ChunksHigh; y++) {
				World.DestroyChunk (WorldData.Chunks [x, y, 0]);
			}
		}
		m_World = null;
	}

	//@NOTE: Should there be a Stop()?

	#endregion

	//////////////////////////////////////////////////
}