using UnityEngine;

[CreateAssetMenu(menuName = "Block Template")]
public class BlockTemplate : ScriptableObject
{
    [TextArea(3, 5)] public string[] Slices;
    public Sprite Thumbnail;

    internal BlockInstance CreateBlockInstance()
    {
        return new BlockInstance(this);
    }
}
