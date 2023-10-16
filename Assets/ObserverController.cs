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
        try
        {
            _hud.OnActionClick();
            // var startingAngle = float.Parse(startingAngleInput.text)*Mathf.PI/180f;
            var stoppingAngle = float.Parse(stoppingAngleInput.text)*Mathf.PI/180f;
            var counterclockwise = counterclockwiseToggle.isOn;
            
            const string pattern = @"[-+]?\d*\.?\d+";
            var speedMatch = Regex.Matches(speedInput.text, pattern).Cast<Match>().Take(3).ToList();
            var startingPosMatch = Regex.Matches(startingPositionInput.text, pattern).Cast<Match>().Take(3).ToList();
            var loopCenterMatch = Regex.Matches(loopCenterInput.text, pattern).Cast<Match>().Take(3).ToList();
            var startingPosition = new Vector3(float.Parse(startingPosMatch[0].Value), float.Parse(startingPosMatch[1].Value), float.Parse(startingPosMatch[2].Value));
            var loopCenterPosition = new Vector3(float.Parse(loopCenterMatch[0].Value), float.Parse(loopCenterMatch[1].Value), float.Parse(loopCenterMatch[2].Value));
            var walkingSpeed = float.Parse(speedMatch[0].Value);
            var turningSpeed = float.Parse(speedMatch[1].Value);
            var loopingSpeed = float.Parse(speedMatch[2].Value);
            
            _playerController.TestAutoMode(loopCenterPosition,startingPosition, stoppingAngle, counterclockwise, walkingSpeed, turningSpeed, loopingSpeed, _hud);
        }
        catch (System.Exception e)
        {
            AddErrorMessage("Invalid Input\n" + $"Reason: {e.Message}");
            Debug.LogError("Invalid Input");
        }
    }

    public void StartButtonClick()
    {
        if (firstTime)
        {
            _hud.OnActionClick();
            firstTime = false;
        }

        StartCoroutine(StartTrial());
    }
    
    public void StartButtonClickRecenter()
    {
        if (firstTime)
        {
            _hud.OnActionClick();
            firstTime = false;
        }

        StartCoroutine(StartTrial(true));
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
    
    private IEnumerator StartTrial(bool recenter = false)
    {
        yield return new WaitUntil(()=> moveObject.destination != null);
        Debug.Log($"Destination: {moveObject.destination.name}");
        // Get the text in the name of the destination object
        // use the following format xx-xx-xx
        var destinationName = moveObject.destination.name;
        var destinationNameSplit = destinationName.Split('-');
        var speedSplit = speedInput.text.Split(',');
        try
        {
            var type = ConvertTypeToVector(destinationNameSplit[0]);
            var radius = GetNumber(destinationNameSplit[1]);
            var clock = destinationNameSplit[2].ToLower();
            var counterClockwise = clock == "counter";
            var rotation = 90f;
            if (destinationNameSplit.Length == 4)
            {
                rotation = float.Parse(destinationNameSplit[3]);
            }
            
            if (speedSplit.Length != 4)
            {
                AddErrorMessage("Invalid Speed Format\n" + "Reason: Speed must be in the format x,y,z");
                yield break;
            }
            
            var walkingSpeed = float.Parse(speedSplit[0]);
            var turningSpeed = float.Parse(speedSplit[1]);
            var loopingSpeed = float.Parse(speedSplit[2]);
            var duration = float.Parse(speedSplit[3]);
            
            
            Debug.Log("Type: " + type + " Radius: " + radius + " Clock: " + clock);
            _playerController.TestAutoMode(Vector3.zero, type*radius,ToRadians(rotation), counterClockwise, walkingSpeed, turningSpeed, loopingSpeed, _hud, duration ,recenter);
        } catch (System.Exception e)
        {
            AddErrorMessage("Invalid Target Naming Format\n" + $"Reason: {e.Message}");
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
