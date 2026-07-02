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

    #region Trace Style
    [Header("Butterfly Trace Style")]
    [SerializeField] private string resourcesMaterialPath = "ButterflyTrace";
    [SerializeField, Range(0.25f, 1.25f)] private float widthMultiplier = 0.72f;
    [SerializeField, Range(0, 8)] private int roundedCorners = 4;
    [SerializeField, Range(0, 8)] private int roundedCaps = 4;
    [SerializeField, Range(0f, 0.4f)] private float chainWidthBoost = 0.2f;
    [SerializeField, Range(0f, 15f)] private float breathingSpeed = 7f;
    #endregion

    #region Private Fields
    private Mesh mesh;
    private Vector3 start;
    private Vector3 end;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private Vector3[] points = new Vector3[200]; // Cache for point positions
    private Material runtimeMaterial;
    private bool ownsRuntimeMaterial;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeMaterial();
        InitializeLineRenderers();
    }

    private void OnDestroy()
    {
        if (ownsRuntimeMaterial && runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the number of vertices in the line
    /// </summary>
    /// <param name="count">Number of vertices</param>
    public void SetVertexCount(int count)
    {
        int requiredSegmentCount = Mathf.Max(0, count - 1);
        float excitement = Mathf.InverseLerp(2f, 10f, count);
        float breathing = Mathf.Sin(Time.unscaledTime * breathingSpeed) * 0.035f;
        float activeWidth = widthMultiplier * (1f + excitement * chainWidthBoost + breathing);
        Color startTint = Color.Lerp(
            new Color(1f, 1f, 1f, 0.82f),
            new Color(1f, 0.86f, 1f, 1f),
            excitement);
        Color endTint = Color.Lerp(
            new Color(0.88f, 1f, 1f, 0.9f),
            new Color(1f, 0.96f, 0.72f, 1f),
            excitement);

        // Ensure we have enough line renderers
        while (lines.Count < requiredSegmentCount)
            AddLine();
        
        // N points need N - 1 segments. Keeping an extra zero-length segment
        // enabled would render its rounded cap at world origin (the board center).
        for (int i = 0; i < lines.Count; i++)
        {
            if (i < requiredSegmentCount)
            {
                lines[i].enabled = true;
                lines[i].widthMultiplier = activeWidth;
                lines[i].startColor = startTint;
                lines[i].endColor = endTint;
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
            LineRenderer segment = lines[index - 1];
            segment.SetPosition(0, points[index - 1]);
            segment.SetPosition(1, points[index]);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes line renderers at startup
    /// </summary>
    private void InitializeLineRenderers()
    {
        lines.Clear();

        foreach (Transform item in transform)
        {
            LineRenderer lineRenderer = item.GetComponent<LineRenderer>();
            if (lineRenderer == null)
                continue;

            ConfigureLineRenderer(lineRenderer);
            lines.Add(lineRenderer);
        }
    }

    /// <summary>
    /// Loads the project trace material without requiring scene changes.
    /// A runtime fallback keeps the effect working if the resource is moved.
    /// </summary>
    private void InitializeMaterial()
    {
        if (material != null)
        {
            runtimeMaterial = material;
            return;
        }

        Material template = Resources.Load<Material>(resourcesMaterialPath);
        if (template != null)
        {
            runtimeMaterial = new Material(template)
            {
                name = template.name + " (Runtime)"
            };
            ownsRuntimeMaterial = true;
            return;
        }

        Shader traceShader = Shader.Find("JigsawMatch2/Butterfly Trace");
        if (traceShader == null)
        {
            Debug.LogWarning("Butterfly Trace shader was not found. The original line material will be used.", this);
            return;
        }

        runtimeMaterial = new Material(traceShader)
        {
            name = "ButterflyTrace (Runtime Fallback)"
        };
        ownsRuntimeMaterial = true;
    }

    /// <summary>
    /// Applies the soft gold, violet and aqua ribbon style to a trace segment.
    /// </summary>
    private void ConfigureLineRenderer(LineRenderer lineRenderer)
    {
        if (runtimeMaterial != null)
            lineRenderer.sharedMaterial = runtimeMaterial;

        lineRenderer.widthMultiplier = widthMultiplier;
        lineRenderer.numCornerVertices = roundedCorners;
        lineRenderer.numCapVertices = roundedCaps;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        SetSortingLayer(lineRenderer);
    }

    /// <summary>
    /// Creates and adds a new line renderer
    /// </summary>
    private void AddLine()
    {
        GameObject newLine = Instantiate(transform.GetChild(0).gameObject) as GameObject;
        newLine.transform.SetParent(transform, false);

        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();
        ConfigureLineRenderer(lineRenderer);
        lines.Add(lineRenderer);
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
