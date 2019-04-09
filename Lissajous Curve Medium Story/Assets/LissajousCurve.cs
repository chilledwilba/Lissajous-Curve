using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LissajousCurve : MonoBehaviour
{
    public int gridSize = 5;
    public Vector2 positionOffset = new Vector2(2.5f, 2.5f);
    public Transform dotPrefab;
    public Gradient gradient;

    float angle;

    Transform[] xAxis;
    Transform[] yAxis;
    Transform[,] gridShapes;

    void Start()
    {
        // Initialise Arrays
        xAxis = new Transform[gridSize];
        yAxis = new Transform[gridSize];
        gridShapes = new Transform[gridSize, gridSize];

        // Create Axis Dots
        for (int i = 0; i < gridSize; i++)
        {
            xAxis[i] = Instantiate(dotPrefab);
            yAxis[i] = Instantiate(dotPrefab);
        }

        // Create Grid Shape Dots
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                gridShapes[x, y] = Instantiate(dotPrefab);
            }
        }

        SetTrailRendererColors();
        SetCamera();
    }

    void Update()
    {
        angle += Time.deltaTime;

        LoadAxisPositions();
        LoadGridShapePositions();
    }

    void LoadAxisPositions()
    {
        for (int i = 0; i < gridSize; i++)
        {
            float newAngle = angle * (i + 1);
            Vector2 circlePosition = new Vector2(Mathf.Sin(newAngle), Mathf.Cos(newAngle));

            Vector2 centre = new Vector2(positionOffset.x * i, -positionOffset.y);
            xAxis[i].position = centre + circlePosition;

            centre = new Vector2(-positionOffset.x, positionOffset.y * i);
            yAxis[i].position = centre + circlePosition;
        }
    }

    void LoadGridShapePositions()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2 position = xAxis[x].position;
                position.y = yAxis[y].position.y;

                gridShapes[x, y].position = position;
            }
        }
    }

    void SetTrailRendererColors()
    {
        TrailRenderer tr;
        Color trailColor, xColor, yColor;

        float gradientStep = 1.0f / (gridSize - 1);

        for (int i = 0; i < gridSize; i++)
        {
            trailColor = gradient.Evaluate(gradientStep * i);

            tr = xAxis[i].GetComponent<TrailRenderer>();
            tr.startColor = tr.endColor = trailColor;

            tr = yAxis[i].GetComponent<TrailRenderer>();
            tr.startColor = tr.endColor = trailColor;
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                xColor = gradient.Evaluate(gradientStep * x);
                yColor = gradient.Evaluate(gradientStep * y);
                trailColor = (xColor + yColor) / 2;

                tr = gridShapes[x, y].GetComponent<TrailRenderer>();
                tr.startColor = tr.endColor = trailColor;
            }
        }
    }

    void SetCamera()
    {
        Camera cam = Camera.main;

        float xx = positionOffset.x / 2;
        float yy = positionOffset.y / 2;

        float x = -positionOffset.x + (gridSize * xx);
        float y = -positionOffset.y - (gridSize * -yy);

        float size = Mathf.Abs(xx) > Mathf.Abs(yy) ? Mathf.Abs(xx) : Mathf.Abs(yy);

        cam.orthographicSize = (gridSize * size) + size;
        cam.transform.position = new Vector3(x, y, -10);
    }
}
