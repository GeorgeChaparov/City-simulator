using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RoadGenerator
{
    private static int turnsBetweenIntersectionCount = 0;
    private static int streetsWithoutIntersectionCount = 0;
    private static int lastTurnIndex = -1;

    /// <summary>
    /// Counts how many intersections of the same type we have in a row.
    /// </summary>
    private static (CellFeature featureType, int count) lastIntersectionTypeCount = (CellFeature.None, 0);
    /// <summary>
    /// Counts how many turns with the same orientation we have in a row.
    /// </summary>
    private static (CellOrientation orientation, int count) lastTurnOrientationCount = (CellOrientation.None, 0);
    /// <summary>
    /// Holds the orientation of the last two turns. Used to prevent circling around of the streets and crashing into itself.
    /// </summary>
    private static (CellOrientation prevTwo, CellOrientation prevOne) lastTwoTurnsOrientation = (CellOrientation.None, CellOrientation.None);

    private static (int index, int x, int y, CellType type, CellFeature features, CellOrientation orientation) lastCellProps = new (0, 0, 0, CellType.Empty, CellFeature.None, CellOrientation.None);

    /*******************====---> Main code <---====*******************/
    #region Main Code
    public static IEnumerator Generate(int randomsStartIndex)
    {
        // Contains all of the populated cells that have not been checked for possible cells out of them.
        Stack<int> populatedToCheck = new Stack<int>(); ;

        // Adding the first intersection on a random position in the grid.
        Cell.PopulateCell(randomsStartIndex, CellType.Intersection, 2, CellFeature.XShapedIntersection, CellOrientation.East);
        GridGlobals.StreetAdjacencyList.Add(randomsStartIndex, new List<int>());

        populatedToCheck.Push(randomsStartIndex);

        // While we have not checked all of the populated cells.
        do
        {
            // Getting the last populated cell.
            int lastCellIndex = populatedToCheck.Pop();

            // Cashing its values
            lastCellProps.index = lastCellIndex;
            lastCellProps.x = GridUtils.GetXPos(lastCellIndex);
            lastCellProps.y = GridUtils.GetYPos(lastCellIndex);
            lastCellProps.type = Cell.GetType(lastCellIndex);
            lastCellProps.features = Cell.GetFeatures(lastCellIndex);
            lastCellProps.orientation = Cell.GetOrientation(lastCellIndex);

            // Calculating all valid directions out of the last populated cell.
            CalculateNextPositions(out List<int> indexes, out List<CellOrientation> directionsFromLastCell);

            // If, we have hit the end of the grid.
            if (indexes[0] == RoadGenGlobals.HIT_END_OF_GRID)
            {
                SetDeadEnd(lastCellIndex, Cell.GetOrientation(lastCellIndex));
                continue;
            }
            // If there are no possible directions that means that the current cell is Dead end street.
            else if (indexes[0] == RoadGenGlobals.NO_POSSIBLE_DIRECTIONS)
            {
                continue;
            }

            // For each of the possible new cells
            for (int i = 0; i < indexes.Count; i++)
            {
                int index = indexes[i];
                CellOrientation dirFromLastCell = directionsFromLastCell[i];

                // If that cell is not populated yet.
                if (!GridGlobals.StreetAdjacencyList.ContainsKey(index))
                {
                    GridGlobals.StreetAdjacencyList.Add(index, new List<int>());
                    populatedToCheck.Push(index);
                }

                // Connect the last cell to this one.
                GridGlobals.StreetAdjacencyList[index].Add(lastCellIndex);
                GridGlobals.StreetAdjacencyList[lastCellIndex].Add(index);

                // Populate the cell
                PopulateNextStreetCell(index, dirFromLastCell);
                ++RoadGenGlobals.TotalCellCount;

                // Used so you can see the streets build step by step or all at once.
                if (!GameManager.Instance.Skip)
                {
                    yield return new WaitUntil(() => GameManager.Instance.counter > RoadGenGlobals.StepCounter || GameManager.Instance.Continue);
                }
                else
                {
                    GameManager.Instance.counter = RoadGenGlobals.StepCounter;
                }

                RoadGenGlobals.StepCounter++;
            }

        } while (populatedToCheck.Count != 0);
    }

    private static void CalculateNextPositions(out List<int> positions, out List<CellOrientation> directions)
    {
        directions = new List<CellOrientation>();
        positions = new List<int>();

        // Get all of the possible direction for the given type of cell
        CellOrientation allowedDirections = CalculateAllowedDirections();
        bool havePossibleDirections = allowedDirections != CellOrientation.None;

        // If there are no possible directions
        if (!havePossibleDirections)
        {
            positions.Add(RoadGenGlobals.NO_POSSIBLE_DIRECTIONS);
            return;
        }

        // Adding each direction to a list so we can shuffle it, and remove elements easier.
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

        // Shuffle the possible directions so we don't start on the same direction every time.
        directions = new List<CellOrientation>(GridUtils.Shuffle<CellOrientation>(directions.ToArray()));


        // For each valid direction
        for (int i = 0; i < directions.Count; i++)
        {
            CellOrientation direction = directions[i];
            int newX = lastCellProps.x;
            int newY = lastCellProps.y;
            int pos = lastCellProps.index;

            bool isOutOfBounds = false;

            switch (direction)
            {
                case CellOrientation.East:
                    newX++;
                    // Calculate the position in the grid.
                    pos = newY * GridGlobals.Width + newX;

                    // If the X pos of the new index is smaller that means we are on the next row and thus on the other side of the grid. 
                    // I consider that invalid. 
                    if (GridUtils.GetXPos(pos) < lastCellProps.x)
                    {
                        isOutOfBounds = true;
                    }
                    break;
                case CellOrientation.West:
                    newX--;

                    // Calculate the position in the grid.
                    pos = newY * GridGlobals.Width + newX;

                    // If the X pos of the new index is bigger that means we are on the previous row and thus on the other side of the grid. 
                    // I consider that invalid. 
                    if (GridUtils.GetXPos(pos) > lastCellProps.x)
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

            // If we have not calculated the the new pos yet.
            if (pos == lastCellProps.index)
            {
                pos = newY * GridGlobals.Width + newX;
            }

            if (pos < 0 || pos >= GridGlobals.Width * GridGlobals.Height)
            {
                isOutOfBounds = true;
            }

            // If this direction is out of the grid.
            if (isOutOfBounds)
            {
                // Remove it.
                directions.RemoveAt(i);
                --i;
                continue;
            }
            // Or it's already taken.
            else if (GridGlobals.StreetAdjacencyList.ContainsKey(pos))
            {
                // Remove it.
                directions.RemoveAt(i);
                --i;
                continue;
            }

            positions.Add(pos);
        }

        if (directions.Count == 0)
        {
            positions.Add(RoadGenGlobals.HIT_END_OF_GRID);
        }
    }

    private static CellOrientation CalculateAllowedDirections()
    {
        CellOrientation directions = CellOrientation.None;

        switch (lastCellProps.type)
        {
            case CellType.Empty:
                break;
            case CellType.Building:
                break;
            case CellType.Sidewalk:
                break;
            case CellType.Street:
                if ((lastCellProps.features & CellFeature.IShapedStreet) != 0)
                {
                    if (lastCellProps.orientation == CellOrientation.East || lastCellProps.orientation == CellOrientation.West)
                    {
                        directions = CellOrientation.East | CellOrientation.West;
                    }
                    else
                    {
                        directions = CellOrientation.North | CellOrientation.South;
                    }
                }
                else if ((lastCellProps.features & CellFeature.LShapedStreet) != 0)
                {
                    switch (lastCellProps.orientation)
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
                if ((lastCellProps.features & CellFeature.TShapedIntersection) != 0)
                {
                    switch (lastCellProps.orientation)
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
                else if ((lastCellProps.features & CellFeature.XShapedIntersection) != 0)
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

    private static void PopulateNextStreetCell(int currentCellIndex, CellOrientation dirFromLastCell)
    {
        CellFeature possibleStreets = CellFeature.IShapedStreet | CellFeature.LShapedStreet;
        CellFeature possibleIntersections = CellFeature.TShapedIntersection | CellFeature.XShapedIntersection;
        bool foundPossibleProps = false;

        CellType newCellType = CellType.Empty;
        CellFeature newCellFeatures = CellFeature.None;
        CellOrientation newCellOrientation = CellOrientation.None;
        int traversalCost = -1;

        // While we have valid options for streets and intersections, but we have not chosen one.
        while ((possibleIntersections != CellFeature.None || possibleStreets != CellFeature.None) && !foundPossibleProps)
        {
            // Choose the type of the cell.
            CalculateType();

            // If we did not chose a type, we break the loop.
            if (newCellType == CellType.Empty)
            {
                break;
            }

            // Choose the shape of the cell.
            CalculateShape();

            // If we did not chose a feature, we continue the loop so we can try with another type.
            if (newCellFeatures == CellFeature.None)
            {
                newCellType = CellType.Empty;
                continue;
            }

            // Choose the Orientation of the cell.
            CalculateOrientation();

            // If we did not chose an orientation, we continue the loop so we can try with another feature.
            if (newCellOrientation == CellOrientation.None)
            {
                newCellType = CellType.Empty;
                newCellFeatures = CellFeature.None;
                continue;
            }

            foundPossibleProps = true;
        }

        #region Adding new cell

        // if we did not found possible properties, we add a dead end instead.
        if (!foundPossibleProps)
        {
            SetDeadEnd(currentCellIndex, dirFromLastCell);
            return;
        }

        switch (newCellType)
        {
            case CellType.Street:
                ++streetsWithoutIntersectionCount;
                traversalCost = 2;
                break;
            case CellType.Intersection:
                // -1 is the default value. It shows that we don't have a 90 degrees turn yet.
                lastTurnIndex = -1;
                turnsBetweenIntersectionCount = 0;
                traversalCost = 5;
                RoadGenGlobals.IntersectionIndexes.Add(currentCellIndex);

                if (RoadGenGlobals.PreventLoopAroundTurns)
                {
                    lastTwoTurnsOrientation = (CellOrientation.None, CellOrientation.None);
                }

                if (lastIntersectionTypeCount.featureType == newCellFeatures)
                {
                    lastIntersectionTypeCount.count++;
                }
                else
                {
                    lastIntersectionTypeCount.featureType = newCellFeatures;
                    lastIntersectionTypeCount.count = 1;
                }

                break;
            default:
                break;
        }

        switch (newCellFeatures)
        {
            case CellFeature.IShapedStreet:
                RoadGenGlobals.IShapedStreetsCount++;
                break;
            case CellFeature.LShapedStreet:
                lastTurnIndex = streetsWithoutIntersectionCount;
                turnsBetweenIntersectionCount++;
                RoadGenGlobals.LShapedStreetsCount++;
                RoadGenGlobals.TurnIndexes.Add(currentCellIndex);

                if (RoadGenGlobals.PreventLoopAroundTurns)
                {
                    lastTwoTurnsOrientation.prevTwo = lastTwoTurnsOrientation.prevOne;
                    lastTwoTurnsOrientation.prevOne = newCellOrientation;
                }

                if (lastTurnOrientationCount.orientation == newCellOrientation)
                {
                    lastTurnOrientationCount.count++;
                }
                else
                {
                    lastTurnOrientationCount.orientation = newCellOrientation;
                    lastTurnOrientationCount.count = 1;
                }
                break;
            case CellFeature.TShapedIntersection:
                // Setting this to minus one because when creating an T shaped intersection,
                // the first cell of each possible way is created and then it continues from the last created cell.
                // Because of this, between creating the intersection and the next intersection, there will be created one additional cell.
                streetsWithoutIntersectionCount = -1;
                break;
            case CellFeature.XShapedIntersection:
                // Setting this to minus two because when creating an X shaped intersection,
                // the first cell of each possible way is created and then it continues from the last created cell.
                // Because of this, between creating the intersection and the next intersection, there will be created two additional cells.
                streetsWithoutIntersectionCount = -2;
                break;
            default:
                break;
        }

        Cell.PopulateCell(currentCellIndex, newCellType, traversalCost, newCellFeatures, newCellOrientation);

        #endregion

        void CalculateType()
        {
            // If we have exceeded the maximum allowed street count without an intersection, we will try to create one.
            if (streetsWithoutIntersectionCount > RoadGenGlobals.MaxStreetsWithoutIntersection)
            {
                // If there are no more possible intersections, we return.
                if (possibleIntersections == CellFeature.None)
                {
                    return;
                }

                newCellType = CellType.Intersection;
            }
            // If we have exceeded the minimum allowed street count without an intersection, we decide randomly if we will try to create one or not.
            else if (streetsWithoutIntersectionCount >= RoadGenGlobals.MinStreetsWithoutIntersection)
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
                            break;
                        }

                        newCellType = CellType.Street;
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
                            break;
                        }

                        newCellType = CellType.Intersection;
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
                    return;
                }

                newCellType = CellType.Street;
            }
        }
        void CalculateShape()
        {
            switch (newCellType)
            {
                case CellType.Street:
                    newCellFeatures = CellFeature.None;

                    // If we have enough turns. 
                    // Or we are too close to the last intersection to make a turn.
                    // Or we already have at least one turn, but we are too close to it.
                    // We try to make a street.
                    if ((turnsBetweenIntersectionCount >= RoadGenGlobals.MaxTurnsBetweenIntersection) ||
                        (streetsWithoutIntersectionCount <= RoadGenGlobals.MinStreetsBeforeFirstTurn) ||
                        (lastTurnIndex != -1 && streetsWithoutIntersectionCount - lastTurnIndex <= RoadGenGlobals.MinStreetsBetweenTurns))
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
                        break;
                    }

                    tryFeatures(possibleStreets, CellFeature.IShapedStreet, CellFeature.LShapedStreet, RoadGenGlobals.IStreetLikelihood);
                    break;
                case CellType.Intersection:
                    tryFeatures(possibleIntersections, CellFeature.XShapedIntersection, CellFeature.TShapedIntersection, RoadGenGlobals.XIntersectionLikelihood);
                    break;
                default:
                    Debug.LogError("This cell type is unsupported.");
                    break;
            }

            // Helper function that chose feature based on random value and if the chosen one is not possible, it tries with the other.
            void tryFeatures(CellFeature possibleFeatures, CellFeature first, CellFeature second, float firstChance)
            {
                if (Random.value <= firstChance)
                {
                    // If the first feature is possible.
                    if ((possibleFeatures & first) != 0)
                    {
                        newCellFeatures = first;
                    }
                    // If the first feature is not possible, but the second feature is possible.
                    else if ((possibleFeatures & second) != 0)
                    {
                        newCellFeatures = second;
                    }
                }
                else
                {
                    // If the second feature is possible.
                    if ((possibleFeatures & second) != 0)
                    {
                        newCellFeatures = second;
                    }
                    // If the second feature is not possible, but the first feature is possible.
                    else if ((possibleFeatures & first) != 0)
                    {
                        newCellFeatures = first;
                    }
                }
            }
        }
        void CalculateOrientation()
        {
            switch (newCellFeatures)
            {
                case CellFeature.IShapedStreet:

                    // Check if we can place it there with that orientation.
                    if (!checkForSpace(dirFromLastCell))
                    {
                        possibleStreets ^= CellFeature.IShapedStreet;
                        break;
                    }

                    newCellOrientation = dirFromLastCell;
                    break;
                case CellFeature.LShapedStreet:

                    switch (dirFromLastCell)
                    {
                        case CellOrientation.East:
                            // Check if we can place it there with ether of the two orientations.
                            tryLStreetDirections(CellOrientation.West, CellOrientation.North);
                            break;
                        case CellOrientation.West:
                            // Check if we can place it there with ether of the two orientations.
                            tryLStreetDirections(CellOrientation.South, CellOrientation.East);
                            break;
                        case CellOrientation.North:
                            // Check if we can place it there with ether of the two orientations.
                            tryLStreetDirections(CellOrientation.South, CellOrientation.West);
                            break;
                        case CellOrientation.South:
                            // Check if we can place it there with ether of the two orientations.
                            tryLStreetDirections(CellOrientation.North, CellOrientation.East);
                            break;
                        default:
                            Debug.LogError("This cell orientation is unsupported.");
                            break;
                    }
                    break;
                case CellFeature.TShapedIntersection:
                    CellOrientation[] orders = { };

                    // Get the possible directions form the cashe
                    switch (dirFromLastCell)
                    {
                        case CellOrientation.East:
                            orders = RoadGenCache.TDirectionMask[0];
                            break;
                        case CellOrientation.West:
                            orders = RoadGenCache.TDirectionMask[1];
                            break;
                        case CellOrientation.North:
                            orders = RoadGenCache.TDirectionMask[2];
                            break;
                        case CellOrientation.South:
                            orders = RoadGenCache.TDirectionMask[3];
                            break;
                        default:
                            break;
                    }

                    // For each possible orientation.
                    for (int i = orders.Length - 1; i >= 0; i--)
                    {
                        int randomRotation = 0;

                        // If we have more then one left option.
                        if (i != 0)
                        {
                            // Get a random number between 0 and the number of options.
                            randomRotation = Random.Range(0, i);
                        }

                        // Get the orientation.
                        var orientation = orders[randomRotation];

                        // Check if we can place it there with that orientation.
                        if (checkForSpace(orientation))
                        {
                            newCellOrientation = orientation;

                            break;
                        }

                        // If we have more then one left option.
                        if (i != 0)
                        {
                            // Change the places of the chosen one with the last possible.
                            // That way we will not encounter it again, because "i" will decrement.
                            (orders[randomRotation], orders[i]) = (orders[i], orders[randomRotation]);
                        }
                    }

                    if (newCellOrientation == CellOrientation.None)
                    {
                        possibleIntersections ^= CellFeature.TShapedIntersection;
                    }
                    break;
                case CellFeature.XShapedIntersection:


                    if (!checkForSpace(dirFromLastCell))
                    {
                        possibleIntersections ^= CellFeature.XShapedIntersection;
                        break;
                    }

                    newCellOrientation = dirFromLastCell;
                    break;
                default:
                    Debug.LogError("This cell feature is unsupported.");
                    break;
            }

            // Helper function that chose direction based on random value and if the chosen one is not possible, it tries with the other.
            void tryLStreetDirections(CellOrientation first, CellOrientation second)
            {
                // Try first direction.
                if (Random.Range(0, 2) == 0)
                {
                    // If the space around the first direction is free.
                    if (checkForSpace(first))
                    {
                        checkLOrientationRulesFor(first, second);
                    }
                    // If the space around the second direction is free
                    else if (checkForSpace(second))
                    {
                        checkLOrientationRulesFor(second, first);
                    }
                }
                // Try east
                else
                {
                    // If the space around the second direction is free
                    if (checkForSpace(second))
                    {
                        checkLOrientationRulesFor(second, first);
                    }
                    // If the space around the first direction is free
                    else if (checkForSpace(first))
                    {
                        checkLOrientationRulesFor(first, second);
                    }
                }

                possibleStreets ^= CellFeature.LShapedStreet;
            }

            void checkLOrientationRulesFor(CellOrientation first, CellOrientation second)
            {
                // if the last N turn were not oriented the same way as this one.
                if (lastTurnOrientationCount.orientation == first &&
                    lastTurnOrientationCount.count > RoadGenGlobals.AllowedConsecutiveTurnsInSameOrientation)
                {
                    return;
                }

                // If we don't want to try and prevent the streets from looping around and crash into each other.
                if (!RoadGenGlobals.PreventLoopAroundTurns)
                {
                    newCellOrientation = first;
                    return;
                }

                // Try all combinations of denied consecutive orientations (series of orientations that will likely make the streets crash into each other).
                for (int i = 0; i < RoadGenCache.LDeniedConsecutiveOrientations.Length; i++)
                {
                    // If we have a match.
                    if (lastTwoTurnsOrientation.prevTwo == RoadGenCache.LDeniedConsecutiveOrientations[i][0] &&
                        lastTwoTurnsOrientation.prevOne == RoadGenCache.LDeniedConsecutiveOrientations[i][1] &&
                        first == RoadGenCache.LDeniedConsecutiveOrientations[i][2])
                    {
                        // And the second option is possible
                        if (checkForSpace(second))
                        {
                            newCellOrientation = second;
                        }

                        return;
                    }
                }

                newCellOrientation = first;
            }

            bool checkForSpace(CellOrientation direction)
            {
                (int x, int y)[] mask = GetRotatedMask(direction);

                int x = GridUtils.GetXPos(currentCellIndex);
                int y = GridUtils.GetYPos(currentCellIndex);

                // Used for visualizing the check bounds.
                GridGlobals.CheckBounds = (currentCellIndex, mask);

                // For each position, check if there is a street.
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

            (int x, int y)[] GetRotatedMask(CellOrientation direction)
            {
                (int x, int y)[] mask = new (int, int)[0];

                switch (newCellFeatures)
                {
                    case CellFeature.IShapedStreet:
                        mask = RotateOffsets(RoadGenGlobals.IMaskOffsets, direction);
                        break;
                    case CellFeature.LShapedStreet:
                        if (AreDirectionsOpposite(direction, dirFromLastCell))
                        {
                            mask = RotateOffsets(RoadGenGlobals.LForwardMaskOffsets, direction);
                        }
                        else
                        {
                            mask = RotateOffsets(RoadGenGlobals.LBackwardMaskOffsets, direction);
                        }
                        break;
                    case CellFeature.TShapedIntersection:
                        if (AreDirectionsOpposite(direction, dirFromLastCell))
                        {
                            mask = RotateOffsets(RoadGenGlobals.TForwardMaskOffsets, direction);
                        }
                        else
                        {
                            if (dirFromLastCell == CellOrientation.West && direction == CellOrientation.South)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TUpwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.West && direction == CellOrientation.North)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TDownwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.East && direction == CellOrientation.South)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TDownwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.East && direction == CellOrientation.North)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TUpwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.South && direction == CellOrientation.East)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TDownwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.South && direction == CellOrientation.West)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TUpwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.North && direction == CellOrientation.East)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TDownwardMaskOffsets, direction);
                            }
                            else if (dirFromLastCell == CellOrientation.North && direction == CellOrientation.West)
                            {
                                mask = RotateOffsets(RoadGenGlobals.TUpwardMaskOffsets, direction);
                            }
                        }
                        break;
                    case CellFeature.XShapedIntersection:
                        mask = RotateOffsets(RoadGenGlobals.XMaskOffsets, direction);
                        break;
                    default:
                        break;
                }

                return mask;
            }
        }
    }
    #endregion

    static bool AreDirectionsOpposite(CellOrientation first, CellOrientation second)
    {
        if ((first == CellOrientation.East && second == CellOrientation.West) || (second == CellOrientation.East && first == CellOrientation.West))
        {
            return true;
        }

        if ((first == CellOrientation.North && second == CellOrientation.South) || (second == CellOrientation.North && first == CellOrientation.South))
        {
            return true;
        }

        return false;
    }

    private static void SetDeadEnd(int index, CellOrientation dirFromLastCell)
    {
        // Setting this to one because when creating an intersection,
        // the first cell of each possible way is created and then it continues from the last created cell.
        // Because of this, When we hit dead end, we well continue from the last intersection with one cell already added.
        streetsWithoutIntersectionCount = 1;
        turnsBetweenIntersectionCount = 0;
        lastTurnIndex = -1;

        if (RoadGenGlobals.PreventLoopAroundTurns)
        {
            lastTwoTurnsOrientation = (CellOrientation.None, CellOrientation.None);
        }

        CellOrientation orientation = CellOrientation.None;

        switch (dirFromLastCell)
        {
            case CellOrientation.East:
                orientation = CellOrientation.West;
                break;
            case CellOrientation.West:
                orientation = CellOrientation.East;
                break;
            case CellOrientation.North:
                orientation = CellOrientation.South;
                break;
            case CellOrientation.South:
                orientation = CellOrientation.North;
                break;
            default:
                break;
        }

        Cell.PopulateCell(index, CellType.Street, 2, CellFeature.DeadEnd, orientation);
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
