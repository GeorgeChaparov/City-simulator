using System.Collections.Generic;

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int m_GridSize = 30;

    public static GridManager instance;

    Dictionary<int, List<int>> m_StreetAdjacencyList;
    Dictionary<int, List<int>> m_SidewalkAdjacencyList;


    private void OnValidate()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        TilemapManager.instance.gridSize = m_GridSize;
    }

    void Start()
    {
        Cell.Init(m_GridSize, m_GridSize);

        Init();
        GenerateGrid();
    }

    void Update()
    {

    }

    private void Init()
    {
        m_StreetAdjacencyList = new Dictionary<int, List<int>>();
        m_SidewalkAdjacencyList = new Dictionary<int, List<int>>();
    }

    private void GenerateGrid()
    {
        int startIndex = Random.Range(0, m_GridSize + 1) * Random.Range(0, m_GridSize + 1);

        CreateStreets(startIndex);
    }

    private void CreateStreets(int _startIndex)
    {
        int index = _startIndex;

        Cell.PopulateCell(_startIndex, CellType.Street, 2, CellFeature.None, CellOrientation.East);
        m_StreetAdjacencyList.Add(index, new List<int>());

        do
        {
            if (true)
            {
                
            }


        } while (index >= 0 && index < m_GridSize * m_GridSize);
    }


}
