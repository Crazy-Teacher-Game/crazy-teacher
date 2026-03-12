using System.Collections.Generic;

public static class GameControlsDatabase
{
    private static Dictionary<string, ControlType> controlMap =
        new Dictionary<string, ControlType>()
        {
            { "BallDropper", ControlType.JoystickY },
            { "TriPommePoire", ControlType.JoystickX },
            { "SlotMachine", ControlType.ButtonF },
            {"Dice", ControlType.JoystickX },
            {"MentalMath", ControlType.ButtonsFGH },
            {"PopTheBottle", ControlType.JoystickY},
            {"FlashTheCar", ControlType.ButtonF},
            {"TimerGame", ControlType.ButtonF},
        };

    public static ControlType GetControlType(string sceneName)
    {
        if (controlMap.TryGetValue(sceneName, out var type))
            return type;

        // Valeur par défaut si oubli
        return ControlType.Buttons;
    }
}
