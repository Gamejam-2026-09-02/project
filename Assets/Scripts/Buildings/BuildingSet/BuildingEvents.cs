using System;


public static class BuildingEvents
{
    public static event Action<Building> OnBuilt;

    public static event Action<Building> OnDestroyed;



    public static void NotifyBuilt(
        Building building)
    {
        OnBuilt?.Invoke(building);
    }



    public static void NotifyDestroyed(
        Building building)
    {
        OnDestroyed?.Invoke(building);
    }
}