using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RoadReconstructor
{
    public static IEnumerator Reconstruct()
    {
        // Check if we can connect any dead end to another dead end, intersection or a 90 degrees turn.
        // That is done by projecting forward from each dead end and checking if we hit one of the above.
        yield return ConnectDeadEnds();
        

        // Check if any intersection or 90 degrees turn have dead ends in a given depth and if so - fix them (remove them).
        yield return FindAndFixDeadEnd(new Queue<int>(RoadGenGlobals.TurnIndexes), RoadGenGlobals.IStreetsAfterLStreetsBeforeDeadEnd, 2);
        yield return FindAndFixDeadEnd(new Queue<int>(RoadGenGlobals.TIntersectionIndexes), RoadGenGlobals.StreetsAfterTIntersectionBeforeDeadEnd, 3);
        yield return FindAndFixDeadEnd(new Queue<int>(RoadGenGlobals.XIntersectionIndexes), RoadGenGlobals.StreetsAfterXIntersectionBeforeDeadEnd, 4);
    }

    private static IEnumerator ConnectDeadEnds()
    {
        List<int> deadEndIndexes = RoadGenGlobals.DeadEndIndexes;

        // Go trough each dead end and try to connect it to something.
        for (int i = deadEndIndexes.Count - 1; i >= 0; i--)
        {
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


            int deadEndIndex = deadEndIndexes[i];
            int deadEndX = GridUtils.GetXPos(deadEndIndex);
            int deadEndY = GridUtils.GetYPos(deadEndIndex);

            // Get the direction to project in.
            CellOrientation projDirection = GridUtils.GetOppositeDirectionOf(deadEndIndex);

            // Get the projection coordinates. The format is (x = 1, y = 0) for east or (x = 0, y = -1) for south and so one.
            (int x, int y) coordinatesProjDir = GridUtils.GetCoordinatesDirection(projDirection);

            // The list in which the found index resign.
            List<int> indexesList = new List<int>();

            // Contains all the indexes that we need to change in order to connect the two roads.
            Stack<int> indexesToChange = new Stack<int>();

            // If this is true, that means that we did not find anything to connect this dead end to.
            if (!FindConnectionPoint())
            {
                continue;
            }

            // Connect the dead end with the compatible cell.
            ConnectDeadEnd();


            // Projects froward from the current dead cell and checks if there is another cell in that range that it can connect to.
            bool FindConnectionPoint(int currentDepth = 1)
            {
                // If we hit the depth limit.
                if (currentDepth > RoadGenGlobals.CellsBetweenRoads + 10)
                {
                    return false;
                }

                // Get the next index based on the projected coordinates.
                int newX = deadEndX + (coordinatesProjDir.x * currentDepth);
                int newY = deadEndY + (coordinatesProjDir.y * currentDepth);
                int projIndex = GridUtils.GetIndex(newX, newY);

                // If we hit the end of the grid with the new index.
                if (GridUtils.IsProjOutOfGridBounds(projIndex, deadEndIndex, projDirection))
                {
                    return false;
                }

                // Get the features of the cell on this coordinates. 
                CellFeature projCellFeature = Cell.GetFeatures(projIndex);


                // Determine the list that the cell is a part of based on its feature.

                // If the cell is empty.
                if (projCellFeature == CellFeature.None)
                {
                    // Continue with the next cell in the projection path.
                    if (FindConnectionPoint(++currentDepth))
                    {
                        // If we found cell we can connect to,
                        // we add the current index to the list of indexes that need to be change in order to connect the two roads.
                        indexesToChange.Push(projIndex);

                        return true;
                    }

                    return false;
                }
                else if ((projCellFeature & CellFeature.LShapedStreet) != 0)
                {
                    indexesList = RoadGenGlobals.TurnIndexes;
                }
                else if ((projCellFeature & CellFeature.XShapedIntersection) != 0)
                {
                    indexesList = RoadGenGlobals.XIntersectionIndexes;
                }
                else if ((projCellFeature & CellFeature.TShapedIntersection) != 0)
                {
                    indexesList = RoadGenGlobals.TIntersectionIndexes;
                }
                else if ((projCellFeature & CellFeature.DeadEnd) != 0)
                {
                    indexesList = RoadGenGlobals.DeadEndIndexes;
                    // As we are currently iterating over this list and we are going to remove two of the elements, not only one, we need to decrement by two.
                    // As we are going to decrement once in the declaration of the loop itself, we need to decrement by one here.
                    i--;
                }
                else
                {
                    Debug.LogError($"Unsupported cell feature: {projCellFeature}");
                    return false;
                }

                // If we found cell that is compatible.
                if (indexesList.Count != 0)
                {
                    indexesToChange.Push(projIndex);
                    return true;
                }

                return false;
            }


            // Connects the current dead cell to the cell we found using by creating new cells on the positions in the list of indexes to change.
            void ConnectDeadEnd()
            {
                // Update the dead cell to be a I shaped street.
                Cell.UpdateCell(deadEndIndex, CellType.Street, RoadGenCache.StreetTraverseBaseCost, CellFeature.IShapedStreet, projDirection);
                deadEndIndexes.Remove(deadEndIndex);

                int lastIndex = deadEndIndex;

                // While we have not updated all of the indexes of the new road.
                while (indexesToChange.Count != 0)
                {
                    // Get the current index
                    int index = indexesToChange.Pop();

                    // If that cell is not populated yet.
                    if (!GridGlobals.StreetAdjacencyList.ContainsKey(index))
                    {
                        GridGlobals.StreetAdjacencyList.Add(index, new List<int>());
                    }

                    // Update the adjacency list of the current and the last index.
                    GridGlobals.StreetAdjacencyList[index].Add(lastIndex);
                    GridGlobals.StreetAdjacencyList[lastIndex].Add(index);

                    // Populate the cell
                    Cell.PopulateCell(index, CellType.Street, RoadGenCache.StreetTraverseBaseCost, CellFeature.IShapedStreet, projDirection);

                    // If this is the last index in the list.
                    if (indexesToChange.Count == 0)
                    {
                        // That means it is the cell we are trying to connect to.
                        // Update it so that it accounts for multiple neighbors.
                        UpdateCellOnIndex(index);

                        // Remove the index from the list with 
                        indexesList.Remove(index);
                    }

                    lastIndex = index;
                }
            }
        }
    }

    private static IEnumerator FindAndFixDeadEnd(Queue<int> indexesToCheck, int maxDepth, int maxDirections)
    {
        // While we have not checked all indexes in the queue. 
        while (indexesToCheck.Count > 0)
        {
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

            // Get the first index and its neighbors.
            int indexToCheck = indexesToCheck.Dequeue();
            List<int> neighborIndexes = GridGlobals.StreetAdjacencyList[indexToCheck];

            Queue<int>[] deadEndRoadsIndexes = new Queue<int>[maxDirections];

            // For each neighbor.
            for (int i = 0; i < neighborIndexes.Count; i++)
            {
                int neighborIndex = neighborIndexes[i];

                Queue<int> deadEndRoadIndexes = new Queue<int>(maxDepth);
                // Check if there is a dead end in given max depth.

                // If there is dead end.
                if (CheckNeighborsForDeadEnd(neighborIndex, indexToCheck, 1, maxDepth, ref deadEndRoadIndexes))
                {
                    // Add it to the list of dead ends.
                    deadEndRoadsIndexes[i] = deadEndRoadIndexes;
                }
            }

            // Removing dead end roads.


            // If we have removed any roads.
            if (RemoveDeadEnds(indexToCheck, deadEndRoadsIndexes))
            {
                // Changing the current cell so that it matches the roads that are left.
                UpdateCellOnIndex(indexToCheck);
            }
        }

        // It finds all dead end roads form given intersection or 90 degrees turn, and adds the index of each street that makes that road to deadEndRoadIndexes.
        bool CheckNeighborsForDeadEnd(int index, int lastIndex, int currentDepth, int maxDepth, ref Queue<int> deadEndRoadIndexes)
        {
            CellFeature feature = Cell.GetFeatures(index);
            CellType type = Cell.GetType(index);

            // If we hit max depth.
            if (currentDepth >= maxDepth)
            {
                // We did not found dead end.
                return false;
            }
            // If we reached an intersection.
            else if (type == CellType.Intersection)
            {
                // We did not found dead end.
                return false;
            }
            // If we reached an 90 degrees turn.
            else if (feature == CellFeature.LShapedStreet)
            {
                // We did not found dead end.
                return false;
            }

            if ((feature & CellFeature.DeadEnd) != 0)
            {
                // We found dead end.
                deadEndRoadIndexes.Enqueue(index);
                return true;
            }

            // If there is no dead end on this index, we check the neighbors of that index.

            List<int> neighborIndexes = GridGlobals.StreetAdjacencyList[index];

            // For each neighbor of that index.
            foreach (var neighborIndex in neighborIndexes)
            {
                // If the current neighbor is the cell that we are coming from.
                if (neighborIndex == lastIndex)
                {
                    continue;
                }

                // If we find a neighbor that is a dead end.
                if (CheckNeighborsForDeadEnd(neighborIndex, index, ++currentDepth, maxDepth, ref deadEndRoadIndexes))
                {
                    // Add it to the queue of that street.
                    deadEndRoadIndexes.Enqueue(index);
                    return true;
                }
            }

            // We should not hit this case, because that will mean that the last cell that we checked did not have any neighbors that are not the cell that we are coming form.
            // That is not possible as during the generation we do not leave streets uncapped (without dead end).
            return false;
        }

        // Goes trough each cell in each dead end road and resets it.
        bool RemoveDeadEnds(int checkedIndex, Queue< int>[] roadsToRemove)
        {
            bool RemovedStreet = false;

            // For each road that comes out of this cell and have a dead end.
            foreach (var streetsIndex in roadsToRemove)
            {
                // If there are no streets.
                if (streetsIndex == null)
                {
                    continue;
                }

                RemovedStreet = true;

                // For each street that make up the dead end road.
                for (int i = streetsIndex.Count; i >= 1 ; i--)
                {
                    int streetIndex = streetsIndex.Dequeue();

                    // Clear that cell and reset the adjacencyList on the index.
                    GridGlobals.StreetAdjacencyList[streetIndex].Clear();
                    Cell.ClearCell(streetIndex);

                    // If this is the last cell that means that this is adjacent to the intersection/turn that we are checking
                    // and we want to remove it from its adjacency list.
                    if (i == 1)
                    {
                        GridGlobals.StreetAdjacencyList[checkedIndex].Remove(streetIndex);
                    }
                }
            }

            return RemovedStreet;
        }
    }

    // Calculates the orientation and the type of the cell based on each adjacent cell
    private static void UpdateCellOnIndex(int checkedIndex)
    {
        CellType newCellType = CellType.Empty;
        CellOrientation newCellOrientation = CellOrientation.None;
        CellFeature newCellFeature = CellFeature.None;
        int newTraversBaseCost = 0;

        CellOrientation neighborsDirections = CellOrientation.None;
        // Get all neighbors that are left.
        List<int> neighborIndexes = GridGlobals.StreetAdjacencyList[checkedIndex];

        int neighborCount = neighborIndexes.Count;

        // If we have more the two neighbors, we need an intersection. Otherwise, we need a street.
        // Setting the base cost as well as its based only on the type of cell.
        if (neighborCount > 2)
        {
            newCellType = CellType.Intersection;
            newTraversBaseCost = RoadGenCache.IntersectionTraverseBaseCost;
        }
        else
        {
            newCellType = CellType.Street;
            newTraversBaseCost = RoadGenCache.StreetTraverseBaseCost;
        }

        // For each neighbor
        foreach (var neighborIndex in neighborIndexes)
        {
            // Get there direction in relation to this cell and add it to the flag with directions.
            neighborsDirections |= GetNeighborDirection(checkedIndex, neighborIndex);
        }

        // Calculate what exact shape we need based on the directions and count of the neighbors.
        newCellFeature = CalculateCellFeature(newCellType, neighborsDirections, neighborCount);

        // Calculate the orientation of the new cell based on the direction of its neighbors in relation to itself.
        int firstNeighborIndex = neighborIndexes[0];
        newCellOrientation = CalculateCellOrientation(newCellFeature, neighborsDirections, firstNeighborIndex);

        if (newCellType == CellType.Empty)
        {
            Debug.LogError("Invalid Type of new cell during reconstruction");
            return;
        }

        if (newCellOrientation == CellOrientation.None)
        {
            Debug.LogError("Invalid Orientation of new cell during reconstruction");
            return;
        }

        if (newCellFeature == CellFeature.None)
        {
            Debug.LogError("Invalid Feature of new cell during reconstruction");
            return;
        }

        // Updating the cell.
        Cell.UpdateCell(checkedIndex, newCellType, newTraversBaseCost, newCellFeature, newCellOrientation);

        #region Helper functions

        // Calculates the direction of a cell in relation to the current cell.
        CellOrientation GetNeighborDirection(int currentCellIndex, int neighborCellIndex)
        {
            // If there index is bigger then the index of the cell that we are checking
            if (neighborCellIndex > currentCellIndex)
            {
                // And its bigger exactly by one.
                if (neighborCellIndex == currentCellIndex + 1)
                {
                    // its on the east side of the current one.
                    return CellOrientation.East;
                }
                // And its bigger with more then one.
                else
                {
                    // its on north side of the current one.
                    return CellOrientation.North;
                }
            }
            else
            {
                // And its smaller exactly by one.
                if (neighborCellIndex == currentCellIndex - 1)
                {
                    // its on the east side of the current one.
                    return CellOrientation.West;
                }
                // And its smaller with more then one.
                else
                {
                    // its on south side of the current one.
                    return CellOrientation.South;
                }
            }
        }

        // Helper function that calculates what exact shape we need base on the directions or count of the neighbors
        CellFeature CalculateCellFeature(CellType cellType, CellOrientation neighborsDirections, int neighborCount)
        {
            if (neighborCount == 1)
            {
                return CellFeature.DeadEnd;
            }

            switch (cellType)
            {
                case CellType.Street:
                    if (GridUtils.AreDirectionsOpposite(neighborsDirections))
                    {
                        return CellFeature.IShapedStreet;
                    }
                    else
                    {
                        return CellFeature.LShapedStreet;
                    }
                    break;
                case CellType.Intersection:
                    if (neighborCount == 3)
                    {
                        return CellFeature.TShapedIntersection;
                    }
                    else
                    {
                        return CellFeature.XShapedIntersection;
                    }
                    break;
                default:
                    Debug.LogError("Invalid type of new cell during reconstruction.");
                    break;
            }

            return CellFeature.None;
        }

        // Helper function that calculates the orientation of the new cell based on the direction of its neighbors in relation to itself.
        CellOrientation CalculateCellOrientation(CellFeature feature, CellOrientation neighborsDirections, int firstNeighborIndex)
        {
            switch (feature)
            {
                case CellFeature.DeadEnd:
                    if (neighborsDirections == CellOrientation.None)
                    {
                        Debug.LogError("Invalid NEIGHBOR Orientation of new cell during reconstruction");
                    }

                    return neighborsDirections;
                    break;

                case CellFeature.IShapedStreet:
                    return Cell.GetOrientation(firstNeighborIndex);
                    break;
                case CellFeature.LShapedStreet:
                    // Get the possible directions out of the current cell in a given orientation.
                    return TryEachOrientation(RoadGenCache.LDirectionMask);
                    break;
                case CellFeature.TShapedIntersection:
                    // Get the possible directions out of the current cell in a given orientation.
                    return TryEachOrientation(RoadGenCache.TDirectionMask);
                    break;
                case CellFeature.XShapedIntersection:
                    return Cell.GetOrientation(firstNeighborIndex);
                    break;
                default:
                    Debug.LogError("Invalid feature of new cell during reconstruction.");
                    break;
            }

            return CellOrientation.None;


            CellOrientation TryEachOrientation(Dictionary<CellOrientation, CellOrientation[]> DirectionMask)
            {
                foreach (var orientations in DirectionMask)
                {
                    CellOrientation[] directions = orientations.Value;
                    int j = 0;

                    // Check if all of the directions are present.
                    for (; j < directions.Length; j++)
                    {
                        CellOrientation direction = directions[j];

                        // If any of the directions is not present.
                        if ((neighborsDirections & direction) == 0)
                        {
                            break;
                        }
                    }

                    // If we manage to check each direction in the current orientation without breaking
                    // That means this orientation is valid for our case.
                    if (j == directions.Length)
                    {
                        // Return it.
                        return orientations.Key;
                    }
                }

                // We should never hit this.
                return CellOrientation.None;
            }
        }

        #endregion
    }
}
