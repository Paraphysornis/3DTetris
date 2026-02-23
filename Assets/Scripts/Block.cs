using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left, Right, Up, Down, Forward, Back
}

enum RotateDirection
{
    Minus, None, Plus
}

static class DirectionUtils
{
    static readonly Dictionary<Direction, Vector3> dirVectorDict = new Dictionary<Direction, Vector3>()
    {
        {Direction.Left, Vector3.left },
        {Direction.Right, Vector3.right },
        {Direction.Up, Vector3.up },
        {Direction.Down, Vector3.down },
        {Direction.Forward, Vector3.forward },
        {Direction.Back, Vector3.back }
    };

    public static Vector3 GetDirVec(Direction dir)
    {
        return dirVectorDict[dir];
    }

    public static Vector3Int Rotate(Vector3Int p, float deg, Direction axis)
    {
        var mat = Matrix4x4.Rotate(Quaternion.AngleAxis(deg, DirectionUtils.GetDirVec(axis)));
        Vector3 newPos = mat.MultiplyVector(p);
        newPos.x += newPos.x > 0 ? 0.5f : -0.5f;
        newPos.y += newPos.y > 0 ? 0.5f : -0.5f;
        newPos.z += newPos.z > 0 ? 0.5f : -0.5f;
        return new Vector3Int((int)newPos.x, (int)newPos.y, (int)newPos.z);
    }
}

public class Block : MonoBehaviour
{
    [SerializeField] GameObject MassPrefab;
    [SerializeField] BlockTemplate _BlockTemplate;
    [SerializeField] Vector3Int _pos;
    public Vector3 MassSize = Vector3.one;
    [SerializeField] Direction _forward = Direction.Forward;
    [SerializeField] Direction _up = Direction.Up;
    BlockInstance Instance;

    public Vector3Int Size
    {
        get => Instance?.Size ?? Vector3Int.zero;
    }

    public Vector3Int MinPos
    {
        get => Instance != null ? Pos + Instance.MinPos : Pos;
    }

    public Vector3Int MaxPos
    {
        get => Instance != null ? Pos + Instance.MaxPos : Pos;
    }

    public Vector3Int Pos
    {
        get
        {
            return _pos;
        }

        set
        {
            _pos = value;
            transform.localPosition = new Vector3(MassSize.x + _pos.x, MassSize.y + _pos.y, MassSize.z + _pos.z);
        }
    }

    public Direction Forward
    {
        get => _forward;
    }

    public Direction Up
    {
        get => _up;
    }

    public void SetTemplate(BlockTemplate template)
    {
        _BlockTemplate = template;
        CreateBlockGameObjects();
    }

    void RemoveAllMass()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(transform.childCount - 1);
            Object.Destroy(child.gameObject);
        }
    }

    void CreateBlockGameObjects()
    {
        RemoveAllMass();

        if (_BlockTemplate == null)
        {
            return;
        }

        Instance = _BlockTemplate.CreateBlockInstance();

        if (MassPrefab == null)
        {
            return;
        }

        for (int y = 0; y < Size.y; ++y)
        {
            for (int z = 0; z < Size.z; ++z)
            {
                for (int x = 0; x < Size.x; ++x)
                {
                    GameObject obj = Object.Instantiate(MassPrefab, transform);
                    obj.transform.localPosition = new Vector3(MassSize.x * (x - Instance.Origin.x), MassSize.y * (y - Instance.Origin.y), MassSize.z * (z - Instance.Origin.z));
                    var type = Instance[x, y, z];
                    obj.SetActive(type == MassType.Mass);
                }
            }
        }
    }

    public void FillMass(Grid grid, bool doRemove = false)
    {
        Vector3Int pos = Vector3Int.zero;

        for (pos.y = 0; pos.y < Size.y; ++pos.y)
        {
            for (pos.z = 0; pos.z < Size.z; ++pos.z)
            {
                for (pos.x = 0; pos.x < Size.x; ++pos.x)
                {
                    Vector3Int p = Instance.ToRotateCoordination(pos) + Pos;

                    if (grid.Contains(p) && Instance.Contains(pos) && Instance[pos] == MassType.Mass)
                    {
                        grid[p].Type = doRemove ? MassType.Empty : MassType.Mass;
                    }
                }
            }
        }
    }

    public bool IsTouch(Grid grid, Vector3Int offset)
    {
        Vector3Int minPos = MinPos;

        if (minPos.y + offset.y < 0)
        {
            return true;
        }

        Vector3Int pos = Vector3Int.zero;

        for (pos.y = 0; pos.y < Size.y; ++pos.y)
        {
            for (pos.z = 0; pos.z < Size.z; ++pos.z)
            {
                for (pos.x = 0; pos.x < Size.x; ++pos.x)
                {
                    Vector3Int p = Instance.ToRotateCoordination(pos) + Pos + offset;

                    if (Instance.Contains(pos) && Instance[pos] == MassType.Mass && grid.Contains(p) && grid[p].Type == MassType.Mass)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void Move(Grid grid, Vector3Int move)
    {
        if (IsTouch(grid, move))
        {
            return;
        }

        Vector3Int minPos = MinPos;
        Vector3Int maxPos = MaxPos;

        if (minPos.x + move.x < 0)
        {
            move.x -= minPos.x + move.x;
        }

        if (minPos.y + move.y < 0)
        {
            move.y -= minPos.y + move.y;
        }

        if (minPos.z + move.z < 0)
        {
            move.z -= minPos.z + move.z;
        }

        if (maxPos.x + move.x > grid.Size.x)
        {
            move.x -= maxPos.x + move.x - grid.Size.x;
        }

        if (maxPos.z + move.z > grid.Size.z)
        {
            move.z -= maxPos.z + move.z - grid.Size.z;
        }

        Vector3Int pos = Pos + move;
        pos.x = Mathf.Clamp(pos.x, 0, grid.Size.x - 1);
        pos.z = Mathf.Clamp(pos.z, 0, grid.Size.z - 1);
        Pos = pos;
    }

    public void DropMove(int moveY)
    {
        Vector3Int pos = Pos;
        pos.y -= moveY;
        Pos = pos;
    }

    public void RotateForward(Grid grid, bool isLeft)
    {
        Instance.RotateForward(isLeft);

        if (IsTouch(grid, Vector3Int.zero))
        {
            Instance.RotateForward(!isLeft);
        }
        else
        {
            _forward = Instance.Forward;
        }

        transform.localRotation = Instance.rotaMat.rotation;
    }

    public void RotateUp(Grid grid, bool isLeft)
    {
        Instance.RotateUp(isLeft);

        if (IsTouch(grid, Vector3Int.zero))
        {
            Instance.RotateUp(!isLeft);
        }
        else
        {
            _up = Instance.Up;
        }

        transform.localRotation = Instance.rotaMat.rotation;
    }

    private void Awake()
    {
        Pos = Pos;
        transform.localRotation = Instance?.rotaMat.rotation ?? Quaternion.identity;
    }
}
