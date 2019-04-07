using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLinesController : MonoBehaviour
{
    public FieldFloat GridLinesSize = new FieldFloat("gridLinesCheapSize", 0, 1, 0.15f);
    public FieldFloat GridLinesLength = new FieldFloat("gridLinesCheapLength", 1, 255, 100);
    public FieldFloat GridLinesSpread = new FieldFloat("gridLinesExpensiveSpread", 0, 5, 2);

    [Header("References")]
    public LissajousCurveController controller;
    public GridLinesCheap cheap;
    public GridLinesExpensive expensive;

    [Header("Show Lines")]
    public bool showCheap = false;
    public bool showExpensive = false;

    // Detect State Change
    float _size;
    float _length;
    float _spread;
    bool _showCheap;
    bool _showExpensive;

    private void Start()
    {
        controller.Map.Add(GridLinesSize.key, GridLinesSize);
        controller.Map.Add(GridLinesLength.key, GridLinesLength);
        controller.Map.Add(GridLinesSpread.key, GridLinesSpread);

        // Set Initial Rotation
        SetAxisRotation(cheap.Lines, 0, 90);
        SetAxisRotation(expensive.Lines, 0, 90);
    }

    void FixedUpdate()
    {
        // Grid Lines Turn On/Off
        if (_showCheap != showCheap)
        {
            SetActive(0, controller.GridSize.value, showCheap, cheap.Lines);
            _showCheap = showCheap;
        }
        if (_showExpensive != showExpensive)
        {
            SetActive(0, controller.GridSize.value, showExpensive, expensive.Lines);
            _showExpensive = showExpensive;
        }

        // State Change
        if (_size != GridLinesSize.value)
        {
            cheap.SetSize(GridLinesSize.value);
            ExpensiveSetSizeLengthSpread();
            _size = GridLinesSize.value;
        }
        if (_length != GridLinesLength.value)
        {
            CheapSetLength();
            ExpensiveSetSizeLengthSpread();
            _length = GridLinesLength.value;
        }
        if (_spread != GridLinesSpread.value)
        {
            ExpensiveSetSizeLengthSpread();
            _spread = GridLinesSpread.value;
        }
    }

    #region State Change
    void CheapSetLength()
    {
        // Get Direction
        float x = Mathf.Sign(controller.Position.value.x);
        float y = Mathf.Sign(controller.Position.value.y);

        float length = GridLinesLength.value / GridLinesSize.value;

        cheap.SetLength(length, x, y);
    }

    void ExpensiveSetSizeLengthSpread()
    {
        // Get Direction
        float x = Mathf.Sign(controller.Position.value.x);
        float y = Mathf.Sign(controller.Position.value.y);

        float spread = GridLinesSize.value * GridLinesSpread.value;

        expensive.CalculatePositions(GridLinesLength.value, spread);
        expensive.SetPositions(GridLinesSize.value, x, y);
    }
    #endregion

    #region Update Positions
    public void UpdatePositions(Transform[,] positions)
    {
        if (showCheap)
        {
            float offset = GridLinesSize.value / 2;
            UpdatePositions(cheap.Lines, positions, new Vector2(-offset, offset));
        }
        if (showExpensive)
        {
            UpdatePositions(expensive.Lines, positions, Vector2.zero);
        }
    }

    void UpdatePositions(Transform[,] lines, Transform[,] positions, Vector3 offset)
    {
        Vector3 position;
        for (int i = 0; i < controller.GridSize.value; i++)
        {
            // X Axis
            position = positions[0, i].position;
            position.y += offset.y;
            lines[0, i].position = position;

            // Y Axis
            position = positions[1, i].position;
            position.x += offset.x;
            lines[1, i].position = position;
        }
    }
    #endregion

    #region Rotation
    public void UpdateRotation()
    {
        CheapSetLength();
        ExpensiveSetSizeLengthSpread();
    }

    void SetAxisRotation(Transform[,] lines, int axis, float rotation)
    {
        for (int i = 0; i < controller.GridSize.max; i++)
        {
            lines[axis, i].rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }
    }
    #endregion

    #region Grid Lines Turn On/Off - Resize Grid
    public void ResizeAxis(int startIndex, int endIndex, bool state)
    {
        if (showCheap) SetActive(startIndex, endIndex, state, cheap.Lines);
        if (showExpensive) SetActive(startIndex, endIndex, state, expensive.Lines);
    }

    void SetActive(int startIndex, int endIndex, bool state, Transform[,] lines)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            lines[0, i].gameObject.SetActive(state);
            lines[1, i].gameObject.SetActive(state);
        }
    }
    #endregion
}
