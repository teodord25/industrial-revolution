namespace IndustrialRevolution.Entities;

public class SteamVolume
{
    private long totalVolumeInVoxels;
    private SteamVolume(long voxels) { totalVolumeInVoxels = voxels; }

    public static SteamVolume FromVoxels(long voxels)
    {
        return new SteamVolume(voxels);
    }

    public static SteamVolume? FromBlocks(decimal blocks)
    {
        // if the blocks arent a multiple of 1/(16*16*16) reject
        if (blocks * 4096 % 1 != 0) return null;
        return new SteamVolume((long)(blocks * 4096));
    }

    public long AsVoxels() => this.totalVolumeInVoxels;
    public decimal AsBlocks() => this.totalVolumeInVoxels / 4096;

    public void AddVoxels(long voxels) {
        this.totalVolumeInVoxels += voxels;
    }
}
