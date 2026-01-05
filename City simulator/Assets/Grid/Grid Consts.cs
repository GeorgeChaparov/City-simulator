using System.Collections.Generic;

using UnityEngine;

public class GridConsts
{
    public static int Width = 30;
    public static int Height = 30;
    public static int CellSize = 10;

    public static Dictionary<int, List<int>> StreetAdjacencyList = new Dictionary<int, List<int>>();
    public static Dictionary<int, List<int>> SidewalkAdjacencyList = new Dictionary<int, List<int>>();

    public static void Reset()
    {
        StreetAdjacencyList.Clear();
        SidewalkAdjacencyList.Clear();
    }
}
