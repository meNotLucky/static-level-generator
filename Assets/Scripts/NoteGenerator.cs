using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using UnityEngine;

public class NoteGenerator : MonoBehaviour
{
    public static void CreateNewNotes()
    {
        const string fileName = "Assets/Notes/NewFile.txt";
        Directory.CreateDirectory("Assets/Notes");

        // Check if file already exists. If yes, delete it.     
        if (File.Exists(fileName))    
        {    
            File.Delete(fileName);    
        }    
    
        // Create a new file     
        using (var fs = File.Create(fileName))     
        {    
            // Add some text to file    
            var title = new UTF8Encoding(true).GetBytes("New Text File");    
            fs.Write(title, 0, title.Length);    
            var author = new UTF8Encoding(true).GetBytes("Mahesh Chand");    
            fs.Write(author, 0, author.Length);    
        }    
    
        // Open the stream and read it back.    
        using (var sr = File.OpenText(fileName))    
        {    
            var s = "";    
            while ((s = sr.ReadLine()) != null)    
            {    
                Console.WriteLine(s);    
            }    
        }    
    }
}
