using System;

public struct IntVect
{
    private readonly int m_X;
    private readonly int m_Y;
    private readonly int m_Z;
    public IntVect(int x, int y, int z)
    {
        m_X = x;
        m_Y = y;
        m_Z = z;
    }

    public int X
    {
        get { return m_X; }
    }

    public int Y
    {
        get { return m_Y; }
    }

    public int Z
    {
        get { return m_Z; }
    }
	
	public static IntVect ZERO = new IntVect(0,0,0);
	
    public override bool Equals(object obj)
    {
        IntVect other = (IntVect)obj;
        return (other.X == m_X && other.Y == m_Y && other.Z == m_Z);
    }

    public static bool operator ==(IntVect one, IntVect other)
    {
        return (one.X == other.X) && (one.Y == other.Y) && (one.Z == other.Z);
    }

    public static bool operator !=(IntVect one, IntVect other)
    {
        return !(one == other);
    }
	
	public static bool is8Neighbour(IntVect one, IntVect other)
	{
		if ( (one.Z == other.Z) &&
			 ( ((one.X - other.X) >=-1) && ((one.X - other.X) <= 1)) &&
			 ( ((one.Y - other.Y) >=-1) && ((one.Y - other.Y) <= 1))
			)
			return true;
		else
			return false;
	}
	
	public static bool is4Neighbour(IntVect one, IntVect other)
	{
		if ( (one.Z == other.Z) &&
			 ( ((one.X - other.X) >=-1) && ((one.X - other.X) <= 1)) &&
			 ( ((one.Y - other.Y) >=-1) && ((one.Y - other.Y) <= 1))
			)
		{
			if ((one.X == other.X) || (one.Y == other.Y))
				return true;
			else
				return false;
		}
			
		else
			return false;
	}
	
	public static double getDistance(IntVect one, IntVect other)
	{
		return Math.Sqrt((one.X - other.X)*(one.X - other.X) + (one.Y - other.Y)*(one.Y - other.Y)
				+ (one.Z - other.Z)*(one.Z - other.Z));
	}

    public override int GetHashCode()
    {
        return m_X ^ m_Y;
    }
}