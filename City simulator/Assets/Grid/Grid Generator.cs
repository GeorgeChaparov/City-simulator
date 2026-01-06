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

    private static int m_EmptyCellsBetweenStreets = 1;

    private static int m_IShapedStreetsCount = 0;
    private static int m_LShapedStreetsCount = 0;

    public static void Init(int _minStreetsWithoutIntersection, int _maxStreetsWithoutIntersection, int _maxTurnsBetweenIntersection, 
        int _minStreetsBetweenTurns, int _minStreetsAfterIntersectionBeforeTurn, int _emptyCellsBetweenStreets)
    {
        m_MinStreetsWithoutIntersection = _minStreetsWithoutIntersection;
        m_MaxStreetsWithoutIntersection = _maxStreetsWithoutIntersection;
        m_MaxTurnsBetweenIntersection = _maxTurnsBetweenIntersection;
        m_MinStreetsBetweenTurns = _minStreetsBetweenTurns;
        m_MinStreetsAfterIntersectionBeforeTurn = _minStreetsAfterIntersectionBeforeTurn;
        m_EmptyCellsBetweenStreets = _emptyCellsBetweenStreets;


        m_IShapedStreetsCount = 0;
        m_LShapedStreetsCount = 0;
    }


    public static void Generate()
    {
        int x = Random.Range(0, GridConsts.Width);
        int y = Random.Range(0, GridConsts.Height);
        int randomStartIndex = y * GridConsts.Width + x;

        CreateStreets(randomStartIndex);

        Debug.Log($"I shaped: {m_IShapedStreetsCount}");
        Debug.Log($"L shaped: {m_LShapedStreetsCount}");
    }

    private static void CreateStreets(int _startIndex)
    {
        Stack<int> ToCheck = new Stack<int>();
        int cellCount = 1;

        Cell.PopulateCell(_startIndex, CellType.Intersection, 2, CellFeature.XShapedIntersection, CellOrientation.East);
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

            bool isOutOfBounds = false;

            switch (direction)
            {
                case CellOrientation.East:
                    newX++;

                    pos = newY * GridConsts.Width + newX;
                    if (GridUtils.GetXPos(pos) < x)
                    {
                        isOutOfBounds = true;
                    }
                    break;
                case CellOrientation.West:
                    newX--;

                    pos = newY * GridConsts.Width + newX;
                    if (GridUtils.GetXPos(pos) > x)
                    {
                        isOutOfBounds = true;
                    }
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

            if (pos == _index)
            {
                pos = newY * GridConsts.Width + newX;
            }

            if (pos < 0 || pos >= GridConsts.Width * GridConsts.Height)
            {
                isOutOfBounds = true;
            }

            // It's out of bounds.
            if (isOutOfBounds)
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

            // If we hit the grid, and there is no way to continue from this cell, that means we will continue from the last intersection.
            // As when creating the intersection, we put on cell in each possible way, there already will be one street. That is why we set the count to -1.
            m_StreetsWithoutIntersectionCount = -1;
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

    private static void PopulateNextStreetCell(int _currentCellIndex, int _lastCellIndex, CellOrientation _dirFromLastCell)
    {
        CellFeature possibleStreets = CellFeature.IShapedStreet | CellFeature.LShapedStreet;
        CellFeature possibleIntersections = CellFeature.TShapedIntersection | CellFeature.XShapedIntersection;
        bool foundPossible = false;

        CellType newCellType = CellType.Empty;
        CellFeature newCellFeatures = CellFeature.None;
        CellOrientation newCellOrientation = CellOrientation.None;
        int traversalCost = -1;

        // While we have valid options for streets and intersections, but we have not chosen one.
        while ((possibleIntersections != CellFeature.None && possibleStreets != CellFeature.None) && !foundPossible)
        {
            /* CALCULATING TYPE */

            // If we have exceeded the maximum allowed street count without an intersection, we will try to create one.
            if (m_StreetsWithoutIntersectionCount > m_MaxStreetsWithoutIntersection)
            {
                // If there are no more possible intersections, we break the loop.
                if (possibleIntersections == CellFeature.None)
                {
                    break;
                }

                newCellType = selectType(CellType.Intersection);
                m_StreetsWithoutIntersectionCount = 0;
            }
            // If we have exceeded the minimum allowed street count without an intersection, we decide randomly if we will try to create one or not.
            else if (m_StreetsWithoutIntersectionCount >= m_MinStreetsWithoutIntersection)
            {
                bool choseType = false;
                switch (Random.Range(0, 2))
                {
                    case 0:
                        // If there are no more possible streets, we try to create an intersection.
                        if (possibleStreets == CellFeature.None)
                        {
                            // If there are no more possible intersections, we break the switch.
                            if (possibleIntersections == CellFeature.None)
                            {
                                break;
                            }

                            newCellType = selectType(CellType.Intersection);
                            choseType = true;
                            break;
                        }

                        newCellType = selectType(CellType.Street);
                        choseType = true;
                        break;
                    case 1:

                        // If there are no more possible intersections, we try to create a street.
                        if (possibleIntersections == CellFeature.None)
                        {
                            // If there are no more possible streets, we break the switch.
                            if (possibleStreets == CellFeature.None)
                            {
                                break;
                            }

                            newCellType = selectType(CellType.Street);
                            choseType = true;
                            break;
                        }

                        newCellType = selectType(CellType.Intersection);
                        choseType = true;
                        break;
                    default:
                        Debug.LogError("Range unsupported!");
                        break;
                }

                if (!choseType)
                {
                    // If we did not chose a type, we break the loop.
                    break;
                }
            }
            else
            {
                newCellType = selectType(CellType.Street);
            }

            /* CALCULATING SHAPE */

            bool choseFeature = false;
            switch (newCellType)
            {
                case CellType.Street:

                    traversalCost = 2;
                    newCellFeatures = CellFeature.None;

                    // If we have enough turns. 
                    // Or we are too close to the last intersection to make a turn.
                    // Or we already have at least one turn, but we are too close to it.
                    // We try to make a street.
                    if ((m_TurnsBetweenIntersectionCount >= m_MaxTurnsBetweenIntersection) ||
                        (m_StreetsWithoutIntersectionCount <= m_MinStreetsAfterIntersectionBeforeTurn) ||
                        (m_LastTurnIndex != -1 && m_StreetsWithoutIntersectionCount - m_LastTurnIndex <= m_MinStreetsBetweenTurns))
                    {
                        // We remove the turn as a possibility
                        if ((possibleStreets & CellFeature.LShapedStreet) != 0)
                        {
                            possibleStreets ^= CellFeature.LShapedStreet;
                        }

                        // If straight streets are not possible, we break the switch.
                        if ((possibleStreets & CellFeature.IShapedStreet) == 0)
                        {
                            break;
                        }

                        newCellFeatures = selectFeature(CellFeature.IShapedStreet);
                        choseFeature = true;
                        break;
                    }

                    switch (Random.Range(0, 2))
                    {
                        // I shaped street.
                        case 0:
                            // If straight streets are not possible, we try to make a turn.
                            if ((possibleStreets & CellFeature.IShapedStreet) == 0)
                            {
                                // If turns are not possible, we break the switch.
                                if ((possibleStreets & CellFeature.LShapedStreet) == 0)
                                {
                                    break;
                                }

                                newCellFeatures = selectFeature(CellFeature.LShapedStreet);
                                choseFeature = true;
                                break;
                            }

                            newCellFeatures = selectFeature(CellFeature.IShapedStreet);
                            choseFeature = true;
                            break;

                        // L shaped street.
                        case 1:
                            // If turns are not possible, we try to make a straight streets.
                            if ((possibleStreets & CellFeature.LShapedStreet) == 0)
                            {
                                // If straight streets not possible, we break the switch.
                                if ((possibleStreets & CellFeature.IShapedStreet) == 0)
                                {
                                    break;
                                }

                                newCellFeatures = selectFeature(CellFeature.IShapedStreet);
                                choseFeature = true;
                                break;
                            }

                            newCellFeatures = selectFeature(CellFeature.LShapedStreet);
                            choseFeature = true;
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
                        // X shaped intersection
                        case 0:
                            // If X shaped intersection are not possible, we try to make a T shaped intersection.
                            if ((possibleIntersections & CellFeature.XShapedIntersection) == 0)
                            {
                                // If T shaped intersection not possible, we break the switch.
                                if ((possibleIntersections & CellFeature.TShapedIntersection) == 0)
                                {
                                    break;
                                }

                                newCellFeatures = selectFeature(CellFeature.TShapedIntersection);
                                choseFeature = true;
                                break;
                            }

                            newCellFeatures = selectFeature(CellFeature.XShapedIntersection);
                            choseFeature = true;
                            break;

                        // T shaped intersection
                        case 1:
                            // If T shaped intersection are not possible, we try to make a X shaped intersection.
                            if ((possibleIntersections & CellFeature.TShapedIntersection) == 0)
                            {
                                // If X shaped intersection not possible, we break the switch.
                                if ((possibleIntersections & CellFeature.XShapedIntersection) == 0)
                                {
                                    break;
                                }

                                newCellFeatures = selectFeature(CellFeature.XShapedIntersection);
                                choseFeature = true;
                                break;
                            }

                            newCellFeatures = selectFeature(CellFeature.TShapedIntersection);
                            choseFeature = true;
                            break;
                        default:
                            Debug.LogError("Range unsupported!");
                            break;
                    }
                    // -1 is the default value. It shows that we don't have a 90 degrees turn yet.
                    m_LastTurnIndex = -1;
                    m_TurnsBetweenIntersectionCount = 0;
                    break;
                default:
                    Debug.LogError("This cell type is unsupported.");
                    break;
            }

            if (!choseFeature)
            {
                // If we did not chose a feature, we continue the loop so we can try with another type.
                continue;
            }

            /* CALCULATING ORIANTATION */

            int randomRotation = 0;

            bool choseOrientation = false;
            switch (newCellFeatures)
            {
                case CellFeature.IShapedStreet:

                    if (!checkForSpace(_dirFromLastCell))
                    {
                        possibleStreets ^= CellFeature.IShapedStreet;
                        break;
                    }

                    newCellOrientation = _dirFromLastCell;
                    choseOrientation = true;
                    break;
                case CellFeature.LShapedStreet:
                    randomRotation = Random.Range(0, 2);

                    switch (_dirFromLastCell)
                    {
                        case CellOrientation.East:
                            
                            // Try west
                            if (randomRotation == 0)
                            {
                                // If the space around west directions is free
                                if (checkForSpace(CellOrientation.West))
                                {
                                    newCellOrientation = CellOrientation.West;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around north directions is free
                                if (checkForSpace(CellOrientation.North))
                                {
                                    newCellOrientation = CellOrientation.North;
                                    choseOrientation = true;
                                    break;
                                }                          
                            }
                            // Try north
                            else
                            {
                                // If the space around north directions is free
                                if (checkForSpace(CellOrientation.North))
                                {
                                    newCellOrientation = CellOrientation.North;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around west directions is free
                                if (checkForSpace(CellOrientation.West))
                                {
                                    newCellOrientation = CellOrientation.West;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            possibleStreets ^= CellFeature.LShapedStreet;
                            break;
                        case CellOrientation.West:

                            // Try south
                            if (randomRotation == 0)
                            {
                                // If the space around south directions is free
                                if (checkForSpace(CellOrientation.South))
                                {
                                    newCellOrientation = CellOrientation.South;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around east directions is free
                                if (checkForSpace(CellOrientation.East))
                                {
                                    newCellOrientation = CellOrientation.East;
                                    choseOrientation = true;
                                    break;
                                }
                            }
                            // Try east
                            else
                            {
                                // If the space around east directions is free
                                if (checkForSpace(CellOrientation.East))
                                {
                                    newCellOrientation = CellOrientation.East;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around south directions is free
                                if (checkForSpace(CellOrientation.South))
                                {
                                    newCellOrientation = CellOrientation.South;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            possibleStreets ^= CellFeature.LShapedStreet;
                            break;
                        case CellOrientation.North:
                            // Try south
                            if (randomRotation == 0)
                            {
                                // If the space around south directions is free
                                if (checkForSpace(CellOrientation.South))
                                {
                                    newCellOrientation = CellOrientation.South;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around west directions is free
                                if (checkForSpace(CellOrientation.West))
                                {
                                    newCellOrientation = CellOrientation.West;
                                    choseOrientation = true;
                                    break;
                                }
                            }
                            // Try west
                            else
                            {
                                // If the space around west directions is free
                                if (checkForSpace(CellOrientation.West))
                                {
                                    newCellOrientation = CellOrientation.West;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around south directions is free
                                if (checkForSpace(CellOrientation.South))
                                {
                                    newCellOrientation = CellOrientation.South;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            possibleStreets ^= CellFeature.LShapedStreet;
                            break;
                        case CellOrientation.South:
                            // Try north
                            if (randomRotation == 0)
                            {
                                // If the space around north directions is free
                                if (checkForSpace(CellOrientation.North))
                                {
                                    newCellOrientation = CellOrientation.North;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around east directions is free
                                if (checkForSpace(CellOrientation.East))
                                {
                                    newCellOrientation = CellOrientation.East;
                                    choseOrientation = true;
                                    break;
                                }
                            }
                            // Try east
                            else
                            {
                                // If the space around east directions is free
                                if (checkForSpace(CellOrientation.East))
                                {
                                    newCellOrientation = CellOrientation.East;
                                    choseOrientation = true;
                                    break;
                                }

                                // If the space around north directions is free
                                if (checkForSpace(CellOrientation.North))
                                {
                                    newCellOrientation = CellOrientation.North;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            possibleStreets ^= CellFeature.LShapedStreet;

                            newCellOrientation = randomRotation == 0 ? CellOrientation.North : CellOrientation.East;
                            break;
                        default:
                            Debug.LogError("This cell orientation is unsupported.");
                            break;
                    }
                    break;
                case CellFeature.TShapedIntersection:
                    randomRotation = Random.Range(0, 3);
                    CellOrientation[,] orders;
                    // Setting this to minus one because when creating an T shaped intersection,
                    // the first cell of each possible way is created and then it continues from the last created cell.
                    // Because of this, between creating the intersection and the next intersection, there will be created one additional cell.
                    m_StreetsWithoutIntersectionCount = -1;

                    switch (_dirFromLastCell)
                    {
                        case CellOrientation.East:
                            orders = new CellOrientation[6, 3] {
                                { CellOrientation.West,  CellOrientation.North, CellOrientation.South },
                                { CellOrientation.West,  CellOrientation.South, CellOrientation.North },
                                { CellOrientation.North, CellOrientation.West,  CellOrientation.South },
                                { CellOrientation.North, CellOrientation.South, CellOrientation.West },
                                { CellOrientation.South, CellOrientation.West,  CellOrientation.North },
                                { CellOrientation.South, CellOrientation.North, CellOrientation.West }
                            };

                            randomRotation = Random.Range(0, 6);

                            for (int i = 0; i < 3; i++)
                            {
                                var orientation = orders[randomRotation, i];
                                if (checkForSpace(orientation))
                                {
                                    newCellOrientation = orientation;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            if (!choseOrientation)
                            {
                                possibleIntersections ^= CellFeature.TShapedIntersection;
                            }
                            break;
                        case CellOrientation.West:
                            orders = new CellOrientation[6, 3]
                            {
                                { CellOrientation.East,  CellOrientation.North, CellOrientation.South },
                                { CellOrientation.East,  CellOrientation.South, CellOrientation.North },
                                { CellOrientation.North, CellOrientation.East,  CellOrientation.South },
                                { CellOrientation.North, CellOrientation.South, CellOrientation.East },
                                { CellOrientation.South, CellOrientation.East,  CellOrientation.North },
                                { CellOrientation.South, CellOrientation.North, CellOrientation.East }
                            };

                            randomRotation = Random.Range(0, 6);

                            for (int i = 0; i < 3; i++)
                            {
                                var orientation = orders[randomRotation, i];
                                if (checkForSpace(orientation))
                                {
                                    newCellOrientation = orientation;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            if (!choseOrientation)
                            {
                                possibleIntersections ^= CellFeature.TShapedIntersection;
                            }
                            break;
                        case CellOrientation.North:
                            orders = new CellOrientation[6, 3]
                            {
                                { CellOrientation.East,  CellOrientation.South, CellOrientation.West },
                                { CellOrientation.East,  CellOrientation.West,  CellOrientation.South },
                                { CellOrientation.South, CellOrientation.East,  CellOrientation.West },
                                { CellOrientation.South, CellOrientation.West,  CellOrientation.East },
                                { CellOrientation.West,  CellOrientation.East,  CellOrientation.South },
                                { CellOrientation.West,  CellOrientation.South, CellOrientation.East }
                            };

                            randomRotation = Random.Range(0, 6);

                            for (int i = 0; i < 3; i++)
                            {
                                var orientation = orders[randomRotation, i];
                                if (checkForSpace(orientation))
                                {
                                    newCellOrientation = orientation;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            if (!choseOrientation)
                            {
                                possibleIntersections ^= CellFeature.TShapedIntersection;
                            }
                            break;
                        case CellOrientation.South:
                            orders = new CellOrientation[6, 3]
                            {
                                { CellOrientation.East,  CellOrientation.West,  CellOrientation.North },
                                { CellOrientation.East,  CellOrientation.North, CellOrientation.West },
                                { CellOrientation.West,  CellOrientation.East,  CellOrientation.North },
                                { CellOrientation.West,  CellOrientation.North, CellOrientation.East },
                                { CellOrientation.North, CellOrientation.East,  CellOrientation.West },
                                { CellOrientation.North, CellOrientation.West,  CellOrientation.East }
                            };

                            randomRotation = Random.Range(0, 6);

                            for (int i = 0; i < 3; i++)
                            {
                                var orientation = orders[randomRotation, i];
                                if (checkForSpace(orientation))
                                {
                                    newCellOrientation = orientation;
                                    choseOrientation = true;
                                    break;
                                }
                            }

                            if (!choseOrientation)
                            {
                                possibleIntersections ^= CellFeature.TShapedIntersection;
                            }
                            break;
                        default:
                            Debug.LogError("This cell orientation is unsupported.");
                            break;
                    }
                    break;
                case CellFeature.XShapedIntersection:
                    // Setting this to minus two because when creating an X shaped intersection,
                    // the first cell of each possible way is created and then it continues from the last created cell.
                    // Because of this, between creating the intersection and the next intersection, there will be created two additional cells.
                    m_StreetsWithoutIntersectionCount = -2;

                    if (!checkForSpace(_dirFromLastCell))
                    {
                        possibleIntersections ^= CellFeature.XShapedIntersection;
                        break;
                    }

                    newCellOrientation = _dirFromLastCell;
                    choseOrientation = true;
                    break;
                default:
                    Debug.LogError("This cell feature is unsupported.");
                    break;
            }

            if (!choseOrientation)
            {
                // If we did not chose an orientation, we continue the loop so we can try with another feature.
                continue;
            }

            foundPossible = true;
        }


        if (!foundPossible)
        {
            newCellType = CellType.Street;
            traversalCost = 2;
            newCellFeatures = CellFeature.DeadEnd;

            switch (_dirFromLastCell)
            {
                case CellOrientation.East:
                    newCellOrientation = CellOrientation.West;
                    break;
                case CellOrientation.West:
                    newCellOrientation = CellOrientation.East;
                    break;
                case CellOrientation.North:
                    newCellOrientation = CellOrientation.South;
                    break;
                case CellOrientation.South:
                    newCellOrientation = CellOrientation.North;
                    break;
                default:
                    break;
            }
        }

        Cell.PopulateCell(_currentCellIndex, newCellType, traversalCost, newCellFeatures, newCellOrientation);





        CellType selectType(CellType typeToSelect)
        {
            switch (typeToSelect)
            {
                case CellType.Street:
                    ++m_StreetsWithoutIntersectionCount;
                    break;
                case CellType.Intersection:
                    m_StreetsWithoutIntersectionCount = 0;
                    break;
                default:
                    break;
            }

            return typeToSelect;
        }

        CellFeature selectFeature(CellFeature featureToSelect)
        {
            switch (featureToSelect)
            {
                case CellFeature.IShapedStreet:
                    m_IShapedStreetsCount++;
                    break;
                case CellFeature.LShapedStreet:
                    m_LastTurnIndex = m_StreetsWithoutIntersectionCount;
                    m_TurnsBetweenIntersectionCount++;
                    m_LShapedStreetsCount++;
                    break;
                case CellFeature.TShapedIntersection:
                    break;
                case CellFeature.XShapedIntersection:
                    break;
                default:
                    break;
            }

            return featureToSelect;
        }

        bool checkForSpace(CellOrientation direction)
        {
            bool result = false;

            switch (newCellFeatures)
            {
                case CellFeature.IShapedStreet:
                    result = checkForSpaceForIShapedStreet(direction);
                    break;
                case CellFeature.LShapedStreet:
                    result = checkForSpaceForLShapedStreet(direction);
                    break;
                case CellFeature.TShapedIntersection:
                    result = checkForSpaceForTShapedIntersection(direction);
                    break;
                case CellFeature.XShapedIntersection:
                    result = checkForSpaceForXShapedIntersection(direction);
                    break;
                default:
                    break;
            }

            return result;
        }

        bool checkForSpaceForIShapedStreet(CellOrientation direction)
        {
            bool result = true;

            int x = GridUtils.GetXPos(_currentCellIndex);
            int y = GridUtils.GetYPos(_currentCellIndex);

            int[] indexes = new int[3];
            switch (direction)
            {
                case CellOrientation.East:
                    indexes[0] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = (y + m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    indexes[2] = (y - m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                case CellOrientation.West:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = (y + m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    indexes[2] = (y - m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                case CellOrientation.North:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = y * GridConsts.Width + (x - m_EmptyCellsBetweenStreets);
                    indexes[2] = y * GridConsts.Width + (x + m_EmptyCellsBetweenStreets);
                    break;
                case CellOrientation.South:
                    indexes[0] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = y * GridConsts.Width + (x - m_EmptyCellsBetweenStreets);
                    indexes[2] = y * GridConsts.Width + (x + m_EmptyCellsBetweenStreets);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                if (GridConsts.StreetAdjacencyList.ContainsKey(indexes[i]))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        bool checkForSpaceForLShapedStreet(CellOrientation direction)
        {
            bool result = true;

            int x = GridUtils.GetXPos(_currentCellIndex);
            int y = GridUtils.GetYPos(_currentCellIndex);

            int[] indexes = new int[3];
            switch (direction)
            {
                case CellOrientation.East:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = y * GridConsts.Width + (x - m_EmptyCellsBetweenStreets);
                    indexes[2] = (y - m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                case CellOrientation.West:
                    indexes[0] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = (y + m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x + m_EmptyCellsBetweenStreets);
                    break;
                case CellOrientation.North:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = (y - m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x + m_EmptyCellsBetweenStreets);
                    break;
                case CellOrientation.South:
                    indexes[0] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = y * GridConsts.Width + (x - m_EmptyCellsBetweenStreets);
                    indexes[2] = (y + m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                if (GridConsts.StreetAdjacencyList.ContainsKey(indexes[i]))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        bool checkForSpaceForTShapedIntersection(CellOrientation direction)
        {
            bool result = true;

            int x = GridUtils.GetXPos(_currentCellIndex);
            int y = GridUtils.GetYPos(_currentCellIndex);

            int[] indexes = new int[3];
            int index = 0;
            switch (direction)
            {
                case CellOrientation.East:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x - m_EmptyCellsBetweenStreets);
                    break;
                case CellOrientation.West:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x + m_EmptyCellsBetweenStreets);
                    break;
                case CellOrientation.North:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[2] = (y - m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                case CellOrientation.South:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[2] = (y + m_EmptyCellsBetweenStreets) * GridConsts.Width + x;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                if (GridConsts.StreetAdjacencyList.ContainsKey(indexes[i]))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        bool checkForSpaceForXShapedIntersection(CellOrientation direction)
        {
            bool result = true;

            int x = GridUtils.GetXPos(_currentCellIndex);
            int y = GridUtils.GetYPos(_currentCellIndex);

            int[] indexes = new int[3];
            switch (direction)
            {
                case CellOrientation.East:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    break;
                case CellOrientation.West:
                    indexes[0] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[1] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    indexes[2] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    break;
                case CellOrientation.North:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[2] = (y + (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    break;
                case CellOrientation.South:
                    indexes[0] = y * GridConsts.Width + (x - (m_EmptyCellsBetweenStreets + 1));
                    indexes[1] = y * GridConsts.Width + (x + (m_EmptyCellsBetweenStreets + 1));
                    indexes[2] = (y - (m_EmptyCellsBetweenStreets + 1)) * GridConsts.Width + x;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (GridConsts.StreetAdjacencyList.ContainsKey(index))
                {
                    CellType type = Cell.GetType(index);

                    if ((type & CellType.Street) != 0 && (type & CellType.Intersection) != 0)
                    {
                        continue;
                    }

                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
