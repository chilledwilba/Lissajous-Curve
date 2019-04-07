using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLinesCheap : MonoBehaviour
{
    public LissajousCurveController controller;
    public Transform prefab;
    public Transform[,] Lines { get; set; }

    #region Load
    void Start()
    {
        Lines = new Transform[2, controller.GridSize.max];

        // Load Lines
        Transform parent = new GameObject("Grid Lines Cheap").transform;
        parent.SetParent(transform);

        GenerateList(parent, "X", 0);
        GenerateList(parent, "Y", 1);
    }

    void GenerateList(Transform parent, string name, int axis)
    {
        Transform shape;
        for (int lineIndex = 0; lineIndex < controller.GridSize.max; lineIndex++)
        {
            shape = Instantiate(prefab, parent);
            shape.name = string.Format("{0}{1}", name, lineIndex);
            shape.gameObject.SetActive(false);
            Lines[axis, lineIndex] = shape;
        }
    }
    #endregion

    public void SetSize(float size)
    {
        Vector3 scale = Vector3.one * size;

        for (int i = 0; i < controller.GridSize.max; i++)
        {
            Lines[0, i].localScale = scale;
            Lines[1, i].localScale = scale;
        }
    }

    public void SetLength(float length, float xDir, float yDir)
    {
        int maxLength = 10000;

        float xlength = Tools.ClampValues(length * xDir, -maxLength, maxLength);
        float ylength = Tools.ClampValues(length * yDir, -maxLength, maxLength);

        Vector2 xSize = Lines[0, 0].GetComponent<SpriteRenderer>().size;
        Vector2 ySize = xSize;

        xSize.x = xlength;
        ySize.x = ylength;

        for (int i = 0; i < controller.GridSize.max; i++)
        {
            Lines[0, i].GetComponent<SpriteRenderer>().size = ySize;
            Lines[1, i].GetComponent<SpriteRenderer>().size = xSize;
        }
    }
}
