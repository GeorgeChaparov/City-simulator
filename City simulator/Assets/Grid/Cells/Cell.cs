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
    SpeedBump = 1 << 0, Crosswalk = 2 << 1, RoadWork = 3 << 2, Potholes = 4 << 3, RoadBlock = 5 << 4, 
    IShapedStreet = 6 << 5, LShapedStreet = 7 << 6,
    // Intersection features
    TShapedIntersection = 8 << 7, XShapedIntersection = 9 << 8,
    // Sidewalk features
    Bench = 10 << 9, Trees = 11 << 10,
}

public static class Cell
{
    /// <summary> This is the cost that is given when populating the cell. It is used to calculate the real travel cost. </summary>
    static private float[] m_BaseTravelCost;

    static private CellType[] m_Type;

    /// <summary> This is the real travel cost that is used for calculating the path of the agents. It is based on the base travel cost and the different features of the cell. </summary>
    static private float[] m_TravelCost;

    /// <summary> Shows which agent/agents is/are in the cell. </summary>
    static private List<Agent>[] m_Occupants;

    static private AgentType[] m_TraversableBy;

    /// <summary> The features of the cell. Those change the cost of traversing. </summary>
    static private CellFeature[] m_Features;

    /// <summary> To where is the cell facing. Used to make better decisions for neighbor placing. </summary>
    static private CellOrientation[] m_Orientation;

    static public void Init(int _gridWidth, int _gridHeight)
    {
        m_Type = new CellType[_gridWidth * _gridHeight];
        m_BaseTravelCost = new float[_gridWidth * _gridHeight];
        m_TravelCost = new float[_gridWidth * _gridHeight];
        m_Occupants = new List<Agent>[_gridWidth * _gridHeight];

        for (int i = 0; i < m_Occupants.Length; i++)
        {
            m_Occupants[i] = new List<Agent>();
        }

        m_TraversableBy = new AgentType[_gridWidth * _gridHeight];
        m_Features = new CellFeature[_gridWidth * _gridHeight];
        m_Orientation = new CellOrientation[_gridWidth * _gridHeight];
    }

    static public void PopulateCell(int _index, CellType _type, int _baseTravelCost, CellFeature _featuresBitmap, CellOrientation _orientation)
    {
        m_Type[_index] = _type;
        m_BaseTravelCost[_index] = _baseTravelCost;
        m_Features[_index] = _featuresBitmap;
        m_Orientation[_index] = _orientation;

        CalculateTravelCost(_index);
        CalculateTraversability(_index);
    }

    public static CellFeature GetFeatures(int _index)
    {
        return m_Features[_index];
    }

    public static void SetFeature(int _index, CellFeature _featuresBitmap)
    {
        m_Features[_index] = _featuresBitmap;

        CalculateTravelCost(_index);
        CalculateTraversability(_index);
    }

    public static void AddFeature(int _index, CellFeature _featureBitmap)
    {
        m_Features[_index] |= _featureBitmap;

        CalculateTravelCost(_index);
        CalculateTraversability(_index);
    }

    public static void RemoveFeature(int _index, CellFeature _featureBitmap)
    {
        m_Features[_index] &= ~_featureBitmap;

        CalculateTravelCost(_index);
        CalculateTraversability(_index);
    }

    public static CellType GetType(int _index)
    {
        return m_Type[_index];
    }

    public static void SetType(int _index, CellType _type, CellFeature _featuresBitmap)
    {
        m_Type[_index] = _type;
        m_Features[_index] = _featuresBitmap;

        CalculateTravelCost(_index);
        CalculateTraversability(_index);
    }

    public static List<Agent> GetOccupants(int _index)
    {
        return m_Occupants[_index];
    }

    public static void AddOccupant(int _index, Agent _occupant)
    {
        m_Occupants[_index].Add(_occupant);
    }

    public static void RemoveOccupant(int _index, Agent _occupant)
    {
        m_Occupants[_index].Remove(_occupant);
    }

    public static float GetTravelCost(int _index)
    {
        return m_TravelCost[_index];
    }

    public static AgentType GetTraversableBy(int _index)
    {
        return m_TraversableBy[_index];
    }

    public static CellOrientation GetOrientation(int _index)
    {
        return m_Orientation[_index];
    }

    private static void CalculateTraversability(int _index)
    {
        switch (m_Type[_index])
        {
            case CellType.Empty:
                m_TraversableBy[_index] = AgentType.None;
                break;
            case CellType.Building:
                m_TraversableBy[_index] = AgentType.None;
                break;
            case CellType.Sidewalk:
                m_TraversableBy[_index] = AgentType.Person;
                break;
            case CellType.Street:
                AgentType agentsAbleToTravers = AgentType.Car;

                if ((m_Features[_index] & CellFeature.Crosswalk) != 0)
                {
                    agentsAbleToTravers |= AgentType.Person;
                }

                m_TraversableBy[_index] = agentsAbleToTravers;
                break;
            default:
                break;
        }
    }

    private static void CalculateTravelCost(int _index) 
    {
        CellFeature features = m_Features[_index];

        float finalCost = m_BaseTravelCost[_index];

        if ((features & CellFeature.None) != 0)
        {
            m_TravelCost[_index] = finalCost;
            return;
        }

        switch (m_Type[_index])
        {
            case CellType.Empty:
                break;
            case CellType.Building:
                m_TravelCost[_index] = int.MaxValue;
                break;
            case CellType.Sidewalk:
                if ((features & CellFeature.Bench) != 0)
                {
                    finalCost -= 0.8f;
                }

                if ((features & CellFeature.Trees) != 0)
                {
                    finalCost -= 2f;
                }
                break;
            case CellType.Street:
                if ((features & CellFeature.RoadBlock) != 0)
                {
                    m_TravelCost[_index] = int.MaxValue;
                    return;
                }

                if ((features & CellFeature.SpeedBump) != 0)
                {
                    finalCost += 0.5f;
                }

                if ((features & CellFeature.RoadWork) != 0)
                {
                    finalCost += 5f;
                }

                if ((features & CellFeature.Potholes) != 0)
                {
                    finalCost += 5f;
                }
                break;
            default:
                break;
        }

        m_TravelCost[_index] = finalCost;
    }
}
