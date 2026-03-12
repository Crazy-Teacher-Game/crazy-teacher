using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class LoadingScene : MonoBehaviour
{
    public TMP_Text instructionsText;

    public GameObject JoystickYPrefab;
    public GameObject JoystickXPrefab;
    public GameObject ButtonFPrefab;
    public GameObject ButtonsFGHPrefab;

    private void OnEnable()
    {
        ControlType type = GameManager.Instance.CurrentControlType;

        switch (type)
        {
            case ControlType.JoystickY:
                JoystickYPrefab.SetActive(true);
                break;

            case ControlType.JoystickX:
                JoystickXPrefab.SetActive(true);
                break;

            case ControlType.ButtonF:
                ButtonFPrefab.SetActive(true);
                break;

            case ControlType.ButtonsFGH:
                ButtonsFGHPrefab.SetActive(true);
                break;

            default:
                instructionsText.text = "Prépare-toi...";
                break;
        }
    }
}
