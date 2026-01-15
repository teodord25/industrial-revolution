using IndustrialRevolution.Entities.Steam;

namespace IndustrialRevolution.Tests;

public class SteamSerializerTests
{
    [Fact]
    public void RoundTrip_EmptyList_ReturnsEmpty()
    {
        var empty = new List<SteamPos>();

        var (coords, chisel) = SteamSerializer.Serialize(empty);
        var result = SteamSerializer.Deserialize(coords, chisel);

        Assert.Empty(result);
    }

    [Fact]
    public void RoundTrip_SingleFullBlock_PreservesData()
    {
        var original = new List<SteamPos>
        {
            new SteamPos { X = 10, Y = 20, Z = 30, IsFullBlock = true }
        };

        var (coords, chisel) = SteamSerializer.Serialize(original);
        var result = SteamSerializer.Deserialize(coords, chisel);

        Assert.Single(result);
        var pos = result.First();
        Assert.Equal(10, pos.X);
        Assert.Equal(20, pos.Y);
        Assert.Equal(30, pos.Z);
        Assert.True(pos.IsFullBlock);
        Assert.Null(pos.SteamGrid);
    }

    [Fact]
    public void RoundTrip_MultipleFullBlocks_PreservesAll()
    {
        var original = new List<SteamPos>
        {
            new SteamPos { X = 0, Y = 0, Z = 0, IsFullBlock = true },
            new SteamPos { X = 1, Y = 2, Z = 3, IsFullBlock = true },
            new SteamPos { X = -5, Y = 100, Z = 42, IsFullBlock = true },
        };

        var (coords, chisel) = SteamSerializer.Serialize(original);
        var result = SteamSerializer.Deserialize(coords, chisel);

        Assert.Equal(3, result.Count);
        Assert.Contains(new SteamPos { X = 0, Y = 0, Z = 0, IsFullBlock = true }, result);
        Assert.Contains(new SteamPos { X = 1, Y = 2, Z = 3, IsFullBlock = true }, result);
        Assert.Contains(new SteamPos { X = -5, Y = 100, Z = 42, IsFullBlock = true }, result);
    }

    [Fact]
    public void RoundTrip_SingleChiseledBlock_PreservesGrid()
    {
        bool[,,] grid = new bool[16, 16, 16];
        grid[0, 0, 0] = true;
        grid[15, 15, 15] = true;
        grid[8, 8, 8] = true;

        var original = new List<SteamPos>
        {
            new SteamPos { X = 5, Y = 5, Z = 5, IsFullBlock = false, SteamGrid = grid }
        };

        var (coords, chisel) = SteamSerializer.Serialize(original);
        var result = SteamSerializer.Deserialize(coords, chisel);

        Assert.Single(result);
        var pos = result.First();
        Assert.Equal(5, pos.X);
        Assert.Equal(5, pos.Y);
        Assert.Equal(5, pos.Z);
        Assert.False(pos.IsFullBlock);
        Assert.NotNull(pos.SteamGrid);
        Assert.True(pos.SteamGrid[0, 0, 0]);
        Assert.True(pos.SteamGrid[15, 15, 15]);
        Assert.True(pos.SteamGrid[8, 8, 8]);
        Assert.False(pos.SteamGrid[1, 1, 1]);
    }

    [Fact]
    public void RoundTrip_MixedBlocks_PreservesAllData()
    {
        bool[,,] grid1 = new bool[16, 16, 16];
        grid1[0, 0, 0] = true;

        bool[,,] grid2 = new bool[16, 16, 16];
        grid2[15, 15, 15] = true;

        var original = new List<SteamPos>
        {
            new SteamPos { X = 0, Y = 0, Z = 0, IsFullBlock = true },
            new SteamPos { X = 1, Y = 1, Z = 1, IsFullBlock = false, SteamGrid = grid1 },
            new SteamPos { X = 2, Y = 2, Z = 2, IsFullBlock = true },
            new SteamPos { X = 3, Y = 3, Z = 3, IsFullBlock = false, SteamGrid = grid2 },
        };

        var (coords, chisel) = SteamSerializer.Serialize(original);
        var result = SteamSerializer.Deserialize(coords, chisel);

        Assert.Equal(4, result.Count);

        // Check full blocks
        Assert.Contains(result, p => p.X == 0 && p.Y == 0 && p.Z == 0 && p.IsFullBlock);
        Assert.Contains(result, p => p.X == 2 && p.Y == 2 && p.Z == 2 && p.IsFullBlock);

        // Check chiseled blocks
        var chiseled1 = result.First(p => p.X == 1 && p.Y == 1 && p.Z == 1);
        Assert.False(chiseled1.IsFullBlock);
        Assert.True(chiseled1.SteamGrid?[0, 0, 0]);

        var chiseled2 = result.First(p => p.X == 3 && p.Y == 3 && p.Z == 3);
        Assert.False(chiseled2.IsFullBlock);
        Assert.True(chiseled2.SteamGrid?[15, 15, 15]);
    }

    [Fact]
    public void Serialize_ChiseledWithoutGrid_Throws()
    {
        var invalid = new List<SteamPos>
        {
            new SteamPos { X = 0, Y = 0, Z = 0, IsFullBlock = false, SteamGrid = null }
        };

        Assert.Throws<InvalidOperationException>(() =>
            SteamSerializer.Serialize(invalid)
        );
    }

    [Fact]
    public void Deserialize_NullCoords_ReturnsEmpty()
    {
        var result = SteamSerializer.Deserialize(null, null);
        Assert.Empty(result);
    }

    [Fact]
    public void Serialize_CoordsByteArraySize_IsCorrect()
    {
        var positions = new List<SteamPos>
        {
            new SteamPos { X = 1, Y = 2, Z = 3, IsFullBlock = true },
            new SteamPos { X = 4, Y = 5, Z = 6, IsFullBlock = true },
        };

        var (coords, _) = SteamSerializer.Serialize(positions);

        // 2 positions * 3 coords * 4 bytes per int
        Assert.Equal(24, coords.Length);
    }

    [Fact]
    public void Serialize_ChiselByteArraySize_IsCorrect()
    {
        bool[,,] grid = new bool[16, 16, 16];
        var positions = new List<SteamPos>
        {
            new SteamPos { X = 0, Y = 0, Z = 0, IsFullBlock = false, SteamGrid = grid }
        };

        var (_, chisel) = SteamSerializer.Serialize(positions);

        // 4 bytes (index) + 4096 bytes (grid)
        Assert.Equal(4100, chisel.Length);
    }
}
