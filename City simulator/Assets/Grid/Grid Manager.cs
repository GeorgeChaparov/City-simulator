using System.Collections.Generic;

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int m_GridSize = 30;

    [SerializeField]
    private int m_CellSize = 30;

    [SerializeField]
    private int m_MinStreetsWithoutIntersection = 10;

    [SerializeField]
    private int m_MaxStreetsWithoutIntersection = 20;

    [SerializeField]
    private int m_MaxTurnsBetweenIntersection = 2;

    [SerializeField]
    private int m_MinStreetsBetweenTurns = 0;

    [SerializeField]
    private int m_MinStreetsAfterIntersectionBeforeTurn = 0;

    [SerializeField]
    private int m_EmptyCellsBetweenStreets = 1;

    public static GridManager Instance { get; private set; }

    private void Awake()
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

    void Update()
    {
        GridConsts.CellSize = m_CellSize;

        if (Input.GetKeyUp(KeyCode.R))
        {
            GridConsts.Reset();

            GridConsts.Width = GridConsts.Height = m_GridSize;

            Cell.Init();
            GridGenerator.Init(m_MinStreetsWithoutIntersection, m_MaxStreetsWithoutIntersection, m_MaxTurnsBetweenIntersection, 
                m_MinStreetsBetweenTurns, m_MinStreetsAfterIntersectionBeforeTurn, m_EmptyCellsBetweenStreets);

            GridGenerator.Generate();
        }
    }
}
