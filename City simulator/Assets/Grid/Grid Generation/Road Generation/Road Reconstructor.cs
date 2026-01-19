using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RoadReconstructor
{
    public static IEnumerator Reconstruct()
    {
        Queue<int> intersectionIndexes = RoadGenGlobals.IntersectionIndexes;
        Queue<int> turnIndexes = RoadGenGlobals.TurnIndexes;

        // Check if any intersection or 90 degrees turn have 
        FindAndFixDeadEnd(turnIndexes, RoadGenGlobals.IStreetsAfterLStreetsBeforeDeadEnd, 2);
        FindAndFixDeadEnd(intersectionIndexes, RoadGenGlobals.StreetsAfterTIntersectionBeforeDeadEnd, 3);
        FindAndFixDeadEnd(intersectionIndexes, RoadGenGlobals.StreetsAfterXIntersectionBeforeDeadEnd, 4);

        yield return new WaitForEndOfFrame();
    }

    private static void FindAndFixDeadEnd(Queue<int> indexesToCheck, int maxDepth, int maxDirections)
    {
        // While we have not checked all indexes in the queue. 
        while (indexesToCheck.Count > 0)
        {
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
                if (streetsIndex.Count <= 0)
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

        
        void UpdateCellOnIndex(int checkedIndex)
        {
            // Based on the index and type of each adjecent cell we can decide the type of the new cell and its orientation. If the index is bigger then the current index its ether to the right or below (if its bigger with more then one its below, else its to the right). Based on the number of neighbors we decide the type.
        }
    }
}
