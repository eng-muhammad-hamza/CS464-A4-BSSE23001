using UnityEngine;

public static class ColourUtil
{
    static float linear(float c) => c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);

    public static float luminance(Color c) => 0.2126f * linear(c.r) + 0.7152f * linear(c.g) + 0.0722f * linear(c.b);

    public static float contrast(Color a, Color b)
    {
        float la = luminance(a), lb = luminance(b);
        if (la < lb) (la, lb) = (lb, la);
        return (la + 0.05f) / (lb + 0.05f);
    }

    public static string hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
}
