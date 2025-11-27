using UnityEngine;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    public TMP_Text instructionsText;

    private void OnEnable()
    {
        ControlType type = GameManager.Instance.CurrentControlType;

        switch (type)
        {
            case ControlType.Joystick:
                instructionsText.text = "Utilise le joystick pour jouer !";
                break;

            case ControlType.Buttons:
                instructionsText.text = "Utilise les boutons pour jouer !";
                break;

            default:
                instructionsText.text = "Prépare-toi...";
                break;
        }
    }
}
