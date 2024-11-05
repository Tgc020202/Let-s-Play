public static class VariableHolder
{
    public static NetworkRole networkRole;
    public static string roomCode;
    public static string mapCode = "Map1";
    public static string modeCode = "Mode1";
    public static bool isBossWin = false;

    public enum Role { Boss, Worker }
    public static Role currentRole = Role.Worker;

    public static int totalNumberOfPlayer = 4;
    public static int maxNumberOfBosses = 1;
    public static int maxNumberOfWorkers = 3;

    public static bool isFromEndGameToRoom = false;
}
