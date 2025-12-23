using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

using System.Text;

using IndustrialRevolution.util;

namespace IndustrialRevolution.BlockEntities
{
    public class BlockEntityBoiler : BlockEntity
    {
        private ModLogger? log = IndustrialRevolutionModSystem.Logger;

        private float temperature = 20f;
        private const float MAX_TEMP = 1000f;
        private const float AMBIENT_TEMP = 20f;
        private const float HEAT_RATE = 2f;
        private const float COOL_RATE = 0.5f;

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            dsc.AppendLine($"Temperature: {temperature:F1}°C");

            if (temperature > 80f)
            {
                dsc.AppendLine("Status: Hot");
            }
            else if (temperature > 50f)
            {
                dsc.AppendLine("Status: Warm");
            }
            else
            {
                dsc.AppendLine("Status: Cold");
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            log?.Debug($"BlockEntityBoiler initialized at {Pos}");
            log?.Debug($"Boiler BE created at {Pos}");

            RegisterGameTickListener(OnGameTick, 1000); // every 1000ms
        }

        private void OnGameTick(float dt)
        {
            if (Api.Side != EnumAppSide.Server) return;

            BlockPos belowPos = Pos.DownCopy();
            Block blockBelow = Api.World.BlockAccessor.GetBlock(belowPos);

            bool isHeated = IsHeatSource(blockBelow, belowPos);

            if (isHeated)
            {
                temperature = Math.Min(temperature + HEAT_RATE, MAX_TEMP);
            }
            else
            {
                temperature = Math.Max(temperature - COOL_RATE, AMBIENT_TEMP);
            }

            // mark dirty if temperature changed significantly
            MarkDirty(false);
        }

        private bool IsHeatSource(Block block, BlockPos pos)
        {
            if (block == null || block.Code == null)
                return false;

            string blockCode = block.Code.Path;

            if (blockCode.Contains("firepit") && blockCode.Contains("lit"))
                return true;

            if (blockCode.Contains("forge") && blockCode.Contains("lit"))
                return true;

            if (blockCode.Contains("bloomery") && blockCode.Contains("lit"))
                return true;

            return false;
        }

        public float GetTemperature()
        {
            return temperature;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("temperature", temperature);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            temperature = tree.GetFloat("temperature", AMBIENT_TEMP);
        }
    }
}
