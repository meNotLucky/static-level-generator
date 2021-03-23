using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ENoteGenerator))]
public class ENoteGenerator : Editor
{
    [MenuItem("Note Generator/Create New Note")]
    public static void CreateNote()
    {
        NoteGenerator.CreateNewNotes();
    }

}
