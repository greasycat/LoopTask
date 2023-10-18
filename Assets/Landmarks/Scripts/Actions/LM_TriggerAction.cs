
using System;
using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_TriggerAction : LM_Action
    {
        public Action TriggerAction { set; get; }
        public override IEnumerator Execute(Transform _)
        {
            Debug.Log("Trigger Action");
            TriggerAction?.Invoke();
            yield return null;
        }
        public LM_TriggerAction()
        {
            Type = ActionType.Trigger;
        }
    }
}