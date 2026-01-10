using UnityEngine;

public class StreetGenGlobals
{
    public static readonly int HIT_END_OF_GRID = -100;
    public static readonly int NO_POSSIBLE_DIRECTIONS = -101;

    public static int MinStreetsWithoutIntersection = 10;
    public static int MaxStreetsWithoutIntersection = 20;

    public static int MaxTurnsBetweenIntersection = 2;

    public static int MinStreetsBetweenTurns = 0;

    public static int MinStreetsAfterIntersectionBeforeTurn = 0;

    public static int EmptyCellsBetweenStreets = 1;

    public static int AllowedConsecutiveTurnsInSameOrientation = 2;

    public static float XIntersectionLikelihood = 0.5f;

    public static bool PreventLoopAroundTurns = true;

    public static float IStreetLikelihood = 0.5f;

    public static int IShapedStreetsCount = 0;
    public static int LShapedStreetsCount = 0;
    public static int TotalCellCount = 1;

    public static int Counter = 0;

    public static (int x, int y)[] IMaskOffsets;
    public static (int x, int y)[] LForwardMaskOffsets;
    public static (int x, int y)[] LBackwardMaskOffsets;

    public static (int x, int y)[] TForwardMaskOffsets;
    public static (int x, int y)[] TUpwardMaskOffsets;
    public static (int x, int y)[] TDownwardMaskOffsets;
    public static (int x, int y)[] XMaskOffsets;
}
