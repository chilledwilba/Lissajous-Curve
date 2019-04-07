using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public LissajousCurveController controller;
    public GridLinesController gridLineController;
    public Toggle gridLines_ShowLinesCheap_Toggle;
    public Toggle gridLines_ShowLinesExpensive_Toggle;

    #region Set Values
    private void Start()
    {
        foreach (var pair in controller.Map)
        {
            string key = pair.Key;
            Field value = pair.Value;

            if (value.GetType() == typeof(FieldFloat))
            {
                FieldFloat field = (FieldFloat)value;
                field.inputField.text = field.value.ToString();
                SetSliderValues(field.slider, field.min, field.max, field.value);
            }
            else if (value.GetType() == typeof(UIFieldInt))
            {
                UIFieldInt field = (UIFieldInt)value;
                field.inputField.text = field.value.ToString();
                SetSliderValues(field.slider, field.min, field.max, field.value);
            }
            else if (value.GetType() == typeof(FieldVector2))
            {
                FieldVector2 field = (FieldVector2)value;
                Vector2 v = field.value;

                field.inputField_x.text = v.x.ToString();
                field.inputField_y.text = v.y.ToString();

                SetSliderValues(field.slider_x, field.min, field.max, v.x);
                SetSliderValues(field.slider_y, field.min, field.max, v.y);
            }
        }

        gridLines_ShowLinesCheap_Toggle.isOn = gridLineController.showCheap;
        gridLines_ShowLinesExpensive_Toggle.isOn = gridLineController.showExpensive;
    }

    void SetSliderValues(Slider slider, float min, float max, float value)
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
    }
    #endregion

    #region Slider
    public void Slider(string key)
    {
        if (controller.Map.TryGetValue(key, out Field mapValue))
        {
            if (mapValue.GetType() == typeof(FieldFloat))
            {
                FieldFloat field = (FieldFloat)mapValue;
                SetSlider(field.slider.value, field.inputField, ref field.value);
            }
            else if (mapValue.GetType() == typeof(UIFieldInt))
            {
                UIFieldInt field = (UIFieldInt)mapValue;
                SetSlider(field.slider.value, field.inputField, ref field.value);
            }
            else if (mapValue.GetType() == typeof(FieldVector2))
            {
                FieldVector2 field = (FieldVector2)mapValue;
                SetSlider(field.slider_x.value, field.inputField_x, ref field.value.x);
                SetSlider(field.slider_y.value, field.inputField_y, ref field.value.y);
            }
        }
        else print(string.Format("Error key: {0}", key));
    }

    void SetSlider(float num, TMP_InputField inputField, ref float value)
    {
        inputField.text = num.ToString();
        value = num;
    }

    void SetSlider(float num, TMP_InputField inputField, ref int value)
    {
        inputField.text = num.ToString();
        value = (int)num;
    }
    #endregion

    #region InputField
    public void InputField(string key)
    {
        if (controller.Map.TryGetValue(key, out Field mapValue))
        {
            if (mapValue.GetType() == typeof(FieldFloat))
            {
                FieldFloat field = (FieldFloat)mapValue;
                SetInputField(field.inputField.text, field.min, field.max, field.slider, ref field.value);
            }
            else if (mapValue.GetType() == typeof(UIFieldInt))
            {
                UIFieldInt field = (UIFieldInt)mapValue;
                SetInputField(field.inputField.text, field.min, field.max, field.slider, ref field.value);
            }
            else if (mapValue.GetType() == typeof(FieldVector2))
            {
                FieldVector2 field = (FieldVector2)mapValue;
                SetInputField(field.inputField_x.text, field.min, field.max, field.slider_x, ref field.value.x);
                SetInputField(field.inputField_y.text, field.min, field.max, field.slider_y, ref field.value.y);
            }
        }
        else print(string.Format("Error key: {0}", key));
    }

    void SetInputField(string text, float min, float max, Slider slider, ref int value)
    {
        if (int.TryParse(text, out int num))
        {
            num = (int)Tools.ClampValues(num, min, max);
            slider.value = num;
            value = num;
        }
    }

    void SetInputField(string text, float min, float max, Slider slider, ref float value)
    {
        if (float.TryParse(text, out float num))
        {
            num = Tools.ClampValues(num, min, max);
            slider.value = num;
            value = num;
        }
    }
    #endregion

    public void EndEdit(string key)
    {
        if (controller.Map.TryGetValue(key, out Field mapValue))
        {
            if (mapValue.GetType() == typeof(FieldFloat))
            {
                FieldFloat field = (FieldFloat)mapValue;
                field.inputField.text = field.value.ToString();
            }
            else if (mapValue.GetType() == typeof(UIFieldInt))
            {
                UIFieldInt field = (UIFieldInt)mapValue;
                field.inputField.text = field.value.ToString();
            }
            else if (mapValue.GetType() == typeof(FieldVector2))
            {
                FieldVector2 field = (FieldVector2)mapValue;
                field.inputField_x.text = field.value.x.ToString();
                field.inputField_y.text = field.value.y.ToString();
            }
        }
        else print(string.Format("Error key: {0}", key));
    }

    public void GridSize_Add_Button(int i)
    {
        float num = controller.GridSize.value + i;
        num = Tools.ClampValues(num, controller.GridSize.min, controller.GridSize.max);

        controller.GridSize.inputField.text = num.ToString();
        controller.GridSize.slider.value = num;
        controller.GridSize.value = (int)num;
    }

    public void GridLines_ShowLines_Toggle()
    {
        gridLineController.showCheap = gridLines_ShowLinesCheap_Toggle.isOn;
        gridLineController.showExpensive = gridLines_ShowLinesExpensive_Toggle.isOn;
    }
}
