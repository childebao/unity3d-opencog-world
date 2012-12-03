using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Substrate;

/// <summary>
/// File terrain method.
/// </summary>
public class FileTerrainMethod : ITerrainGenerationMethod
{
	public static string mapDir = "Assets/Maps";
	public static string mapExt = ".blockmap";
	
//	string theData;
	AnvilWorld mcWorld;
	
	// in chunks
	public uint worldWidth;
	public uint worldHeight;
	public uint worldDepth;
	
	// in blocks
	public uint chunkWidth;
	public uint chunkHeight;
	public uint chunkDepth;
	
	public int worldWidthOffset;
	public int worldHeightOffset;
	
	public int chunkWidthOffset;
	public int chunkHeightOffset;
	
//	public uint chunksWide() {
//
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//            line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
//	
//	public uint chunksHigh() {
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//			// on second line
//			for (uint i = 0; i < 2; i++) line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
//	
//	public uint chunksDeep() {
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//			// on third line
//			for (uint i = 0; i < 3; i++) line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
//	
//	public uint chunkWidth() {
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//			// on fourth line
//			for (uint i = 0; i < 4; i++) line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
//	
//	public uint chunkHeight() {
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//			// on fifth line
//			for (uint i = 0; i < 5; i++) line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
//	
//	public uint chunkDepth() {
//		using (StringReader reader = new StringReader( theData )) {
//            string line = "";
//			// on sixth line
//			for (uint i = 0; i < 6; i++) line = reader.ReadLine();
//			string[] words = line.Split(':');
//			return uint.Parse(words[1]);
//        }
//	}
	
	public FileTerrainMethod(string mapName) {
		Debug.Log("In FileTerrainMethod: start");
		
		mapName = System.IO.Path.Combine(mapDir, mapName);
		
		mcWorld = AnvilWorld.Open(mapName);
		
		RegionChunkManager rcm = mcWorld.GetChunkManager();
		AnvilRegionManager arm = mcWorld.GetRegionManager();
		
		int minWorldWidth = int.MaxValue;
		int minWorldHeight = int.MaxValue;
		int minWorldDepth = int.MaxValue;
		
		int minChunkHeight = int.MaxValue;
		int minChunkWidth = int.MaxValue;
		int minChunkDepth = int.MaxValue;
		
		int maxWorldWidth = int.MinValue;
		int maxWorldHeight = int.MinValue;
		int maxWorldDepth = int.MinValue;
		
		int maxChunkHeight = int.MinValue;
		int maxChunkWidth = int.MinValue;
		int maxChunkDepth = int.MinValue;
		
		int minWorldWidthOffset = int.MaxValue;
		int minWorldHeightOffset = int.MaxValue;
		
		int minChunkWidthOffset = int.MaxValue;
		int minChunkHeightOffset = int.MaxValue;
		
		int count = 0;
		
		foreach( AnvilRegion cmRegion in arm )
		{
			minWorldWidthOffset = Math.Min (minWorldWidthOffset, cmRegion.X);
			minWorldHeightOffset = Math.Min (minWorldHeightOffset, cmRegion.Z);
		}
		
		foreach( ChunkRef chunk in rcm )
		{
			if( chunk.IsTerrainPopulated && !chunk.IsDirty )
			{
				
				minWorldWidth = Math.Min(minWorldWidth, chunk.X);
				maxWorldWidth = Math.Max(maxWorldWidth, chunk.X);
				
				minWorldHeight = Math.Min(minWorldHeight, chunk.Z);
				maxWorldHeight = Math.Max(maxWorldHeight, chunk.Z);
				
				minWorldDepth = 0;//No chunk.y because chunks are defined as a column
				maxWorldDepth = 0;
				
				minChunkWidth = Math.Min(minChunkWidth, chunk.Blocks.XDim);
				maxChunkWidth = Math.Max(maxChunkWidth, chunk.Blocks.XDim);
				
				minChunkDepth = Math.Min(minChunkDepth, chunk.Blocks.YDim);
				maxChunkDepth = Math.Max(maxChunkDepth, chunk.Blocks.YDim);
				
				minChunkHeight = Math.Min(minChunkHeight, chunk.Blocks.ZDim);
				maxChunkHeight = Math.Max(maxChunkHeight, chunk.Blocks.ZDim);
				
				++count;
				
			}
			else
			{
				//Debug.Log("In FileTerrainMethod: chunk terrain is not populated or is dirty: " + chunk.ToString());
			}
		}
		
		
//		foreach( AnvilRegion cmRegion in arm )
//		{
//			minWorldWidthOffset = (int)Math.Min (minWorldWidthOffset, (int)cmRegion.X);
//			minWorldHeightOffset = (int)Math.Min (minWorldHeightOffset, (int)cmRegion.Z);
//		}
		
		worldWidthOffset = -minWorldWidthOffset;
		worldHeightOffset = -minWorldHeightOffset;
		
		chunkWidthOffset = -minWorldWidth;
		chunkHeightOffset = -minWorldHeight;
			
//		worldWidthOffset = (uint)Math.Abs(minWorldWidthOffset);
//		worldHeightOffset = (uint)Math.Abs(minWorldHeightOffset);
		
		worldWidth = (uint)(maxWorldWidth - minWorldWidth + 1);
		worldDepth = (uint)(maxWorldDepth - minWorldDepth + 1);
		worldHeight = (uint)(maxWorldHeight - minWorldHeight + 1);
		
		chunkWidth = (uint)(maxChunkWidth == minChunkWidth ? minChunkWidth : maxChunkWidth);
		chunkHeight = (uint)(maxChunkHeight == minChunkHeight ? minChunkHeight : maxChunkHeight);
		chunkDepth = (uint)(maxChunkDepth == minChunkDepth ? minChunkDepth : maxChunkDepth);
		
//		mapName = System.IO.Path.Combine(mapDir, mapName) + mapExt;
//		using (StreamReader sr = new StreamReader(mapName)) {
//			theData = sr.ReadToEnd();
//		}
		
		Debug.Log("In FileTerrainMethod: end: " + "(" + worldWidthOffset + ", " + worldHeightOffset + ") " + maxWorldWidth + "-" + minWorldWidth + ", " + maxWorldHeight + "-" + minWorldHeight + ", " + count);
		
	}
	
