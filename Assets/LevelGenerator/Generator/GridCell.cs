using System.Collections.Generic;
using LevelGenerator.Utility;
using UnityEditor;
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

        private GridRoomLayout _gridRoom;
        private bool _roomSet;

        public bool isConnected;
        
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

        public void InstantiateRoom(GridRoomLayout gridRoom)
        {
            if (!_cellObject)
                return;

            if(_roomObject)
            {
                if (EditorApplication.isPlaying)
                    Object.Destroy(_roomObject);
                else
                    Undo.DestroyObjectImmediate(_roomObject);
            }

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

        public GridRoomLayout GetRoom()
        {
            return _gridRoom;
        }

        public bool HasExit(ExitDirection exit)
        {
            return _gridRoom.HasAllOfExitDirections(exit);
        }

        public bool HasRoom()
        {
            return _roomSet;
        }
    }
}
