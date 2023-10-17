using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Landmarks.Scripts
{
    public class LM_ActionSet: IEnumerable
    {
        private string _name;
        private readonly List<LM_Action> _actions; //set of the actions
        
        public LM_ActionSet(Transform transform)
        {
            _name = transform.name;
            _actions = new List<LM_Action>();
            
            // get all the name of children of the game object
            foreach (Transform child in transform)
            {
                _actions.Add(LM_Action.FromObject(child));
            }
        }
        
        public LM_Action First => _actions.First();
        public LM_Action GetAction(int index) => _actions[index];

        public Vector3 GetFirstWalkToPosition()
        {
            foreach (var action in _actions.Where(action => action.type == ActionType.WalkTo))
            {
                return ((LM_WalkToAction)action).destination;
            }

            return Vector3.zero;
        }

        public LM_Action GetFirstActionOfType(ActionType type)
        {
            foreach (var action in _actions.Where(action => action.type == type))
            {
                return action;
            }

            return new LM_NoneAction();
        }

        public IEnumerator<LM_Action> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}