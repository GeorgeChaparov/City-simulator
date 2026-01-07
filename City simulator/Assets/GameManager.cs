using UnityEngine;
using UnityEngine.InputSystem.Android;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int counter = 0;

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
    }

    void Start()
    {
        GridGlobals.Init();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            counter++;
        }
    }
}
