using UnityEngine;

public class GridUtils
{
    public static int GetXPos(int _index)
    {
        return _index % GridConsts.Width;
    }

    public static int GetYPos(int _index)
    {
        return (_index % (GridConsts.Width * GridConsts.Height)) / GridConsts.Width;
    }
}
