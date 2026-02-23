using UnityEngine;

public class BlockInstance
{
    public BlockTemplate Template { get; private set; }
    public Vector3Int Origin { get; private set; }
    public Vector3Int Size { get; private set; }
    public Vector3Int MinPos { get; private set; }
    public Vector3Int MaxPos { get; private set; }
    public Direction Forward { get; private set; } = Direction.Forward;
    public Direction Up { get; private set; } = Direction.Up;
    public Matrix4x4 rotaMat = Matrix4x4.identity;
    MassType[,,] _data;

    public MassType this[int x, int y, int z]
    {
        get
        {
            return _data[x, y, z];
        }

        set
        {
            _data[x, y, z] = value;
        }
    }

    public MassType this[Vector3Int pos]
    {
        get
        {
            return this[pos.x, pos.y, pos.z];
        }

        set
        {
            this[pos.x, pos.y, pos.z] = value;
        }
    }

    public bool Contains(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;
        return 0 <= x && x < _data.GetLength(0) && 0 <= y && y < _data.GetLength(1) && 0 <= z && z < _data.GetLength(2);
    }

    public Vector3Int ToRotateCoordination(Vector3Int pos)
    {
        return new Vector3Int(pos.x, pos.y, pos.z);
    }

    public BlockInstance(BlockTemplate template)
    {
        Template = template;
        CreateData(template);
    }

    (Vector3Int size, Vector3Int min, Vector3Int max) CalcSize(BlockTemplate template)
    {
        Vector3Int minPos = Vector3Int.zero;
        Vector3Int maxPos = Vector3Int.zero;
        maxPos.y = Template.Slices.Length;

        for (int y = 0; y < maxPos.y; ++y)
        {
            var lines = Template.Slices[y].Split('\n');
            maxPos.z = Mathf.Max(maxPos.z, lines.Length);

            for (int z = 0; z < lines.Length; ++z)
            {
                var line = lines[z];
                maxPos.x = Mathf.Max(maxPos.x, line.Length);
            }
        }

        return (maxPos, minPos, maxPos);
    }

    void CreateData(BlockTemplate template)
    {
        (Size, MinPos, MaxPos) = CalcSize(template);
        _data = new MassType[Size.x, Size.y, Size.z];
        Vector3Int origin = Vector3Int.zero;

        for (var y = 0; y < Size.y; ++y)
        {
            string[] lines = Template.Slices[y].Split('\n');

            for (int z = 0; z < lines.Length; ++z)
            {
                string line = lines[z];

                for (int x = 0; x < line.Length; ++x)
                {
                    MassType mass;

                    switch (line[x])
                    {
                        case '#':
                            mass = MassType.Mass;
                            break;
                        case 'o':
                            origin = new Vector3Int(x, y, z);
                            mass = MassType.Mass;
                            break;
                        default:
                            mass = MassType.Empty;
                            break;
                    }
                }
            }
        }

        Origin = origin;
        MinPos -= Origin;
        MaxPos -= Origin;
    }

