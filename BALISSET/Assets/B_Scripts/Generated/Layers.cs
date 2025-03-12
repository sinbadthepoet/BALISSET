// Auto-generated Layers class
// DO NOT MODIFY! Run 'Tools > Generate Layers & Tags' to regenerate.
// Create Layer Masks with (1 << Layer.LayerName1) | (1 << Layer.LayerName2).
public static class Layers
{
    public static readonly int Default = 0;
    public static readonly int TransparentFX = 1;
    public static readonly int IgnoreRaycast = 2;
    public static readonly int Interactive = 3;
    public static readonly int Water = 4;
    public static readonly int UI = 5;
    public static readonly int Environment = 6;
    public static readonly int BallisticsHitboxes = 7;

    public static int GetLayerMask(params int[] layers)
    {
        int mask = 0;
        foreach (int layer in layers)
        {
            mask |= 1 << layer;
        }
        return mask;
    }
}
