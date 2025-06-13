using System;

public static class EnumExtensions
{
    public static T RandomValue<T>(this T _) where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        int idx = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(idx);
    }
}

public enum StackErrors
{
    OnFire,
    Wet,
    Bent,
    TooHigh,
    TooDeep,
}

public enum EnclosureTestFailures
{
    CvmMissalignment,
    ValveLeak,
    CoolantEmpty,
    GasLeak,
}



