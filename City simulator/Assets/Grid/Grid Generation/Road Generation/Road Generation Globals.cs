using System.Collections.Generic;

public class RoadGenGlobals
{
    public static readonly int HIT_END_OF_GRID = -100;
    public static readonly int NO_POSSIBLE_DIRECTIONS = -101;


    public static int MinStreetsWithoutIntersection = 10;
    public static int MaxStreetsWithoutIntersection = 20;

    public static int MaxTurnsBetweenIntersection = 2;

    public static int MinStreetsBetweenTurns = 0;

    /// <summary>
    /// How many streets do we have to have before the first turn.
    /// </summary>
    public static int MinStreetsBeforeFirstTurn = 0;

    public static int CellsBetweenRoads = 0;

    /// <summary>
    /// How many turns in the same direction, that come one after another, are allowed between two intersections.
    /// </summary>
    public static int AllowedConsecutiveTurnsInSameOrientation = 2;

    /// <summary>
    /// How likely is to choose X shaped intersection.
    /// </summary>
    public static float XIntersectionLikelihood = 0.5f;

    /// <summary>
    /// If we want to prevent the road from making 3 or more turns in the same direction and so making a circle and crashing into itself.
    /// </summary>
    public static bool PreventLoopAroundTurns = true;

    /// <summary>
    /// How likely is to choose I shaped street.
    /// </summary>
    public static float IStreetLikelihood = 0.5f;

    public static int IShapedStreetsCount = 0;
    public static int LShapedStreetsCount = 0;
    public static int TotalCellCount = 1;

    public static Queue<int> TIntersectionIndexes = new Queue<int>();
    public static Queue<int> XIntersectionIndexes = new Queue<int>();
    public static Queue<int> TurnIndexes = new Queue<int>();
    public static Queue<int> DeadEndIndexes = new Queue<int>();

    public static int StreetsAfterXIntersectionBeforeDeadEnd = 10;
    public static int StreetsAfterTIntersectionBeforeDeadEnd = 10;
    public static int IStreetsAfterLStreetsBeforeDeadEnd = 10;

    public static int StepCounter = 0;

    public static (int x, int y)[] IMaskOffsets;
    public static (int x, int y)[] LForwardMaskOffsets;
    public static (int x, int y)[] LBackwardMaskOffsets;

    public static (int x, int y)[] TForwardMaskOffsets;
    public static (int x, int y)[] TUpwardMaskOffsets;
    public static (int x, int y)[] TDownwardMaskOffsets;
    public static (int x, int y)[] XMaskOffsets;
}
