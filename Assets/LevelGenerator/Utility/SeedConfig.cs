using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace LevelGenerator.Utility
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SeedConfig
    {
        [FieldOffset(0)] private Random.State state;
        [FieldOffset(0)] public uint v0;
        [FieldOffset(4)] public uint v1;
        [FieldOffset(8)] public uint v2;
        [FieldOffset(12)] public uint v3;
    
        public static implicit operator SeedConfig(Random.State aState) { return new SeedConfig { state = aState }; }
        public static implicit operator Random.State(SeedConfig aState) { return aState.state; }
    }
    
    public static class SeedConfigUtility
    {
        public static bool ValidateSeed(string seed)
        {
            var seedValid = true;
            var stringData = seed.Split('-');
            if (stringData.Length != 4) seedValid = false;
            if (stringData.Any(t => t.Length < 7 || t.Length > 10))
            {
                seedValid = false;
            }

            return seedValid;
        }

        public static uint[] ExtractSeedData(string seed)
        {
            var seedData = new uint[4];
            var stringData = seed.Split('-');
            for (var i = 0; i < stringData.Length; i++)
            {
                seedData[i] = uint.Parse(stringData[i]);
            }

            return seedData;
        }
    }
}
