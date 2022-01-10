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

        public bool HasExitDirections(ExitDirection direction, params ExitDirection[] directions)
        {
            if (!exitDirections.Contains(direction))
                return false;

            foreach (var dir in directions)
            {
                if (!exitDirections.Contains(dir))
                    return false;
            }

            return true;
        }

        public bool HasAnyOfExitDirections(ExitDirection direction, params ExitDirection[] directions)
        {
            if (exitDirections.Contains(direction))
                return true;

            foreach (var dir in directions)
            {
                if (exitDirections.Contains(dir))
                    return true;
            }

            return false;
        }
    }
}
