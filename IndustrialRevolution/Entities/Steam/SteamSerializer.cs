using System.Collections.Generic;
using System;

namespace IndustrialRevolution.Entities.Steam;

public static class SteamSerializer
{
    public static (
        byte[] coordsFull, byte[] chiselGrids
    ) Serialize(
        List<SteamPos> occupied
    )
    {
        List<int> coordsFull = new List<int>();
        List<byte> chiselGrids = new List<byte>();

        for (int i = 0; i < occupied.Count; i++)
        {
            var pos = occupied[i];
            coordsFull.AddRange([pos.X, pos.Y, pos.Z]);

            if (!pos.IsFullBlock)
            {
                if (pos.SteamGrid == null)
                {
                    throw new InvalidOperationException(
                        $"Non-fullblock at index {i} has no steam grid"
                    );
                }
                byte[] index = BitConverter.GetBytes(i);
                chiselGrids.AddRange(index);

                byte[] serializedGrid = new byte[16 * 16 * 16];
                Buffer.BlockCopy(pos.SteamGrid, 0, serializedGrid, 0, 4096);
                chiselGrids.AddRange(serializedGrid);
            }
        }

        byte[] serializedCoords = new byte[coordsFull.Count * 4];
        for (int i = 0; i < coordsFull.Count; i++)
        {
            byte[] intBytes = BitConverter.GetBytes(coordsFull[i]);
            Buffer.BlockCopy(intBytes, 0, serializedCoords, i * 4, 4);
        }

        return (
            serializedCoords,
            chiselGrids.ToArray()
        );
    }

    public static HashSet<SteamPos> Deserialize(
        byte[] coordsData, byte[] chiselData
    )
    {
        if (coordsData == null || coordsData.Length == 0)
            return new HashSet<SteamPos>();

        List<SteamPos> positions = new List<SteamPos>();

        for (int i = 0; i < coordsData.Length; i += 12) // 3 ints * 4 bytes
        {
            int x = BitConverter.ToInt32(coordsData, i);
            int y = BitConverter.ToInt32(coordsData, i + 4);
            int z = BitConverter.ToInt32(coordsData, i + 8);

            positions.Add(new SteamPos
            {
                X = x,
                Y = y,
                Z = z,
                IsFullBlock = true // default to full, override if chiseled
            });
        }

        if (chiselData != null && chiselData.Length > 0)
        {
            for (int i = 0; i < chiselData.Length; i += 4 + 4096)
            {
                int index = BitConverter.ToInt32(chiselData, i);
                bool[,,] steamGrid = new bool[16, 16, 16];
                Buffer.BlockCopy(chiselData, i + 4, steamGrid, 0, 4096);

                positions[index] = positions[index] with
                {
                    IsFullBlock = false,
                    SteamGrid = steamGrid,
                };
            }
        }

        return new HashSet<SteamPos>(positions);
    }
}
