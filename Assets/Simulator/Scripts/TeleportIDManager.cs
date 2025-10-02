using System.Collections.Generic;

public static class TeleportIDManager
{
    private static HashSet<int> usedEntranceIDs = new HashSet<int>();
    private static HashSet<int> usedExitIDs = new HashSet<int>();

    private static int nextEntranceID = 1;
    private static int nextExitID = 1;

    public static int GetUniqueEntranceID()
    {
        while (usedEntranceIDs.Contains(nextEntranceID))
        {
            nextEntranceID++;
        }

        usedEntranceIDs.Add(nextEntranceID);
        return nextEntranceID;
    }

    public static int GetUniqueExitID()
    {
        while (usedExitIDs.Contains(nextExitID))
        {
            nextExitID++;
        }

        usedExitIDs.Add(nextExitID);
        return nextExitID;
    }

    public static void RegisterEntranceID(int id)
    {
        usedEntranceIDs.Add(id);
    }

    public static void RegisterExitID(int id)
    {
        usedExitIDs.Add(id);
    }

    public static void UnregisterEntranceID(int id)
    {
        usedEntranceIDs.Remove(id);
    }

    public static void UnregisterExitID(int id)
    {
        usedExitIDs.Remove(id);
    }
}
