using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridGenerator
{
    public static void Init(int minStreetsWithoutIntersection, int maxStreetsWithoutIntersection, int maxTurnsBetweenIntersection,
        int minStreetsBetweenTurns, int minStreetsAfterIntersectionBeforeTurn, int emptyCellsBetweenStreets, int allowedConsecutiveTurnsInSameOrientation,
        float xIntersectionLikelihood, bool preventLoopAroundTurns, float iStreetLikelihood, int streetsAfterXIntersectionBeforeDeadEnd,
        int streetsAfterTIntersectionBeforeDeadEnd, int streetsAfterLStreetsBeforeDeadEnd, int connectionRange, bool connectToIShapedStreets,
        int iShapedStreetConnectionRange, int iDistanceFromTurnOrIntersection) 
    {

        int seed = GameManager.Instance.seed;
        if (seed != -1)
        {
            RoadGenGlobals.RandomInstance = new System.Random(seed);
        }

        //                  Generation
        //
        // Intersection related.
        RoadGenGlobals.MinStreetsWithoutIntersection = minStreetsWithoutIntersection;
        RoadGenGlobals.MaxStreetsWithoutIntersection = maxStreetsWithoutIntersection;
        RoadGenGlobals.MaxTurnsBetweenIntersection = maxTurnsBetweenIntersection;
        RoadGenGlobals.MinStreetsBetweenTurns = minStreetsBetweenTurns;
        RoadGenGlobals.MinStreetsBeforeFirstTurn = minStreetsAfterIntersectionBeforeTurn;
        RoadGenGlobals.TIntersectionIndexes = new List<int>();
        RoadGenGlobals.XIntersectionIndexes = new List<int>();

        // Street related.
        RoadGenGlobals.AllowedConsecutiveTurnsInSameOrientation = allowedConsecutiveTurnsInSameOrientation;
        RoadGenGlobals.XIntersectionLikelihood = xIntersectionLikelihood;
        RoadGenGlobals.PreventLoopAroundTurns = preventLoopAroundTurns;
        RoadGenGlobals.IStreetLikelihood = iStreetLikelihood;
        RoadGenGlobals.LShapedStreetIndexes = new List<int>();
        RoadGenGlobals.DeadEndIndexes = new List<int>();

        //                  Reconstruction
        //
        RoadGenGlobals.StreetsAfterXIntersectionBeforeDeadEnd = streetsAfterXIntersectionBeforeDeadEnd;
        RoadGenGlobals.StreetsAfterTIntersectionBeforeDeadEnd = streetsAfterTIntersectionBeforeDeadEnd;
        RoadGenGlobals.StreetsAfterLStreetsBeforeDeadEnd = streetsAfterLStreetsBeforeDeadEnd;
        RoadGenGlobals.ConnectionRange = connectionRange;
        RoadGenGlobals.ConnectToIShapedStreets = connectToIShapedStreets;
        RoadGenGlobals.IShapedStreetConnectionRange = iShapedStreetConnectionRange;
        RoadGenGlobals.IDistanceFromTurnOrIntersection = iDistanceFromTurnOrIntersection;

        //                  Counters
        RoadGenGlobals.TotalCellCount = 1;
        RoadGenGlobals.StepCounter = 0;

        if (RoadGenGlobals.CellsBetweenRoads != emptyCellsBetweenStreets)
        {
            RoadGenGlobals.CellsBetweenRoads = emptyCellsBetweenStreets;

            if (RoadGenGlobals.CellsBetweenRoads != 1)
            {
                bool horizontalRightCheck((int x, int y) offset) { return offset.y != 0 || offset.x > 0; };
                bool horizontalLeftCheck((int x, int y) offset) { return offset.y != 0 || offset.x < 0; };
                bool verticalUpCheck((int x, int y) offset) { return offset.x != 0 || offset.y < 0; };
                bool verticalDownCheck((int x, int y) offset) { return offset.x != 0 || offset.y > 0; };

                RoadGenGlobals.IReconstructionMaskOffsets = GenerateMaskOffset(RoadGenCache.IBaseReconstructionMaskOffsets, horizontalRightCheck);
                RoadGenGlobals.IMaskOffsets = GenerateMaskOffset(RoadGenCache.IBaseMaskOffsets, horizontalRightCheck);
                RoadGenGlobals.LForwardMaskOffsets = GenerateMaskOffset(RoadGenCache.LBaseForwardMaskOffsets, horizontalLeftCheck);
                RoadGenGlobals.TForwardMaskOffsets = GenerateMaskOffset(RoadGenCache.TBaseForwardMaskOffsets, horizontalLeftCheck);
                RoadGenGlobals.XMaskOffsets = GenerateMaskOffset(RoadGenCache.XBaseMaskOffsets, horizontalRightCheck);

                RoadGenGlobals.LBackwardMaskOffsets = GenerateMaskOffset(RoadGenCache.LBaseBackwardMaskOffsets, verticalUpCheck, false);
                RoadGenGlobals.TUpwardMaskOffsets = GenerateMaskOffset(RoadGenCache.TBaseUpwardMaskOffsets, verticalUpCheck, false);
                RoadGenGlobals.TDownwardMaskOffsets = GenerateMaskOffset(RoadGenCache.TBaseDownwardMaskOffsets, verticalDownCheck, false);
            }
            else
            {
                RoadGenGlobals.IMaskOffsets = RoadGenCache.IBaseMaskOffsets;
                RoadGenGlobals.LForwardMaskOffsets = RoadGenCache.LBaseForwardMaskOffsets;
                RoadGenGlobals.TForwardMaskOffsets = RoadGenCache.TBaseForwardMaskOffsets;
                RoadGenGlobals.XMaskOffsets = RoadGenCache.XBaseMaskOffsets;

                RoadGenGlobals.LBackwardMaskOffsets = RoadGenCache.LBaseBackwardMaskOffsets;
                RoadGenGlobals.TUpwardMaskOffsets = RoadGenCache.TBaseUpwardMaskOffsets;
                RoadGenGlobals.TDownwardMaskOffsets = RoadGenCache.TBaseDownwardMaskOffsets;
            }
        }
    }

    public static IEnumerator Generate()
    {
        int x = RoadGenGlobals.RandomInstance.Next(0, GridGlobals.Width);
        int y = RoadGenGlobals.RandomInstance.Next(0, GridGlobals.Height);
        int randomStartIndex = GridUtils.GetIndex(x, y);

        // Generate the Road
        yield return RoadGenerator.Generate(randomStartIndex);

        GridVisualizer.Instance.StopVisualizingCheckPos();
        GameManager.Instance.ResetState();
        RoadGenGlobals.StepCounter = 0;

        // Fix any mistakes made during generation (make the road look prettier).
        yield return RoadReconstructor.Reconstruct();

        Debug.Log($"All: {RoadGenGlobals.TotalCellCount}");
        Debug.Log($"I shaped: {RoadGenGlobals.IShapedStreetIndexes.Count}");
        Debug.Log($"L shaped: {RoadGenGlobals.LShapedStreetIndexes.Count}");
    }

    private static (int x, int y)[] GenerateMaskOffset((int x, int y)[] offsets, Func<(int x, int y), bool> addCondition, bool horizontal = true)
    {
        // Stores all calculated offsets.
        List<(int x, int y)> mask = new List<(int x, int y)>(offsets);

        // Stores the last calculated set of offsets.
        List<(int x, int y)> lastLayer = new List<(int x, int y)>(offsets);

        // The index at which the last layer of offsets started.
        int lastLayerStartIndex = 0;

        // For the amount of empty cells that we have to have between roads.
        for (int i = 0; i < RoadGenGlobals.CellsBetweenRoads; i++)
        {
            lastLayerStartIndex = mask.Count;

            // For each offset in the last layer.
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

                // If the base mask is based on the horizontal plane.
                if (horizontal)
                {
                    // If the last offset was to the right
                    if (x > 0)
                    {
                        rightOffset = (x + 1, y);

                        // If the last offset was higher then the center
                        if (y > 0)
                        {
                            upOffset = (x, y + 1);
                            upRightOffset = (x + 1, y + 1);
                        }
                        // If the last offset was lower then the center
                        else if (y < 0)
                        {
                            downOffset = (x, y - 1);
                            downRightOffset = (x + 1, y - 1);
                        }
                    }
                    // If the last offset was to the left
                    else if (x < 0)
                    {
                        leftOffset = (x - 1, y);

                        // If the last offset was higher then the center
                        if (y > 0)
                        {
                            upOffset = (x, y + 1);
                            upLeftOffset = (x - 1, y + 1);
                        }
                        // If the last offset was lower then the center
                        else if (y < 0)
                        {
                            downOffset = (x, y - 1);
                            downLeftOffset = (x - 1, y - 1);
                        }
                    }
                    // If the last offset was centered
                    else
                    {
                        // If the last offset was higher then the center
                        if (y > 0)
                        {
                            upOffset = (x, y + 1);
                        }
                        // If the last offset was lower then the center
                        else if (y < 0)
                        {
                            downOffset = (x, y - 1);
                        }
                    }
                }
                // If the base mask is based on the vertical plane.
                else
                {
                    // If the last offset was lower then the center
                    if (y > 0)
                    {
                        upOffset = (x, y + 1);

                        // If the last offset was to the right
                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                            upRightOffset = (x + 1, y + 1);
                        }
                        // If the last offset was to the left
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                            upLeftOffset = (x - 1, y + 1);
                        }
                    }
                    // If the last offset was higher then the center
                    else if (y < 0)
                    {
                        downOffset = (x, y - 1);

                        // If the last offset was to the right
                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                            downRightOffset = (x + 1, y - 1);
                        }
                        // If the last offset was to the left
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                            downLeftOffset = (x - 1, y - 1);
                        }
                    }
                    // If the last offset was centered
                    else
                    {
                        // If the last offset was to the right
                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                        }
                        // If the last offset was to the left
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                        }
                    }
                }

                // If any of the new offset are not already added to the mask, and they pass the adding condition - add them to the mask.
                if (!mask.Contains(upOffset) && addCondition(upOffset)) mask.Add(upOffset);
                if (!mask.Contains(downOffset) && addCondition(downOffset)) mask.Add(downOffset);
                if (!mask.Contains(leftOffset) && addCondition(leftOffset)) mask.Add(leftOffset);
                if (!mask.Contains(rightOffset) && addCondition(rightOffset)) mask.Add(rightOffset);

                if (!mask.Contains(upRightOffset) && addCondition(upRightOffset)) mask.Add(upRightOffset);
                if (!mask.Contains(downRightOffset) && addCondition(downRightOffset)) mask.Add(downRightOffset);

                if (!mask.Contains(upLeftOffset) && addCondition(upLeftOffset)) mask.Add(upLeftOffset);
                if (!mask.Contains(downLeftOffset) && addCondition(downLeftOffset)) mask.Add(downLeftOffset);
            }

            // Clear the last layer and set it to the current one.
            lastLayer.Clear();
            lastLayer.AddRange(mask.GetRange(lastLayerStartIndex, mask.Count - lastLayerStartIndex));
        }

        return mask.ToArray();
    }
}
