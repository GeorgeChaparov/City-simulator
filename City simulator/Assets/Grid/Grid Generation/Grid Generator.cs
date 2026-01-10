using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridGenerator
{
    public static void Init(int _minStreetsWithoutIntersection, int _maxStreetsWithoutIntersection, int _maxTurnsBetweenIntersection,
        int _minStreetsBetweenTurns, int _minStreetsAfterIntersectionBeforeTurn, int _emptyCellsBetweenStreets, int _allowedConsecutiveTurnsInSameOrientation,
        float _xIntersectionLikelihood, bool _preventLoopAroundTurns, float _iStreetLikelihood)
    {
        StreetGenGlobals.MinStreetsWithoutIntersection = _minStreetsWithoutIntersection;
        StreetGenGlobals.MaxStreetsWithoutIntersection = _maxStreetsWithoutIntersection;
        StreetGenGlobals.MaxTurnsBetweenIntersection = _maxTurnsBetweenIntersection;
        StreetGenGlobals.MinStreetsBetweenTurns = _minStreetsBetweenTurns;
        StreetGenGlobals.MinStreetsAfterIntersectionBeforeTurn = _minStreetsAfterIntersectionBeforeTurn;
        
        StreetGenGlobals.AllowedConsecutiveTurnsInSameOrientation = _allowedConsecutiveTurnsInSameOrientation;
        StreetGenGlobals.XIntersectionLikelihood = _xIntersectionLikelihood;
        StreetGenGlobals.PreventLoopAroundTurns = _preventLoopAroundTurns;
        StreetGenGlobals.IStreetLikelihood = _iStreetLikelihood;

        StreetGenGlobals.IShapedStreetsCount = 0;
        StreetGenGlobals.LShapedStreetsCount = 0;
        StreetGenGlobals.TotalCellCount = 1;
        StreetGenGlobals.Counter = 0;

        if (StreetGenGlobals.EmptyCellsBetweenStreets != _emptyCellsBetweenStreets)
        {
            StreetGenGlobals.EmptyCellsBetweenStreets = _emptyCellsBetweenStreets;

            if (StreetGenGlobals.EmptyCellsBetweenStreets != 1)
            {
                bool horizontalRightCheck((int x, int y) offset) { return offset.y != 0 || offset.x > 0; };
                bool horizontalLeftCheck((int x, int y) offset) { return offset.y != 0 || offset.x < 0; };
                bool verticalUpCheck((int x, int y) offset) { return offset.x != 0 || offset.y < 0; };
                bool verticalDownCheck((int x, int y) offset) { return offset.x != 0 || offset.y > 0; };

                StreetGenGlobals.IMaskOffsets = GenerateMaskOffset(StreetGenCache.IBaseMaskOffsets, horizontalRightCheck);
                StreetGenGlobals.LForwardMaskOffsets = GenerateMaskOffset(StreetGenCache.LBaseForwardMaskOffsets, horizontalLeftCheck);
                StreetGenGlobals.TForwardMaskOffsets = GenerateMaskOffset(StreetGenCache.TBaseForwardMaskOffsets, horizontalLeftCheck);
                StreetGenGlobals.XMaskOffsets = GenerateMaskOffset(StreetGenCache.XBaseMaskOffsets, horizontalRightCheck);

                StreetGenGlobals.LBackwardMaskOffsets = GenerateMaskOffset(StreetGenCache.LBaseBackwardMaskOffsets, verticalUpCheck, false);
                StreetGenGlobals.TUpwardMaskOffsets = GenerateMaskOffset(StreetGenCache.TBaseUpwardMaskOffsets, verticalUpCheck, false);
                StreetGenGlobals.TDownwardMaskOffsets = GenerateMaskOffset(StreetGenCache.TBaseDownwardMaskOffsets, verticalDownCheck, false);
            }
            else
            {
                StreetGenGlobals.IMaskOffsets = StreetGenCache.IBaseMaskOffsets;
                StreetGenGlobals.LForwardMaskOffsets = StreetGenCache.LBaseForwardMaskOffsets;
                StreetGenGlobals.TForwardMaskOffsets = StreetGenCache.TBaseForwardMaskOffsets;
                StreetGenGlobals.XMaskOffsets = StreetGenCache.XBaseMaskOffsets;

                StreetGenGlobals.LBackwardMaskOffsets = StreetGenCache.LBaseBackwardMaskOffsets;
                StreetGenGlobals.TUpwardMaskOffsets = StreetGenCache.TBaseUpwardMaskOffsets;
                StreetGenGlobals.TDownwardMaskOffsets = StreetGenCache.TBaseDownwardMaskOffsets;
            }
        }
    }

    public static IEnumerator Generate()
    {
        int x = UnityEngine.Random.Range(0, GridGlobals.Width);
        int y = UnityEngine.Random.Range(0, GridGlobals.Height);
        int randomStartIndex = y * GridGlobals.Width + x;

        yield return StreetGenerator.CreateStreets(randomStartIndex);

        Debug.Log($"I shaped: {StreetGenGlobals.TotalCellCount}");
        Debug.Log($"I shaped: {StreetGenGlobals.IShapedStreetsCount}");
        Debug.Log($"L shaped: {StreetGenGlobals.LShapedStreetsCount}");
    }

    private static (int x, int y)[] GenerateMaskOffset((int x, int y)[] offsets, Func<(int x, int y), bool> addCondition, bool horizontal = true)
    {
        List<(int x, int y)> mask = new List<(int x, int y)>(offsets);
        List<(int x, int y)> lastLayer = new List<(int x, int y)>(offsets);
        int lastLayerStartIndex = 0;

        for (int i = 0; i < StreetGenGlobals.EmptyCellsBetweenStreets; i++)
        {
            lastLayerStartIndex = mask.Count;

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

                if (horizontal)
                {
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
                }
                else
                {
                    if (y > 0)
                    {
                        upOffset = (x, y + 1);

                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                            upRightOffset = (x + 1, y + 1);
                        }
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                            upLeftOffset = (x - 1, y + 1);
                        }
                    }
                    else if (y < 0)
                    {
                        downOffset = (x, y - 1);

                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                            downRightOffset = (x + 1, y - 1);
                        }
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                            downLeftOffset = (x - 1, y - 1);
                        }
                    }
                    else
                    {
                        if (x > 0)
                        {
                            rightOffset = (x + 1, y);
                        }
                        else if (x < 0)
                        {
                            leftOffset = (x - 1, y);
                        }
                    }
                }

                if (!mask.Contains(upOffset) && addCondition(upOffset)) mask.Add(upOffset);
                if (!mask.Contains(downOffset) && addCondition(downOffset)) mask.Add(downOffset);
                if (!mask.Contains(leftOffset) && addCondition(leftOffset)) mask.Add(leftOffset);
                if (!mask.Contains(rightOffset) && addCondition(rightOffset)) mask.Add(rightOffset);

                if (!mask.Contains(upRightOffset) && addCondition(upRightOffset)) mask.Add(upRightOffset);
                if (!mask.Contains(downRightOffset) && addCondition(downRightOffset)) mask.Add(downRightOffset);

                if (!mask.Contains(upLeftOffset) && addCondition(upLeftOffset)) mask.Add(upLeftOffset);
                if (!mask.Contains(downLeftOffset) && addCondition(downLeftOffset)) mask.Add(downLeftOffset);
            }

            lastLayer.Clear();
            lastLayer.AddRange(mask.GetRange(lastLayerStartIndex, mask.Count - lastLayerStartIndex));
        }

        return mask.ToArray();
    }
}
