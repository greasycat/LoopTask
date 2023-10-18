using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public enum ActionType
    {
        Teleport,
        Loop,
        Walk,
        Trigger,
        Pause,
        Turn,
        None
    }

    public interface IAction
    {
        IEnumerator Execute(Transform transform);
    }


    public class LM_Action: IAction
    {
        public ActionType Type { get; protected set; }

        public static LM_Action FromObject(Transform transform)
        {
            switch (transform.name)
            {
                case "teleport":
                    return LM_TeleportAction.FromObject(transform);
                case "loop":
                    return LM_LoopAction.FromObject(transform);
                case "walk":
                    return LM_WalkAction.FromObject(transform);
                case "trigger":
                    return new LM_TriggerAction();
                case "pause":
                    return LM_PauseAction.FromObject(transform);
                case "turn":
                    return LM_TurnAction.FromObject(transform);
                default:
                    return new LM_NoneAction();
            }
        }

        public virtual IEnumerator Execute(Transform _)
        {
            yield return null;
        }

        protected static Dictionary<string, string> GetFieldsDictFromChildrenName(Transform transform)
        {
            var dict = new Dictionary<string, string>();
            foreach (var name in transform
                         .Cast<Transform>()
                         .OrderBy(t => t.GetSiblingIndex())
                         .Select(t => t.name))
            {
                var split = name.Replace("\"","").Split(':');
                if (split.Length != 2)
                {
                    Debug.LogError("Incorrect number of fields for LM_Action");
                    continue;
                }

                //check if split[0] or split[1] is empty or 
                if (split[0].Replace("\t", "").Replace(" ", "") == "" ||
                    split[1].Replace("\t", "").Replace(" ", "") == "")
                {
                    Debug.LogError("Empty field for Action Field");
                    continue;
                }

                dict.Add(split[0], split[1]);
            }

            return dict;
        }

        protected static Vector3 GetDirectionFromAngle(float angle)
        {
            var rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
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
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse Vector For LM_Action based on the following text: " + text +
                               "Reason: " + e.Message);
            }

            return new Vector3(x, y, z);
        }
    }

    public class LM_NoneAction : LM_Action
    {
        public LM_NoneAction()
        {
            Type = ActionType.None;
        }
    }
}