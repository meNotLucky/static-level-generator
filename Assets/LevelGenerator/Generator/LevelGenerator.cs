﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using LevelGenerator.Utility;

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
        /// Destroys the current level completely with all of the instantiated <see cref="GridRoomLayout"/>s.
        /// </summary>
        /// <remarks>
        /// The levelSeed will not be deleted, but will be replaced once a new level is generated.
        /// </remarks>
        public static void ClearLevel()
        {
            var level = GameObject.Find(_localCache.editModeCache.levelObjectID);
            if (level)
            {
                if (EditorApplication.isPlaying)
                    Object.Destroy(level);
                else
                {
                    Undo.DestroyObjectImmediate(level);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
            else
            {
                Debug.LogError("Failed to find cached object: " + _localCache.editModeCache.levelObjectID);
            }

            _essentialRoomFailed = false;
            _cellRoomFailed = false;
            
            OpenCells.Clear();
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

            int validationTimeOut = 0;
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

                bool gridSuccess = true, minMaxSizeSucess = true, allRoomsSucess = true;
                levelValid = ValidateLevel(ref gridSuccess, ref minMaxSizeSucess, ref allRoomsSucess);
                if(!levelValid)
                    validationTimeOut++;

                if (validationTimeOut > 200)
                {
                    Debug.LogError("Partial generation fail! Generator timed out since over 200 level iterations failed validation. Errors:\n");
                    if (!gridSuccess)
                        Debug.LogError("Validation Error: Grid could not be created!");
                    if (!minMaxSizeSucess)
                        Debug.LogError("Validation Error: Level size within minimum and maximum parameters could not be reached with current configuration!");
                    if (!allRoomsSucess)
                        Debug.LogError("Validation Error: One or more rooms failed to spawn! Make sure all rooms marked 'essential' align with your current configuration.");
                    return;
                }
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

        private static bool ValidateLevel(ref bool gridSucceeded, ref bool minMaxSizeSucceeded, ref bool allRoomsSucceeded)
        {
            // note : check that grid is created
            if (Grid.Count == 0)
                gridSucceeded = false;

            // note : check that level is above minimum size
            var size = Grid.Count(cell => cell.HasRoom());
            if (size < _config.minLevelSize || size > _config.maxLevelSize)
                minMaxSizeSucceeded = false;

            // note : check that all rooms spawned correctly
            if (_essentialRoomFailed || _cellRoomFailed)
                allRoomsSucceeded = false;

            return (gridSucceeded && minMaxSizeSucceeded && allRoomsSucceeded);
        }

        private static void GenerateGrid()
        {
            for (var x = 0; x < _config.gridWidth; x++)
            {
                for (var y = 0; y < _config.gridHeight; y++)
                {
                    var worldPos = _config.gridAlignment == GridAlignment.Horizontal ? new Vector3(x * _config.cellPositionOffset.x, 0, y * _config.cellPositionOffset.y)
                                                                                           : new Vector3(x * _config.cellPositionOffset.x, y * _config.cellPositionOffset.y, 0);
                    var levelObject = GameObject.Find(_localCache.editModeCache.levelObjectID);
                    var newCell = new GridCell(levelObject.transform, new Vector2(x, y), worldPos, _config.cellRotation, _config.cellScale);
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
            if (_config is null) return;

            // note : set the initial cell and room
            var startCell = GetCell(new Vector2(_config.gridWidth / 2, _config.gridHeight / 2));

            var validStartRooms = _config.roomTemplates.Where(room => room.isStartRoom).ToArray();
            if (validStartRooms.Length == 0)
                validStartRooms = _config.roomTemplates.Where(room => !room.isEssential).ToArray();

            var index = Random.Range(0, validStartRooms.Length);
            var startRoom = validStartRooms[index];

            startCell.InstantiateRoom(startRoom);

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

            // note : add essential rooms
            GenerateEssentialRooms();

            //_lastCell.SetRoomColor(new Color(0.67f, 0f, 0f));
        }

        private static void GenerateEssentialRooms()
        {
            foreach (var room in _config.roomTemplates.Where(room => room.isEssential))
            {
                var validCells = GetRoomsForEssentialReplacement(room);
                if (validCells.Count == 0)
                {
                    _essentialRoomFailed = true;
                    break;
                }
                
                var index = Random.Range(0, validCells.Count);
                var randomCell = validCells[index];

                randomCell.InstantiateRoom(room);
                //OpenCells.Add(randomCell);
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

            foreach (var cell in parentGridCell.GetNeighbours().Where(cell => cell.HasRoom() && cell.GetRoom().isEssential))
            {
                cell.isConnected = true;
                OpenCells.Add(cell);
            }
            
            // note : find valid room
            var validRooms = GetValidRooms(parentGridCell);
            if (validRooms.Count == 0)
            {
                Debug.LogWarning("Failed to add room to cell: " + parentGridCell.GetGridPosition() + ". No template that fits can be found.");            
                _cellRoomFailed = true;
                return;
            }

            // todo: mess around with room spawn chance
            
            var index = Random.Range(0, validRooms.Count);
            var room = validRooms[index];

            float lowestDensity = (float)_config.minLevelSize / (float)(_config.gridWidth * _config.gridHeight) * 30.0f;
            int density = Mathf.Clamp(_config.levelDensity + (int)lowestDensity, 0, 100);

            bool roomAccepted = false;
            foreach (var vRoom in validRooms)
            {
                var chance = Random.Range(0, 100);

                if ((vRoom.HasAllOfExitDirections(ExitDirection.Top, ExitDirection.Bottom) && !vRoom.HasAnyOfExitDirections(ExitDirection.Right, ExitDirection.Left)) ||
                    (!vRoom.HasAnyOfExitDirections(ExitDirection.Top, ExitDirection.Bottom) && vRoom.HasAllOfExitDirections(ExitDirection.Right, ExitDirection.Left)))
                {
                    if (chance <= density)
                        continue;

                    room = vRoom;
                    roomAccepted = true;
                }

                var size = Grid.Count(cell => cell.HasRoom());
                if (!roomAccepted && vRoom.exitDirections.Count == 1 && size > _config.minLevelSize)
                {
                    if (chance <= density)
                        continue;

                    room = vRoom;
                }

                if (roomAccepted)
                    break;
            }

            // note : set and spawn room
            parentGridCell.InstantiateRoom(room);
            //_lastCell = parentCell;
        
            // note : if room has open directions, add cell to open list
            if(room.exitDirections.Count > 1)
                OpenCells.Add(parentGridCell);
        }
    
        private static List<GridRoomLayout> GetValidRooms(GridCell gridCell)
        {
            var mustInclude = new List<ExitDirection>();
            var mustExclude = new List<ExitDirection>();
        
            CalculateExitRequirements(gridCell, ref mustInclude, ref mustExclude);
            return _config.roomTemplates.Where(room => !room.isEssential)
                                        .Where(room => !room.isStartRoom)
                                        .Where(room => ValidateExitRequirements(room, mustInclude, mustExclude)).ToList();
        }

        private static List<GridCell> GetRoomsForEssentialReplacement(GridRoomLayout gridRoom)
        {
            var validCells = new List<GridCell>();
            foreach (var cell in Grid.Where(cell => cell.HasRoom()))
            {
                if (cell.GetRoom().HasExactExitDirections(gridRoom.exitDirections.ToArray()))
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

        private static bool ValidateExitRequirements(GridRoomLayout gridRoom, IEnumerable<ExitDirection> mustInclude, IReadOnlyCollection<ExitDirection> mustExclude)
        {
            var exitsValid = true;
            foreach (var unused in mustExclude.Where(direction => gridRoom.HasAllOfExitDirections(direction)))  exitsValid = false;
            foreach (var unused in mustInclude.Where(direction => !gridRoom.HasAllOfExitDirections(direction))) exitsValid = false;
            //if (gridRoom.exitDirections.Count == 1 && mustExclude.Count < 3) exitsValid = false;

            return exitsValid;
        }
        
        // -- CACHING FUNCTIONALITY

        private static bool DeserializedAndApplyCache()
        {
            var cache = SceneCacheUtility.DeserializeCache(SceneManager.GetActiveScene());
            if (!cache)
            {
                Debug.LogError("Failed applying cache");
                return false;
            }
            
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
            if (_config.disableSceneCaching)
                return;

            if (DeserializedAndApplyCache())
                return;
            
            var cache = ScriptableObject.CreateInstance<SceneCache>();
            cache.sceneID = AssetDatabase.AssetPathToGUID(scene.path);
            cache.editModeCache = new CacheData();
            cache.playModeCache = new CacheData();
            _localCache = cache;
            SceneCacheUtility.SerializeCache(cache);
        }
        
        private static void CacheOnSceneSaved(Scene scene)
        {
            if (_config.disableSceneCaching)
                return;

            SerializeLocalCache();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void CacheOnGeneratorRecompiled()
        {
            if (_config.disableSceneCaching)
                return;

            if(EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            
            EditorApplication.delayCall += CacheOnEditorUpdate;
        }

        private static void CacheOnEditorUpdate()
        {
            if (_config.disableSceneCaching)
                return;

            DeserializedAndApplyCache();
            EditorApplication.delayCall -= CacheOnEditorUpdate;
        }

        private static void CacheOnPlayModeStateChange(PlayModeStateChange stateChange)
        {
            if (_config.disableSceneCaching)
                return;

            switch (stateChange)
            {
            case PlayModeStateChange.ExitingEditMode:
                CreatePlayModeCache();
                break;
            case PlayModeStateChange.EnteredPlayMode:
            case PlayModeStateChange.EnteredEditMode:
                DeserializedAndApplyCache();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateChange), stateChange, null);
            }
        }

        private static void CacheOnUndoRedoPerformed()
        {
            Undo.FlushUndoRecordObjects();
            SerializeLocalCache();
        }
    }
}