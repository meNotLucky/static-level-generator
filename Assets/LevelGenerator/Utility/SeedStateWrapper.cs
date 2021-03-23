using System.Runtime.InteropServices;
using UnityEngine;

namespace LevelGenerator.Utility
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SeedStateWrapper
    {
        [FieldOffset(0)] private Random.State state;
        [FieldOffset(0)] public uint v0;
        [FieldOffset(4)] public uint v1;
        [FieldOffset(8)] public uint v2;
        [FieldOffset(12)] public uint v3;
    
        public static implicit operator SeedStateWrapper(Random.State aState) { return new SeedStateWrapper { state = aState }; }
        public static implicit operator Random.State(SeedStateWrapper aState) { return aState.state; }
    }
}
