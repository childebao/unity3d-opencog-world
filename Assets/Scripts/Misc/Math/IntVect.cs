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

    public override int GetHashCode()
    {
        return m_X ^ m_Y;
    }
}