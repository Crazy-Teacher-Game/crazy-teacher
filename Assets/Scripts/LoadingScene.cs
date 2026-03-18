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

    [Header("Audio")]
    [SerializeField] private AudioClip inputScreenSound;
    [SerializeField] [Range(0f, 3f)] private float inputScreenSoundVolume = 1f;

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

        if (inputScreenSound != null)
            GameManager.Instance.PlaySFX(inputScreenSound, inputScreenSoundVolume);
    }
}
