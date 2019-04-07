using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static float ClampValues(float value, float min, float max)
    {
        if (value > max) value = max;
        else if (value < min) value = min;
        return value;
    }

    public static bool CompareGradient(Gradient gradient, Gradient otherGradient)
    {
        // Compare the lengths before checking actual colors and alpha components
        if (gradient.colorKeys.Length != otherGradient.colorKeys.Length || gradient.alphaKeys.Length != otherGradient.alphaKeys.Length)
        {
            return false;
        }

        // Compare all the colors
        for (int i = 0; i < gradient.colorKeys.Length; i++)
        {
            // Test if the color and alpha is the same
            GradientColorKey key = gradient.colorKeys[i];
            GradientColorKey otherKey = otherGradient.colorKeys[i];
            if (key.color != otherKey.color || key.time != otherKey.time)
            {
                return false;
            }
        }

        // Compare all the alphas
        for (int i = 0; i < gradient.alphaKeys.Length; i++)
        {
            // Test if the color and alpha is the same
            GradientAlphaKey key = gradient.alphaKeys[i];
            GradientAlphaKey otherKey = otherGradient.alphaKeys[i];
            if (key.alpha != otherKey.alpha || key.time != otherKey.time)
            {
                return false;
            }
        }

        // They're the same
        return true;
    }

    /// Compares the two gradients by testing points at a given interval, note this does not detect when new nodes are added 
    public static bool QuickCompareGradient(this Gradient gradient, Gradient otherGradient, int testInterval = 3)
    {
        // Tests the gradient at a couple points to see if they are the same, may fail is various cases
        for (int i = 0; i < testInterval; i++)
        {
            float time = (float)i / (float)(testInterval - 1);
            if (gradient.Evaluate(time) != otherGradient.Evaluate(time))
            {
                return false;
            }
        }

        // All the test points match
        return true;
    }
}
