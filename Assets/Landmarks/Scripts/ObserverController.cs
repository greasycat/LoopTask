using System;
using System.Collections;
using Landmarks.Scripts.Actions;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

namespace Landmarks.Scripts
{
    public class ObserverController : MonoBehaviour
    {
        [SerializeField] private Experiment experiment;

        // [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private TMP_InputField speedInput;

        // [SerializeField] private TMP_InputField startingAngleInput;
        [SerializeField] private TMP_InputField stoppingAngleInput;
        [SerializeField] private TMP_InputField startingPositionInput;
        [SerializeField] private TMP_InputField loopCenterInput;
        [SerializeField] private TMP_Text errorMessageText;
        [SerializeField] private Toggle counterclockwiseToggle;
        [SerializeField] private MoveObject moveObject;
        private FirstPersonController _playerController;
        private HUD _hud;
        private bool firstTime = true;

        // Start is called before the first frame update
        private void Start()
        {
            _playerController = experiment.GetController().GetComponent<FirstPersonController>();
            var avatar = GameObject.FindWithTag("Player");
            _hud = avatar.GetComponent("HUD") as HUD;
            Assert.IsNotNull(_playerController);
            Assert.IsNotNull(_hud);
        }

        public void TestButtonClick()
        {
        }

        public void StartButtonClick()
        {
        }

        public void StartButtonClickRecenter()
        {
        }

        public void RunAllTrials()
        {
            if (firstTime)
            {
                _hud.OnActionClick();
                firstTime = false;
            }

            StartCoroutine(RunAllActionSets());
        }

        public IEnumerator RunAllActionSets()
        {
            _playerController.DisableManualMovement();
            yield return new WaitUntil(() => moveObject.destination != null);
            var actionSet = new LM_ActionSet(moveObject.destination.transform);
            yield return actionSet.PerformAll(_playerController.transform, () => { _hud.OnActionClick(); });
            _playerController.EnableManualMovement();
        }

        private void AddErrorMessage(string message)
        {
            // add timestamp to the message
            message = $"{System.DateTime.Now:HH:mm:ss} {message}";
            errorMessageText.text += message + "\n";
        }
    }
}