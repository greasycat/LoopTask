using System.Text.RegularExpressions;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityStandardAssets.Characters.FirstPerson;

public class ObserverController : MonoBehaviour
{
    [SerializeField] private Experiment experiment;
    [SerializeField] private TMP_InputField radiusInput;
    [SerializeField] private TMP_InputField speedInput;
    [SerializeField] private TMP_InputField startingAngleInput;
    [SerializeField] private TMP_InputField stoppingAngleInput;
    [SerializeField] private TMP_InputField startingPositionInput;
    [SerializeField] private TMP_InputField loopCenterInput;
    [SerializeField] private TMP_Text errorMessageText;
    private FirstPersonController _playerController;
    
    // Start is called before the first frame update
    private void Start()
    {
        _playerController = experiment.GetController().GetComponent<FirstPersonController>();
        Assert.IsNotNull(_playerController);
    }

    public void TestButtonClick()
    {
        try
        {
            var radius = float.Parse(radiusInput.text);
            var speed = float.Parse(speedInput.text);
            var startingAngle = float.Parse(startingAngleInput.text)*Mathf.PI/180f;
            var stoppingAngle = float.Parse(stoppingAngleInput.text)*Mathf.PI/180f;
            
            const string pattern = @"[-+]?\d*\.?\d+";
            var startingPosMatch = Regex.Matches(startingPositionInput.text, pattern).Cast<Match>().Take(3).ToList();
            var loopCenterMatch = Regex.Matches(loopCenterInput.text, pattern).Cast<Match>().Take(3).ToList();
            var startingPosition = new Vector3(float.Parse(startingPosMatch[0].Value), float.Parse(startingPosMatch[1].Value), float.Parse(startingPosMatch[2].Value));
            var loopCenter = new Vector3(float.Parse(loopCenterMatch[0].Value), float.Parse(loopCenterMatch[1].Value), float.Parse(loopCenterMatch[2].Value));
            
            
            if (startingAngle < 0 || startingAngle >= stoppingAngle)
                throw new System.Exception("Starting angle must be greater than 0 and less than stopping angle");
            
            _playerController.TestAutoMode(radius, speed, startingAngle,stoppingAngle,loopCenter, startingPosition);
        }
        catch (System.Exception e)
        {
            errorMessageText.text = "Invalid Input\n" + $"Reason: {e.Message}";
            Debug.LogError("Invalid Input");
        }
    }

    public void StartButtonClick()
    {
        
    }
}
