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


    public static bool AreDirectionsOpposite(CellOrientation first, CellOrientation second)
    {
        if ((first == CellOrientation.East && second == CellOrientation.West) || (second == CellOrientation.East && first == CellOrientation.West))
        {
            return true;
        }

        if ((first == CellOrientation.North && second == CellOrientation.South) || (second == CellOrientation.North && first == CellOrientation.South))
        {
            return true;
        }

        return false;
    }

    public static bool AreDirectionsOpposite(CellOrientation directions)
    {
        CellOrientation first = CellOrientation.None;
        CellOrientation second = CellOrientation.None;

        if ((directions & CellOrientation.East) != 0)
        {
            first = CellOrientation.East;
        }

        if ((directions & CellOrientation.West) != 0)
        {
            if (first == CellOrientation.None)
            {
                first = CellOrientation.West;
            }
            else
            {
                second = CellOrientation.West;
            }
        }

        if ((directions & CellOrientation.North) != 0)
        {
            if (first == CellOrientation.None)
            {
                first = CellOrientation.North;
            }
            else
            {
                second = CellOrientation.North;
            }
        }

        if ((directions & CellOrientation.South) != 0)
        {
            if (first == CellOrientation.None)
            {
                first = CellOrientation.South;
            }
            else
            {
                second = CellOrientation.South;
            }
        }

        if ((first == CellOrientation.East && second == CellOrientation.West) || (second == CellOrientation.East && first == CellOrientation.West))
        {
            return true;
        }

        if ((first == CellOrientation.North && second == CellOrientation.South) || (second == CellOrientation.North && first == CellOrientation.South))
        {
            return true;
        }

        return false;
    }

    public static CellOrientation GetOppositeDirection(CellOrientation direction)
    {
        switch (direction)
        {
            case CellOrientation.East:
                return CellOrientation.West;
                break;
            case CellOrientation.West:
                return CellOrientation.East;
                break;
            case CellOrientation.North:
                return CellOrientation.South;
                break;
            case CellOrientation.South:
                return CellOrientation.North;
                break;
            default:
                return CellOrientation.None;
                break;
        }
    }

    public static CellOrientation GetOppositeDirectionOf(int index)
    {
        CellOrientation direction = Cell.GetOrientation(index);
        switch (direction)
        {
            case CellOrientation.East:
                return CellOrientation.West;
                break;
            case CellOrientation.West:
                return CellOrientation.East;
                break;
            case CellOrientation.North:
                return CellOrientation.South;
                break;
            case CellOrientation.South:
                return CellOrientation.North;
                break;
            default:
                return CellOrientation.None;
                break;
        }
    }
}