	public void GenerateTerrain(WorldData worldData, Chunk chunk)
	{
		GenerateTerrain(worldData, chunk, 0);
	}
	
	public void GenerateTerrain(WorldData worldData, Chunk chunk, uint noiseBlockOffset)
    {
		string blockTypes = "";
		
		//Debug.Log("In FileTerrainMethod, GenerateTerrain: start");
		
		//Console.print("In File Terrain Method Generate Terrain, chunk: " + chunk.ToString() + ", block (width, height): (" + worldData.ChunkBlockWidth + ", " + worldData.ChunkBlockHeight + ")");
		
		//Open our world
		//AnvilWorld mcWorld = AnvilWorld.Open(_mapName);
		
		lock(mcWorld)
		{
		// The chunk manager is more efficient than the block manager fr
		// this purpose, since we'll inspect every block
		AnvilRegionManager arm = mcWorld.GetRegionManager();
		RegionChunkManager rcm = mcWorld.GetChunkManager();
		
		int chunkBlockX = chunk.ArrayX * worldData.ChunkBlockWidth;
		int chunkBlockZ = chunk.ArrayZ * worldData.ChunkBlockDepth;
        int chunkBlockY = chunk.ArrayY * worldData.ChunkBlockHeight;
		
		//AnvilRegion cmRegion = (AnvilRegion)arm.GetRegion("r.0.1.mca");
		//ChunkRef cmChunk = cmRegion.GetChunkRef(chunk.ArrayX, chunk.ArrayY);
		
		//chunk.Position.X
		
		foreach( AnvilRegion cmRegion in arm )
		{
		
		//lock (cmRegion)
		//{
		
		int clx = -1 * cmRegion.ChunkGlobalX(-1 * chunk.ArrayX + chunkWidthOffset);
		int clz = -1 * cmRegion.ChunkGlobalZ(-1 * chunk.ArrayY + chunkHeightOffset);
		ChunkRef cmChunk = cmRegion.GetChunkRef(clx, clz);
						   
		
		if(cmChunk != null)
		{
				
			//ChunkRef cmChunk = armRegion.GetChunkRef(armRegion.ChunkLocalX(rx), armRegion.ChunkLocalZ(rz));
			//if(cmChunk.LocalX == chunk.ArrayX && cmChunk.LocalZ == chunk.ArrayZ)
			{
				int xdim = cmChunk.Blocks.XDim;
                int ydim = cmChunk.Blocks.YDim;
                int zdim = cmChunk.Blocks.ZDim;
				
				for (int x = 0; x < xdim; ++x)
				{
					int globalBlockX = chunkBlockX + x + worldData.MapBlockOffset.X;
					
					for (int z = 0; z < zdim; ++z)
					{
						int globalBlockZ = chunkBlockZ + z + worldData.MapBlockOffset.Z;
						
						for (int y = 0; y < ydim; ++y)
						{
							int globalBlockY = chunkBlockY + y + worldData.MapBlockOffset.Y;
							
							BlockType bt = (BlockType)cmChunk.Blocks.GetID(x,y,z);
							
							if(!Block.StringToTypeMap.ContainsValue( bt ) )
							{
								bt = BlockType.Dirt;
							}
							
							//chunk.TopSoilBlocks.Add(new Vector3i(globalBlockX, globalBlockY, globalBlockZ));
							chunk.SetBlockType(x, z, y, bt);
						}
					}
				}
			}
		}
		else
		{
			Debug.Log("cmChunk == NULL");
			
			foreach (ChunkRef cmChunk2 in rcm)
			{
				Debug.Log("cmChunk2 = " + cmChunk2.LocalX + ", " + cmChunk2.LocalZ + " ... " + cmChunk2.ToString());
			}
				
		}
		//}
		//}
		
		//Debug.Log("In FileTerrainMethod, GenerateTerrain: end");
		
//		// search for the correct position in the data where the chunk starts.
//		using (Stream ms = new MemoryStream( ASCIIEncoding.Default.GetBytes(theData) )) 
//		{
//			//Console.print("Before Async Line Read");
//			
//			StreamReader reader = new StreamReader(ms);
//			
//			//AsyncCallback rc = null;
//			
//			bool foundChunk = false;
//			string line = "";
//			string chunkStr = chunk.ToStringOld();
//			
//			//Console.print("ChunkString:" + chunkStr + "With Encoding: " + reader.CurrentEncoding.ToString());
//			
////			int chunkStrSize = UTF8Encoding.Default.GetBytes(chunkStr).Count();
////			byte[] buffer = new byte[chunkStrSize];
//			do
//			{
//				line = reader.ReadLine();
//			}
//			while(line != null && chunkStr != null && line.IndexOf(chunkStr) == -1);
//			
//			if(line != null && chunkStr != null)
//			{
//				foundChunk = true;
//				//ms.EndRead(rc);
//				
//				//line = reader.ReadLine();
//				//Console.print("Line:" + line + "With Encoding: " + reader.CurrentEncoding.ToString());
//				
//				//reader.CurrentEncoding = System.Text.Encoding.ASCII;
//				
//				int chunkBlockX = chunk.ArrayX * worldData.ChunkBlockWidth;
//        		int chunkBlockY = chunk.ArrayY * worldData.ChunkBlockHeight;
//				
//				int numColInChunk = 0;
//				
//				// We are now in the right place to start reading the chunk data
//		        for (int x = 0; x < worldData.ChunkBlockWidth; x++)
//		        {
//					int globalBlockX = chunkBlockX + x + worldData.MapBlockOffset.X;
//					
//					//Console.print("x: " + x + ", width:" + worldData.ChunkBlockWidth);
//		            for (int y = 0; y < worldData.ChunkBlockHeight; y++)
//		            {
//						int globalBlockY = chunkBlockY + y + worldData.MapBlockOffset.Y;
//						
//						//Console.print("y: " + y + ", height:" + worldData.ChunkBlockHeight);
//						line = reader.ReadLine();
//						string[] words = line.Split(':');
//						string[] blockRuns = words[1].Split(' ');
//						int z=0;
//						for (int i = 0; i < blockRuns.Length; i++) {
//							//Console.print("i: " + i + ", numRuns:" + blockRuns.Length);
//							// Runs of blocks of the same type have format:
//							// [length]x[block type] e.g. 100x255
//							string[] run = blockRuns[i].Split('x');
//							int runLength = int.Parse(run[0]);
//							
//							BlockType bt = 0;
//							
//							if(!Block.StringToTypeMap.ContainsValue((BlockType) int.Parse(run[1])))
//							{
//								bt = BlockType.Dirt;
//							}
//							else
//							{
//								bt = (BlockType) int.Parse(run[1]);
//								blockTypes += bt.ToString() + " ";
//							}
//							
//							++numColInChunk;
//							
//							for (int j = 0; j < runLength; j++) {
//								//Console.print("j: " + j + ", runLength:" + runLength + ", z: " + z);
////								if(chunk.GetBlock(x, y, z) == null)
////								{
////									chunk.SetBlockLight(x, y, z, 255);
////								}
//								chunk.TopSoilBlocks.Add(new Vector3i(globalBlockX, globalBlockY, z));
//								chunk.SetBlockType(x, y, z, bt);
//								z+=1;
//							}
//							
//							
//							
//						}
//		            }
//				}
//				
//				//Console.print("Number of Columns in Chunk: " + numColInChunk);
//			}
//			
//			//Console.print("After Async Line Read");
//
//	    }
				
		
//		}
			
    	}
		
		}
		
	}


}
