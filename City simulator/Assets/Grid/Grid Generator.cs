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

    private static int m_AllowedConsecutiveTurnsInSameOrientation = 2;

    private static float m_XIntersectionLikelihood = 0.5f;

    private static int m_IShapedStreetsCount = 0;
    private static int m_LShapedStreetsCount = 0;
    private static int m_TotalCellCount = 1;

    private static int m_Counter = 0;

    // Counts how many intersections of the same type we have in a row.
    private static (CellFeature featureType, int count) m_LastIntersectionTypeCount = (CellFeature.None, 0);
    // Counts how many turns with the same orientation we have in a row.
    private static (CellOrientation orientation, int count) m_LastTurnOrientationCount = (CellOrientation.None, 0);

    public static void Init(int _minStreetsWithoutIntersection, int _maxStreetsWithoutIntersection, int _maxTurnsBetweenIntersection,
        int _minStreetsBetweenTurns, int _minStreetsAfterIntersectionBeforeTurn, int _emptyCellsBetweenStreets, int _allowedConsecutiveTurnsInSameOrientation,
        float _xIntersectionLikelihood)
    {
        m_MinStreetsWithoutIntersection = _minStreetsWithoutIntersection;
        m_MaxStreetsWithoutIntersection = _maxStreetsWithoutIntersection;
        m_MaxTurnsBetweenIntersection = _maxTurnsBetweenIntersection;
        m_MinStreetsBetweenTurns = _minStreetsBetweenTurns;
        m_MinStreetsAfterIntersectionBeforeTurn = _minStreetsAfterIntersectionBeforeTurn;
        m_EmptyCellsBetweenStreets = _emptyCellsBetweenStreets;
        m_AllowedConsecutiveTurnsInSameOrientation = _allowedConsecutiveTurnsInSameOrientation;
        m_XIntersectionLikelihood = _xIntersectionLikelihood;

        m_IShapedStreetsCount = 0;
        m_LShapedStreetsCount = 0;
        m_TotalCellCount = 1;
        m_Counter = 0;

        if (m_EmptyCellsBetweenStreets != 1)
        {
            m_IMaskOffsets = GenerateMaskOffset(m_IMaskOffsets, true, false);
            m_LMaskOffsets = GenerateMaskOffset(m_LMaskOffsets);
            m_TMaskOffsets = GenerateMaskOffset(m_TMaskOffsets);
            m_XMaskOffsets = GenerateMaskOffset(m_XMaskOffsets, true);
        }
    }

    private static readonly CellOrientation[,] m_TEastDirectionMask = new CellOrientation[6, 3]
    {
        { CellOrientation.West,  CellOrientation.North, CellOrientation.South },
        { CellOrientation.West,  CellOrientation.South, CellOrientation.North },
        { CellOrientation.North, CellOrientation.West,  CellOrientation.South },
        { CellOrientation.North, CellOrientation.South, CellOrientation.West },
        { CellOrientation.South, CellOrientation.West,  CellOrientation.North },
        { CellOrientation.South, CellOrientation.North, CellOrientation.West }
    };

    private static readonly CellOrientation[,] m_TWestDirectionMask = new CellOrientation[6, 3]
    {
        { CellOrientation.East,  CellOrientation.North, CellOrientation.South },
        { CellOrientation.East,  CellOrientation.South, CellOrientation.North },
        { CellOrientation.North, CellOrientation.East,  CellOrientation.South },
        { CellOrientation.North, CellOrientation.South, CellOrientation.East },
        { CellOrientation.South, CellOrientation.East,  CellOrientation.North },
        { CellOrientation.South, CellOrientation.North, CellOrientation.East }
    };

    private static readonly CellOrientation[,] m_TNorthDirectionMask = new CellOrientation[6, 3]
    {
        { CellOrientation.East,  CellOrientation.South, CellOrientation.West },
        { CellOrientation.East,  CellOrientation.West,  CellOrientation.South },
        { CellOrientation.South, CellOrientation.East,  CellOrientation.West },
        { CellOrientation.South, CellOrientation.West,  CellOrientation.East },
        { CellOrientation.West,  CellOrientation.East,  CellOrientation.South },
        { CellOrientation.West,  CellOrientation.South, CellOrientation.East }
    };

    private static readonly CellOrientation[,] m_TSouthDirectionMask = new CellOrientation[6, 3]
    {
        { CellOrientation.East,  CellOrientation.West,  CellOrientation.North },
        { CellOrientation.East,  CellOrientation.North, CellOrientation.West },
        { CellOrientation.West,  CellOrientation.East,  CellOrientation.North },
        { CellOrientation.West,  CellOrientation.North, CellOrientation.East },
        { CellOrientation.North, CellOrientation.East,  CellOrientation.West },
        { CellOrientation.North, CellOrientation.West,  CellOrientation.East }
    };

    public static (int x, int y)[] m_IMaskOffsets = new (int, int)[]
    {
                                         
                                         
                                         (0, 1),  (1, 1),  (2, 1),
        /*I shaped street facing east -> (0, 0)*/ (1, 0),  (2, 0), (3, 0),
                                         (0, -1), (1, -1), (2, -1),
                                         

    };

    public static (int x, int y)[] m_LMaskOffsets = new (int, int)[]
    {             
        (-1, 2),  (0, 2),  (1, 2),
        (-1, 1),  (0, 1),  (1, 1),
        (-1, 0),//(0, 0), <- L shaped street facing east
        (-1, -1), (0, -1), (1, -1),
    };

    public static (int x, int y)[] m_TMaskOffsets = new (int, int)[]
    {             
                  (0, 3),
                  (0, 2),
        (-1, 1),  (0, 1),  (1, 1),
        (-1, 0),//(0, 0), <- T shaped intersection facing east
        (-1, -1), (0, -1), (1, -1),
                  (0, -2), 
                  (0, -3),                     
    };

    public static (int x, int y)[] m_XMaskOffsets = new (int, int)[]
    {                    
                           (-2, 2),  (-1, 2),  (0, 2),  (1, 2),  (2, 2),  
                           (-2, 1),  (-1, 1),  (0, 1),  (1, 1),  (2, 1),  
        /*X shaped intersection facing east -> (0, 0)*/ (1, 0),  (2, 0),   
                           (-2, -1), (-1, -1), (0, -1), (1, -1), (2, -1), 
                           (-2, -2), (-1, -2), (0, -2), (1, -2), (2, -2),   
    };

    private static (int x, int y)[] GenerateMaskOffset((int x, int y)[] offsets, bool lookingToRight = false, bool leftAndRight = true)
    {
        List<(int x, int y)> mask = new List<(int x, int y)>(offsets);
        List<(int x, int y)> currentLayer = new List<(int x, int y)>();
        List<(int x, int y)> lastLayer = new List<(int x, int y)>(offsets);

        for (int i = 0; i < m_EmptyCellsBetweenStreets; i++)
        {
            foreach ((int x, int y) in lastLayer)
            {
                (int x, int y) upOffset = (0, 0);
                (int x, int y) downOffset = (0, 0);
                (int x, int y) leftOffset = (0, 0);
                (int x, int y) rightOffset = (0, 0);

                (int x, int y) upRightOffset = (0, 0);
                (int x, int y) downRightOffset = (0, 0);
                (int x, int y) downLeftOffset = (0, 0);
                (int x, int y) upLeftOffset = (0, 0);

                if (x > 0)
                {
                    rightOffset = (x + 1, y);

                    if (y > 0)
                    {
                        upOffset = (x, y + 1);
                        upRightOffset = (x + 1, y + 1);
                    }
                    else if (y < 0)
                    {
                        downOffset = (x, y - 1);
                        downRightOffset = (x + 1, y - 1);
                    }
                }
                else if (x < 0)
                {
                    leftOffset = (x - 1, y);

                    if (y > 0)
                    {
                        upOffset = (x, y + 1);
                        upLeftOffset = (x - 1, y + 1);
                    }
                    else if (y < 0)
                    {
                        downOffset = (x, y - 1);
                        downLeftOffset = (x - 1, y - 1);
                    }
                }
                else
                {
                    if (y > 0)
                    {
                        upOffset = (x, y + 1);
                    }
                    else if (y < 0)
                    {
                        downOffset = (x, y - 1);
                    }
                }

                if (lookingToRight)
                {
                    if (!mask.Contains(upOffset) && (upOffset.y != 0 || upOffset.x > 0)) { mask.Add(upOffset); currentLayer.Add(upOffset); }
                    if (!mask.Contains(downOffset) && (downOffset.y != 0 || downOffset.x > 0)) { mask.Add(downOffset); currentLayer.Add(downOffset); }
                    if (!mask.Contains(leftOffset) && (leftOffset.y != 0 || leftOffset.x > 0)) { mask.Add(leftOffset); currentLayer.Add(leftOffset); }
                    if (!mask.Contains(rightOffset) && (rightOffset.y != 0 || rightOffset.x > 0)) { mask.Add(rightOffset); currentLayer.Add(rightOffset); }

                    if (!mask.Contains(upRightOffset) && (upRightOffset.y != 0 || upRightOffset.x > 0)) { mask.Add(upRightOffset); currentLayer.Add(upRightOffset); }
                    if (!mask.Contains(downRightOffset) && (downRightOffset.y != 0 || downRightOffset.x > 0)) { mask.Add(downRightOffset); currentLayer.Add(downRightOffset); }

                    if (!mask.Contains(upLeftOffset) && (upLeftOffset.y != 0 || upLeftOffset.x > 0)) { mask.Add(upLeftOffset); currentLayer.Add(upLeftOffset); }
                    if (!mask.Contains(downLeftOffset) && (downLeftOffset.y != 0 || downLeftOffset.x > 0)) { mask.Add(downLeftOffset); currentLayer.Add(downLeftOffset); }
                }
                else
                {
                    if (!mask.Contains(upOffset) && (upOffset.y != 0 || upOffset.x < 0)) { mask.Add(upOffset); currentLayer.Add(upOffset); }
                    if (!mask.Contains(downOffset) && (downOffset.y != 0 || downOffset.x < 0)) { mask.Add(downOffset); currentLayer.Add(downOffset); }
                    if (!mask.Contains(leftOffset) && (leftOffset.y != 0 || leftOffset.x < 0)) { mask.Add(leftOffset); currentLayer.Add(leftOffset); }
                    if (!mask.Contains(rightOffset) && (rightOffset.y != 0 || rightOffset.x < 0)) { mask.Add(rightOffset); currentLayer.Add(rightOffset); }

                    if (!mask.Contains(upRightOffset) && (upRightOffset.y != 0 || upRightOffset.x < 0)) { mask.Add(upRightOffset); currentLayer.Add(upRightOffset); }
                    if (!mask.Contains(downRightOffset) && (downRightOffset.y != 0 || downRightOffset.x < 0)) { mask.Add(downRightOffset); currentLayer.Add(downRightOffset); }

                    if (!mask.Contains(upLeftOffset) && (upLeftOffset.y != 0 || upLeftOffset.x < 0)) { mask.Add(upLeftOffset); currentLayer.Add(upLeftOffset); }
                    if (!mask.Contains(downLeftOffset) && (downLeftOffset.y != 0 || downLeftOffset.x < 0)) { mask.Add(downLeftOffset); currentLayer.Add(downLeftOffset); }
                }
            }

            lastLayer.Clear();
            lastLayer.AddRange(currentLayer);
            currentLayer.Clear();
        }

        return mask.ToArray();
    }

    public static IEnumerator Generate()
    {
        int x = Random.Range(0, GridGlobals.Width);
        int y = Random.Range(0, GridGlobals.Height);
        int randomStartIndex = y * GridGlobals.Width + x;

        yield return CreateStreets(randomStartIndex);

        Debug.Log($"I shaped: {m_TotalCellCount}");
        Debug.Log($"I shaped: {m_IShapedStreetsCount}");
        Debug.Log($"L shaped: {m_LShapedStreetsCount}");
}

    private static IEnumerator CreateStreets(int _startIndex)
    {
        Stack<int> ToCheck = new Stack<int>();;

        Cell.PopulateCell(_startIndex, CellType.Intersection, 2, CellFeature.XShapedIntersection, CellOrientation.East);
        GridGlobals.StreetAdjacencyList.Add(_startIndex, new List<int>());

        ToCheck.Push(_startIndex);

        do
        {
            int lastCellIndex = ToCheck.Pop();

            CalculateNextPosition(lastCellIndex, out List<int> indexes, out List<CellOrientation> directionsFromLastCell);

            if (indexes[0] == m_HIT_END_OF_GRID)
            {
                continue;
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                int index = indexes[i];
                CellOrientation dirFromLastCell = directionsFromLastCell[i];

                if (!GridGlobals.StreetAdjacencyList.ContainsKey(index))
                {
                    GridGlobals.StreetAdjacencyList.Add(index, new List<int>());
                    ToCheck.Push(index);
                }

                GridGlobals.StreetAdjacencyList[index].Add(lastCellIndex);
                GridGlobals.StreetAdjacencyList[lastCellIndex].Add(index);

                PopulateNextStreetCell(index, lastCellIndex, dirFromLastCell);

                ++m_TotalCellCount;
                

                if (!GameManager.Instance.m_Skip)
                {
                    yield return new WaitUntil(() => GameManager.Instance.counter > m_Counter || GameManager.Instance.m_Continue);
                }
                else 
                {
                    GameManager.Instance.counter = m_Counter;
                }

                m_Counter++;
            }

        } while (ToCheck.Count != 0);
    }

    private static void CalculateNextPosition(int _index, out List<int> positions, out List<CellOrientation> directions)
    {
        directions = new List<CellOrientation>();
        positions = new List<int>();
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

        directions = new List<CellOrientation>(GridUtils.Shuffle<CellOrientation>(directions.ToArray()));

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

                    pos = newY * GridGlobals.Width + newX;
                    if (GridUtils.GetXPos(pos) < x)
                    {
                        isOutOfBounds = true;
                    }
                    break;
                case CellOrientation.West:
                    newX--;

                    pos = newY * GridGlobals.Width + newX;
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
                pos = newY * GridGlobals.Width + newX;
            }

            if (pos < 0 || pos >= GridGlobals.Width * GridGlobals.Height)
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
            else if (GridGlobals.StreetAdjacencyList.ContainsKey(pos))
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
            // As when creating the intersection, we put on cell in each possible way, there already will be one street. That is why we set the count to 1.
            m_StreetsWithoutIntersectionCount = 1;
        }
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

        bool choseType = false;

        // While we have valid options for streets and intersections, but we have not chosen one.
        while ((possibleIntersections != CellFeature.None || possibleStreets != CellFeature.None) && !foundPossible)
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

                newCellType = CellType.Intersection;
                choseType = true;
            }
            // If we have exceeded the minimum allowed street count without an intersection, we decide randomly if we will try to create one or not.
            else if (m_StreetsWithoutIntersectionCount >= m_MinStreetsWithoutIntersection)
            {
                
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

                            newCellType = CellType.Intersection;
                            choseType = true;
                            break;
                        }

                        newCellType = CellType.Street;
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

                            newCellType = CellType.Street;
                            choseType = true;
                            break;
                        }

                        newCellType = CellType.Intersection;
                        choseType = true;
                        break;
                    default:
                        Debug.LogError("Range unsupported!");
                        break;
                }
            }
            // Intersections are not possible because of one of the rules.
            else
            {
                // So we remove all intersections from the list with possible intersections.
                possibleIntersections = CellFeature.None;

                if (possibleStreets == CellFeature.None)
                {
                    break;
                }

                newCellType = CellType.Street;
                choseType = true;
            }

            if (!choseType)
            {
                // If we did not chose a type, we break the loop.
                break;
            }

            /* CALCULATING SHAPE */
            bool choseFeature = false;
            switch (newCellType)
            {
                case CellType.Street:
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

                        newCellFeatures = CellFeature.IShapedStreet;
                        choseFeature = true;
                        break;
                    }

                    tryFeatures(possibleStreets , CellFeature.IShapedStreet, CellFeature.LShapedStreet, 0.5f);
                    break;
                case CellType.Intersection:
                    tryFeatures(possibleIntersections, CellFeature.XShapedIntersection, CellFeature.TShapedIntersection, m_XIntersectionLikelihood);
                    break;
                default:
                    Debug.LogError("This cell type is unsupported.");
                    break;
            }

            // Helper function that chose feature based on random value and if the chosen one is not possible, it tries with the other.
            void tryFeatures(CellFeature possibleFeatures, CellFeature _first, CellFeature _second, float _firstChance)
            {
                float random = Random.value;

                if (random < _firstChance)
                {
                    // If the first feature is not possible, we try with the second.
                    if ((possibleFeatures & _first) == 0)
                    {
                        // If the second feature is not possible, we return.
                        if ((possibleFeatures & _second) == 0)
                        {
                            return;
                        }

                        newCellFeatures = _second;
                        choseFeature = true;
                        return;
                    }

                    newCellFeatures = _first;
                    choseFeature = true;
                }
                else
                {
                    // If the second feature is not possible, we try with the first.
                    if ((possibleFeatures & _second) == 0)
                    {
                        // If the first feature is not possible, we return.
                        if ((possibleFeatures & _first) == 0)
                        {
                            return;
                        }

                        newCellFeatures = _first;
                        choseFeature = true;
                        return;
                    }

                    newCellFeatures = _second;
                    choseFeature = true;
                }
            }

            if (!choseFeature)
            {
                // If we did not chose a feature, we continue the loop so we can try with another type.
                continue;
            }

            /* CALCULATING ORIANTATION */
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

                    switch (_dirFromLastCell)
                    {
                        case CellOrientation.East:
                            tryStreetDirections(CellOrientation.West, CellOrientation.North);
                            break;
                        case CellOrientation.West:
                            tryStreetDirections(CellOrientation.South, CellOrientation.East);
                            break;
                        case CellOrientation.North:
                            tryStreetDirections(CellOrientation.South, CellOrientation.West);
                            break;
                        case CellOrientation.South:
                            tryStreetDirections(CellOrientation.North, CellOrientation.East);
                            break;
                        default:
                            Debug.LogError("This cell orientation is unsupported.");
                            break;
                    }

                    // Helper function that chose direction based on random value and if the chosen one is not possible, it tries with the other.
                    void tryStreetDirections(CellOrientation _first, CellOrientation _second)
                    {
                        // Try first direction
                        if (Random.Range(0, 2) == 0)
                        {
                            // If the space around the first direction is free
                            if (checkForSpace(_first))
                            {
                                // if the last N turn were not oriented the same way as this one.
                                if (m_LastTurnOrientationCount.orientation != _first || 
                                    m_LastTurnOrientationCount.count <= m_AllowedConsecutiveTurnsInSameOrientation)
                                {
                                    newCellOrientation = _first;
                                    choseOrientation = true;
                                }
                            }
                            // If the space around the second direction is free
                            else if (checkForSpace(_second))
                            {
                                // if the last N turn were not oriented the same way as this one.
                                if (m_LastTurnOrientationCount.orientation != _second || 
                                    m_LastTurnOrientationCount.count <= m_AllowedConsecutiveTurnsInSameOrientation)
                                {
                                    newCellOrientation = _second;
                                    choseOrientation = true;
                                }
                            }
                        }
                        // Try east
                        else
                        {
                            // If the space around the second direction is free
                            if (checkForSpace(_second))
                            {
                                // if the last N turn were not oriented the same way as this one.
                                if (m_LastTurnOrientationCount.orientation != _second || 
                                    m_LastTurnOrientationCount.count <= m_AllowedConsecutiveTurnsInSameOrientation)
                                {
                                    newCellOrientation = _second;
                                    choseOrientation = true;
                                }
                            }
                            // If the space around the first direction is free
                            else if (checkForSpace(_first))
                            {
                                // if the last N turn were not oriented the same way as this one.
                                if (m_LastTurnOrientationCount.orientation != _first || 
                                    m_LastTurnOrientationCount.count <= m_AllowedConsecutiveTurnsInSameOrientation)
                                {
                                    newCellOrientation = _first;
                                    choseOrientation = true;
                                }
                            }
                        }

                        possibleStreets ^= CellFeature.LShapedStreet;
                    }
                    break;
                case CellFeature.TShapedIntersection:
                    CellOrientation[,] orders = { };

                    switch (_dirFromLastCell)
                    {
                        case CellOrientation.East:
                            orders = m_TEastDirectionMask;
                            break;
                        case CellOrientation.West:
                            orders = m_TWestDirectionMask;
                            break;
                        case CellOrientation.North:
                            orders = m_TNorthDirectionMask;
                            break;
                        case CellOrientation.South:
                            orders = m_TSouthDirectionMask;
                            break;
                        default:
                            Debug.LogError("This cell orientation is unsupported.");
                            break;
                    }

                    int randomRotation = Random.Range(0, 6);

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
                case CellFeature.XShapedIntersection:
                    

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
            // Setting this to one because when creating an intersection,
            // the first cell of each possible way is created and then it continues from the last created cell.
            // Because of this, When we hit dead end, we well continue from the last intersection with one cell already added.
            m_StreetsWithoutIntersectionCount = 1;

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
        else
        {
            switch (newCellType)
            {
                case CellType.Street:
                    ++m_StreetsWithoutIntersectionCount;
                    traversalCost = 2;
                    break;
                case CellType.Intersection:
                    // -1 is the default value. It shows that we don't have a 90 degrees turn yet.
                    m_LastTurnIndex = -1;
                    m_TurnsBetweenIntersectionCount = 0;
                    traversalCost = 5;

                    if (m_LastIntersectionTypeCount.featureType == newCellFeatures)
                    {
                        m_LastIntersectionTypeCount.count++;
                    }
                    else
                    {
                        m_LastIntersectionTypeCount.featureType = newCellFeatures;
                        m_LastIntersectionTypeCount.count = 1;
                    }

                    break;
                default:
                    break;
            }

            switch (newCellFeatures)
            {
                case CellFeature.IShapedStreet:
                    m_IShapedStreetsCount++;
                    break;
                case CellFeature.LShapedStreet:
                    m_LastTurnIndex = m_StreetsWithoutIntersectionCount;
                    m_TurnsBetweenIntersectionCount++;
                    m_LShapedStreetsCount++;

                    if (m_LastTurnOrientationCount.orientation == newCellOrientation)
                    {
                        m_LastTurnOrientationCount.count++;
                    }
                    else
                    {
                        m_LastTurnOrientationCount.orientation = newCellOrientation;
                        m_LastTurnOrientationCount.count = 1;
                    }
                    break;
                case CellFeature.TShapedIntersection:
                    // Setting this to minus one because when creating an T shaped intersection,
                    // the first cell of each possible way is created and then it continues from the last created cell.
                    // Because of this, between creating the intersection and the next intersection, there will be created one additional cell.
                    m_StreetsWithoutIntersectionCount = -1;
                    break;
                case CellFeature.XShapedIntersection:
                    // Setting this to minus two because when creating an X shaped intersection,
                    // the first cell of each possible way is created and then it continues from the last created cell.
                    // Because of this, between creating the intersection and the next intersection, there will be created two additional cells.
                    m_StreetsWithoutIntersectionCount = -2;
                    break;
                default:
                    break;
            }
        }

        Cell.PopulateCell(_currentCellIndex, newCellType, traversalCost, newCellFeatures, newCellOrientation);




        bool checkForSpace(CellOrientation direction)
        {
            (int x, int y)[] mask = new (int, int)[0];
            switch (newCellFeatures)
            {
                case CellFeature.IShapedStreet:
                    mask = RotateOffsets(m_IMaskOffsets, direction);
                    break;
                case CellFeature.LShapedStreet:
                    mask = RotateOffsets(m_LMaskOffsets, direction);
                    break;
                case CellFeature.TShapedIntersection:
                    mask = RotateOffsets(m_TMaskOffsets, direction);
                    break;
                case CellFeature.XShapedIntersection:
                    mask = RotateOffsets(m_XMaskOffsets, direction);
                    break;
                default:
                    break;
            }

            int x = GridUtils.GetXPos(_currentCellIndex);
            int y = GridUtils.GetYPos(_currentCellIndex);

            GridGlobals.PositionsToCheck = (_currentCellIndex, mask);

            for (int i = 0; i < mask.Length; i++)
            {
                (int x, int y) offset = mask[i];

                int index = ((y + offset.y) * GridGlobals.Width + (x + offset.x));

                if (GridGlobals.StreetAdjacencyList.ContainsKey(index))
                {
                    return false;
                }
            }

            return true;
        }
    }

    static (int x, int y)[] RotateOffsets((int x, int y)[] offsets, CellOrientation orientation)
    {
        // Rotate 90 degrees clockwise per orientation
        (int x, int y)[] rotated = new (int, int)[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
        {
            int x = offsets[i].x;
            int y = offsets[i].y;

            switch (orientation)
            {
                case CellOrientation.East:
                    rotated[i] = (x, y);
                    break;
                case CellOrientation.West:
                    rotated[i] = (-x, -y);
                    break;
                case CellOrientation.North:
                    rotated[i] = (-y, x);
                    break;
                case CellOrientation.South:
                    rotated[i] = (y, -x);
                    break;
                default:
                    break;
            }
        }
        return rotated;
    }
}
