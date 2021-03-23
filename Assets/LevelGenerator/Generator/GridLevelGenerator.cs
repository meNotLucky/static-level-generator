using System.Collections.Generic;
using System.Linq;
using LevelGenerator.Utility;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace LevelGenerator.Generator
{
    /// <summary>
    /// The Level Generator Component.
    /// </summary>
    public class GridLevelGenerator : MonoBehaviour
    {
        public string levelSeed = "";
    
        public int gridWidth = 5, gridHeight = 5;
        public int minLevelSize;
    
        public bool forcedLevelGeneration;

        public GridAlignment gridAlignment = GridAlignment.Horizontal;
    
        public Vector2 cellPositionOffset = new Vector2(1.0f, 1.0f);
    
        public Vector3 cellRotation;
        public Vector3 cellScale = new Vector3(1.0f, 1.0f, 1.0f);
    
        public Vector3 levelPosition;
        public Vector3 levelRotation;
        public Vector3 levelScale = new Vector3(1.0f, 1.0f, 1.0f);

        public List<GridRoom> roomTemplates = new List<GridRoom>();

        private readonly List<GridCell> _grid = new List<GridCell>();
        private readonly List<GridCell> _openCells = new List<GridCell>();

        private GameObject _levelObject;

        //private Cell _lastCell;

        private bool _essentialRoomFailed;
        private bool _cellRoomFailed;
    
        // --- PUBLIC FUNCTIONS --- //

        /// <summary>
        /// Generates a new level.
        /// </summary>
        public void GenerateNewLevel()
        {
            SeedStateWrapper data = Random.state;
            levelSeed = data.v0 + "-" + data.v1 + "-" + data.v2 + "-" + data.v3;

            InitiateLevelGeneration();
        }

        /// <summary>
        /// Generates Level From Seed.
        /// </summary>
        public void GenerateLevelFromSeed()
        {
            if (GridLevelUtility.ValidateSeed(levelSeed))
            {
                var seedData = GridLevelUtility.ExtractSeedData(levelSeed);
                var data = new SeedStateWrapper { v0 = seedData[0], v1 = seedData[1], v2 = seedData[2], v3 = seedData[3] };
                Random.state = data;   
            }
            else
            {
                SeedStateWrapper data = Random.state;
                levelSeed = data.v0 + "-" + data.v1 + "-" + data.v2 + "-" + data.v3;
                Debug.LogWarning("Seed format was invalid, generated new level from seed: " + levelSeed);
            }

            InitiateLevelGeneration();
        }

        /// <summary>
        /// Destroys the current level completely with all of the instantiated <see cref="GridRoom"/>s.
        /// </summary>
        /// <remarks>
        /// The levelSeed will not be deleted, but will be replaced once a new level is generated.
        /// </remarks>
        public void ClearLevel()
        {
            _essentialRoomFailed = false;
            _cellRoomFailed = false;
        
            if(EditorApplication.isPlaying)
                Destroy(_levelObject);
            else
                DestroyImmediate(_levelObject);
            
            _grid.Clear();
        }

        /// <summary>
        /// Sets the <see cref="levelSeed"/> for the <see cref="GridLevelGenerator"/>.
        /// </summary>
        /// <remarks>
        /// The seed format is defined by four sections of 7-10 numbers, divided by a dash (-).
        ///
        /// Example seed: 2885257376-2099986581-1044521005-723764510
        /// </remarks>
        /// <returns>true on success, false if seed is invalid</returns>
        /// <seealso><cref>levelSeed, GetSeed</cref></seealso>
        public bool SetSeed(string seed)
        {
            if (!GridLevelUtility.ValidateSeed(seed))
                return false;
        
            levelSeed = seed;
            return true;
        }

        /// <summary>
        /// Returns the <see cref="levelSeed"/> of the last generated level.
        /// </summary>
        /// <returns><see cref="levelSeed"/> string</returns>
        /// <seealso><cref>levelSeed, SetSeed</cref></seealso>
        public string GetSeed()
        {
            return levelSeed;
        }

        // --- PRIVATE FUNCTIONS --- //
    
        private void InitiateLevelGeneration()
        {
            while (!ValidateLevel())
            {
                ClearLevel();
                
                _levelObject = new GameObject { name = "Level" };
            
                GenerateGrid();  // note : generate the grid of cells
                GenerateLevel(); // note : generate the rooms of the level

                _levelObject.transform.position = levelPosition;
                _levelObject.transform.eulerAngles = levelRotation;
                _levelObject.transform.localScale = levelScale;
            }
        }

        private bool ValidateLevel()
        {
            // note : check that grid is created
            if (_grid.Count == 0)
                return false;
        
            // note : check that level is above minimum size
            var size = _grid.Count(cell => cell.HasRoom());
            if (size < minLevelSize)
                return false;

            // note : if forced level generation, check that all rooms spawned correctly
            if (forcedLevelGeneration)
            {
                if (_essentialRoomFailed || _cellRoomFailed)
                    return false;
            }

            return true;
        }

        private void GenerateGrid()
        {
            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    var worldPos = gridAlignment == GridAlignment.Horizontal ? new Vector3(x * cellPositionOffset.x, 0, y * cellPositionOffset.y)
                        : new Vector3(x * cellPositionOffset.x, y * cellPositionOffset.y, 0);
                    var newCell = new GridCell(new Vector2(x, y), worldPos, cellRotation, cellScale);
                    _grid.Add(newCell);
                }
            }

            for (var i = 0; i < (gridWidth * gridHeight); i++)
            {
                var x = (int)_grid[i].GetGridPosition().x;
                var y = (int)_grid[i].GetGridPosition().y;

                if (x < gridWidth - 1)	_grid[i].AddNeighbour(_grid[i + gridHeight]);
                if (y < gridHeight - 1)	_grid[i].AddNeighbour(_grid[i + 1]);
                if (x > 0)				_grid[i].AddNeighbour(_grid[i - gridHeight]);
                if (y > 0)				_grid[i].AddNeighbour(_grid[i - 1]);
            }
        }
        
        private GridCell GetCell(Vector2 position)
        {
            var index = 0;
            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    if (x == (int) position.x && y == (int) position.y)
                        return _grid[index];
                    index++;
                }
            }
            return null;
        }

        private void GenerateLevel()
        {
            // note : set the initial cell and room
            var startCellIndex = ((gridHeight * gridWidth) / 2) - (gridHeight / 2);
            var startCell = _grid[startCellIndex];
            //_lastCell = startCell;

            foreach (var room in roomTemplates)
            {
                if (room.exitDirections.Contains(ExitDirection.Top) &&
                    room.exitDirections.Contains(ExitDirection.Right) &&
                    room.exitDirections.Contains(ExitDirection.Left) &&
                    !room.exitDirections.Contains(ExitDirection.Bottom))
                {
                    //startCell.SetRoomColor(new Color(0f, 0.47f, 0f));
                    startCell.InstantiateRoom(room);
                }
            }
        
            // note : add essential rooms
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            GenerateEssentialRooms();
            //watch.Stop();
            //Debug.Log($"Execution Time: {watch.ElapsedMilliseconds} ms");
        
            // note : generate neighbour rooms for the starting cell
            GenerateNeighbourRooms(startCell);

            // note : generate random rooms for the rest of the level
            while (_openCells.Count > 0)
            {
                var currentlyGeneratingCells = new List<GridCell>(_openCells);
                _openCells.Clear();
                foreach (var cell in currentlyGeneratingCells)
                    GenerateNeighbourRooms(cell);
            }
        
            //_lastCell.SetRoomColor(new Color(0.67f, 0f, 0f));
        }

        private void GenerateEssentialRooms()
        {
            foreach (var room in roomTemplates.Where(room => room.isEssential).Where(room => room.hasFixedPosition))
            {
                var cell = GetCell(room.fixedPosition);
                if (cell == null)
                {
                    Debug.LogWarning("Essential room " + room.prefab.name + " has invalid fixed position " + room.fixedPosition);
                    continue;
                }
            
                if(cell.HasRoom())
                    Debug.LogWarning("Essential room " + room.prefab.name + " and " + cell.GetPrefab().name + " are both fixed to grid position " + room.fixedPosition + ", this has caused overlapping!");
            
                cell.InstantiateRoom(room);
                _openCells.Add(cell);
            }

            foreach (var room in roomTemplates.Where(room => room.isEssential).Where(room => !room.hasFixedPosition))
            {
                var validCells = GetValidCells(room);
                if (validCells.Count == 0)
                {
                    if(!forcedLevelGeneration)
                        Debug.LogWarning("Failed to add Essential room: " + room.prefab.name);
                    _essentialRoomFailed = true;
                    break;
                }
                
                var index = Random.Range(0, validCells.Count);
                var randomCell = validCells[index];

                randomCell.InstantiateRoom(room);
                _openCells.Add(randomCell);
            }
        }

        private void GenerateNeighbourRooms(GridCell gridCell)
        {
            if (!gridCell.HasRoom())
                return;
        
            foreach (var neighbour in gridCell.GetNeighbours())
            {
                if (neighbour.HasRoom())
                    continue;
            
                var neighbourGridPos = neighbour.GetGridPosition();
                var cellGridPos = gridCell.GetGridPosition();
            
                if ((neighbourGridPos.x > cellGridPos.x && gridCell.HasExit(ExitDirection.Right)) ||
                    (neighbourGridPos.x < cellGridPos.x && gridCell.HasExit(ExitDirection.Left)) ||
                    (neighbourGridPos.y > cellGridPos.y && gridCell.HasExit(ExitDirection.Top)) ||
                    (neighbourGridPos.y < cellGridPos.y && gridCell.HasExit(ExitDirection.Bottom)))
                    AddRandomRoom(neighbour);
            }
        }

        private void AddRandomRoom(GridCell parentGridCell)
        {
            if (parentGridCell.HasRoom())
                return;

            // note : find valid room
            var validRooms = GetValidRooms(parentGridCell);
            if (validRooms.Count == 0)
            {
                if(!forcedLevelGeneration)
                    Debug.LogWarning("Failed to add room to cell: " + parentGridCell.GetGridPosition());
            
                _cellRoomFailed = true;
                return;
            }

            var index = Random.Range(0, validRooms.Count);
            var room = validRooms[index];
        
            // note : set and spawn room
            parentGridCell.InstantiateRoom(room);
            //_lastCell = parentCell;
        
            // note : if room has open directions, add cell to open list
            if(room.exitDirections.Count > 1)
                _openCells.Add(parentGridCell);
        }
    
        private List<GridRoom> GetValidRooms(GridCell gridCell)
        {
            var mustInclude = new List<ExitDirection>();
            var mustExclude = new List<ExitDirection>();
        
            CalculateExitRequirements(gridCell, ref mustInclude, ref mustExclude);
            return roomTemplates.Where(room => !room.isEssential).Where(room => ValidateExitRequirements(room, mustInclude, mustExclude)).ToList();
        }

        private List<GridCell> GetValidCells(GridRoom gridRoom)
        {
            var validCells = new List<GridCell>();
            foreach (var cell in _grid.Where(cell => !cell.HasRoom()))
            {
                var mustInclude = new List<ExitDirection>();
                var mustExclude = new List<ExitDirection>();
                CalculateExitRequirements(cell, ref mustInclude, ref mustExclude);
            
                if(ValidateExitRequirements(gridRoom, mustInclude, mustExclude))
                    validCells.Add(cell);
            }

            return validCells;
        }
    
        private void CalculateExitRequirements(GridCell gridCell, ref List<ExitDirection> mustInclude, ref List<ExitDirection> mustExclude)
        {
            var gridPos = gridCell.GetGridPosition();

            foreach (var neighbour in gridCell.GetNeighbours())
            {
                if (!neighbour.HasRoom()) continue;
                var neighbourGridPos = neighbour.GetGridPosition();
                if (neighbourGridPos.x > gridPos.x && !neighbour.HasExit(ExitDirection.Left))        mustExclude.Add(ExitDirection.Right);
                else if (neighbourGridPos.x < gridPos.x && !neighbour.HasExit(ExitDirection.Right))  mustExclude.Add(ExitDirection.Left);
                else if (neighbourGridPos.y > gridPos.y && !neighbour.HasExit(ExitDirection.Bottom)) mustExclude.Add(ExitDirection.Top);
                else if (neighbourGridPos.y < gridPos.y && !neighbour.HasExit(ExitDirection.Top))    mustExclude.Add(ExitDirection.Bottom);
            
                if (neighbourGridPos.x > gridPos.x && neighbour.HasExit(ExitDirection.Left))         mustInclude.Add(ExitDirection.Right);
                else if (neighbourGridPos.x < gridPos.x && neighbour.HasExit(ExitDirection.Right))   mustInclude.Add(ExitDirection.Left);
                else if (neighbourGridPos.y > gridPos.y && neighbour.HasExit(ExitDirection.Bottom))  mustInclude.Add(ExitDirection.Top);
                else if (neighbourGridPos.y < gridPos.y && neighbour.HasExit(ExitDirection.Top))     mustInclude.Add(ExitDirection.Bottom);
            }
        
            if ((int)gridPos.y == gridHeight - 1) mustExclude.Add(ExitDirection.Top);
            if ((int)gridPos.x == gridWidth - 1) mustExclude.Add(ExitDirection.Right);
            if ((int)gridPos.y == 0) mustExclude.Add(ExitDirection.Bottom);
            if ((int)gridPos.x == 0) mustExclude.Add(ExitDirection.Left);
        }

        private bool ValidateExitRequirements(GridRoom gridRoom, IEnumerable<ExitDirection> mustInclude, IReadOnlyCollection<ExitDirection> mustExclude)
        {
            var exitsValid = true;
            foreach (var unused in mustExclude.Where(direction => gridRoom.exitDirections.Contains(direction)))  exitsValid = false;
            foreach (var unused in mustInclude.Where(direction => !gridRoom.exitDirections.Contains(direction))) exitsValid = false;
            if (gridRoom.exitDirections.Count == 1 && mustExclude.Count < 3) exitsValid = false;

            return exitsValid;
        }
    }
}