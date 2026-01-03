using UnityEngine;
public class GameManager : MonoBehaviour
{
    public GameManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
