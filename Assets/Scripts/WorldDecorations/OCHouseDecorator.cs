
/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using System.Collections;
using ProtoBuf;

namespace OpenCog
{

/// <summary>
/// The OpenCog HouseDecorator.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[AttributeExtensions.OCExposeProperties]
#endregion
public class OCHouseDecorator : IDecoration
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	private readonly WorldData m_WorldData;

	/////////////////////////////////////////////////////////////////////////////

	#endregion

	/////////////////////////////////////////////////////////////////////////////

	#region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////



	/////////////////////////////////////////////////////////////////////////////

	#endregion

	/////////////////////////////////////////////////////////////////////////////

	#region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public OCHouseDecorator(WorldData worldData)
	{
		m_WorldData = worldData;
	}

	public bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
	{
		if(IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
		{
			CreateDecorationAt(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random);
			return true;
		}

		return false;
	}

	public override string ToString()
	{
		return "House";
	}

	/////////////////////////////////////////////////////////////////////////////

	#endregion

	/////////////////////////////////////////////////////////////////////////////

	#region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Determines if a tree decoration even wants to be at this location.
	/// </summary>
	/// <param name="blockX"></param>
	/// <param name="blockY"></param>
	/// <param name="blockZ"></param>
	/// <param name="random"></param>
	/// <returns></returns>
	private bool IsAValidLocationforDecoration(int blockX, int blockY, int blockZ, IRandom random)
	{
		// We don't want TOO many cars...make it a 1% chance to be drawn there.
		if(random.RandomRange(1, 100) < 90)
		{
			return false;
		}

		 //Cars don't pile up too high
//        if (blockZ >= (int)(m_WorldData.DepthInBlocks*0.1f))
//        {
//            return false;
//        }

		// Cars like to have a minimum amount of space to drive in.
		return SpaceAboveIsEmpty(blockX, blockY, blockZ, 15, 15, 15);
	}
		
	int decorationNumber = 0;
	private void CreateHouse(int blockX, int blockY, int blockZ, int frameWidth, int frameHeight, int frameDepth, bool forward)
	{
		decorationNumber ++;
		string entityName = "house_" + decorationNumber;
		m_WorldData.printOneEntityToCorpus("House",entityName);
			
		for(int x = blockX + 1; x <= blockX + frameWidth; ++x)
		{
			for(int y = blockY + 1; y <= blockY + frameHeight; ++y)
			{
				for(int z = blockZ + 1; z <= blockZ + frameDepth; ++z)
				{
					if(z == blockZ + 1 && x != blockX + 1 && x != blockX + frameWidth && y != blockY + 1 && y != blockY + frameHeight)
					{
						CreateFloorAt(x, y, z,entityName);
					}
					else
					if(z == blockZ + frameDepth && (x != blockX + 1 || x != blockX + frameWidth) && (y != blockY + 1 || y != blockY + frameHeight))
					{
						CreateRoofAt(x, y, z,entityName);
					}
					else
					if((x == blockX + (frameWidth)/2 || x == blockX + (frameWidth)/2 + 1) && y == blockY + 1 && z < blockZ + frameDepth - 1) // front door
					{
						if(!forward)
						{
							//CreateDoorAt(x, y, z,entityName);
						}
					}
					else
					if((x == blockX + frameWidth/2 || x == blockX + frameWidth/2 + 1) && y == blockY + frameHeight && z < blockZ + frameDepth - 1) // back door
					{
						if(forward)
						{
							CreateDoorAt(x, y, z,entityName);
						}
					}
					else
					if((y == blockY + frameHeight/2 || y == blockY + frameHeight/2 + 1) && x == blockX + 1 && z < blockZ + frameDepth - 1) // side window
					{
						if(forward)
							if(z > blockZ + 2 && z < blockZ + frameDepth - 2)
								CreateGlassAt(x,y,z,entityName);
							else
								CreateWallAt(x,y,z,entityName);
					}
					else
					if((y == blockY + frameHeight/2 || y == blockY + frameHeight/2 + 1) && x == blockX + frameWidth && z < blockZ + frameDepth - 1) // side window
					{
						if(!forward)
							if(z > blockZ + 2 && z < blockZ + frameDepth - 2)
								CreateGlassAt(x,y,z,entityName);
							else
								CreateWallAt(x,y,z,entityName);
					}
					else
					if((x == blockX + 1 || x == blockX + frameWidth || y == blockY + 1 || y == blockY + frameHeight) && z < blockZ + frameDepth)
					{
						//if((x != (blockX + frameWidth)/2 && x != ((blockX + frameWidth)/2+1) && y != (blockY + frameHeight)/2 && y != ((blockY + frameHeight)/2+1)))
						{
							CreateWallAt(x, y, z,entityName);
						}
					}
				}
			}
		}
	}

	private void CreateWallAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.Cobblestone);
		m_WorldData.printOneBlockToCorpus(entityName,"Cobblestone",x,y,z);
	}

	private void CreateFloorAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.WoodenPlanks);
		m_WorldData.printOneBlockToCorpus(entityName,"WoodenPlanks",x,y,z);
	}

	private void CreateRoofAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.MossStone);
		m_WorldData.printOneBlockToCorpus(entityName,"MossStone",x,y,z);
	}

	private void CreateDoorAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.IronDoor);
		m_WorldData.printOneBlockToCorpus(entityName,"IronDoor",x,y,z);
	}

	private void CreateGlassAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.GlassPane);
		m_WorldData.printOneBlockToCorpus(entityName,"GlassPane",x,y,z);
	}

	//@TODO: Swap Depth and Height once we normalize y and z nomenclature
	private void CreateDecorationAt(int blockX, int blockY, int blockZ, IRandom random)
	{
		int frameWidth = random.RandomRange(7, 14);
		int frameHeight = random.RandomRange(7, 14);
		int frameDepth = random.RandomRange(6, 10);

		bool isForward = random.RandomRange(0,2) == 1;

		CreateHouse(blockX, blockY, blockZ, frameWidth, frameHeight, frameDepth, isForward);

	}

	private bool SpaceAboveIsEmpty(int blockX, int blockY, int blockZ, int depthAbove, int widthAround, int heightAround)
	{
		for(int z = blockZ + 1; z <= blockZ + depthAbove; z++)
		{
			for(int x = blockX - widthAround; x <= blockX + widthAround; x++)
			{
				for(int y = blockY - heightAround; y <= blockY + heightAround; y++)
				{
					if(m_WorldData.DoesBlockExist(x, y, z) && m_WorldData.GetBlock(x, y, z).Type != BlockType.Air)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	/////////////////////////////////////////////////////////////////////////////

	#endregion

	/////////////////////////////////////////////////////////////////////////////

}// class HouseDecorator

}// namespace OpenCog



