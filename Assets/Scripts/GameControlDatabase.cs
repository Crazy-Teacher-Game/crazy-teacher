using System.Collections.Generic;

public static class GameControlsDatabase
{
    private static Dictionary<string, ControlType> controlMap =
        new Dictionary<string, ControlType>()
        {
            { "BallDropper", ControlType.Joystick },
            { "TriPommePoire", ControlType.Joystick },
            { "SlotMachine", ControlType.Buttons },
            {"Dice", ControlType.Joystick },
            {"MentalMath", ControlType.Buttons },
            {"PopTheBottle", ControlType.Joystick},
            {"FlashTheCar", ControlType.Buttons}
        };

    public static ControlType GetControlType(string sceneName)
    {
        if (controlMap.TryGetValue(sceneName, out var type))
            return type;

        // Valeur par défaut si oubli
        return ControlType.Buttons;
    }
}
