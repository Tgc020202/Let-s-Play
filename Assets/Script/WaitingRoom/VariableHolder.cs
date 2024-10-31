using System.Collections.Generic;
public static class VariableHolder
{
    public static string roomCode;
    public static string mapCode = "Map1";
    public static string modeCode = "Mode1";
    public static bool isBossWin = true;
    public static string currentRole = "Worker";

    // Number of player roles per game
    public static int totalNumberOfPlayer = 4;
    public static int maxNumberOfBosses = 1;
    public static int maxNumberOfWorkers = 3;
}
