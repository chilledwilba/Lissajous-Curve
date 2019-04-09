using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LissajousCurveController : MonoBehaviour
{
    public Dictionary<string, Field> Map { get; set; } = new Dictionary<string, Field>();

    public UIFieldInt GridSize = new UIFieldInt("gridSize", 1, 50, 10);
    public FieldFloat TimeScale = new FieldFloat("timeScale", 0, 1, 0.5f);
    public FieldVector2 Position = new FieldVector2("position", -10, 10, new Vector2(2.5f, -2.5f));
    public FieldVector2 Angle = new FieldVector2("angle", 0, 360, new Vector2(0, 0));
    public FieldVector2 Radius = new FieldVector2("radius", 0, 5, new Vector2(1, 1));
    public FieldVector2 DotSize = new FieldVector2("dotSize", 0, 2, new Vector2(0.5f, 0.25f));
    public FieldFloat TrailLength = new FieldFloat("trailLength", 0, 50, 10);
    public FieldVector2 TrailWidth = new FieldVector2("trailWidth", 0, 2, new Vector2(0.075f, 0.05f));
    public FieldVector2 AxisMultiplier = new FieldVector2("axisMultiplier", 0, 10, new Vector2(1, 1));

    [Header("Trail Colors")]
    public bool updateGradient = false;
    public Gradient gradient;

    [Header("References")]
    public Camera cam;
    public GridLinesController gridLineController;

    [Header("Prefabs")]
    public Transform AxisPrefab;
    public Transform GridPrefab;

    // Lists
    Transform[,] axis;
    TrailRenderer[,] axisTrailRenderers;
    Transform[,] grid;
    TrailRenderer[,] gridTrailRenderers;

    // Circle Angle
    float angle = 0;

    // Detect State Change
    int _currentGridSize;
    Vector2 _position;
    Vector2 _pointSize;
    float _trailLength;
    Vector2 _trailWidth;
    Gradient _gradient = new Gradient();

    #region Start
    void Awake()
    {
        // Initialise Map
        Map.Add(GridSize.key, GridSize);
        Map.Add(TimeScale.key, TimeScale);
        Map.Add(Position.key, Position);
        Map.Add(Angle.key, Angle);
        Map.Add(Radius.key, Radius);
        Map.Add(DotSize.key, DotSize);
        Map.Add(TrailLength.key, TrailLength);
        Map.Add(TrailWidth.key, TrailWidth);
        Map.Add(AxisMultiplier.key, AxisMultiplier);

        // Initialise Arrays
        axis = new Transform[2, GridSize.max];
        axisTrailRenderers = new TrailRenderer[2, GridSize.max];
        grid = new Transform[GridSize.max, GridSize.max];
        gridTrailRenderers = new TrailRenderer[GridSize.max, GridSize.max];

        LoadAxis();
        LoadGrid();
    }

    void LoadAxis()
    {
        Transform shape;

        // X Axis
        Transform parent = new GameObject("X Axis").transform;
        for (int i = 0; i < GridSize.max; i++)
        {
            shape = Instantiate(AxisPrefab, parent);
            shape.name = string.Format("{0}{1}", "X", i);
            shape.gameObject.SetActive(false);
            axis[0, i] = shape;
            axisTrailRenderers[0, i] = shape.GetComponent<TrailRenderer>();
        }

        // Y Axis
        parent = new GameObject("Y Axis").transform;
        for (int i = 0; i < GridSize.max; i++)
        {
            shape = Instantiate(AxisPrefab, parent);
            shape.name = string.Format("{0}{1}", "Y", i);
            shape.gameObject.SetActive(false);
            axis[1, i] = shape;
            axisTrailRenderers[1, i] = shape.GetComponent<TrailRenderer>();
        }
    }

    void LoadGrid()
    {
        Transform shape;
        Transform parent = new GameObject("Grid").transform;
        string name;

        for (int x = 0; x < GridSize.max; x++)
        {
            name = string.Format("X{0}Y", x);
            for (int y = 0; y < GridSize.max; y++)
            {
                shape = Instantiate(GridPrefab, parent);
                shape.name = string.Format("{0}{1}", name, y);
                shape.gameObject.SetActive(false);
                grid[x, y] = shape;
                gridTrailRenderers[x, y] = shape.GetComponent<TrailRenderer>();
            }
        }
    }
    #endregion

    void Update()
    {
        angle += Time.deltaTime;
        Time.timeScale = TimeScale.value;

        // Key Inputs
        if (Input.GetKeyDown(KeyCode.Return)) Reload();
        else if (Input.GetKeyDown(KeyCode.Space)) ResetTrails();
        else if (Input.GetKeyDown(KeyCode.DownArrow)) GridSize.value = (int)Tools.ClampValues(GridSize.value - 1, GridSize.min, GridSize.max);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) GridSize.value = (int)Tools.ClampValues(GridSize.value + 1, GridSize.min, GridSize.max);
        else if (Input.GetKeyDown(KeyCode.Escape)) CloseApplication();
        else if (Input.GetKeyDown(KeyCode.F)) SwitchFullScreen();

        // Update Positions
        UpdateAxisPositions();
        UpdateGridPositions();
        gridLineController.UpdatePositions(axis);
    }

    void FixedUpdate()
    {
        // Resize Grid
        if (GridSize.value > _currentGridSize) UpdateGridSize(_currentGridSize, GridSize.value, true);
        else if (GridSize.value < _currentGridSize) UpdateGridSize(GridSize.value, _currentGridSize, false);

        // State Change
        if (_pointSize != DotSize.value) UpdateDotSize();
        if (_trailLength != TrailLength.value) UpdateTrailLength();
        if (_trailWidth != TrailWidth.value) UpdateTrailWidth();
        if (updateGradient && !Tools.CompareGradient(_gradient, gradient)) UpdateTrailColors();
        if (_position != Position.value) PositionChanged();
    }

    #region Update Positions
    void UpdateAxisPositions()
    {
        for (int i = 0; i < GridSize.value; i++)
        {
            Vector2 centre = new Vector2(Position.value.x * i, -Position.value.y);
            axis[0, i].position = centre + GetCirclePosition(Angle.value.x, i, AxisMultiplier.value.x) * Radius.value.x;

            centre = new Vector2(-Position.value.x, Position.value.y * i);
            axis[1, i].position = centre + GetCirclePosition(Angle.value.y, i, AxisMultiplier.value.y) * Radius.value.y;
        }
    }

    Vector2 GetCirclePosition(float angleOffset, int i, float multiplier)
    {
        float newAngle = angle * ((i + 1) * multiplier) + (angleOffset * Mathf.Deg2Rad);
        return new Vector2(Mathf.Sin(newAngle), Mathf.Cos(newAngle));
    }

    void UpdateGridPositions()
    {
        for (int x = 0; x < GridSize.value; x++)
        {
            for (int y = 0; y < GridSize.value; y++)
            {
                grid[x, y].position = new Vector2(axis[0, x].position.x, axis[1, y].position.y);
            }
        }
    }
    #endregion

    #region Resize Grid
    void UpdateGridSize(int a, int b, bool state)
    {
        // Set State - Axis
        AxisSetActive(a, b, state);

        // Set State - Shapes in alive columns
        GridSetActive(0, a, a, b, state);

        // Set State - Entire Columns
        GridSetActive(a, b, 0, b, state);

        // Set State - Grid Lines
        gridLineController.ResizeAxis(a, b, state);

        UpdateCamera();
        UpdateTrailColors();

        _currentGridSize = GridSize.value;
    }

    void AxisSetActive(int startIndex, int endIndex, bool state)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            axis[0, i].gameObject.SetActive(state);
            axis[1, i].gameObject.SetActive(state);
        }
    }

    void GridSetActive(int xs, int xe, int ys, int ye, bool state)
    {
        for (int x = xs; x < xe; x++)
        {
            for (int y = ys; y < ye; y++)
            {
                grid[x, y].gameObject.SetActive(state);
            }
        }
    }
    #endregion

    void UpdateCamera()
    {
        float xx = Position.value.x / 2;
        float yy = Position.value.y / 2;

        float x = -Position.value.x + (GridSize.value * xx);
        float y = -Position.value.y - (GridSize.value * -yy);

        float size = Mathf.Abs(xx) > Mathf.Abs(yy) ? Mathf.Abs(xx) : Mathf.Abs(yy);

        cam.orthographicSize = (GridSize.value * size) + size;
        cam.transform.position = new Vector3(x, y, -10);
    }

    void UpdateDotSize()
    {
        Vector3 scalex = Vector3.one * DotSize.value.x;
        Vector3 scaley = Vector3.one * DotSize.value.y;

        for (int x = 0; x < GridSize.max; x++)
        {
            axis[0, x].localScale = scalex;
            axis[1, x].localScale = scalex;

            for (int y = 0; y < GridSize.max; y++)
            {
                grid[x, y].localScale = scaley;
            }
        }
        _pointSize = DotSize.value;
    }

    #region Trail Renderer
    void UpdateTrailLength()
    {
        for (int i = 0; i < GridSize.max; i++)
        {
            axisTrailRenderers[0, i].time = TrailLength.value;
            axisTrailRenderers[1, i].time = TrailLength.value;

            for (int y = 0; y < GridSize.max; y++)
            {
                gridTrailRenderers[i, y].time = TrailLength.value;
            }
        }
        _trailLength = TrailLength.value;
    }

    void UpdateTrailWidth()
    {
        for (int i = 0; i < GridSize.max; i++)
        {
            axisTrailRenderers[0, i].widthMultiplier = TrailWidth.value.x;
            axisTrailRenderers[1, i].widthMultiplier = TrailWidth.value.x;

            for (int y = 0; y < GridSize.max; y++)
            {
                gridTrailRenderers[i, y].widthMultiplier = TrailWidth.value.y;
            }
        }
        _trailWidth = TrailWidth.value;
    }

    void UpdateTrailColors()
    {
        TrailRenderer tr;
        Color trailColor, xColor, yColor;

        float gradientStep = 1.0f / (GridSize.value - 1);

        for (int x = 0; x < GridSize.value; x++)
        {
            trailColor = gradient.Evaluate(gradientStep * x);

            tr = axisTrailRenderers[0, x];
            tr.startColor = tr.endColor = trailColor;

            tr = axisTrailRenderers[1, x];
            tr.startColor = tr.endColor = trailColor;

            // Set Verticle Grid Colors
            tr = gridTrailRenderers[x, x];
            tr.startColor = tr.endColor = trailColor;

            for (int y = x + 1; y < GridSize.value; y++)
            {
                xColor = gradient.Evaluate(gradientStep * x);
                yColor = gradient.Evaluate(gradientStep * y);
                trailColor = (xColor + yColor) / 2;

                tr = gridTrailRenderers[x, y];
                tr.startColor = tr.endColor = trailColor;

                tr = gridTrailRenderers[y, x];
                tr.startColor = tr.endColor = trailColor;
            }
        }

        _gradient.colorKeys = gradient.colorKeys;
        _gradient.alphaKeys = gradient.alphaKeys;
    }
    #endregion

    void PositionChanged()
    {
        // If Axis Direction Switched Update Grid line directions
        if ((_position.x >= 0 && Position.value.x <= 0) || (_position.x <= 0 && Position.value.x >= 0) ||
        (_position.y >= 0 && Position.value.y <= 0) || (_position.y <= 0 && Position.value.y >= 0))
        {
            gridLineController.UpdateRotation();
        }

        UpdateCamera();
        _position = Position.value;
    }

    #region Buttons
    public void SwitchFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetTrails()
    {
        for (int i = 0; i < GridSize.value; i++)
        {
            axisTrailRenderers[0, i].Clear();
            axisTrailRenderers[1, i].Clear();

            for (int y = 0; y < GridSize.value; y++)
            {
                gridTrailRenderers[i, y].Clear();
            }
        }
    }

    public void CloseApplication()
    {
        Application.Quit();
    }
    #endregion
}