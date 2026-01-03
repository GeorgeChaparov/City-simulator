using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager instance;
    private int m_GridSize = 30;

    public int gridSize
    {
        set {
            if (!m_Tilemap)
            {
                Debug.LogError("Grid not set");
            }

            m_Tilemap.size = new Vector3Int(value, value);
        }
    }


    private int m_CellSize = 1;
    public int CellSize
    {
        set
        {
            if (!m_Tilemap)
            {
                Debug.LogError("Grid not set");
            }

        }
    }

    [SerializeField]
    private Grid m_Grid;

    [SerializeField]
    private Tilemap m_Tilemap;

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
