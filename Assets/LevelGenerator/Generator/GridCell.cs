using System.Collections.Generic;
using LevelGenerator.Utility;
using UnityEngine;

namespace LevelGenerator.Generator
{
    [System.Serializable]
    public class GridCell
    {
        private readonly Vector2 _gridPosition;
        private readonly List<GridCell> _neighbours = new List<GridCell>();
        private readonly GameObject _cellObject;
        private GameObject _roomObject;

        private GridRoom _gridRoom;
        private bool _roomSet;
        
        public GridCell(Transform parent, Vector2 gridPosition, Vector3 worldPosition, Vector3 worldRotation, Vector3 worldScale)
        {
            _gridPosition = gridPosition;
            var cellObject = new GameObject { name = "Cell_" + _gridPosition.x + "_" + gridPosition.y };
            
            cellObject.transform.parent = parent;
            cellObject.transform.position = worldPosition;
            cellObject.transform.eulerAngles = worldRotation;
            cellObject.transform.localScale = worldScale;

            _cellObject = cellObject;
            _roomSet = false;
        }

        public void AddNeighbour(GridCell neighbour)
        {
            _neighbours.Add(neighbour);
        }

        public void InstantiateRoom(GridRoom gridRoom)
        {
            if(_roomSet) Object.Destroy(_roomObject);
                
            _gridRoom = gridRoom;
            _roomSet = true;
            _roomObject = Object.Instantiate(gridRoom.prefab, _cellObject.transform);
        }

        public Vector2 GetGridPosition()
        {
            return _gridPosition;
        }

        public IEnumerable<GridCell> GetNeighbours()
        {
            return _neighbours;
        }

        public GameObject GetPrefab()
        {
            return _gridRoom.prefab;
        }

        public bool HasExit(ExitDirection exit)
        {
            return _gridRoom.exitDirections.Contains(exit);
        }

        public bool HasRoom()
        {
            return _roomSet;
        }
    }
}
