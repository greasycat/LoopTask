using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using Landmarks.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

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
        var avatar = GameObject.FindWithTag ("Player");
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

    private Vector3 ConvertTypeToVector(string type)
    {
        switch (type)
        {
            case "A":
                return new Vector3(0, 0, -1);
            case "B":
                return new Vector3(-1, 0, 0);
            case "C":
                return new Vector3(0, 0, 1);
            case "D":
                return new Vector3(1, 0, 0);
        }

        return new Vector3(-1,0,-1);
    }
    
    //function to get the number out of the format x.xm where x is the digit
    private float GetNumber(string input)
    {
        var pattern = @"[-+]?\d*\.?\d+";
        var match = Regex.Matches(input, pattern).Cast<Match>().Take(1).ToList();
        return float.Parse(match[0].Value);
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
        _playerController.EnableAutoMode();
        yield return new WaitUntil(()=> moveObject.destination != null);
        var actionSet = new LM_ActionSet(moveObject.destination.transform);
        foreach (var action in actionSet)
        {
            switch (action.type)
            {
                case ActionType.Teleport:
                    Debug.Log("Teleport");
                    yield return _playerController.TeleportAction((LM_TeleportAction)action);
                    break;
                case ActionType.Loop:
                    Debug.Log("Loop");
                    yield return _playerController.LoopAction((LM_LoopAction)action);
                    break;
                case ActionType.WalkTo:
                    Debug.Log("WalkTo");
                    yield return _playerController.WalkToAction((LM_WalkToAction)action);
                    break;
                case ActionType.Trigger:
                    Debug.Log("Trigger");
                    yield return _playerController.TriggerAction((LM_TriggerAction)action, _hud);
                    break;
                case ActionType.Pause:
                    Debug.Log("Pause");
                    yield return _playerController.PauseAction((LM_PauseAction)action);
                    break;
                case ActionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



    }
    
    private float ToRadians(float degrees)
    {
        return degrees * Mathf.PI / 180f;
    }
    
    private void AddErrorMessage(string message)
    {
        // add timestamp to the message
        message = $"{System.DateTime.Now:HH:mm:ss} {message}";
        errorMessageText.text += message + "\n";
    }
    
}
