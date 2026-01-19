using Unity.VisualScripting;

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int gridSize = 30;

    [Tooltip("How big one cell is.")]
    [SerializeField]
    private int cellSize = 30;

    [Header("\t\tRoad generation")]
    [Space(10)]

    [Header("General")]

    [Tooltip("The minimum amount of non-road cells between each road")]
    [Min(1)]
    [SerializeField]
    private int cellsBetweenRoads = 1;

    [Space(5)]
    [Header("Intersections")]

    [Tooltip("The maximum amount of streets between each intersection.")]
    [SerializeField]
    private int minStreetsWithoutIntersection = 10;

    [Tooltip("The minimum amount of streets between each intersection.")]
    [SerializeField]
    private int maxStreetsWithoutIntersection = 20;

    [Tooltip("How likely is to choose T shaped intersection.")]
    [Range(0, 1)]
    [SerializeField]
    private float tIntersectionLikelihood = 0.5f;

    [Tooltip("How likely is to choose X shaped intersection.")]
    [Range(0, 1)]
    [SerializeField]
    private float xIntersectionLikelihood = 0.5f;

    [Space(5)]
    [Header("Streets")]

    [Tooltip("How likely is to choose I shaped street.")]
    [Range(0, 1)]
    [SerializeField]
    private float iStreetLikelihood = 0.5f;

    [Space(5)]
    [Header("90 degree turns")]

    [Tooltip("How likely is to choose L shaped street.")]
    [Range(0, 1)]
    [SerializeField]
    private float lStreetLikelihood = 0.5f;

    [Tooltip("The maximum amount of 90 degree turns between each intersection.")]
    [SerializeField]
    private int maxTurnsBetweenIntersection = 2;

    [Tooltip("The minimum amount of streets between each turn.")]
    [SerializeField]
    private int minStreetsBetweenTurns = 0;

    [Tooltip("The minimum amount of streets before the first turn.")]
    [SerializeField]
    private int minStreetsBeforeFirstTurn = 0;

    [Tooltip("How many turns in the same direction, that come one after another, are allowed between two intersections.")]
    [SerializeField]
    private int allowedConsecutiveTurnsInSameOrientation = 2;

    [Tooltip("If we want to prevent the road from making 3 or more turns in the same direction and so making a circle and crashing into itself.")]
    [SerializeField]
    private bool preventLoopAroundTurns = true;

    [Space(10)]
    [Header("\t\tRoad reconstruction")]
    [Space(10)]

    [Tooltip("The amount of streets, after given X shaped intersection, before a dead end, after which we will just replace the X shaped intersection with another intersection or street and remove everything after that branch")]
    [SerializeField]
    private int streetsAfterXIntersectionBeforeDeadEnd = 10;

    [Tooltip("The amount of streets, after given T shaped intersection, before a dead end, after which we will just replace the T shaped intersection with another intersection or street and remove everything after that branch")]
    [SerializeField]
    private int streetsAfterTIntersectionBeforeDeadEnd = 10;

    [Tooltip("The amount of I shaped streets, after given L shaped street, before a dead end, after which we will just replace the L shaped street with a dead end and remove everything after that branch")]
    [SerializeField]
    private int IStreetsAfterLStreetsBeforeDeadEnd = 10;

    private float lastTIntersectionLikelihood = 0.5f;
    private float lastXIntersectionLikelihood = 0.5f;

    private float lastIStreetLikelihood = 0.5f;
    private float lastLStreetLikelihood = 0.5f;

    public static GridManager Instance { get; private set; }

    private void OnValidate()
    {
        if (lastTIntersectionLikelihood != tIntersectionLikelihood)
        {
            xIntersectionLikelihood = 1 - tIntersectionLikelihood;
        }
        else if (lastXIntersectionLikelihood != xIntersectionLikelihood)
        {
            tIntersectionLikelihood = 1 - xIntersectionLikelihood;
        }

        lastTIntersectionLikelihood = tIntersectionLikelihood;
        lastXIntersectionLikelihood = xIntersectionLikelihood;

        if (lastIStreetLikelihood != iStreetLikelihood)
        {
            lStreetLikelihood = 1 - iStreetLikelihood;
        }
        else if (lastLStreetLikelihood != lStreetLikelihood)
        {
            iStreetLikelihood = 1 - lStreetLikelihood;
        }

        lastIStreetLikelihood = iStreetLikelihood;
        lastLStreetLikelihood = lStreetLikelihood;
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
        GridGlobals.CellSize = cellSize;

        if (GameManager.Instance.Reset)
        {
            GameManager.Instance.Reset = false;
            GridGlobals.Reset();
            Init();
            StartGeneration();
            Visualize();
        }
    }

    private void Init()
    {
        GridGlobals.Width = GridGlobals.Height = gridSize;

        Cell.Init();
        GridGenerator.Init(minStreetsWithoutIntersection, maxStreetsWithoutIntersection, maxTurnsBetweenIntersection,
            minStreetsBetweenTurns, minStreetsBeforeFirstTurn, cellsBetweenRoads, allowedConsecutiveTurnsInSameOrientation,
            xIntersectionLikelihood, preventLoopAroundTurns, iStreetLikelihood, streetsAfterXIntersectionBeforeDeadEnd,
            streetsAfterTIntersectionBeforeDeadEnd, IStreetsAfterLStreetsBeforeDeadEnd);
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