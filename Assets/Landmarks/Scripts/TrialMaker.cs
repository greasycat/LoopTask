using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Landmarks.Scripts
{
    public class TrialMaker : EditorWindow
    {
        private TextField csvPath;
        private static readonly string[] Keyword = { "teleport", "name", "loop", "walkto", "trigger", "pause" };

        [MenuItem("LoopTask/TrialMaker")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(TrialMaker));
            window.titleContent = new GUIContent("TrialMaker");
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(new Label("Path to text containing trial information"));
            csvPath = new TextField();

            var csvPathButton = new Button(() =>
            {
                // Get the parent dir of Application.dataPath
                csvPath.value = EditorUtility.OpenFilePanel("Select a file",
                    Directory.GetParent(Application.dataPath)?.ToString(), "tsv,txt");
            })
            {
                text = "Select file"
            };

            var generateButton = new Button(GenerateTrialObjects)
            {
                text = "Generate Trial Objects"
            };

            root.Add(csvPath);
            root.Add(csvPathButton);
            root.Add(generateButton);
            root.Add(new Button(() =>
            {
                var gameObject = Selection.activeGameObject;
                var list = gameObject.transform.Cast<Transform>()
                    .OrderBy(t => t.GetSiblingIndex()).Select(t => t.name)
                    .ToList();
                //print first 5
                Debug.Log(string.Join(",", list.Take(5).ToArray()));
            }));
        }

        private void GenerateTrialObjects()
        {
            // Try to read the file
            try
            {
                var lines = File.ReadAllLines(csvPath.value);
                foreach (var line in lines)
                {
                    // Create an empty game object and add to the scene
                    var root = new GameObject();
                    if (Selection.activeObject != null)
                    {
                        root.transform.parent = Selection.activeGameObject.transform;
                    }

                    // Split the line by tabs
                    ParseLine(line, root);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private static void ParseLine(string line, GameObject parent)
        {
            var split = line.Split('\t');
            GameObject currentGameObject = null;

            foreach (var rawToken in split)
            {
                var token = rawToken.Trim();
                if (token == "") continue;
                if (Keyword.Contains(token))
                {
                    currentGameObject = new GameObject(token);
                    currentGameObject.transform.parent = parent.transform;
                }
                else
                {
                    // create a new game object
                    if (currentGameObject == null) continue;
                    if (currentGameObject.name == "name")
                    {
                        parent.name = token;
                        DestroyImmediate(currentGameObject);
                        continue;
                    }
                    var field = new GameObject(token);
                    field.transform.parent = currentGameObject.transform;
                }
            }
        }
    }
}