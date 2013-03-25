using UnityEngine;

public class CarDecorator : IDecoration
{
    private readonly WorldData m_WorldData;

    public CarDecorator(WorldData worldData)
    {
        m_WorldData = worldData;
    }


    public bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
    {
        if (IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
        {
            CreateDecorationAt(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random);
            return true;
        }

        return false;
    }


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
        if (random.RandomRange(1, 100) < 95)
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
	
	int decorationNumber = 0;
	private void CreateCar(int blockX, int blockY, int blockZ, int frameWidth, int frameHeight, int frameDepth, bool isForward)
	{
		decorationNumber ++;
		string entityName = "car_" + decorationNumber;
		m_WorldData.printOneEntityToCorpus("Car",entityName);
		
		for (int x = blockX + 1; x <= blockX + frameWidth; ++x)
		{
			for (int y = blockY + 1; y <= blockY + frameHeight; ++y)
			{
				for (int z = blockZ + 1; z <= blockZ + frameDepth; ++z)
				{
					if((x == blockX + 2 || x == blockX + frameWidth - 1) && z == blockZ + 1 && (y == blockY + 1 || y == blockY + frameHeight))
					{
						if(isForward)
							CreateWheelAt(x,y,z,entityName);
						else 
							CreateWheelAt(y,x,z,entityName);
					}
					else if(((x == blockX + 1 || x == blockX + frameWidth) && z == blockZ + frameDepth - 1) 
						 || ((x == blockX + 2 || x == blockX + frameWidth - 1) && z == blockZ + frameDepth))
					{
						if(isForward)
							CreateGlassAt(x,y,z,entityName);
						else 
							CreateGlassAt(y,x,z,entityName);
					}
					else if(Mathf.Abs(x - (blockX + frameWidth) / 2) <= 1 && Mathf.Abs(z - (blockZ + frameDepth) / 2) <= 1 && (y == blockY + 1 || y == blockY + frameHeight))
					{
						if(isForward)
							CreateDoorAt(x,y,z,entityName);
						else 
							CreateDoorAt(y,x,z,entityName);
					}
					else if((x != blockX + 1 && x != blockX + frameWidth || z != blockZ + frameDepth) && z != blockZ + 1 )
					{
						if(isForward)
							CreateBodyAt(x,y,z,entityName);
						else 
							CreateBodyAt(y,x,z,entityName);
					}
				}
			}
		}
	}

	//@TODO: Swap Depth and Height once we normalize y and z nomenclature
    private void CreateDecorationAt(int blockX, int blockY, int blockZ, IRandom random)
    {
		int frameWidth = random.RandomRange(5,9);
		int frameHeight = random.RandomRange(3,4);
		int frameDepth = random.RandomRange(3,4);
		
		bool isForward = random.RandomRange(0,2) == 1;
		
		CreateCar (blockX, blockY, blockZ, frameWidth, frameHeight, frameDepth, isForward);

    }
	
	

    /// <summary>
    /// Creates the tree canopy...a ball of leaves.
    /// </summary>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    /// <param name="radius"></param>
//    private void CreateDiskAt(int blockX, int blockY, int blockZ, int radius)
//    {
//        for (int x = blockX - radius; x <= blockX + radius; x++)
//        {
//            for (int y = blockY - radius; y <= blockY + radius; y++)
//            {
//                if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, blockZ)) <= radius)
//                {
//                    m_WorldData.SetBlockType(x, y, blockZ, BlockType.Leaves);
//                }
//            }
//        }
//    }
//
//    private void CreateTrunkAt(int blockX, int blockY, int z)
//    {
//        m_WorldData.SetBlockType(blockX, blockY, z, BlockType.Dirt);
//    }
//
//    private void CreateLeavesAt(int blockX, int blockY, int z)
//    {
//        m_WorldData.SetBlockType(blockX, blockY, z, BlockType.Leaves);
//    }
	
	private void CreateWheelAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.Wheel);
		m_WorldData.printOneBlockToCorpus(entityName,"Wheel",x,y,z);
	}
	
	private void CreateGlassAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.GlassPane);
		m_WorldData.printOneBlockToCorpus(entityName,"GlassPane",x,y,z);
	}
	
	private void CreateBodyAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType(x,y,z, BlockType.BlockOfIron);
		m_WorldData.printOneBlockToCorpus(entityName,"BlockOfIron",x,y,z);
	}
	
	private void CreateDoorAt(int x, int y, int z, string entityName)
	{
		m_WorldData.SetBlockType (x,y,z, BlockType.IronDoor);
		m_WorldData.printOneBlockToCorpus(entityName,"IronDoor",x,y,z);
	}

    private bool SpaceAboveIsEmpty(int blockX, int blockY, int blockZ, int depthAbove, int widthAround, int heightAround)
    {
        for (int z = blockZ + 1; z <= blockZ + depthAbove; z++)
        {
            for (int x = blockX - widthAround; x <= blockX + widthAround; x++)
            {
                for (int y = blockY - heightAround; y <= blockY + heightAround; y++)
                {
                    if (m_WorldData.DoesBlockExist(x,y,z) && m_WorldData.GetBlock(x, y, z).Type != BlockType.Air)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public override string ToString()
    {
        return "Car";
    }
}
