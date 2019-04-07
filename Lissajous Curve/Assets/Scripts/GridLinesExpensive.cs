using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLinesExpensive : MonoBehaviour
{
    public LissajousCurveController controller;
    public Transform prefab;
    public Transform[,] Lines { get; set; }
    Transform[,,] dots;
    public int numDots = 100;

    Vector2[] positions;
    int positionsLength;

    #region Loading
    void Start()
    {
        positionsLength = numDots;
        positions = new Vector2[numDots];
        dots = new Transform[2,controller.GridSize.max, numDots];

        Lines = new Transform[2, controller.GridSize.max];

        // Load Lines
        Transform parent = new GameObject("Grid Lines Expensive").transform;
        parent.SetParent(transform);

        GenerateList(parent, "X", 0);
        GenerateList(parent, "Y", 1);
    }

    void GenerateList(Transform parent, string name, int axis)
    {
        Transform gridLine;
        for (int lineIndex = 0; lineIndex < controller.GridSize.max; lineIndex++)
        {
            gridLine = CreateLine(name, axis, lineIndex);
            gridLine.SetParent(parent);
            gridLine.gameObject.SetActive(false);
            Lines[axis, lineIndex] = gridLine;
        }
    }

    Transform CreateLine(string name, int axis, int lineIndex)
    {
        Transform gridLine = new GameObject(string.Format("{0}{1}", name, lineIndex)).transform;

        Transform dot;
        for (int dotIndex = 0; dotIndex < numDots; dotIndex++)
        {
            dot = Instantiate(prefab, gridLine);
            dot.gameObject.SetActive(false);
            dots[axis, lineIndex, dotIndex] = dot;
        }

        return gridLine;
    }
    #endregion

    public void CalculatePositions(float length, float spread)
    {
        Vector2 start = Vector2.zero;
        Vector2 end = new Vector2(length, 0);

        Vector2 point = start;
        Vector2 direction = (end - start).normalized;

        float lengthFromStartToEnd = (end - start).sqrMagnitude;
        float lengthFromPointToStart = (point - start).sqrMagnitude;

        int index = 0;
        while (lengthFromStartToEnd > lengthFromPointToStart && index < numDots)
        {
            positions[index] = point;
            point += (direction * spread);
            lengthFromPointToStart = (point - start).sqrMagnitude;
            index++;
        }

        positionsLength = index;
    }

    public void SetPositions(float size, float xDir, float yDir)
    {
        Vector3 scale = Vector3.one * size;

        // Set Active, Scale, Position, Direction
        for (int i = 0; i < positionsLength; i++)
        {
            for (int line = 0; line < controller.GridSize.max; line++)
            {
                Transform dot = dots[0, line, i];
                dot.gameObject.SetActive(true);
                dot.localScale = scale;
                dot.localPosition = positions[i] * yDir;

                dot = dots[1, line, i];
                dot.gameObject.SetActive(true);
                dot.localScale = scale;
                dot.localPosition = positions[i] * xDir;
            }
        }

        // Deactivate Dots
        for (int i = positionsLength; i < numDots; i++)
        {
            for (int line = 0; line < controller.GridSize.max; line++)
            {
                dots[0, line, i].gameObject.SetActive(false);
                dots[1, line, i].gameObject.SetActive(false);
            }
        }
    }
}
