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

    public static T[] Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return array;
    }
}
