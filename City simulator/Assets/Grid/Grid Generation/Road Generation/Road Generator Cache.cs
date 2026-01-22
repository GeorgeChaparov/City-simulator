using System.Collections.Generic;

using UnityEngine;

public class RoadGenCache
{
    public static readonly int StreetTraverseBaseCost = 2;
    public static readonly int IntersectionTraverseBaseCost = 5;

    /// <summary>
    /// Holds possible orientations for the T shaped intersection based on which side of the last cell it is.
    /// </summary>
    /// 
    public static readonly Dictionary<CellOrientation, CellOrientation[]> TOrientationBasedOnLastCellMask = new Dictionary<CellOrientation, CellOrientation[]>(4)
    {
        { CellOrientation.East , new[] { CellOrientation.West,  CellOrientation.North, CellOrientation.South } }, // 0 - If on the east side of the last cell
        { CellOrientation.West , new[] { CellOrientation.East,  CellOrientation.North, CellOrientation.South } }, // 1 - If on the west side of the last cell
        { CellOrientation.North , new[] { CellOrientation.South, CellOrientation.East,  CellOrientation.West } },  // 2 - If on the north side of the last cell
        { CellOrientation.South , new[] { CellOrientation.North, CellOrientation.East,  CellOrientation.West } },  // 3 - If on the south side of the last cell
    };

    /// <summary>
    /// Holds possible orientations for the L shaped street based on which side of the last cell it is.
    /// </summary>
    public static readonly Dictionary<CellOrientation, CellOrientation[]> LOrientationBasedOnLastCellMask = new Dictionary<CellOrientation, CellOrientation[]>(4)
    {
        { CellOrientation.East , new[] { CellOrientation.West,  CellOrientation.North } }, // 0 - If on the east side of the last cell
        { CellOrientation.West , new[] { CellOrientation.South,  CellOrientation.East } },  // 1 - If on the west side of the last cell
        { CellOrientation.North, new[] { CellOrientation.South, CellOrientation.West } }, // 2 - If on the north side of the last cell
        { CellOrientation.South , new[] { CellOrientation.North,  CellOrientation.East } },  // 3 - If on the south side of the last cell
    };

    /// <summary>
    /// Holds the possible directions for the T shaped intersection.
    /// </summary>
    /// 
    public static readonly Dictionary<CellOrientation, CellOrientation[]> TDirectionMask = new Dictionary<CellOrientation, CellOrientation[]>(4)
    {
        { CellOrientation.East , new[] { CellOrientation.East,  CellOrientation.North, CellOrientation.South } }, // 0 - If oriented east
        { CellOrientation.West , new[] { CellOrientation.West,  CellOrientation.North, CellOrientation.South } }, // 1 - If oriented west
        { CellOrientation.North , new[] { CellOrientation.North, CellOrientation.East,  CellOrientation.West } },  // 2 - If oriented north
        { CellOrientation.South , new[] { CellOrientation.South, CellOrientation.East,  CellOrientation.West } },  // 3 - If oriented south
    };

    /// <summary>
    /// Holds the possible directions for the L shaped street.
    /// </summary>
    public static readonly Dictionary<CellOrientation, CellOrientation[]> LDirectionMask = new Dictionary<CellOrientation, CellOrientation[]>(4)
    {
        { CellOrientation.East , new[] { CellOrientation.East,  CellOrientation.North } }, // 0 - If oriented east
        { CellOrientation.West , new[] { CellOrientation.South,  CellOrientation.West } },  // 1 - If oriented west
        { CellOrientation.North, new[] { CellOrientation.North, CellOrientation.West } }, // 2 - If oriented north
        { CellOrientation.South , new[] { CellOrientation.South,  CellOrientation.East } },  // 3 - If oriented south
    };


    // Masks for each type of road used to create the masks used during simulation for collision detection.
    #region Base collistion detection masks

    public static readonly (int x, int y)[] IBaseMaskOffsets = new (int, int)[]
    {
                                         (0, 1),  (1, 1),  (2, 1),
        /*I shaped street facing east -> (0, 0)*/ (1, 0),  (2, 0), (3, 0),
                                         (0, -1), (1, -1), (2, -1),
    };

    public static readonly (int x, int y)[] LBaseForwardMaskOffsets = new (int, int)[]
    {
        (-1, 2),  (0, 2),  (1, 2),
        (-1, 1),  (0, 1),  (1, 1),
        (-1, 0),//(0, 0), <- L shaped street facing east
        (-1, -1), (0, -1), (1, -1),
    };

    public static readonly (int x, int y)[] LBaseBackwardMaskOffsets = new (int, int)[]
    {
        // L shaped street facing east
        (-1, 1), /* | */   (1, 1),  (2, 1),
        (-1, 0),/*(0, 0)*/ (1, 0), (2, 0),
        (-1, -1), (0, -1), (1, -1), (2, -1),
    };

    public static readonly (int x, int y)[] TBaseForwardMaskOffsets = new (int, int)[]
    {
                  (0, 3),
                  (0, 2),
        (-1, 1),  (0, 1),  (1, 1),
        (-1, 0),//(0, 0), <- T shaped intersection facing east
        (-1, -1), (0, -1), (1, -1),
                  (0, -2),
                  (0, -3),
    };

    public static readonly (int x, int y)[] TBaseUpwardMaskOffsets = new (int, int)[]
    {
        // T shaped intersection facing east          
        (-1, 1), /* | */   (1, 1),
        (-1, 0),/*(0, 0)*/ (1, 0), (2, 0), (3, 0),
        (-1, -1), (0, -1), (1, -1),
                  (0, -2),
                  (0, -3),
    };

    public static readonly (int x, int y)[] TBaseDownwardMaskOffsets = new (int, int)[]
    {
                  (0, 3),
                  (0, 2),
        (-1, 1),  (0, 1),  (1, 1),
        (-1, 0),/*(0, 0)*/ (1, 0), (2, 0), (3, 0),
        (-1, -1), /* | */  (1, -1),
        // T shaped intersection facing east
    };

    public static readonly (int x, int y)[] XBaseMaskOffsets = new (int, int)[]
    {
                           (-2, 2),  (-1, 2),  (0, 2),  (1, 2),  (2, 2),
                           (-2, 1),  (-1, 1),  (0, 1),  (1, 1),  (2, 1),  
        /*X shaped intersection facing east -> (0, 0)*/ (1, 0),  (2, 0),
                           (-2, -1), (-1, -1), (0, -1), (1, -1), (2, -1),
                           (-2, -2), (-1, -2), (0, -2), (1, -2), (2, -2),
    };

    #endregion

    /// <summary>
    /// Series of orientations that will likely make the streets crash into each other.
    /// </summary>
    public static readonly CellOrientation[][] LDeniedConsecutiveOrientations = new CellOrientation[6][]
    {
        new CellOrientation[3] { CellOrientation.East,  CellOrientation.South,  CellOrientation.West },
        new CellOrientation[3] { CellOrientation.West,  CellOrientation.North, CellOrientation.East },
        new CellOrientation[3] { CellOrientation.West,  CellOrientation.South, CellOrientation.East },
        new CellOrientation[3] { CellOrientation.North,  CellOrientation.West,  CellOrientation.South },
        new CellOrientation[3] { CellOrientation.South,  CellOrientation.East, CellOrientation.North },
        new CellOrientation[3] { CellOrientation.South,  CellOrientation.West, CellOrientation.North },
    };

}
