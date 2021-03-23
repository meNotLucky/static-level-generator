using System.Collections.Generic;
using LevelGenerator.Utility;
using UnityEngine;

namespace LevelGenerator.Generator
{
    [System.Serializable]
    public class GridRoom
    {
        public GameObject prefab;
        public List<ExitDirection> exitDirections = new List<ExitDirection>();
        public bool isEssential, hasFixedPosition;
        public Vector2 fixedPosition = Vector2.zero;
    }
}
