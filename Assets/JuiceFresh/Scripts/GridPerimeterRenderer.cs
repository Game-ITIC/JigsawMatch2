using System.Collections.Generic;
using JuiceFresh;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GridPerimeterRenderer : MonoBehaviour
{
    [Header("Grid Perimeter Line")]
    [Tooltip("Prefab with LineRenderer (use Assets/Line.prefab).")]
    [SerializeField] private GameObject gridPerimeterLinePrefab;

    [Tooltip("Z offset for the perimeter line (relative to GameField).")]
    [SerializeField] private float gridPerimeterLineZOffset = -0.2f;

    [Tooltip("Time used to draw each grid contour after the cells finish appearing.")]
    [SerializeField, Min(0.05f)] private float gridOutlineDrawDuration = 0.72f;

    [Tooltip("Small delay between separate contour loops.")]
    [SerializeField, Min(0f)] private float gridOutlineLoopDelay = 0.08f;

    private readonly List<GameObject> _lineInstances = new List<GameObject>();
    private readonly List<Material> _lineMaterials = new List<Material>();

    private LevelManager _levelManager;

    readonly struct GridBoundaryEdge
    {
        public readonly Vector2Int Start;
        public readonly Vector2Int End;

        public GridBoundaryEdge(Vector2Int start, Vector2Int end)
        {
            Start = start;
            End = end;
        }
    }

    public void Initialize(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public static bool ShouldHideForState(GameState state)
    {
        return state == GameState.Win ||
               state == GameState.GameOver ||
               state == GameState.ToMap ||
               state == GameState.Map;
    }

    public void Draw()
    {
        if(_levelManager == null || _levelManager.GameField == null)
            return;

        if(gridPerimeterLinePrefab == null)
        {
            Debug.LogWarning(
                "Grid perimeter line prefab is not assigned. Assign Assets/Line.prefab to 'Grid Perimeter Line Prefab'.",
                this);
            return;
        }

        Clear();

        List<GridBoundaryEdge> edges = BuildGridBoundaryEdges();
        List<List<Vector2Int>> contours = TraceGridContours(edges);

        for(int i = 0; i < contours.Count; i++)
            CreateGridContourLine(contours[i], i);
    }

    public void Clear()
    {
        for(int i = 0; i < _lineInstances.Count; i++)
        {
            if(_lineInstances[i] != null)
            {
                _lineInstances[i].SetActive(false);
                Destroy(_lineInstances[i]);
            }
        }

        _lineInstances.Clear();

        for(int i = 0; i < _lineMaterials.Count; i++)
        {
            if(_lineMaterials[i] != null)
                Destroy(_lineMaterials[i]);
        }

        _lineMaterials.Clear();
    }

    public void SetVisible(bool visible)
    {
        for(int i = 0; i < _lineInstances.Count; i++)
        {
            if(_lineInstances[i] != null)
                _lineInstances[i].SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    List<GridBoundaryEdge> BuildGridBoundaryEdges()
    {
        List<GridBoundaryEdge> edges = new List<GridBoundaryEdge>();

        for(int row = 0; row < _levelManager.maxRows; row++)
        {
            for(int col = 0; col < _levelManager.maxCols; col++)
            {
                if(!IsGridCellActive(col, row))
                    continue;

                Vector2Int topLeft = new Vector2Int(col, row);
                Vector2Int topRight = new Vector2Int(col + 1, row);
                Vector2Int bottomRight = new Vector2Int(col + 1, row + 1);
                Vector2Int bottomLeft = new Vector2Int(col, row + 1);

                if(!IsGridCellActive(col, row - 1))
                    edges.Add(new GridBoundaryEdge(topLeft, topRight));
                if(!IsGridCellActive(col + 1, row))
                    edges.Add(new GridBoundaryEdge(topRight, bottomRight));
                if(!IsGridCellActive(col, row + 1))
                    edges.Add(new GridBoundaryEdge(bottomRight, bottomLeft));
                if(!IsGridCellActive(col - 1, row))
                    edges.Add(new GridBoundaryEdge(bottomLeft, topLeft));
            }
        }

        return edges;
    }

    bool IsGridCellActive(int col, int row)
    {
        if(col < 0 || col >= _levelManager.maxCols || row < 0 || row >= _levelManager.maxRows)
            return false;

        Square square = _levelManager.GetSquare(col, row, true);
        return square != null && square.type != SquareTypes.NONE;
    }

    static List<List<Vector2Int>> TraceGridContours(List<GridBoundaryEdge> edges)
    {
        List<List<Vector2Int>> contours = new List<List<Vector2Int>>();

        while(edges.Count > 0)
        {
            GridBoundaryEdge edge = edges[0];
            edges.RemoveAt(0);

            Vector2Int start = edge.Start;
            Vector2Int current = edge.End;
            Vector2Int direction = edge.End - edge.Start;
            List<Vector2Int> contour = new List<Vector2Int> { start };
            int safety = 0;

            while(current != start && safety++ < 10000)
            {
                contour.Add(current);
                int nextIndex = FindNextBoundaryEdge(edges, current, direction);
                if(nextIndex < 0)
                    break;

                GridBoundaryEdge next = edges[nextIndex];
                edges.RemoveAt(nextIndex);
                direction = next.End - next.Start;
                current = next.End;
            }

            if(current == start && contour.Count >= 4)
                contours.Add(SimplifyGridContour(contour));
        }

        return contours;
    }

    static int FindNextBoundaryEdge(
        List<GridBoundaryEdge> edges,
        Vector2Int start,
        Vector2Int previousDirection)
    {
        int previousDirectionIndex = GetGridDirectionIndex(previousDirection);
        int bestIndex = -1;
        int bestRank = int.MaxValue;

        for(int i = 0; i < edges.Count; i++)
        {
            if(edges[i].Start != start)
                continue;

            int directionIndex = GetGridDirectionIndex(edges[i].End - edges[i].Start);
            int turn = (directionIndex - previousDirectionIndex + 4) % 4;
            int rank = turn == 1 ? 0 : turn == 0 ? 1 : turn == 3 ? 2 : 3;
            if(rank >= bestRank)
                continue;

            bestRank = rank;
            bestIndex = i;
        }

        return bestIndex;
    }

    static int GetGridDirectionIndex(Vector2Int direction)
    {
        if(direction.x > 0)
            return 0;
        if(direction.y > 0)
            return 1;
        if(direction.x < 0)
            return 2;
        return 3;
    }

    static List<Vector2Int> SimplifyGridContour(List<Vector2Int> contour)
    {
        List<Vector2Int> simplified = new List<Vector2Int>();
        int count = contour.Count;

        for(int i = 0; i < count; i++)
        {
            Vector2Int previous = contour[(i - 1 + count) % count];
            Vector2Int current = contour[i];
            Vector2Int next = contour[(i + 1) % count];

            if(current - previous == next - current)
                continue;

            simplified.Add(current);
        }

        return simplified;
    }

    void CreateGridContourLine(List<Vector2Int> contour, int contourIndex)
    {
        if(contour.Count < 4)
            return;

        GameObject instance = Instantiate(gridPerimeterLinePrefab, _levelManager.GameField, false);
        instance.name = $"Grid Perimeter Line {contourIndex + 1}";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        LineRenderer lineRenderer = instance.GetComponentInChildren<LineRenderer>();
        if(lineRenderer == null)
        {
            Debug.LogWarning("Grid perimeter line prefab has no LineRenderer component.", this);
            Destroy(instance);
            return;
        }

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = contour.Count;
        lineRenderer.numCornerVertices = Mathf.Max(lineRenderer.numCornerVertices, 6);
        lineRenderer.numCapVertices = Mathf.Max(lineRenderer.numCapVertices, 4);
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 50;
        lineRenderer.widthMultiplier = Mathf.Max(lineRenderer.widthMultiplier, 0.24f);

        Material sharedMaterial = lineRenderer.sharedMaterial;
        if(sharedMaterial != null)
        {
            Material runtimeMaterial = new Material(sharedMaterial)
            {
                name = sharedMaterial.name + " (Grid Outline Instance)",
                renderQueue = 3000
            };

            runtimeMaterial.SetOverrideTag("RenderType", "Transparent");
            runtimeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            runtimeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            if(runtimeMaterial.HasProperty("_Surface"))
                runtimeMaterial.SetFloat("_Surface", 1f);
            if(runtimeMaterial.HasProperty("_Blend"))
                runtimeMaterial.SetFloat("_Blend", 0f);
            if(runtimeMaterial.HasProperty("_SrcBlend"))
                runtimeMaterial.SetFloat("_SrcBlend", 5f);
            if(runtimeMaterial.HasProperty("_DstBlend"))
                runtimeMaterial.SetFloat("_DstBlend", 10f);
            if(runtimeMaterial.HasProperty("_ZWrite"))
                runtimeMaterial.SetFloat("_ZWrite", 0f);

            lineRenderer.sharedMaterial = runtimeMaterial;
            _lineMaterials.Add(runtimeMaterial);
        }

        List<Vector3> positions = new List<Vector3>(contour.Count);
        for(int i = 0; i < contour.Count; i++)
            positions.Add(GridCornerToLocalPosition(contour[i]));

        lineRenderer.SetPositions(positions.ToArray());

        GridOutlineRevealAnimation reveal = instance.AddComponent<GridOutlineRevealAnimation>();
        reveal.Play(
            lineRenderer,
            positions,
            gridOutlineDrawDuration,
            contourIndex * gridOutlineLoopDelay);

        _lineInstances.Add(instance);
    }

    Vector3 GridCornerToLocalPosition(Vector2Int corner)
    {
        float x = _levelManager.firstSquarePosition.x + (corner.x - 0.5f) * _levelManager.squareWidth;
        float y = _levelManager.firstSquarePosition.y - (corner.y - 0.5f) * _levelManager.squareHeight;
        return new Vector3(x, y, gridPerimeterLineZOffset);
    }
}
