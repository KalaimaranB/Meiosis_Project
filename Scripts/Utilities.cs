using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities 
{
    public static void AssignMaterialTransparency(Material targetMaterial, float targetAlpha)
    {
        Color newColor = targetMaterial.color;
        newColor.a = targetAlpha;
        targetMaterial.color = newColor;
        targetMaterial.SetColor("Transparent", newColor);
    }
}
