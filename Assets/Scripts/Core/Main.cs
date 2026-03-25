using UnityEngine;

public static class Main
{
    public static CustomEvents CustomEvents { get; private set; }
    public static Player Player { get; private set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Start()
    {
        CustomEvents = new CustomEvents();
        Player = new Player();

    }

}