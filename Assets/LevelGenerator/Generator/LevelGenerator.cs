using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using LevelGenerator.Utility;

using EventHandler = LevelGenerator.Utility.EventHandler;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LevelGenerator.Generator
{
    /// <summary>
    /// The static Level Generator.
    /// </summary>
    public static class LevelGenerator
    {
        private static GeneratorConfig _config = ScriptableObject.CreateInstance<GeneratorConfig>();
        private static SceneCache _localCache = ScriptableObject.CreateInstance<SceneCache>();

        private static readonly List<GridCell> Grid = new List<GridCell>();
        private static readonly List<GridCell> OpenCells = new List<GridCell>();

        //private Cell _lastCell;

        private static bool _essentialRoomFailed;
        private static bool _cellRoomFailed;
    
        // --- PUBLIC FUNCTIONS --- //

        public static void SetConfiguration(GeneratorConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Generates a new level and from a new seed.
        /// </summary>
        /// <example>
        /// This sample shows how to call the <see cref="GenerateNewLevel"/> method from an ExampleClass component on a GameObject that also has a <see cref="LevelGenerator"/> component.
        /// <code>
        /// using LevelGenerator.Generator;
        /// 
        /// class ExampleClass : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         LevelGenerator.GenerateNewLevel();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void GenerateNewLevel()
        {
            Undo.RegisterCompleteObjectUndo(_localCache, "Updated scene cache");

            SeedConfig data = Random.state;
            _localCache.editModeCache.levelSeed = data.v0 + "-" + data.v1 + "-" + data.v2 + "-" + data.v3;
            
            InitiateLevelGeneration();
        }

        /// <summary>
        /// Generates a Level from the current seed. If the seed is invalid it will generate a new level.
        /// </summary>
        /// <seealso cref="GenerateNewLevel"/>
        public static void GenerateLevelFromSeed()
        {
            Undo.RegisterCompleteObjectUndo(_localCache, "Updated scene cache");

            if (SeedConfigUtility.ValidateSeed(_localCache.editModeCache.levelSeed))
            {
                var seedData = SeedConfigUtility.ExtractData(_localCache.editModeCache.levelSeed);
                var data = new SeedConfig { v0 = seedData[0], v1 = seedData[1], v2 = seedData[2], v3 = seedData[3] };
                Random.state = data;
                InitiateLevelGeneration();
            }
            else
            {
                Debug.LogWarning("Seed format was invalid, generated new level from seed: " + _localCache.editModeCache.levelSeed);
                GenerateNewLevel();
            }
        }

        /// <summary>
        /// Destroys the current level completely with all of the instantiated <see cref="GridRoom"/>s.
        /// </summary>
        /// <remarks>
        /// The levelSeed will not be deleted, but will be replaced once a new level is generated.
        /// </remarks>
        public static void ClearLevel()
        {
            var level = GameObject.Find(_localCache.editModeCache.levelObjectID);
            if (level == null) return;

            if (EditorApplication.isPlaying)
                Object.Destroy(level);
            else
            {
                Undo.DestroyObjectImmediate(level);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            
            _essentialRoomFailed = false;
            _cellRoomFailed = false;
            
            Grid.Clear();
        }

        /// <summary>
        /// Sets the seed for the <see cref="LevelGenerator"/>.
        /// </summary>
        /// <param name="seed">Level Seed string</param>
        /// <returns>true on success, false if seed is invalid</returns>
        /// <remarks>
        /// The seed format is defined by four sections of 6-10 numbers, divided by a dash (-).<br/>
        /// Example seed: 2885257376-2099986581-1044521005-723764510
        /// </remarks>
        /// <seealso cref="GetSeed"/>
        public static bool SetSeed(string seed)
        {
            if (!SeedConfigUtility.ValidateSeed(seed))
                return false;
        
            _localCache.editModeCache.levelSeed = seed;
            return true;
        }

        /// <summary>
        /// Returns the seed of the last generated level.
        /// </summary>
        /// <returns>Level Seed string</returns>
        /// <seealso cref="SetSeed"/>
        /// <seealso cref="GenerateLevelFromSeed"/>
        public static string GetSeed()
        {
            return _localCache ? _localCache.editModeCache.levelSeed : "";
        }

        /// <summary>
        /// Enables scene caching for saving info on generated levels.
        /// </summary>
        /// <seealso cref="DisableCaching"/>
        public static void EnableCaching()
        {
            if (_config.disableSceneCaching) return;
            DisableCaching();
            EditorSceneManager.sceneOpened += CacheOnSceneOpened;
            EditorSceneManager.sceneSaved += CacheOnSceneSaved;
            EventHandler.generatorRecompiled += CacheOnGeneratorRecompiled;
            EditorApplication.playModeStateChanged += CacheOnPlayModeStateChange;
            Undo.undoRedoPerformed += CacheOnUndoRedoPerformed;
        }

        /// <summary>
        /// Disables scene caching for saving info on generated levels.
        /// </summary>
        /// <seealso cref="EnableCaching"/>
        public static void DisableCaching()
        {
            EditorSceneManager.sceneOpened -= CacheOnSceneOpened;
            EditorSceneManager.sceneSaved -= CacheOnSceneSaved;
            EventHandler.generatorRecompiled -= CacheOnGeneratorRecompiled;
            EditorApplication.playModeStateChanged -= CacheOnPlayModeStateChange;
            Undo.undoRedoPerformed -= CacheOnUndoRedoPerformed;
        }
        
        public static void ClearCache()
        {
            var cache = Directory.GetFiles("Library/LevelGeneratorCache/");
            foreach (var file in cache)
                File.Delete(file);
        }

        // --- PRIVATE FUNCTIONS --- //
        
        // -- GENERATING

        private static void InitiateLevelGeneration()
        {
            var levelValid = false;
            while (!levelValid)
            {
                ClearLevel();

                var data = SeedConfigUtility.ExtractData(_localCache.editModeCache.levelSeed);
                _localCache.editModeCache.levelObjectID = data[0].ToString();
                var level = new GameObject() { name = _localCache.editModeCache.levelObjectID };
                Undo.RegisterCreatedObjectUndo(level, "Created new level");

                GenerateGrid();
                GenerateRooms();

                level.transform.position = _config.levelPosition;
                level.transform.eulerAngles = _config.levelRotation;
                level.transform.localScale = _config.levelScale;

                levelValid = ValidateLevel();
            }

            if (Application.isPlaying)
                return;
            
            var scene = SceneManager.GetActiveScene();
            _localCache.sceneID = AssetDatabase.AssetPathToGUID(scene.path);
            EditorSceneManager.MarkSceneDirty(scene);
            
            // EXPERIMENTAL
            
            if(_config.automaticSave)
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            if(_config.automaticOcclusionCulling)
                StaticOcclusionCulling.Compute();
        }

        private static bool ValidateLevel()
        {
            // note : check that grid is created
            if (Grid.Count == 0)
                return false;
        
            // note : check that level is above minimum size
            var size = Grid.Count(cell => cell.HasRoom());
            if (size < _config.minLevelSize)
                return false;

            // note : if forced level generation, check that all rooms spawned correctly
            if (_config.forcedLevelGeneration)
            {
                if (_essentialRoomFailed || _cellRoomFailed)
                    return false;
            }

            return true;
        }

        private static void GenerateGrid()
        {
            for (var x = 0; x < _config.gridWidth; x++)
            {
                for (var y = 0; y < _config.gridHeight; y++)
                {
                    var worldPos = _config.gridAlignment == GridAlignment.Horizontal ? new Vector3(x * _config.cellPositionOffset.x, 0, y * _config.cellPositionOffset.y)
                        : new Vector3(x * _config.cellPositionOffset.x, y * _config.cellPositionOffset.y, 0);
                    var newCell = new GridCell(GameObject.Find(_localCache.editModeCache.levelObjectID).transform, new Vector2(x, y), worldPos, _config.cellRotation, _config.cellScale);
                    Grid.Add(newCell);
                }
            }

            for (var i = 0; i < (_config.gridWidth * _config.gridHeight); i++)
            {
                var x = (int)Grid[i].GetGridPosition().x;
                var y = (int)Grid[i].GetGridPosition().y;

                if (x < _config.gridWidth - 1) Grid[i].AddNeighbour(Grid[i + _config.gridHeight]);
                if (y < _config.gridHeight - 1) Grid[i].AddNeighbour(Grid[i + 1]);
                if (x > 0) Grid[i].AddNeighbour(Grid[i - _config.gridHeight]);
                if (y > 0) Grid[i].AddNeighbour(Grid[i - 1]);
            }
        }
        
        private static GridCell GetCell(Vector2 position)
        {
            var index = 0;
            for (var x = 0; x < _config.gridWidth; x++)
            {
                for (var y = 0; y < _config.gridHeight; y++)
                {
                    if (x == (int) position.x && y == (int) position.y)
                        return Grid[index];
                    index++;
                }
            }
            return null;
        }

        private static void GenerateRooms()
        {
            // note : set the initial cell and room
            var startCell = GetCell(new Vector2(_config.gridWidth / 2, 0));
            //_lastCell = startCell;
            
            foreach (var room in _config.roomTemplates)
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
            while (OpenCells.Count > 0)
            {
                var currentlyGeneratingCells = new List<GridCell>(OpenCells);
                OpenCells.Clear();
                foreach (var cell in currentlyGeneratingCells)
                    GenerateNeighbourRooms(cell);
            }
            
            //_lastCell.SetRoomColor(new Color(0.67f, 0f, 0f));
        }

        private static void GenerateEssentialRooms()
        {
            // note : fixed positions
            foreach (var room in _config.roomTemplates.Where(room => room.isEssential).Where(room => room.hasFixedPosition))
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
                OpenCells.Add(cell);
            }

            // note : loose positions
            foreach (var room in _config.roomTemplates.Where(room => room.isEssential).Where(room => !room.hasFixedPosition))
            {
                var validCells = GetValidCells(room);
                if (validCells.Count == 0)
                {
                    if(!_config.forcedLevelGeneration)
                        Debug.LogWarning("Failed to add Essential room: " + room.prefab.name);
                    _essentialRoomFailed = true;
                    break;
                }
                
                var index = Random.Range(0, validCells.Count);
                var randomCell = validCells[index];

                randomCell.InstantiateRoom(room);
                OpenCells.Add(randomCell);
            }
        }

        private static void GenerateNeighbourRooms(GridCell gridCell)
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

        private static void AddRandomRoom(GridCell parentGridCell)
        {
            if (parentGridCell.HasRoom())
                return;

            // note : find valid room
            var validRooms = GetValidRooms(parentGridCell);
            if (validRooms.Count == 0)
            {
                if(!_config.forcedLevelGeneration)
                    Debug.LogWarning("Failed to add room to cell: " + parentGridCell.GetGridPosition() + ". No template that fits can be found.");
            
                _cellRoomFailed = true;
                return;
            }

            // todo: mess around with room spawn chance
            
            var index = Random.Range(0, validRooms.Count);
            var room = validRooms[index];
            foreach (var vRoom in validRooms)
            {
                var chance = Random.Range(0, 10);
                if (vRoom.exitDirections.Contains(ExitDirection.Top) && vRoom.exitDirections.Contains(ExitDirection.Bottom) && !vRoom.exitDirections.Contains(ExitDirection.Left) && !vRoom.exitDirections.Contains(ExitDirection.Right) ||
                    !vRoom.exitDirections.Contains(ExitDirection.Top) && !vRoom.exitDirections.Contains(ExitDirection.Bottom) && vRoom.exitDirections.Contains(ExitDirection.Left) && vRoom.exitDirections.Contains(ExitDirection.Right) ||
                    vRoom.exitDirections.Count == 1)
                {
                    if (chance < 2) continue;
                    room = vRoom;
                }
            }

            // note : set and spawn room
            parentGridCell.InstantiateRoom(room);
            //_lastCell = parentCell;
        
            // note : if room has open directions, add cell to open list
            if(room.exitDirections.Count > 1)
                OpenCells.Add(parentGridCell);
        }
    
        private static List<GridRoom> GetValidRooms(GridCell gridCell)
        {
            var mustInclude = new List<ExitDirection>();
            var mustExclude = new List<ExitDirection>();
        
            CalculateExitRequirements(gridCell, ref mustInclude, ref mustExclude);
            return _config.roomTemplates.Where(room => !room.isEssential).Where(room => ValidateExitRequirements(room, mustInclude, mustExclude)).ToList();
        }

        private static List<GridCell> GetValidCells(GridRoom gridRoom)
        {
            var validCells = new List<GridCell>();
            foreach (var cell in Grid.Where(cell => !cell.HasRoom()))
            {
                var mustInclude = new List<ExitDirection>();
                var mustExclude = new List<ExitDirection>();
                CalculateExitRequirements(cell, ref mustInclude, ref mustExclude);
            
                if(ValidateExitRequirements(gridRoom, mustInclude, mustExclude))
                    validCells.Add(cell);
            }

            return validCells;
        }
    
        private static void CalculateExitRequirements(GridCell gridCell, ref List<ExitDirection> mustInclude, ref List<ExitDirection> mustExclude)
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
        
            if ((int)gridPos.y == _config.gridHeight - 1) mustExclude.Add(ExitDirection.Top);
            if ((int)gridPos.x == _config.gridWidth - 1) mustExclude.Add(ExitDirection.Right);
            if ((int)gridPos.y == 0) mustExclude.Add(ExitDirection.Bottom);
            if ((int)gridPos.x == 0) mustExclude.Add(ExitDirection.Left);
        }

        private static bool ValidateExitRequirements(GridRoom gridRoom, IEnumerable<ExitDirection> mustInclude, IReadOnlyCollection<ExitDirection> mustExclude)
        {
            var exitsValid = true;
            foreach (var unused in mustExclude.Where(direction => gridRoom.exitDirections.Contains(direction)))  exitsValid = false;
            foreach (var unused in mustInclude.Where(direction => !gridRoom.exitDirections.Contains(direction))) exitsValid = false;
            //if (gridRoom.exitDirections.Count == 1 && mustExclude.Count < 3) exitsValid = false;

            return exitsValid;
        }
        
        // -- CACHING FUNCTIONALITY

        private static bool ApplyDeserializedCache()
        {
            var cache = SceneCacheUtility.DeserializeCache(SceneManager.GetActiveScene());
            if (!cache) return false;
            
            _localCache = cache;
            return true;
        }

        private static void SerializeLocalCache()
        {
            SceneCacheUtility.SerializeCache(_localCache);
        }

        private static void CreatePlayModeCache()
        {
            _localCache.playModeCache = _localCache.editModeCache;
            SerializeLocalCache();
        }

        // -- CACHING EVENT DELEGATES
        
        private static void CacheOnSceneOpened(Scene scene, OpenSceneMode openSceneMode)
        {
            if (ApplyDeserializedCache()) return;
            
            var cache = ScriptableObject.CreateInstance<SceneCache>();
            cache.sceneID = AssetDatabase.AssetPathToGUID(scene.path);
            cache.editModeCache = new CacheData();
            cache.playModeCache = new CacheData();
            _localCache = cache;
            SceneCacheUtility.SerializeCache(cache);
        }
        
        private static void CacheOnSceneSaved(Scene scene)
        {
            SerializeLocalCache();
        }

        private static void CacheOnGeneratorRecompiled()
        {
            ApplyDeserializedCache();
        }

        private static void CacheOnPlayModeStateChange(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
            case PlayModeStateChange.ExitingEditMode:
                CreatePlayModeCache();
                break;
            case PlayModeStateChange.EnteredPlayMode:
            case PlayModeStateChange.EnteredEditMode:
                ApplyDeserializedCache();
                break;
            }
        }

        private static void CacheOnUndoRedoPerformed()
        {
            Undo.FlushUndoRecordObjects();
            SerializeLocalCache();
        }
    }
}