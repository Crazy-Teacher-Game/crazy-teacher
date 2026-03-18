using System.Collections.Generic;

public static class GameDescriptionDatabase
{
    private static Dictionary<string, string> descriptionMap =
        new Dictionary<string, string>()
        {
            { "DropTheFish", "Fais tomber !" },
            { "TriPommePoire", "Trie !" },
            { "SlotMachine", "777 !" },
            { "Dice", "Tourne !" },
            { "MentalMath", "Résous !" },
            { "PopTheBottle", "Secoue !" },
            { "FlashTheCar", "Flash !" },
            { "TimerGame", "Devine !" },
            { "Loop", "Dezoome !" },
            { "ExplodeTheBalloon", "Explose !" },
        };

    public static string GetDescription(string sceneName)
    {
        if (descriptionMap.TryGetValue(sceneName, out var description))
            return description;

        return "";
    }
}
