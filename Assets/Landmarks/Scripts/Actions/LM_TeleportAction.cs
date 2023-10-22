using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_TeleportAction : LM_Action
    {
        protected Vector3 Destination { get; set; }
        protected float Angle { get; set; }

        public override IEnumerator Execute(Transform transform)
        {
            Debug.Log("Teleport Action");
            transform.position = Destination;
            transform.LookAt(transform.position + GetDirectionFromAngle(Angle));
            yield return null;
        }

        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var destination = GetVectorFromText(fieldDict["pos"]);
                var angle = float.Parse(fieldDict["face"]);

                return new LM_TeleportAction()
                {
                    Destination = destination,
                    Angle = angle,
                    Type = ActionType.Teleport
                    
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse Teleport Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
    }
}