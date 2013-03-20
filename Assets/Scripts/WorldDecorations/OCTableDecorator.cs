
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
/// The OpenCog TableDecorator.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[AttributeExtensions.OCExposeProperties]
#endregion
public class OCTableDecorator : IDecoration
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

	public OCTableDecorator(WorldData worldData)
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
		return "Table";
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
		if(random.RandomRange(1, 100) < 99)
		{
			return false;
		}

		// Cars don't pile up too high
//        if (blockZ >= (int)(m_WorldData.DepthInBlocks*0.1f))
//        {
//            return false;
//        }

		// Cars like to have a minimum amount of space to drive in.
		return SpaceAboveIsEmpty(blockX, blockY, blockZ, 8, 8, 8);
	}
	
	private void CreateTable(int blockX, int blockY, int blockZ, int frameWidth, int frameHeight, int frameDepth)
	{
		for(int x = blockX + 1; x <= blockX + frameWidth; ++x)
		{
			for(int y = blockY + 1; y <= blockY + frameHeight; ++y)
			{
				for(int z = blockZ + 1; z <= blockZ + frameDepth; ++z)
				{
					if((x == blockX + 1 || x == blockX + frameWidth) && z < blockZ + frameDepth && (y == blockY + 1 || y == blockY + frameHeight))
					{
						CreateLegAt(x, y, z);
					}
					else
					if(z == blockZ + frameDepth)
					{
							CreateTopAt(x, y, z);
					}
				}
			}
		}
	}

	private void CreateLegAt(int x, int y, int z)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.Wood);
	}

	private void CreateTopAt(int x, int y, int z)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.WoodenPlanks);
	}

	//@TODO: Swap Depth and Height once we normalize y and z nomenclature
	private void CreateDecorationAt(int blockX, int blockY, int blockZ, IRandom random)
	{
		int frameWidth = random.RandomRange(3, 5);
		int frameHeight = random.RandomRange(3, 5);
		int frameDepth = random.RandomRange(2, 3);

		CreateTable(blockX, blockY, blockZ, frameWidth, frameHeight, frameDepth);

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

}// class TableDecorator

}// namespace OpenCog



