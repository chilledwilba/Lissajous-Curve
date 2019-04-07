using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Field
{
    public string key;
}

[System.Serializable]
public class FieldFloat : Field
{
    public float min;
    public float max;
    public float value;
    public TMP_InputField inputField;
    public Slider slider;

    public FieldFloat(string key, float min, float max, float value)
    {
        this.key = key;
        this.min = min;
        this.max = max;
        this.value = value;
    }
}

[System.Serializable]
public class UIFieldInt : Field
{
    public int min;
    public int max;
    public int value;
    public TMP_InputField inputField;
    public Slider slider;

    public UIFieldInt(string key, int min, int max, int value)
    {
        this.key = key;
        this.min = min;
        this.max = max;
        this.value = value;
    }
}

[System.Serializable]
public class FieldVector2 : Field
{
    public float min;
    public float max;
    public Vector2 value;
    public TMP_InputField inputField_x;
    public Slider slider_x;
    public TMP_InputField inputField_y;
    public Slider slider_y;

    public FieldVector2(string key, int min, int max, Vector2 value)
    {
        this.key = key;
        this.min = min;
        this.max = max;
        this.value = value;
    }
}