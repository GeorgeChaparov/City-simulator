using UnityEngine;

public class GridUtils
{
    public static int GetXPos(int _index)
    {
        return _index % GridGlobals.Width;
    }

    public static int GetYPos(int _index)
    {
        return (_index % (GridGlobals.Width * GridGlobals.Height)) / GridGlobals.Width;
    }
}
