using System.Collections.Generic;

using UnityEngine;

public class GridGlobals
{
    public static int Width = 30;
    public static int Height = 30;
    public static int CellSize = 10;

    public static Dictionary<int, List<int>> StreetAdjacencyList;
    public static Dictionary<int, List<int>> SidewalkAdjacencyList;
    public static (int, (int x, int y)[]) CheckBounds;

    public static void Init()
    {
        StreetAdjacencyList = new Dictionary<int, List<int>>();
        SidewalkAdjacencyList = new Dictionary<int, List<int>>();
        CheckBounds = new();
        CheckBounds.Item2 = new (int, int)[0];
    }

    public static void Reset()
    {
        StreetAdjacencyList.Clear();
        SidewalkAdjacencyList.Clear();
        CheckBounds = new();
        CheckBounds.Item2 = new (int, int)[0];
    }
}
