using System.Text;
using System;
using Vintagestory.API.MathTools;

namespace IndustrialRevolution.util;

public class SteamUtils
{
    public static string GridToString<T>(T[,,] array) where T : IConvertible
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
                    var val = array[k, i, j].ToByte(null);

                    // NOTE: swapped around ijk to align printing such that Y is
                    // the layers, Z is the rows, X is the elements (basically
                    // just aligned it with world axes)
                    sb.Append($"{val, 2}");
                    if (k < 15) sb.Append(",");
                }
                sb.AppendLine("]");
            }
        }

        return sb.ToString();
    }

    public static string PosAsLocal<T>(
        T pos, Vintagestory.API.Common.ICoreAPI api
    ) where T : BlockPos {
        return pos.ToLocalPosition(api).ToString();
    }
}
