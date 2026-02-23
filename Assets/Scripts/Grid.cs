using UnityEngine;

public enum MassType
{
    Empty, Mass
}

public class Mass
{
    public GameObject Obj
    {
        get;
        set;
    }

    MassType _type;

    public MassType Type
    {
        get => _type;
        set
        {
            _type = value;
            Obj.SetActive(_type == MassType.Mass);
        }
    }
}

public class Grid : MonoBehaviour
{
    public GameObject MassPrefab;
    public Vector3Int Size = new Vector3Int(5, 10, 5);
    public Vector3 MassSize = Vector3.one;
    Mass[,,] _data;

    public Mass this[int x, int y, int z]
    {
        get => _data[x, y, z];
        set => _data[x, y, z] = value;
    }

    public Mass this[Vector3Int pos]
    {
        get => this[pos.x, pos.y, pos.z];
        set => this[pos.x, pos.y, pos.z] = value;
    }

    public bool Contains(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;
        return 0 <= x && x < _data.GetLength(0) && 0 <= y && y < _data.GetLength(1) && 0 <= z && z < _data.GetLength(2);
    }

    public void BuildGrid()
    {
        _data = new Mass[Size.x, Size.y, Size.z];

        for (int y = 0; y < Size.y; ++y)
        {
            GameObject depth = new GameObject($"Depth{y}");
            depth.transform.localPosition = Vector3.up * y;
            depth.transform.SetParent(transform);

            for (int z = 0; z < Size.z; ++z)
            {
                for (int x = 0; x < Size.x; ++x)
                {
                    GameObject mass = Object.Instantiate(MassPrefab);
                    mass.transform.localPosition = new Vector3(MassSize.x * x, MassSize.y * y, MassSize.z * z);
                    mass.transform.SetParent(depth.transform);
                    this[x, y, z] = new Mass
                    {
                        Obj = mass,
                        Type = MassType.Empty
                    };
                }
            }
        }
    }

    public bool DoFillDepth(int depth)
    {
        if (depth < 0 || Size.y <= depth)
        {
            return false;
        }

        for (int z = 0; z < Size.z; ++z)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                if (this[x, depth, z].Type == MassType.Empty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool RemoveDepth(int depth)
    {
        if (depth < 0 || Size.y <= depth)
        {
            return false;
        }

        for (int z = 0; z < Size.z; ++z)
        {
            for (int x= 0; x < Size.x; ++x)
            {
                this[x, depth, z].Type = MassType.Empty;
            }
        }

        for (int y = depth + 1; y < Size.y; ++y)
        {
            for (int z = 0; z < Size.z; ++z)
            {
                for (int x = 0; x < Size.x; ++x)
                {
                    this[x, y - 1, z].Type = this[x, y, z].Type;
                    this[x, y, z].Type = MassType.Empty;
                }
            }
        }

        return true;
    }

    public void ClearMass()
    {
        for (int y = 0; y < Size.y; ++y)
        {
            for (int z = 0; z < Size.z; ++z)
            {
                for (int x = 0; x < Size.x; ++x)
                {
                    this[x, y, z].Type = MassType.Empty;
                }
            }
        }
    }

    private void Awake()
    {
        BuildGrid();
    }
}
