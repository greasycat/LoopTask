using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Landmarks.Scripts
{
    public enum ActionType
    {
        Teleport,
        Loop,
        WalkTo,
        Trigger,
        Pause,
        None
    }

    
    public class LM_Action
    {
        public ActionType type;

        public static LM_Action FromObject(Transform transform)
        {
            switch (transform.name)
            {
                case "teleport":
                    return LM_TeleportAction.FromObject(transform);
                case "loop":
                    return LM_LoopAction.FromObject(transform);
                case "walkto":
                    return LM_WalkToAction.FromObject(transform);
                case "trigger":
                    return new LM_TriggerAction() { type = ActionType.Trigger};
                case "pause":
                    return LM_PauseAction.FromObject(transform);
                default:
                    return new LM_NoneAction() { type = ActionType.None };
            }
        }

        protected static List<string> GetAllChildrenNames(Transform transform)
        {
            return transform.Cast<Transform>()
                .OrderBy(t => t.GetSiblingIndex()).Select(t => t.name)
                .ToList();
        }
        
        protected static Vector3 ConvertTypeToVector(string type)
        {
            switch (type)
            {
                case "A":
                    return new Vector3(0, 0, -1);
                case "B":
                    return new Vector3(-1, 0, 0);
                case "C":
                    return new Vector3(0, 0, 1);
                case "D":
                    return new Vector3(1, 0, 0);
            }

            return new Vector3(-1,0,-1);
        }

        protected static Vector3 GetVectorFromText(string text)
        {
            
            const string pattern = @"[-+]?\d*\.?\d+";
            var match = Regex.Matches(text, pattern).Cast<Match>().Take(3).ToList();
            var x = 0f;
            var y = 0f;
            var z = 0f;
            try
            {
                x = float.Parse(match[0].Value);
                y = float.Parse(match[1].Value);
                z = float.Parse(match[2].Value);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Cannot Parse Vector For LM_Action based on the following text: " + text);
            }

            return new Vector3(x, y, z);
        }
    }

    public class LM_TeleportAction : LM_Action
    {
        public Vector3 destination;

        public new static LM_Action FromObject(Transform transform)
        {
            //Get all the children
            var childrenNames = GetAllChildrenNames(transform);
            if (childrenNames.Count != 1)
            {
                Debug.LogError("Incorrect number of destination position for LM_TeleportAction");
                return new LM_NoneAction();
            }

            //regex to match three floats separated by commas

            return new LM_TeleportAction()
            {
                destination = GetVectorFromText(childrenNames[0]),
                type = ActionType.Teleport
            };
        }
    }

    public class LM_LoopAction : LM_Action
    {
        
        public Vector3 loopCenter;
        public float loopRadius;
        public float loopAngle;
        public string loopDirection;
        public float loopSpeed;
        
        public new static LM_Action FromObject(Transform transform)
        {
            var childrenNames = GetAllChildrenNames(transform);
            if (childrenNames.Count != 5)
            {
                Debug.LogError("Incorrect number of fields for LM_LoopAction");
                return new LM_NoneAction();
            }
            
            var loopCenter = GetVectorFromText(childrenNames[0]);
            var loopRadius = float.Parse(childrenNames[1]);
            var loopAngle = float.Parse(childrenNames[2]) / 180f * Mathf.PI;
            var loopDirection = childrenNames[3];
            var loopSpeed = float.Parse(childrenNames[4]);

            return new LM_LoopAction()
            {
                loopCenter = loopCenter,
                loopRadius = loopRadius,
                loopAngle = loopAngle,
                loopDirection = loopDirection,
                loopSpeed = loopSpeed,
                type = ActionType.Loop
            };
        }
    }

    public class LM_WalkToAction : LM_Action
    {
        public Vector3 destination;
        public float speed;
        public new static LM_Action FromObject(Transform transform)
        {
            var childrenNames = GetAllChildrenNames(transform);
            if (childrenNames.Count != 2)
            {
                Debug.LogError("Incorrect number of fields for LM_WalkToAction");
                return new LM_NoneAction();
            }

            var destination = Vector3.zero;
            if (childrenNames[0].Length == 1)
            {
                destination = ConvertTypeToVector(childrenNames[0]);
            }
            else
            {
                
                destination = GetVectorFromText(childrenNames[0]);
            }
            
            var speed = float.Parse(childrenNames[1]);
            
            return new LM_WalkToAction()
            {
                destination = destination,
                speed = speed,
                type = ActionType.WalkTo
            };
        }
    }

    public class LM_TriggerAction : LM_Action
    {
        // public new static LM_Action FromObject(Transform transform)
        // {
        //     return new LM_TriggerAction();
        // }
    }

    public class LM_NoneAction : LM_Action
    {
        // public static LM_Action FromObject(Transform transform)
        // {
        //     return new LM_NoneAction();
        // }
    }
    
    public class LM_PauseAction : LM_Action
    {
        public float duration;
        public new static LM_Action FromObject(Transform transform)
        {
            var childrenNames = GetAllChildrenNames(transform);
            if (childrenNames.Count != 1)
            {
                Debug.LogError("Incorrect number of fields for LM_PauseAction");
                return new LM_NoneAction();
            }
            
            var duration = float.Parse(childrenNames[0]);
            
            return new LM_PauseAction()
            {
                duration = duration,
                type = ActionType.Pause
            };
        }
    }
}