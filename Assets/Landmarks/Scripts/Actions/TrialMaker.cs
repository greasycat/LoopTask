using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landmarks.Scripts.Actions
{
    public class TrialMaker : EditorWindow
    {
        private TextField csvPath;
        private static readonly string[] Keyword = { "teleport", "name", "loop", "walk", "trigger", "pause", "turn" };


        private static readonly string DefaultPath = Path.Combine(
            Directory.GetParent(Application.dataPath)?.ToString() ?? "", "DataToGenerateTrials.tsv");

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
            csvPath = new TextField
            {
                value = DefaultPath
            };

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
                    var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    
                    // set the scale to 0.1, 0.1, 0.1
                    root.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    //disable meshrenderer
                    root.GetComponent<MeshRenderer>().enabled = false;
                    root.GetComponent<SphereCollider>().radius = 0.5f;
                    root.GetComponent<SphereCollider>().isTrigger = true;
                    
                    
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