using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_TurnAction : LM_Action
    {
        private string Direction { get; set; }
        private float Angle { get; set; }
        private float RotationSpeed { get; set; }
        
        
        public override IEnumerator Execute(Transform transform)
        {
            Debug.Log("Turn Action");
            var startRotation = transform.rotation;
            var relativeRotationDegree = Angle;

            if (Direction == "ccw")
            {
                relativeRotationDegree = -relativeRotationDegree;
            }

            var endRotation = Quaternion.Euler(0, relativeRotationDegree, 0) * startRotation;

            var duration = Angle / RotationSpeed / 5;

            for (var t = 0f; t < duration; t += Time.deltaTime * RotationSpeed)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, t / duration);
                yield return null;
            }

            transform.rotation = endRotation;
        }

        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var direction = fieldDict["dir"];
                var angle = float.Parse(fieldDict["ang"]);
                var rotationSpeed = float.Parse(fieldDict["spd"]);

                return new LM_TurnAction()
                {
                    Direction = direction,
                    Angle = angle,
                    RotationSpeed = rotationSpeed,
                    Type = ActionType.Turn
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse Turn Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
    }
}