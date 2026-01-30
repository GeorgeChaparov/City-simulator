using UnityEngine;

using static UnityEditor.PlayerSettings;

public class GridUtils
{
    public static int GetIndex(int x, int y)
    {
        return y * GridGlobals.Width + x;
    }

    public static int GetXPos(int index)
    {
        return index % GridGlobals.Width;
    }

    public static int GetYPos(int index)
    {
        return (index % (GridGlobals.Width * GridGlobals.Height)) / GridGlobals.Width;
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

    public static (int x, int y) GetCoordinatesDirection(CellOrientation direction)
    {
        (int x, int y) coordinates = (0, 0);

        switch (direction)
        {
            case CellOrientation.East:
                coordinates = (1, 0);
                break;
            case CellOrientation.West:
                coordinates = (-1, 0);
                break;
            case CellOrientation.North:
                coordinates = (0, 1);
                break;
            case CellOrientation.South:
                coordinates = (0, -1);
                break;
            default:
                Debug.LogError($"Invalid Direction: {direction}");
                break;
        }

        return coordinates;
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
        CellOrientation oppositeDirection = CellOrientation.None;
        switch (direction)
        {
            case CellOrientation.East:
                oppositeDirection = CellOrientation.West;
                break;
            case CellOrientation.West:
                oppositeDirection = CellOrientation.East;
                break;
            case CellOrientation.North:
                oppositeDirection = CellOrientation.South;
                break;
            case CellOrientation.South:
                oppositeDirection = CellOrientation.North;
                break;
            default:
                Debug.LogError($"Invalid Direction: {direction}");
                break;
        }

        return oppositeDirection;
    }

    public static CellOrientation GetOppositeDirectionOf(int index)
    {
        CellOrientation direction = Cell.GetOrientation(index);
        CellOrientation oppositeDirection = CellOrientation.None;

        switch (direction)
        {
            case CellOrientation.East:
                oppositeDirection = CellOrientation.West;
                break;
            case CellOrientation.West:
                oppositeDirection = CellOrientation.East;
                break;
            case CellOrientation.North:
                oppositeDirection = CellOrientation.South;
                break;
            case CellOrientation.South:
                oppositeDirection = CellOrientation.North;
                break;
            default:
                Debug.LogError($"Invalid Direction: {direction}");
                break;
        }

        return oppositeDirection;
    }

    public static bool IsProjOutOfGridBounds(int index, int fromIndex, CellOrientation direction)
    {
        int fromX = GetXPos(fromIndex);
        int x = GetXPos(index);

        switch (direction)
        {
            case CellOrientation.East:

                // If the X pos of the new index is smaller that means we are on the next row and thus on the other side of the grid. 
                // I consider that invalid. 
                if (x < fromX)
                {
                    return true;
                }
                break;
            case CellOrientation.West:

                // If the X pos of the new index is bigger that means we are on the previous row and thus on the other side of the grid. 
                // I consider that invalid. 
                if (x > fromX)
                {
                    return true;
                }
                break;
            default:
                break;
        }

        if (index < 0 || index >= GridGlobals.Width * GridGlobals.Height)
        {
            return true;
        }

        return false;
    }

    public static bool IsProjOutOfGridBounds(int index, int indexX, int fromIndexX, CellOrientation direction)
    {
        switch (direction)
        {
            case CellOrientation.East:

                // If the X pos of the new index is smaller that means we are on the next row and thus on the other side of the grid. 
                // I consider that invalid. 
                if (indexX < fromIndexX)
                {
                    return true;
                }
                break;
            case CellOrientation.West:

                // If the X pos of the new index is bigger that means we are on the previous row and thus on the other side of the grid. 
                // I consider that invalid. 
                if (indexX > fromIndexX)
                {
                    return true;
                }
                break;
            default:
                break;
        }

        if (index < 0 || index >= GridGlobals.Width * GridGlobals.Height)
        {
            return true;
        }

        return false;
    }
}
