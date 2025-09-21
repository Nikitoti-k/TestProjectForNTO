using UnityEngine;
// Структурка для работы с координатами в гексагональной сетке
[System.Serializable]
public struct HexCoord
{
    public int q;
    public int r;

    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public int s => -q - r;

    public int DistanceTo(HexCoord other)
    {
        return (Mathf.Abs(q - other.q) + Mathf.Abs(r - other.r) + Mathf.Abs(s - other.s)) / 2;
    }

    public override int GetHashCode()
    {
        return q.GetHashCode() ^ r.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (!(obj is HexCoord)) return false;
        HexCoord other = (HexCoord)obj;
        return q == other.q && r == other.r;
    }
}