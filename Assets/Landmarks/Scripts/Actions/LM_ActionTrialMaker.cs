using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landmarks.Scripts.Actions
{
    public class LM_ActionTrialMaker : EditorWindow
    {
        private TextField _csvPath;
        private static readonly string[] Keyword = { "teleport", "name", "loop", "walk", "trigger", "pause", "turn", "place" };


        private string _defaultPath;
            

        [MenuItem("LoopTask/LM_ActionTrialMaker")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(LM_ActionTrialMaker));
            window.titleContent = new GUIContent("ActionTrialMaker");
        }

        public void OnEnable()
        {
            _defaultPath = Path.Combine(Directory.GetParent(Application.dataPath)?.ToString() ?? "", "DataToGenerateTrials.tsv");
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(new Label("Path to text containing trial information"));
            _csvPath = new TextField
            {
                value = _defaultPath
            };

            var csvPathButton = new Button(() =>
            {
                // Get the parent dir of Application.dataPath
                _csvPath.value = EditorUtility.OpenFilePanel("Select a file",
                    Directory.GetParent(Application.dataPath)?.ToString(), "tsv,txt");
            })
            {
                text = "Select file"
            };

            var generateButton = new Button(GenerateTrialObjects)
            {
                text = "Import Trial Objects As Children of Selected GameObject"
            };
            
            var exportButton = new Button(ExportSelection)
            {
                text = "Export Children of Selected GameObject"
            };

            var printChildrenButton = new Button(PrintChildren)
            {
                text = "Print Children"
            };

            root.Add(_csvPath);
            root.Add(csvPathButton);
            root.Add(generateButton);
            root.Add(exportButton);
            root.Add(printChildrenButton);
        }

        private void GenerateTrialObjects()
        {
            // Try to read the file
            try
            {
                var lines = File.ReadAllLines(_csvPath.value);
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

        private void ExportSelection()
        {
            // Try to open the file
            try
            {
                var path = EditorUtility.SaveFilePanel("Save file", "", "export.tsv", "tsv");
                if (path.Length == 0) return;

                var writer = new StreamWriter(path);
                
                var gameObject = Selection.activeGameObject;
                var trials = gameObject.transform.Cast<Transform>()
                    .OrderBy(t => t.GetSiblingIndex());
                foreach (var trial in trials)
                {
                    var outputLine = "";
                    var actions = trial.Cast<Transform>().OrderBy(t => t.GetSiblingIndex());
                    
                    outputLine += $"name\t{trial.name}";
                    
                    foreach (var action in actions)
                    {
                        outputLine += $"\t{action.name}";
                        outputLine = action.Cast<Transform>().Aggregate(outputLine, (current, fields) => current + $"\t{fields.name}");
                    }
                    
                    writer.WriteLine(outputLine);
                }
                writer.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            
        }
        private static void PrintChildren()
        {
            var gameObject = Selection.activeGameObject;
            var list = gameObject.transform.Cast<Transform>()
                .OrderBy(t => t.GetSiblingIndex()).Select(t => t.name)
                .ToList();
            //print first 5
            Debug.Log(string.Join(",", list.Take(5).ToArray()));
        }

        private static void TraverseChildren(GameObject gameObject)
        {
            
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