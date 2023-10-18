using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_LoopAction : LM_Action
    {
        private Vector3 LoopCenter { get; set; }
        private float LoopRadius { get; set; }
        private float LoopAngle { get; set; }
        private string LoopDirection { get; set; }
        private float LoopSpeed { get; set; }


        public override IEnumerator Execute(Transform transform)
        {
            var counterclockwise = LoopDirection == "ccw";
            
            var finalAngle = LoopAngle * Mathf.Deg2Rad;

            var currentPosition = transform.position;
            
            // calculate the current angle, 180 to -180
            var currentAngle = Mathf.Atan2(currentPosition.z - LoopCenter.z,
                currentPosition.x - LoopCenter.x);
            
            // convert to 0 to 360
            if (currentAngle < 0) currentAngle += 2 * Mathf.PI;

            // Debug.Log($"Current angle: {currentAngle * Mathf.Rad2Deg}");
            // Debug.Log($"stopping angle: {LoopAngle * Mathf.Rad2Deg}");
            // Debug.Log($"Final angle: {finalAngle * Mathf.Rad2Deg}");

            for (var w = 0f; w < finalAngle; w += Time.deltaTime * LoopSpeed * 0.1f)
            {
                var angle = currentAngle + (counterclockwise ? 1 : -1) * w;
                var x = Mathf.Cos(angle) * LoopRadius + LoopCenter.x;
                var z = Mathf.Sin(angle) * LoopRadius + LoopCenter.z;
                var y = LoopCenter.y;
                var newPosition = new Vector3(x, y, z);
                var direction = newPosition - transform.position;
                transform.position = newPosition;
                transform.rotation = Quaternion.LookRotation(direction);
                yield return null;
            }
        }

        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var loopCenter = GetVectorFromText(fieldDict["pos"]);
                var loopRadius = float.Parse(fieldDict["r"]);
                var loopAngle = float.Parse(fieldDict["ang"]);
                var loopDirection = fieldDict["dir"];
                var loopSpeed = float.Parse(fieldDict["spd"]);

                return new LM_LoopAction()
                {
                    LoopCenter = loopCenter,
                    LoopRadius = loopRadius,
                    LoopAngle = loopAngle,
                    LoopDirection = loopDirection,
                    LoopSpeed = loopSpeed,
                    Type = ActionType.Loop
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse Loop Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
    }
}