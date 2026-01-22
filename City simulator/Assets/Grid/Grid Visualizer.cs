using System.Collections.Generic;

using UnityEngine;

using static UnityEditor.PlayerSettings;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance { get; private set; }

    private Vector3 gridCenter = Vector3.zero;
    private Vector3 gridDownLeftCorner = Vector3.zero;
    private Vector3 gridUpRightCorner = Vector3.zero;

    private bool visualizeCheckPos = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Init()
    {
        gridCenter = transform.position;
        gridDownLeftCorner = new Vector3(gridCenter.x - (GridGlobals.Width / 2) * GridGlobals.CellSize, gridCenter.y - (GridGlobals.Height / 2) * GridGlobals.CellSize, gridCenter.z);
        gridUpRightCorner = new Vector3(gridCenter.x + (GridGlobals.Width / 2) * GridGlobals.CellSize, gridCenter.y + (GridGlobals.Height / 2) * GridGlobals.CellSize, gridCenter.z);
    }

    private void OnDrawGizmos()
    {
        if (GridGlobals.StreetAdjacencyList == null)
        {
            return;
        }

        // Visualizing the grid itself.
        Gizmos.color = Color.black;
        Gizmos.DrawLine(gridDownLeftCorner, new Vector2(gridDownLeftCorner.x + GridGlobals.Width * GridGlobals.CellSize, gridDownLeftCorner.y));
        Gizmos.DrawLine(gridDownLeftCorner, new Vector2(gridDownLeftCorner.x, gridDownLeftCorner.y + GridGlobals.Height * GridGlobals.CellSize));
        Gizmos.DrawLine(gridUpRightCorner, new Vector2(gridUpRightCorner.x - GridGlobals.Width * GridGlobals.CellSize, gridUpRightCorner.y));
        Gizmos.DrawLine(gridUpRightCorner, new Vector2(gridUpRightCorner.x, gridUpRightCorner.y - GridGlobals.Height * GridGlobals.CellSize));

        

        foreach (var cell in GridGlobals.StreetAdjacencyList)
        {
            int index = cell.Key;
            CellType type = Cell.GetType(index);
            CellOrientation orientation = Cell.GetOrientation(index);
            CellFeature features = Cell.GetFeatures(index);

            VisualizeStreet(index, type, orientation, features);
        }


        if (!visualizeCheckPos)
        {
            return;
        }

        foreach (var pos in GridGlobals.CheckBounds.Item2)
        {
            VisualizeCheckedPosition(pos);
        }
    }

    public void StopVisualizingCheckPos()
    {
        visualizeCheckPos = false;
    }

    public void StartVisualizingCheckPos()
    {
        visualizeCheckPos = true;
    }

    public void VisualizeStreet(int index, CellType cellType, CellOrientation cellOrientation, CellFeature cellFeatures)
    {

        int x = GridUtils.GetXPos(index);
        int y = GridUtils.GetYPos(index);
        
        int cellSize = GridGlobals.CellSize;
        float halfCellSize = cellSize / 2;

        Vector3 position = new Vector3(gridDownLeftCorner.x + halfCellSize + x * cellSize, gridDownLeftCorner.y + halfCellSize + y * cellSize, gridCenter.z);

        float oneFourth = halfCellSize / 2;
        float oneFourthEast = position.x + oneFourth;
        float oneFourthWest = position.x - oneFourth;
        float oneFourthNorth = position.y + oneFourth;
        float oneFourthSouth = position.y - oneFourth;

        float x1 = position.x - halfCellSize;
        float y1 = position.y - halfCellSize;

        float x2 = position.x + halfCellSize;
        float y2 = position.y + halfCellSize;


        switch (cellType)
        {
            case CellType.Street:

                if ((cellFeatures & CellFeature.IShapedStreet) != 0)
                {
                    Gizmos.color = Color.yellow;

                    switch (cellOrientation)
                    {
                        case CellOrientation.East:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));

                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(new Vector2(oneFourthWest, position.y), new Vector2(oneFourthEast, position.y));
                            Gizmos.DrawLine(new Vector2(oneFourthEast, position.y), new Vector2(oneFourthEast - oneFourth, position.y + oneFourth));
                            Gizmos.DrawLine(new Vector2(oneFourthEast, position.y), new Vector2(oneFourthEast - oneFourth, position.y - oneFourth));
                            break;
                        case CellOrientation.West:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));

                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(new Vector2(oneFourthEast, position.y), new Vector2(oneFourthWest, position.y));
                            Gizmos.DrawLine(new Vector2(oneFourthWest, position.y), new Vector2(oneFourthWest + oneFourth, position.y + oneFourth));
                            Gizmos.DrawLine(new Vector2(oneFourthWest, position.y), new Vector2(oneFourthWest + oneFourth, position.y - oneFourth));
                            break;
                        case CellOrientation.North:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));

                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthSouth), new Vector2(position.x, oneFourthNorth));
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthNorth), new Vector2(position.x + oneFourth, oneFourthNorth - oneFourth));
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthNorth), new Vector2(position.x - oneFourth, oneFourthNorth - oneFourth));
                            break;
                        case CellOrientation.South:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));

                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthNorth), new Vector2(position.x, oneFourthSouth));
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthSouth), new Vector2(position.x + oneFourth, oneFourthSouth + oneFourth));
                            Gizmos.DrawLine(new Vector2(position.x, oneFourthSouth), new Vector2(position.x - oneFourth, oneFourthSouth + oneFourth));
                            break;
                        default:
                            break;
                    }
                }
                else if ((cellFeatures & CellFeature.LShapedStreet) != 0)
                {
                    Gizmos.color = Color.red;

                    switch (cellOrientation)
                    {
                        case CellOrientation.East:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            break;
                        case CellOrientation.West:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));
                            break;
                        case CellOrientation.North:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            break;
                        case CellOrientation.South:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2 ));
                            break;
                        default:
                            break;
                    }
                }
                else if ((cellFeatures & CellFeature.DeadEnd) != 0)
                {
                    Gizmos.color = Color.pink;
                    switch (cellOrientation)
                    {
                        case CellOrientation.East:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));
                            break;
                        case CellOrientation.West:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            break;
                        case CellOrientation.North:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1));
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            break;
                        case CellOrientation.South:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1, y1 + cellSize));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2, y2 - cellSize));
                            break;
                        default:
                            break;
                    }
                }
                break;
            case CellType.Intersection:
                if ((cellFeatures & CellFeature.TShapedIntersection) != 0)
                {
                    Gizmos.color = Color.green;

                    switch (cellOrientation)
                    {
                        case CellOrientation.East:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 , y1 + cellSize));
                            break;
                        case CellOrientation.West:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 , y2 - cellSize));
                            break;
                        case CellOrientation.North:
                            Gizmos.DrawLine(new Vector2(x1, y1), new Vector2(x1 + cellSize, y1 ));
                            break;
                        case CellOrientation.South:
                            Gizmos.DrawLine(new Vector2(x2, y2), new Vector2(x2 - cellSize, y2));
                            break;
                        default:
                            break;
                    }
                }
                else if ((cellFeatures & CellFeature.XShapedIntersection) != 0)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(position, cellSize / 4);
                }

                break;
            default:
                break;
        }
    }

    private void VisualizeCheckedPosition((int x, int y) _pos)
    {
        int index = GridGlobals.CheckBounds.Item1;

        int x = GridUtils.GetXPos(index);
        int y = GridUtils.GetYPos(index);

        int cellSize = GridGlobals.CellSize;
        float halfCellSize = cellSize / 2;

        Vector3 position = new Vector3(gridDownLeftCorner.x + halfCellSize + (x + _pos.x) * cellSize, gridDownLeftCorner.y + halfCellSize + (y + _pos.y) * cellSize, gridCenter.z);
        Vector3 size = new Vector3(cellSize, cellSize);

        Color color = Color.green;

        color.a = 0.2f;

        Gizmos.color = color;
        Gizmos.DrawCube(position, size);
    }
}
