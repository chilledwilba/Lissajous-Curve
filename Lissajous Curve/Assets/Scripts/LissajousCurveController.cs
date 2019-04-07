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
    public FieldVector2 PointSize = new FieldVector2("pointSize", 0, 2, new Vector2(0.5f, 0.75f));
    public FieldFloat TrailLength = new FieldFloat("trailLength", 0, 10, 7);
    public FieldVector2 TrailWidth = new FieldVector2("trailWidth", 0, 2, new Vector2(0.075f, 0.05f));

    [Header("Trail Colors")]
    public bool updateGradient = false;
    public Gradient gradient;

    [Header("References")]
    public Camera cam;
    public GridLinesController gridLineController;

    [Header("Prefabs")]
    public Transform AxisPrefab;
    public Transform DerivedShapePrefab;

    // Lists
    Transform[,] axis;
    TrailRenderer[,] axisTrailRenderers;
    Transform[,] derivedShapesMatrix;
    TrailRenderer[,] derivedShapesMatrixTrailRenderers;

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
        Map.Add(PointSize.key, PointSize);
        Map.Add(TrailLength.key, TrailLength);
        Map.Add(TrailWidth.key, TrailWidth);

        // Initialise Arrays
        axis = new Transform[2, GridSize.max];
        axisTrailRenderers = new TrailRenderer[2, GridSize.max];
        derivedShapesMatrix = new Transform[GridSize.max, GridSize.max];
        derivedShapesMatrixTrailRenderers = new TrailRenderer[GridSize.max, GridSize.max];

        LoadAxisShapes();
        LoadDerivedShapes();
    }

    void LoadAxisShapes()
    {
        // X Axis
        Transform parent = new GameObject("X Axis").transform;
        GenerateList(AxisPrefab, parent, "X", Vector3.zero, 0, axis, axisTrailRenderers);

        // Y Axis
        parent = new GameObject("Y Axis").transform;
        GenerateList(AxisPrefab, parent, "Y", Vector3.zero, 1, axis, axisTrailRenderers);
    }

    void LoadDerivedShapes()
    {
        Transform parent = new GameObject("Derived Shapes").transform;
        string name;
        int arrayIndex;

        for (int i = 0; i < GridSize.max; i++)
        {
            name = string.Format("X{0}Y", i);
            arrayIndex = i;
            GenerateList(DerivedShapePrefab, parent, name, Vector3.zero, arrayIndex, derivedShapesMatrix, derivedShapesMatrixTrailRenderers);
        }
    }

    void GenerateList(Transform prefab, Transform parent, string name, Vector3 rotation, int arrayIndex, Transform[,] shapeList, TrailRenderer[,] trailRendererList)
    {
        Transform shape;
        for (int i = 0; i < GridSize.max; i++)
        {
            shape = Instantiate(prefab, parent);
            shape.name = string.Format("{0}{1}", name, i);
            shape.gameObject.SetActive(false);
            shape.rotation = Quaternion.Euler(rotation);
            shapeList[arrayIndex, i] = shape;
            if (trailRendererList != null) trailRendererList[arrayIndex, i] = shape.GetComponent<TrailRenderer>();
        }
    }
    #endregion

    void Update()
    {
        angle += Time.deltaTime;
        Time.timeScale = TimeScale.value;

        // Key Inputs
        if (Input.GetKeyDown(KeyCode.Return)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else if (Input.GetKeyDown(KeyCode.Space)) ResetTrails();
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) GridSize.value = (int)Tools.ClampValues(GridSize.value - 1, GridSize.min, GridSize.max);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) GridSize.value = (int)Tools.ClampValues(GridSize.value + 1, GridSize.min, GridSize.max);
        else if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        // Update Positions
        UpdateAxisPositions();
        UpdateDerivedPositions();
        gridLineController.UpdatePositions(axis);
    }

    void FixedUpdate()
    {
        // Resize Grid
        if (GridSize.value > _currentGridSize) UpdateGridSize(_currentGridSize, GridSize.value, true);
        else if (GridSize.value < _currentGridSize) UpdateGridSize(GridSize.value, _currentGridSize, false);

        // State Change
        if (_pointSize != PointSize.value) UpdatePointSize();
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
            SetAxisPosition(axis[0, i], centre, Radius.value.x, Angle.value.x, i);

            centre = new Vector2(-Position.value.x, Position.value.y * i);
            SetAxisPosition(axis[1, i], centre, Radius.value.y, Angle.value.y, i);
        }
    }

    void SetAxisPosition(Transform shape, Vector2 centre, float radius, float angleOffset, int index)
    {
        float newAngle = angle * (index + 1);
        float offset = angleOffset * Mathf.Deg2Rad;
        newAngle += offset;
        Vector2 circlePosition = new Vector2(Mathf.Sin(newAngle), Mathf.Cos(newAngle)) * radius;
        shape.position = centre + circlePosition;
    }

    void UpdateDerivedPositions()
    {
        for (int x = 0; x < GridSize.value; x++)
        {
            for (int y = 0; y < GridSize.value; y++)
            {
                derivedShapesMatrix[x, y].position = new Vector2(axis[0, x].position.x, axis[1, y].position.y);
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
        DerivedShapesMatrixSetActive(0, a, a, b, state);

        // Set State - Entire Columns
        DerivedShapesMatrixSetActive(a, b, 0, b, state);

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

    void DerivedShapesMatrixSetActive(int xs, int xe, int ys, int ye, bool state)
    {
        for (int x = xs; x < xe; x++)
        {
            for (int y = ys; y < ye; y++)
            {
                derivedShapesMatrix[x, y].gameObject.SetActive(state);
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

    void UpdatePointSize()
    {
        Vector3 scalex = Vector3.one * PointSize.value.x;
        Vector3 scaley = Vector3.one * PointSize.value.y;

        for (int x = 0; x < GridSize.max; x++)
        {
            axis[0, x].localScale = scalex;
            axis[1, x].localScale = scalex;

            for (int y = 0; y < GridSize.max; y++)
            {
                derivedShapesMatrix[x, y].localScale = scaley;
            }
        }
        _pointSize = PointSize.value;
    }

    void UpdateTrailLength()
    {
        for (int i = 0; i < GridSize.max; i++)
        {
            axisTrailRenderers[0, i].time = TrailLength.value;
            axisTrailRenderers[1, i].time = TrailLength.value;

            for (int y = 0; y < GridSize.max; y++)
            {
                derivedShapesMatrixTrailRenderers[i, y].time = TrailLength.value;
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
                derivedShapesMatrixTrailRenderers[i, y].widthMultiplier = TrailWidth.value.y;
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

            // Set Verticle Derived Colors
            tr = derivedShapesMatrixTrailRenderers[x, x];
            tr.startColor = tr.endColor = trailColor;

            for (int y = x + 1; y < GridSize.value; y++)
            {
                xColor = gradient.Evaluate(gradientStep * x);
                yColor = gradient.Evaluate(gradientStep * y);
                trailColor = (xColor + yColor) / 2;

                tr = derivedShapesMatrixTrailRenderers[x, y];
                tr.startColor = tr.endColor = trailColor;

                tr = derivedShapesMatrixTrailRenderers[y, x];
                tr.startColor = tr.endColor = trailColor;
            }
        }

        _gradient.colorKeys = gradient.colorKeys;
        _gradient.alphaKeys = gradient.alphaKeys;
    }

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

    public void ResetTrails()
    {
        for (int i = 0; i < GridSize.value; i++)
        {
            axisTrailRenderers[0, i].Clear();
            axisTrailRenderers[1, i].Clear();

            for (int y = 0; y < GridSize.value; y++)
            {
                derivedShapesMatrixTrailRenderers[i, y].Clear();
            }
        }
    }

    public void CloseApplication()
    {
        Application.Quit();
    }
}