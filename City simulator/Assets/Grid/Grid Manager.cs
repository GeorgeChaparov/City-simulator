using Unity.VisualScripting;

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

    [Min(1)]
    [SerializeField]
    private int m_EmptyCellsBetweenStreets = 1;

    [SerializeField]
    private int m_AllowedConsecutiveTurnsInSameOrientation = 2;

    [Range(0, 1)]
    [SerializeField]
    private float m_TIntersectionLikelihood = 0.5f;

    [Range(0, 1)]
    [SerializeField]
    private float m_XIntersectionLikelihood = 0.5f;

    [Range(0, 1)]
    [SerializeField]
    private float m_IStreetLikelihood = 0.5f;

    [Range(0, 1)]
    [SerializeField]
    private float m_LStreetLikelihood = 0.5f;

    [SerializeField]
    private bool m_PreventLoopAroundTurns = true;

    private float m_LastTIntersectionLikelihood = 0.5f;
    private float m_LastXIntersectionLikelihood = 0.5f;

    private float m_LastIStreetLikelihood = 0.5f;
    private float m_LastLStreetLikelihood = 0.5f;

    public static GridManager Instance { get; private set; }

    private void OnValidate()
    {
        if (m_LastTIntersectionLikelihood != m_TIntersectionLikelihood)
        {
            m_XIntersectionLikelihood = 1 - m_TIntersectionLikelihood;
        }
        else if (m_LastXIntersectionLikelihood != m_XIntersectionLikelihood)
        {
            m_TIntersectionLikelihood = 1 - m_XIntersectionLikelihood;
        }

        m_LastTIntersectionLikelihood = m_TIntersectionLikelihood;
        m_LastXIntersectionLikelihood = m_XIntersectionLikelihood;

        if (m_LastIStreetLikelihood != m_IStreetLikelihood)
        {
            m_LStreetLikelihood = 1 - m_IStreetLikelihood;
        }
        else if (m_LastLStreetLikelihood != m_LStreetLikelihood)
        {
            m_IStreetLikelihood = 1 - m_LStreetLikelihood;
        }

        m_LastIStreetLikelihood = m_IStreetLikelihood;
        m_LastLStreetLikelihood = m_LStreetLikelihood;
    }

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

    private void Start()
    {
        Init();
        StartGeneration();
        Visualize();
    }

    void Update()
    {
        GridGlobals.CellSize = m_CellSize;

        if (GameManager.Instance.m_Reset)
        {
            GameManager.Instance.m_Reset = false;
            GridGlobals.Reset();
            Init();
            StartGeneration();
            Visualize();
        }
    }

    private void Init()
    {
        GridGlobals.Width = GridGlobals.Height = m_GridSize;

        Cell.Init();
        GridGenerator.Init(m_MinStreetsWithoutIntersection, m_MaxStreetsWithoutIntersection, m_MaxTurnsBetweenIntersection,
            m_MinStreetsBetweenTurns, m_MinStreetsAfterIntersectionBeforeTurn, m_EmptyCellsBetweenStreets, m_AllowedConsecutiveTurnsInSameOrientation,
            m_XIntersectionLikelihood, m_PreventLoopAroundTurns, m_IStreetLikelihood);
    }

    private void StartGeneration()
    {
        StartCoroutine(GridGenerator.Generate());
    }

    private void Visualize()
    {
        GridVisualizer.Instance.Init();
    }
}