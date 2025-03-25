using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the visual lines connecting matched items during gameplay
/// </summary>
public class Line : MonoBehaviour
{
    #region Public Fields
    public Material material;
    public int lineWidth = 1;
    #endregion

    #region Private Fields
    private Mesh mesh;
    private Vector3 start;
    private Vector3 end;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private Vector3[] points = new Vector3[200]; // Cache for point positions
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeLineRenderers();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the number of vertices in the line
    /// </summary>
    /// <param name="count">Number of vertices</param>
    public void SetVertexCount(int count)
    {
        // Ensure we have enough line renderers
        if (lines.Count < count)
            AddLine();
        
        // Enable/disable line renderers based on count
        for (int i = 0; i < lines.Count; i++)
        {
            if (i < count)
            {
                lines[i].enabled = true;
                SetSortingLayer(lines[i]);
            }
            else
            {
                lines[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// Adds a point to the line
    /// </summary>
    /// <param name="position">Position of the point</param>
    /// <param name="index">Index of the point</param>
    public void AddPoint(Vector3 position, int index)
    {
        points[index] = position;
        
        // If not the first point, connect it to the previous point
        if (index > 0)
        {
            lines[index].SetPosition(0, points[index - 1]);
            lines[index].SetPosition(1, points[index]);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes line renderers at startup
    /// </summary>
    private void InitializeLineRenderers()
    {
        foreach (Transform item in transform)
        {
            if (item.GetComponent<LineRenderer>() != null)
                lines.Add(item.GetComponent<LineRenderer>());
        }
    }

    /// <summary>
    /// Creates and adds a new line renderer
    /// </summary>
    private void AddLine()
    {
        GameObject newLine = Instantiate(transform.GetChild(0).gameObject) as GameObject;
        newLine.transform.SetParent(transform);
        lines.Add(newLine.GetComponent<LineRenderer>());
    }

    /// <summary>
    /// Sets the sorting layer for a line renderer
    /// </summary>
    /// <param name="lineRenderer">Line renderer to set</param>
    private void SetSortingLayer(LineRenderer lineRenderer)
    {
        lineRenderer.sortingLayerID = 0;
        lineRenderer.sortingOrder = 1;
    }
    #endregion
}