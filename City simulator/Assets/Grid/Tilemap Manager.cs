using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager instance;
    private int gridSize = 30;

    public int GridSize
    {
        set {
            if (!tilemap)
            {
                Debug.LogError("Grid not set");
            }

            tilemap.size = new Vector3Int(value, value);
        }
    }


    private int cellSize = 1;
    public int CellSize
    {
        set
        {
            if (!tilemap)
            {
                Debug.LogError("Grid not set");
            }

        }
    }

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap tilemap;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
