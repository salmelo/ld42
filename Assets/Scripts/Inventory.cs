using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(6, 6);
    public float cellSize = 1;
    public LineRenderer linePrefab;

    // Use this for initialization
    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        for (int x = 0; x <= gridSize.x; x++)
        {
            var line = Instantiate(linePrefab, transform, false);
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(x * cellSize, 0));
            line.SetPosition(1, new Vector3(x * cellSize, gridSize.y * cellSize));
        }
        for (int y = 0; y <= gridSize.y; y++)
        {
            var line = Instantiate(linePrefab, transform, false);
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(0, y * cellSize));
            line.SetPosition(1, new Vector3(gridSize.x * cellSize, y * cellSize));
        }
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x <= gridSize.x; x++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(x * cellSize, 0)
                          , transform.position + new Vector3(x * cellSize, gridSize.y * cellSize));
        }
        for (int y = 0; y <= gridSize.y; y++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, y * cellSize)
                          , transform.position + new Vector3(gridSize.x * cellSize, y * cellSize));
        }
    }
}
