using System.Collections.Generic;
using UnityEngine;

namespace LevelGenerator.Generator
{
    public enum ExitDirection { Top, Right, Bottom, Left }

    [System.Serializable]
    public class GridRoom
    {
        public GameObject prefab;
        public List<ExitDirection> exitDirections = new List<ExitDirection>();
        public bool isEssential, hasFixedPosition;
        public Vector2 fixedPosition = Vector2.zero;
    }
}
