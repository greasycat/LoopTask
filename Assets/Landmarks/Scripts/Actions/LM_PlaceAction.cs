using System.Collections;
using UnityEngine;

namespace Landmarks.Scripts.Actions
{
    public class LM_PlaceAction: LM_TeleportAction
    {
        private string Name { set; get; }
        
        
        public override IEnumerator Execute(Transform _)
        {
            Debug.Log("Place Action");
            var targetTransform = GameObject.Find(Name).transform;
            yield return base.Execute(targetTransform);
        }
        
        public new static LM_Action FromObject(Transform transform)
        {
            try
            {
                var fieldDict = GetFieldsDictFromChildrenName(transform);
                var name = fieldDict["name"];
                var destination = GetVectorFromText(fieldDict["pos"]);
                var angle = float.Parse(fieldDict["face"]);

                return new LM_PlaceAction()
                {
                    Name = name,
                    Destination = destination,
                    Angle = angle,
                    Type = ActionType.Place
                    
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError("Cannot Parse Place Action: " + e.Message);
                return new LM_NoneAction();
            }
        }
        
        
    }
}