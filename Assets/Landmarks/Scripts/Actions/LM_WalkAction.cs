using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_WalkAction : LM_Action, IAction
    {
        private float Angle { get; set; }
        private float Distance { get; set; }

        private float Speed { get; set; }

        public override IEnumerator Execute(Transform transform)
        {
            Debug.Log("WalkAction");
            var startPosition = transform.position;
            var direction = GetDirectionFromAngle(Angle);
            var endPosition = startPosition + direction * Distance; 
            
            // set the transform to look at that direction
            transform.LookAt(startPosition + direction);
            

            var duration = Distance / Speed * 5f;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, t / duration);
                yield return null;
            }

            transform.position = endPosition;


        }

        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var angle = float.Parse(fieldDict["face"]);
                var walkingSpeed = float.Parse(fieldDict["spd"]);
                var distance = float.Parse(fieldDict["dist"]);

                return new LM_WalkAction()
                {
                    Angle = angle,
                    Distance = distance,
                    Speed = walkingSpeed,
                    Type = ActionType.Walk
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse WalkTo Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
    }
}