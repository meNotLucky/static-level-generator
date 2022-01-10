using System.Collections.Generic;
using UnityEngine;

namespace LevelGenerator.Generator
{
    public enum ExitDirection { Top, Right, Bottom, Left }

    [System.Serializable]
    public class GridRoomLayout
    {
        public GameObject prefab;
        public List<ExitDirection> exitDirections = new List<ExitDirection>();
        public bool isEssential, isStartRoom;

        public bool HasAllOfExitDirections(params ExitDirection[] directions)
        {
            if (directions.Length == 0)
                return false;

            foreach (var dir in directions)
            {
                if (!exitDirections.Contains(dir))
                    return false;
            }

            return true;
        }

        public bool HasAnyOfExitDirections(params ExitDirection[] directions)
        {
            if (directions.Length == 0)
                return false;

            foreach (var dir in directions)
            {
                if (exitDirections.Contains(dir))
                    return true;
            }

            return false;
        }

        public bool HasExactExitDirections(params ExitDirection[] directions)
        {
            if (directions.Length == 0)
                return false;

            if (!HasAllOfExitDirections(directions))
                return false;

            if (exitDirections.Count != directions.Length)
                return false;

            return true;
        }
    }
}
