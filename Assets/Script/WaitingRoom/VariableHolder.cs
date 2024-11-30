public static class VariableHolder
{
    public static NetworkRole networkRole;
    public static string roomCode;
    public static string mapCode = "Map1";
    public static string modeCode = "Mode1";
    public static bool isBossWin = false;

    public enum Role { Boss, Worker }
    public static Role currentRole = Role.Worker;

    public static int defaultTotalNumberOfPlayer = 3;
    public static int defaultMaxNumberOfBosses = 1;
    public static int defaultMaxNumberOfWorkers = 2;
}
