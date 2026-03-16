using System.Collections.Generic;

public static class GameDescriptionDatabase
{
    private static Dictionary<string, string> descriptionMap =
        new Dictionary<string, string>()
        {
            { "DropTheFish", "Fais tomber !" },
            { "TriPommePoire", "Trie !" },
            { "SlotMachine", "Arrête !" },
            { "Dice", "Tourne !" },
            { "MentalMath", "Résous !" },
            { "PopTheBottle", "Secoue !" },
            { "FlashTheCar", "Flashes !" },
            { "TimerGame", "Devine !" },
            { "Loop", "Dezoome !" },
            { "ExplodeTheBalloon", "Gonfle !" },
        };

    public static string GetDescription(string sceneName)
    {
        if (descriptionMap.TryGetValue(sceneName, out var description))
            return description;

        return "";
    }
}
