using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_PauseAction : LM_Action
    {
        private float Duration { get; set; }

        public override IEnumerator Execute(Transform _)
        {
            Debug.Log("Pause Action for "+Duration);
            yield return new WaitForSeconds(Duration);
        }

        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var duration = float.Parse(fieldDict["t"]);

                return new LM_PauseAction()
                {
                    Duration = duration,
                    Type = ActionType.Pause
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot Parse Pause Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
    }
}