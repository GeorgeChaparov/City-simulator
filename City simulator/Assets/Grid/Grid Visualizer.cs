using System.Collections.Generic;

using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance { get; private set; }

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

    private void OnDrawGizmos()
    {
        foreach (var cell in GridConsts.StreetAdjacencyList)
        {
            int index = cell.Key;
            CellType type = Cell.GetType(index);
            CellOrientation orientation = Cell.GetOrientation(index);
            CellFeature features = Cell.GetFeatures(index);

            VisualizeStreet(index, type, orientation, features);
        }
    }

    public void VisualizeStreet(int _index, CellType _cellType, CellOrientation _cellOrientation, CellFeature _cellFeatures)
    {
        int x = GridUtils.GetXPos(_index);
        int y = GridUtils.GetYPos(_index);

        Vector3 gridCenter = transform.position;
        
        int cellSize = GridConsts.CellSize;
        float halfCellSize = cellSize / 2;

        Vector3 gridDownLeftCorner = new Vector3(gridCenter.x - (GridConsts.Width / 2) * cellSize, gridCenter.y - (GridConsts.Height / 2) * cellSize, gridCenter.z);
        Vector3 gridUpRightCorner = new Vector3(gridCenter.x + (GridConsts.Width / 2) * cellSize, gridCenter.y + (GridConsts.Height / 2) * cellSize, gridCenter.z);

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


        Gizmos.color = Color.black;
        Gizmos.DrawLine(gridDownLeftCorner, new Vector2(gridDownLeftCorner.x + GridConsts.Width * cellSize, gridDownLeftCorner.y));
        Gizmos.DrawLine(gridDownLeftCorner, new Vector2(gridDownLeftCorner.x, gridDownLeftCorner.y + GridConsts.Height * cellSize));
        Gizmos.DrawLine(gridUpRightCorner, new Vector2(gridUpRightCorner.x - GridConsts.Width * cellSize, gridUpRightCorner.y));
        Gizmos.DrawLine(gridUpRightCorner, new Vector2(gridUpRightCorner.x, gridUpRightCorner.y - GridConsts.Height * cellSize));
        



        switch (_cellType)
        {
            case CellType.Street:

                if ((_cellFeatures & CellFeature.IShapedStreet) != 0)
                {
                    Gizmos.color = Color.yellow;

                    switch (_cellOrientation)
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
                else if ((_cellFeatures & CellFeature.LShapedStreet) != 0)
                {
                    Gizmos.color = Color.red;

                    switch (_cellOrientation)
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
                else if ((_cellFeatures & CellFeature.DeadEnd) != 0)
                {
                    Gizmos.color = Color.pink;
                    switch (_cellOrientation)
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
                if ((_cellFeatures & CellFeature.TShapedIntersection) != 0)
                {
                    Gizmos.color = Color.green;

                    switch (_cellOrientation)
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
                else if ((_cellFeatures & CellFeature.XShapedIntersection) != 0)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(position, cellSize / 4);
                }

                break;
            default:
                break;
        }
    }
}
