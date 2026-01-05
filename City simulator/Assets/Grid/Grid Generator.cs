using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GridGenerator
{
    private static int m_HIT_END_OF_GRID = -100;

    private static int m_MinStreetsWithoutIntersection = 10;
    private static int m_MaxStreetsWithoutIntersection = 20;
    private static int m_StreetsWithoutIntersectionCount = 0;

    private static int m_MaxTurnsBetweenIntersection = 2;
    private static int m_TurnsBetweenIntersectionCount = 0;
    private static int m_LastTurnIndex = -1;

    private static int m_MinStreetsBetweenTurns = 0;

    private static int m_MinStreetsAfterIntersectionBeforeTurn = 0;

    private static int m_IShapedStreetsCount = 0;
    private static int m_LShapedStreetsCount = 0;

    public static void Init(int _minStreetsWithoutIntersection, int _maxStreetsWithoutIntersection, int _streetsWithoutIntersectionCount,
        int _maxTurnsBetweenIntersection, int _minStreetsBetweenTurns, int _minStreetsAfterIntersectionBeforeTurn)
    {
        m_MinStreetsWithoutIntersection = _minStreetsWithoutIntersection;
        m_MaxStreetsWithoutIntersection = _maxStreetsWithoutIntersection;
        m_StreetsWithoutIntersectionCount = _streetsWithoutIntersectionCount;
        m_MaxTurnsBetweenIntersection = _maxTurnsBetweenIntersection;
        m_MinStreetsBetweenTurns = _minStreetsBetweenTurns;
        m_MinStreetsAfterIntersectionBeforeTurn = _minStreetsAfterIntersectionBeforeTurn;

        m_IShapedStreetsCount = 0;
        m_LShapedStreetsCount = 0;
}

    public static IEnumerator Generate()
    {
        int x = Random.Range(0, GridConsts.Width);
        int y = Random.Range(0, GridConsts.Height);
        int randomStartIndex = y * GridConsts.Width + x;

        yield return CreateStreets(randomStartIndex);

        Debug.Log($"I shaped: {m_IShapedStreetsCount}");
        Debug.Log($"L shaped: {m_LShapedStreetsCount}");
    }

    private static IEnumerator CreateStreets(int _startIndex)
    {
        Stack<int> ToCheck = new Stack<int>();
        int cellCount = 0;

        Cell.PopulateCell(_startIndex, CellType.Street, 2, CellFeature.IShapedStreet, CellOrientation.East);
        GridConsts.StreetAdjacencyList.Add(_startIndex, new List<int>());

        ToCheck.Push(_startIndex);

        do
        {
            List<CellOrientation> directionsFromLastCell = new List<CellOrientation>();
            List<int> indexes = new List<int>();

            int lastCellIndex = ToCheck.Pop();

            (indexes, directionsFromLastCell) = CalculateNextPosition(lastCellIndex);

            if (indexes[0] == m_HIT_END_OF_GRID)
            {
                continue;
            }

            for (int i = 0; i < indexes.Count; i++)
            {

                yield return new WaitUntil(()=>Input.GetKeyUp(KeyCode.N));

                int index = indexes[i];
                CellOrientation dirFromLastCell = directionsFromLastCell[i];

                if (!GridConsts.StreetAdjacencyList.ContainsKey(index))
                {
                    GridConsts.StreetAdjacencyList.Add(index, new List<int>());
                    ToCheck.Push(index);
                }

                GridConsts.StreetAdjacencyList[index].Add(lastCellIndex);
                GridConsts.StreetAdjacencyList[lastCellIndex].Add(index);

                PopulateNextStreetCell(index, lastCellIndex, dirFromLastCell);

                Debug.Log(++cellCount);
            }

        } while (ToCheck.Count != 0);
    }

    private static (List<int>, List<CellOrientation>) CalculateNextPosition(int _index)
    {
        List<CellOrientation> directions = new List<CellOrientation>();
        List<int> positions = new List<int>();

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

        int x = GridUtils.GetXPos(_index);
        int y = GridUtils.GetYPos(_index);


        for (int i = 0; i < directions.Count; i++)
        {
            CellOrientation direction = directions[i];
            int newX = x;
            int newY = y;
            int pos = _index;

            switch (direction)
            {
                case CellOrientation.East:
                    newX++;
                    break;
                case CellOrientation.West:
                    newX--;
                    break;
                case CellOrientation.North:
                    newY++;
                    break;
                case CellOrientation.South:
                    newY--;
                    break;
                default:
                    Debug.LogError("Cell Orientation is something other then East, West, North or South");
                    break;
            }

            pos = newY * GridConsts.Width + newX;

            // It's out of bounds.
            if (pos < 0 || pos >= GridConsts.Width * GridConsts.Height)
            {
                directions.RemoveAt(i);
                --i;
                continue;
            }
            // Or it's already taken.
            else if (GridConsts.StreetAdjacencyList.ContainsKey(pos))
            {
                directions.RemoveAt(i);
                --i;
                continue;
            }

            positions.Add(pos);
        }

        if (directions.Count == 0)
        {
            positions.Add(m_HIT_END_OF_GRID);
        }

        return (positions, directions);
    }

    private static CellOrientation CalculateAllowedDirections(int _index)
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

    private static void PopulateNextStreetCell(int _currentCellIndex, int _lastCellIndex, CellOrientation dirFromLastCell)
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
                    m_IShapedStreetsCount++;
                    break;
                }

                // We are too close to the last intersection to make a turn.
                if (m_StreetsWithoutIntersectionCount < m_MinStreetsAfterIntersectionBeforeTurn)
                {
                    m_IShapedStreetsCount++;
                    break;
                }

                // If we already have at least one turn but we are too close to it.
                if (m_LastTurnIndex != -1 && m_StreetsWithoutIntersectionCount - m_LastTurnIndex < m_MinStreetsBetweenTurns)
                {
                    m_IShapedStreetsCount++;
                    break;
                }

                switch (Random.Range(0, 2))
                {
                    case 0:
                        // It's I shaped (straight) by default.
                        m_IShapedStreetsCount++;
                        break;
                    case 1:
                        newCellFeatures = CellFeature.LShapedStreet;
                        m_LastTurnIndex = m_StreetsWithoutIntersectionCount;
                        m_TurnsBetweenIntersectionCount++;
                        m_LShapedStreetsCount++;
                        break;
                    default:
                        Debug.LogError("Range unsupported!");
                        break;
                }

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
}
