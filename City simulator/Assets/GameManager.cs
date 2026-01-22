using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int counter = 0;

    public bool Continue = false;
    public bool Skip = false;
    public bool Reset = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        GridGlobals.Init();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            counter++;
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            Continue = true;
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            Skip = true;
            Continue = true;
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            Skip = false;
            Continue = false;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            Reset = true;
            Skip = false;
            counter = 0;
            Debug.ClearDeveloperConsole();
        }
    }

    public void ResetState()
    {
        counter = 0;
        Continue = false;
        Skip = false;
        Reset = false;
    }
}
