using UnityEngine;

public class CommandTerminalManager : MonoBehaviour
{
    private CommandTerminalManager() { }

    private static CommandTerminalManager instance = null;

    public static CommandTerminalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CommandTerminalManager();
            }
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
