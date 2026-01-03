using System.Collections.Generic;

using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int m_GridSize = 30;

    private int m_Width = 30;

    private int m_Height = 30;

    [SerializeField]
    private int m_MinStreetsWithoutIntersection = 10;

    [SerializeField]
    private int m_MaxStreetsWithoutIntersection = 20;

    private int m_StreetsWithoutIntersectionCount = 0;

    [SerializeField]
    private int m_MaxTurnsBetweenIntersection = 2;

    private int m_TurnsBetweenIntersectionCount = 0;

    private int m_LastTurnIndex = -1;

    [SerializeField]
    private int m_MinStreetsBetweenTurns = 0;

    [SerializeField]
    private int m_MinStreetsAfterIntersectionBeforeTurn = 0;

    public static GridManager instance;

    Dictionary<int, List<int>> m_StreetAdjacencyList;
    Dictionary<int, List<int>> m_SidewalkAdjacencyList;


    private void Awake()
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
        m_Width = m_Height = m_GridSize;

        Cell.Init(m_Width, m_Height);

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
        int x = Random.Range(0, m_Width);
        int y = Random.Range(0, m_Height);
        int startIndex = y * m_Width + x;

        CreateStreets(startIndex);
    }

    private void CreateStreets(int _startIndex)
    {
        int index = _startIndex;

        Cell.PopulateCell(index, CellType.Street, 2, CellFeature.IShapedStreet, CellOrientation.East);
        m_StreetAdjacencyList.Add(index, new List<int>());


        //TOCHECK PUSH LOGIC IS BAD. I NEED TO PUSH EACH POSSIBLE SIDE INSTEAD.

        Stack<int> ToCheck = new Stack<int>();
        ToCheck.Push(index);

        do
        {
            int lastCellIndex = ToCheck.Pop();
            CellOrientation newCellDirFromLastCell = CellOrientation.None;
            (index, newCellDirFromLastCell) = CalculateNextPosition(lastCellIndex);

            if (!m_StreetAdjacencyList.ContainsKey(index))
            {
                m_StreetAdjacencyList.Add(index, new List<int>());
                ToCheck.Push(index);
            }

            m_StreetAdjacencyList[index].Add(lastCellIndex);
            m_StreetAdjacencyList[lastCellIndex].Add(index);

            PopulateNextStreetCell(index, lastCellIndex, newCellDirFromLastCell);

        } while (ToCheck.Count != 0);
    }

    private (int, CellOrientation) CalculateNextPosition(int _index)
    {
        List<CellOrientation> directions = new List<CellOrientation>();
        CellOrientation dir = 0;
        int pos = _index;

        CellOrientation allowedDirections = CalculateAllowedDirections(_index);

        if ((allowedDirections & CellOrientation.East) != 0)
        {
            directions.Add(CellOrientation.East);
        }

        if ((allowedDirections & CellOrientation.West) != 0)
        {
            directions.Add(CellOrientation.West);
        }

        if ((allowedDirections & CellOrientation.North) != 0)
        {
            directions.Add(CellOrientation.North);
        }

        if ((allowedDirections & CellOrientation.South) != 0)
        {
            directions.Add(CellOrientation.South);
        }

        int x = GetXPos(_index);
        int y = GetYPos(_index);

        while (directions.Count != 0)
        {
            int random = Random.Range(0, directions.Count);
            int newX = x;
            int newY = y;

            switch (directions[random])
            {
                case CellOrientation.East:
                    newX++;
                    dir = CellOrientation.East;
                    break;
                case CellOrientation.West:
                    newX--;
                    dir = CellOrientation.West;
                    break;
                case CellOrientation.North:
                    newY++;
                    dir = CellOrientation.North;
                    break;
                case CellOrientation.South:
                    newY--;
                    dir = CellOrientation.South;
                    break;
                default:
                    Debug.LogError("Cell Orientation is something other then East, West, North or South");
                    break;
            }

            pos = newY * m_Width + newX;

            // It's out of bounds.
            if (pos < 0 && pos >= m_Width * m_Height)
            {
                directions.RemoveAt(random);
                continue;
            }
            // Or it's already taken.
            else if (m_StreetAdjacencyList.ContainsKey(pos))
            {
                directions.RemoveAt(random);
                continue;
            }

            break;
        }

        if (pos == _index)
        {
            Debug.LogError("Error while calculating the next position. It is equal to the last position.");
        }

        if (directions.Count == 0)
        {
            Debug.LogError("Error while calculating the next position. There are no valid positions that are inside the grid.");
        }

        return (pos, dir);
    }

    private CellOrientation CalculateAllowedDirections(int _index)
    {
        CellOrientation directions = CellOrientation.None;
        CellFeature features = Cell.GetFeatures(_index);
        CellOrientation orientation = Cell.GetOrientation(_index);

        switch (Cell.GetType(_index))
        {
            case CellType.Empty:
                break;
            case CellType.Building:
                break;
            case CellType.Sidewalk:
                break;
            case CellType.Street:
                if ((features & CellFeature.IShapedStreet) != 0)
                {
                    if (orientation == CellOrientation.East || orientation == CellOrientation.West)
                    {
                        directions = CellOrientation.East | CellOrientation.West;
                    }
                    else
                    {
                        directions = CellOrientation.North | CellOrientation.South;
                    }
                }
                else if ((features & CellFeature.LShapedStreet) != 0)
                {
                    switch (orientation)
                    {
                        case CellOrientation.East:
                            directions = CellOrientation.East | CellOrientation.North;
                            break;
                        case CellOrientation.West:
                            directions = CellOrientation.West | CellOrientation.South;
                            break;
                        case CellOrientation.North:
                            directions = CellOrientation.North | CellOrientation.West;
                            break;
                        case CellOrientation.South:
                            directions = CellOrientation.South | CellOrientation.East;
                            break;
                        default:
                            Debug.LogError("Cell Orientation is something other then East, West, North or South");
                            break;
                    }
                }

                break;
            case CellType.Intersection:
                if ((features & CellFeature.TShapedIntersection) != 0)
                {
                    switch (orientation)
                    {
                        case CellOrientation.East:
                            directions = CellOrientation.East | CellOrientation.North | CellOrientation.South;
                            break;
                        case CellOrientation.West:
                            directions = CellOrientation.West | CellOrientation.North | CellOrientation.South;
                            break;
                        case CellOrientation.North:
                            directions = CellOrientation.North | CellOrientation.East | CellOrientation.West;
                            break;
                        case CellOrientation.South:
                            directions = CellOrientation.South | CellOrientation.East | CellOrientation.West;
                            break;
                        default:
                            Debug.LogError("Cell Orientation is something other then East, West, North or South");
                            break;
                    }
                }
                else if ((features & CellFeature.XShapedIntersection) != 0)
                {
                    directions = CellOrientation.East | CellOrientation.West | CellOrientation.North | CellOrientation.South;
                }
                break;
            default:
                Debug.LogError("This cell type is unsupported.");
                break;
        }

        if (directions == CellOrientation.None)
        {
            Debug.LogError($"Can't calculate the allowed directions from Cell at {_index}");
        }

        return directions;
    }

    private void PopulateNextStreetCell(int _currentCellIndex, int _lastCellIndex, CellOrientation dirFromLastCell)
    {
        CellType newCellType = CellType.Street;
        CellFeature newCellFeatures = CellFeature.None;
        CellOrientation newCellOrientation = CellOrientation.None;
        int traversalCost = -1;

        CellType lastCellType = Cell.GetType(_lastCellIndex);
        CellFeature lastCellFeatures = Cell.GetFeatures(_lastCellIndex);
        CellOrientation lastCellOrientation = Cell.GetOrientation(_lastCellIndex);

        /* CALCULATING TYPE */

        // If we have exceeded the maximum allowed street count without an intersection, we will create one.
        if (m_StreetsWithoutIntersectionCount > m_MaxStreetsWithoutIntersection)
        {
            newCellType = CellType.Intersection;
            m_StreetsWithoutIntersectionCount = 0;
        }
        // If we have exceeded the minimum allowed street count without an intersection, we decide randomly if we will create one or not.
        else if (m_StreetsWithoutIntersectionCount >= m_MinStreetsWithoutIntersection)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    // It's a street by default.
                    break;
                case 1:
                    newCellType = CellType.Intersection;
                    m_StreetsWithoutIntersectionCount = 0;
                    break;
                default:
                    Debug.LogError("Range unsupported!");
                    break;
            }
        }
        else
        {
            ++m_StreetsWithoutIntersectionCount;
        }

        /* CALCULATING SHAPE */

        switch (newCellType)
        {
            case CellType.Street:

                traversalCost = 2;
                newCellFeatures = CellFeature.IShapedStreet;

                // If we have enough turns, we choose only straight streets. 
                if (m_TurnsBetweenIntersectionCount >= m_MaxTurnsBetweenIntersection)
                {
                    // It's I shaped (straight) by default.
                    break;
                }

                // We are too close to the last intersection to make a turn.
                if (m_StreetsWithoutIntersectionCount < m_MinStreetsAfterIntersectionBeforeTurn)
                {
                    break;
                }

                // If we already have at least one turn but we are too close to it.
                if (m_LastTurnIndex != -1 && m_StreetsWithoutIntersectionCount - m_LastTurnIndex < m_MinStreetsBetweenTurns)
                {
                    break;
                }

                switch (Random.Range(0, 2))
                {
                    case 0:
                        // It's I shaped (straight) by default.
                        break;
                    case 1:
                        newCellFeatures = CellFeature.LShapedStreet;
                        m_LastTurnIndex = m_StreetsWithoutIntersectionCount;
                        break;
                    default:
                        Debug.LogError("Range unsupported!");
                        break;
                }

                m_TurnsBetweenIntersectionCount++;
                break;
            case CellType.Intersection:
                traversalCost = 5;

                switch (Random.Range(0, 2))
                {
                    case 0:
                        newCellFeatures = CellFeature.XShapedIntersection;
                        break;
                    case 1:
                        newCellFeatures = CellFeature.TShapedIntersection;
                        break;
                    default:
                        Debug.LogError("Range unsupported!");
                        break;
                }

                m_TurnsBetweenIntersectionCount = 0;
                m_LastTurnIndex = -1;
                break;
            default:
                Debug.LogError("This cell type is unsupported.");
                break;
        }

        /* CALCULATING ORIANTATION */

        int randomRotation = 0;
        switch (newCellFeatures)
        {
            case CellFeature.IShapedStreet:
                newCellOrientation = dirFromLastCell;

                break;
            case CellFeature.LShapedStreet:
                randomRotation = Random.Range(0, 2);

                switch (lastCellOrientation)
                {
                    case CellOrientation.East:
                        newCellOrientation = randomRotation == 0 ? CellOrientation.West : CellOrientation.North;
                        break;
                    case CellOrientation.West:
                        newCellOrientation = randomRotation == 0 ? CellOrientation.South : CellOrientation.East;
                        break;
                    case CellOrientation.North:
                        newCellOrientation = randomRotation == 0 ? CellOrientation.North : CellOrientation.East;
                        break;
                    case CellOrientation.South:
                        newCellOrientation = randomRotation == 0 ? CellOrientation.South : CellOrientation.West;
                        break;
                    default:
                        Debug.LogError("This cell orientation is unsupported.");
                        break;
                }
                break;
            case CellFeature.TShapedIntersection:
                randomRotation = Random.Range(0, 3);

                switch (lastCellOrientation)
                {
                    case CellOrientation.East:
                        if (randomRotation == 0)
                        {
                            newCellOrientation = CellOrientation.East;
                        }
                        else if (randomRotation == 1)
                        {
                            newCellOrientation = CellOrientation.North;
                        }
                        else
                        {
                            newCellOrientation = CellOrientation.South;
                        }
                        break;
                    case CellOrientation.West:
                        if (randomRotation == 0)
                        {
                            newCellOrientation = CellOrientation.West;
                        }
                        else if (randomRotation == 1)
                        {
                            newCellOrientation = CellOrientation.North;
                        }
                        else
                        {
                            newCellOrientation = CellOrientation.South;
                        }
                        break;
                    case CellOrientation.North:
                        if (randomRotation == 0)
                        {
                            newCellOrientation = CellOrientation.East;
                        }
                        else if (randomRotation == 1)
                        {
                            newCellOrientation = CellOrientation.North;
                        }
                        else
                        {
                            newCellOrientation = CellOrientation.West;
                        }
                        break;
                    case CellOrientation.South:
                        if (randomRotation == 0)
                        {
                            newCellOrientation = CellOrientation.East;
                        }
                        else if (randomRotation == 1)
                        {
                            newCellOrientation = CellOrientation.West;
                        }
                        else
                        {
                            newCellOrientation = CellOrientation.South;
                        }
                        break;
                    default:
                        Debug.LogError("This cell orientation is unsupported.");
                        break;
                }
                break;
            case CellFeature.XShapedIntersection:
                newCellOrientation = dirFromLastCell;
                break;
            default:
                Debug.LogError("This cell feature is unsupported.");
                break;
        }

        Cell.PopulateCell(_currentCellIndex, newCellType, traversalCost, newCellFeatures, newCellOrientation);
    }

    private int GetXPos(int _index)
    {
        return _index % m_Width;
    }

    private int GetYPos(int _index)
    {
        return (_index % (m_Width * m_Height)) / m_Width;
    }
}
