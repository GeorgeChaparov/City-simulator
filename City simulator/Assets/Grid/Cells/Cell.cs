using System;
using System.Collections.Generic;

[Flags]
public enum CellOrientation
{
    None = 0, East = 1, West = 2, North = 4, South = 8,
}

public enum CellType
{
    Empty, Building, Sidewalk, Street, Intersection
}

[Flags]
public enum CellFeature
{
    None = 0,
    // Street features
    SpeedBump = 1 << 0, Crosswalk = 1 << 1, RoadWork = 1 << 2, Potholes = 1 << 3, RoadBlock = 1 << 4, 
    IShapedStreet = 1 << 5, LShapedStreet = 1 << 6, DeadEnd = 1 << 7,
    // Intersection features
    TShapedIntersection = 1 << 8, XShapedIntersection = 1 << 9,
    // Sidewalk features
    Bench = 1 << 10, Trees = 1 << 11,
}

public static class Cell
{
    /// <summary> This is the cost that is given when populating the cell. It is used to calculate the real travel cost. </summary>
    static private float[] baseTravelCost;

    static private CellType[] type;

    /// <summary> This is the real travel cost that is used for calculating the path of the agents. It is based on the base travel cost and the different features of the cell. </summary>
    static private float[] travelCost;

    /// <summary> Shows which agent/agents is/are in the cell. </summary>
    static private List<Agent>[] occupants;

    static private AgentType[] traversableBy;

    /// <summary> The features of the cell. Those change the cost of traversing. </summary>
    static private CellFeature[] features;

    /// <summary> To where is the cell facing. Used to make better decisions for neighbor placing. </summary>
    static private CellOrientation[] orientation;

    static public void Init()
    {
        int width = GridGlobals.Width;
        int height = GridGlobals.Height;
        type = new CellType[width * height];
        baseTravelCost = new float[width * height];
        travelCost = new float[width * height];
        occupants = new List<Agent>[width * height];

        for (int i = 0; i < occupants.Length; i++)
        {
            occupants[i] = new List<Agent>();
        }

        traversableBy = new AgentType[width * height];
        features = new CellFeature[width * height];
        orientation = new CellOrientation[width * height];
    }

    static public void PopulateCell(int index, CellType type, int baseTravelCost, CellFeature featuresBitmap, CellOrientation orientation)
    {
        Cell.type[index] = type;
        Cell.baseTravelCost[index] = baseTravelCost;
        features[index] = featuresBitmap;
        Cell.orientation[index] = orientation;

        CalculateTravelCost(index);
        CalculateTraversability(index);
    }

    public static CellFeature GetFeatures(int index)
    {
        return features[index];
    }

    public static void SetFeature(int index, CellFeature featuresBitmap)
    {
        features[index] = featuresBitmap;

        CalculateTravelCost(index);
        CalculateTraversability(index);
    }

    public static void AddFeature(int index, CellFeature featureBitmap)
    {
        features[index] |= featureBitmap;

        CalculateTravelCost(index);
        CalculateTraversability(index);
    }

    public static void RemoveFeature(int index, CellFeature featureBitmap)
    {
        features[index] &= ~featureBitmap;

        CalculateTravelCost(index);
        CalculateTraversability(index);
    }

    public static CellType GetType(int index)
    {
        return type[index];
    }

    public static void SetType(int index, CellType type, CellFeature featuresBitmap)
    {
        Cell.type[index] = type;
        features[index] = featuresBitmap;

        CalculateTravelCost(index);
        CalculateTraversability(index);
    }

    public static List<Agent> GetOccupants(int index)
    {
        return occupants[index];
    }

    public static void AddOccupant(int index, Agent occupant)
    {
        occupants[index].Add(occupant);
    }

    public static void RemoveOccupant(int index, Agent occupant)
    {
        occupants[index].Remove(occupant);
    }

    public static float GetTravelCost(int index)
    {
        return travelCost[index];
    }

    public static AgentType GetTraversableBy(int index)
    {
        return traversableBy[index];
    }

    public static CellOrientation GetOrientation(int index)
    {
        return orientation[index];
    }

    private static void CalculateTraversability(int index)
    {
        switch (type[index])
        {
            case CellType.Empty:
                traversableBy[index] = AgentType.None;
                break;
            case CellType.Building:
                traversableBy[index] = AgentType.None;
                break;
            case CellType.Sidewalk:
                traversableBy[index] = AgentType.Person;
                break;
            case CellType.Street:
                AgentType agentsAbleToTravers = AgentType.Car;

                if ((features[index] & CellFeature.Crosswalk) != 0)
                {
                    agentsAbleToTravers |= AgentType.Person;
                }

                traversableBy[index] = agentsAbleToTravers;
                break;
            default:
                break;
        }
    }

    private static void CalculateTravelCost(int index) 
    {
        CellFeature feature = features[index];

        float finalCost = baseTravelCost[index];

        if ((feature & CellFeature.None) != 0)
        {
            travelCost[index] = finalCost;
            return;
        }

        switch (type[index])
        {
            case CellType.Empty:
                break;
            case CellType.Building:
                travelCost[index] = int.MaxValue;
                break;
            case CellType.Sidewalk:
                if ((feature & CellFeature.Bench) != 0)
                {
                    finalCost -= 0.8f;
                }

                if ((feature & CellFeature.Trees) != 0)
                {
                    finalCost -= 2f;
                }
                break;
            case CellType.Street:
                if ((feature & CellFeature.RoadBlock) != 0)
                {
                    travelCost[index] = int.MaxValue;
                    return;
                }

                if ((feature & CellFeature.SpeedBump) != 0)
                {
                    finalCost += 0.5f;
                }

                if ((feature & CellFeature.RoadWork) != 0)
                {
                    finalCost += 5f;
                }

                if ((feature & CellFeature.Potholes) != 0)
                {
                    finalCost += 5f;
                }
                break;
            default:
                break;
        }

        travelCost[index] = finalCost;
    }
}
