using System.Text;

namespace IndustrialRevolution.util;

public class SteamUtils
{
    public static string GridToString(byte[,,] array)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"VOXEL PRINTER GO BRR");
        for (int i = 15; i >= 0; i--) // invert y so its top down
        {
            sb.AppendLine($"Layer {i}:");
            for (int j = 0; j < 16; j++)
            {
                sb.Append("  [");
                for (int k = 0; k < 16; k++)
                {
                    // NOTE: swapped around ijk to align printing such that Y is
                    // the layers, Z is the rows, X is the elements (basically
                    // just aligned it with world axes)
                    sb.Append($"{array[k, i, j], 2}");
                    if (k < 15) sb.Append(",");
                }
                sb.AppendLine("]");
            }
        }

        return sb.ToString();
    }
}
