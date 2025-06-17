using System;
using System.Collections.Generic;

namespace Content.Pirate.Client.UserInterface.TransitionText;

public sealed partial class TransitionText
{
    public static bool DizzyEqualsF(float left, float right, float treshold = float.Epsilon)
    {
        return MathF.Abs(left - right) >= treshold;
    }
    public static List<float> GetLinearFloatTransitionEnumerator(float startValue, float targetValue, float duration, float deltaT)
    {
        if (duration == 0) return new([targetValue]);
        List<float> transitionList = new();
        var newValue = startValue;
        var delta = targetValue - startValue; // Delta
        double growth = delta / duration * deltaT; // How much to add each iteration
        if (growth == 0)
            return new([targetValue]);
        while (DizzyEqualsF(targetValue, newValue, (float) growth))
        {
            newValue += (float) growth;
            transitionList.Add(float.Round(newValue, 2));
        }
        transitionList.Add(targetValue);
        return transitionList;
    }
}