    public void RotateForward(bool isLeft)
    {
        switch (Forward)
        {
            case Direction.Forward:
                switch (Up)
                {
                    case Direction.Left:
                        Forward = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Right:
                        Forward = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Up:
                        Forward = isLeft ? Direction.Left : Direction.Right;
                        break;
                    case Direction.Down:
                        Forward = isLeft ? Direction.Right : Direction.Left;
                        break;
                }

                break;

            case Direction.Back:
                switch (Up)
                {
                    case Direction.Left:
                        Forward = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Right:
                        Forward = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Up:
                        Forward = isLeft ? Direction.Right : Direction.Left;
                        break;
                    case Direction.Down:
                        Forward = isLeft ? Direction.Left : Direction.Right;
                        break;
                }

                break;

            case Direction.Left:
                switch (Up)
                {
                    case Direction.Up:
                        Forward = isLeft ? Direction.Back : Direction.Forward;
                        break;

                    case Direction.Down:
                        Forward = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Forward:
                        Forward = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Back:
                        Forward = isLeft ? Direction.Down : Direction.Up;
                        break;
                }

                break;

            case Direction.Right:
                switch (Up)
                {
                    case Direction.Up:
                        Forward = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Down:
                        Forward = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Forward:
                        Forward = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Back:
                        Forward = isLeft ? Direction.Up : Direction.Down;
                        break;
                }

                break;

            case Direction.Up:
                switch (Up)
                {
                    case Direction.Left:
                        Forward = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Right:
                        Forward = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Forward:
                        Forward = isLeft ? Direction.Right : Direction.Left;
                        break;
                    case Direction.Back:
                        Forward = isLeft ? Direction.Left : Direction.Right;
                        break;
                }

                break;

            case Direction.Down:
                switch (Up)
                {
                    case Direction.Left:
                        Forward = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Right:
                        Forward = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Forward:
                        Forward = isLeft ? Direction.Left : Direction.Right;
                        break;
                    case Direction.Back:
                        Forward = isLeft ? Direction.Right : Direction.Left;
                        break;
                }

                break;
        }

        int deg = isLeft ? -90 : 90;
        var minPos = DirectionUtils.Rotate(MinPos, deg, Up);
        var maxPos = DirectionUtils.Rotate(MaxPos - Vector3Int.one, deg, Up);
        MinPos = Vector3Int.Min(minPos, maxPos);
        MaxPos = Vector3Int.Max(minPos, maxPos) + Vector3Int.one;
        rotaMat = Matrix4x4.Rotate(Quaternion.AngleAxis(deg, DirectionUtils.GetDirVec(Up))) * rotaMat;
    }

    public void RotateUp(bool isLeft)
    {
        switch (Up)
        {
            case Direction.Left:
                switch (Forward)
                {
                    case Direction.Up:
                        Up = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Down:
                        Up = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Forward:
                        Up = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Back:
                        Up = isLeft ? Direction.Up : Direction.Down;
                        break;
                }

                break;

            case Direction.Right:
                switch (Forward)
                {
                    case Direction.Up:
                        Up = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Down:
                        Up = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Forward:
                        Up = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Back:
                        Up = isLeft ? Direction.Down : Direction.Up;
                        break;
                }

                break;

            case Direction.Up:
                switch (Forward)
                {
                    case Direction.Left:
                        Up = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Right:
                        Up = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Forward:
                        Up = isLeft ? Direction.Left : Direction.Right;
                        break;
                    case Direction.Back:
                        Up = isLeft ? Direction.Right : Direction.Left;
                        break;
                }

                break;

            case Direction.Down:
                switch (Forward)
                {
                    case Direction.Left:
                        Up = isLeft ? Direction.Forward : Direction.Back;
                        break;
                    case Direction.Right:
                        Up = isLeft ? Direction.Back : Direction.Forward;
                        break;
                    case Direction.Forward:
                        Up = isLeft ? Direction.Right : Direction.Left;
                        break;
                    case Direction.Back:
                        Up = isLeft ? Direction.Left : Direction.Right;
                        break;
                }

                break;

            case Direction.Forward:
                switch (Forward)
                {
                    case Direction.Left:
                        Up = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Right:
                        Up = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Up:
                        Up = isLeft ? Direction.Right : Direction.Left;
                        break;
                    case Direction.Down:
                        Up = isLeft ? Direction.Left : Direction.Right;
                        break;
                }

                break;

            case Direction.Back:
                switch (Forward)
                {
                    case Direction.Left:
                        Up = isLeft ? Direction.Down : Direction.Up;
                        break;
                    case Direction.Right:
                        Up = isLeft ? Direction.Up : Direction.Down;
                        break;
                    case Direction.Up:
                        Up = isLeft ? Direction.Left : Direction.Right;
                        break;
                    case Direction.Down:
                        Up = isLeft ? Direction.Right : Direction.Left;
                        break;
                }

                break;
        }

        int deg = isLeft ? -90 : 90;
        var minPos = DirectionUtils.Rotate(MinPos, deg, Forward);
        var maxPos = DirectionUtils.Rotate(MaxPos - Vector3Int.one, deg, Forward);
        MinPos = Vector3Int.Min(minPos, maxPos);
        MaxPos = Vector3Int.Max(minPos, maxPos) + Vector3Int.one;
        rotaMat = Matrix4x4.Rotate(Quaternion.AngleAxis(deg, DirectionUtils.GetDirVec(Forward))) * rotaMat;
    }
}
