using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public static class LayerTagGenerator
{
    private const string LAYERS_FILE_PATH = "Assets/Generated/Layers.cs";
    private const string TAGS_FILE_PATH = "Assets/Generated/Tags.cs";

    [MenuItem("Tools/Generate Layers and Tags")]
    public static void GenerateLayersAndTags()
    {
        GenerateLayers();
        GenerateTags();
        AssetDatabase.Refresh();
        Debug.Log("Layers.cs & Tags.cs have been regenerated!");
    }

    private static void GenerateLayers()
    {
        string directory = Path.GetDirectoryName(LAYERS_FILE_PATH);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// Auto-generated Layers class");
        sb.AppendLine("// DO NOT MODIFY! Run 'Tools > Generate Layers & Tags' to regenerate.");
        sb.AppendLine("// Create Layer Masks with (1 << Layer.LayerName1) | (1 << Layer.LayerName2).");
        sb.AppendLine("public static class Layers");
        sb.AppendLine("{");

        for (int i = 0; i < 32; i++) // Unity supports 32 layers
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                sb.AppendLine($"    public static readonly int {SanitizeName(layerName)} = {i};");
            }
        }

        sb.AppendLine("");
        sb.AppendLine("    public static int GetLayerMask(params int[] layers)");
        sb.AppendLine("    {");
        sb.AppendLine("        int mask = 0;");
        sb.AppendLine("        foreach (int layer in layers)");
        sb.AppendLine("        {");
        sb.AppendLine("            mask |= 1 << layer;");
        sb.AppendLine("        }");
        sb.AppendLine("        return mask;");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        File.WriteAllText(LAYERS_FILE_PATH, sb.ToString(), Encoding.UTF8);
    }

    private static void GenerateTags()
    {
        string directory = Path.GetDirectoryName(TAGS_FILE_PATH);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// Auto-generated Tags class");
        sb.AppendLine("// DO NOT MODIFY! Run 'Tools > Generate Layers & Tags' to regenerate.");
        sb.AppendLine("public static class Tags");
        sb.AppendLine("{");

        foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            sb.AppendLine($"    public const string {SanitizeName(tag)} = \"{tag}\";");
        }

        sb.AppendLine("}");
        File.WriteAllText(TAGS_FILE_PATH, sb.ToString(), Encoding.UTF8);
    }

    private static string SanitizeName(string name)
    {
        return name.Replace(" ", "").Replace("-", "_"); // Remove spaces & dashes for valid identifiers
    }
}
