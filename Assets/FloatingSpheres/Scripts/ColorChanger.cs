using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Material target;
    public void UpdateOpacity(float alphaValue)
    {
        Color color = target.color;
        color.a = alphaValue;
        target.color = color;
    }
    public void UpdateBlue(float value)
    {
        Color color = target.color;
        color.b = value;
        target.color = color;
    }
}
