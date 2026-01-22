using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Experimental.GraphView;

using UnityEngine;

public class RoadReconstructor
{
    public static IEnumerator Reconstruct()
    {
        Queue<int> tIntersectionIndexes = RoadGenGlobals.TIntersectionIndexes;
        Queue<int> xIntersectionIndexes = RoadGenGlobals.XIntersectionIndexes;
        Queue<int> turnIndexes = RoadGenGlobals.TurnIndexes;

        // Check if any intersection or 90 degrees turn have dead ends in a given depth and if so - fix them.
        yield return FindAndFixDeadEnd(turnIndexes, RoadGenGlobals.IStreetsAfterLStreetsBeforeDeadEnd, 2);
        yield return FindAndFixDeadEnd(tIntersectionIndexes, RoadGenGlobals.StreetsAfterTIntersectionBeforeDeadEnd, 3);
        yield return FindAndFixDeadEnd(xIntersectionIndexes, RoadGenGlobals.StreetsAfterXIntersectionBeforeDeadEnd, 4);
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

        // Calculates the orientation and the type of the cell based on each adjacent cell
        void UpdateCellOnIndex(int checkedIndex)
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
                // Get its direction in relation to this cell and add it to the flag with directions.
                neighborsDirections |= GetNeighborDirection(checkedIndex, neighborIndex);
            }

            // Calculate what exact shape we need base on the directions and count of the neighbors.
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
        }

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
    }
}
