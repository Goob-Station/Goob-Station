
namespace Content.Pirate.Client.UserInterface.TransitionText;
/// <summary>
/// Provides functions for creating transition effects for UI elements.
/// </summary>
public sealed partial class TransitionText
{
    public static bool DizzyEqualsF(float left, float right, float treshold = float.Epsilon)
    {
        return MathF.Abs(left - right) >= treshold;
    }
    /// <summary>
    /// Interpolates between start and target values linearly.
    /// </summary>
    /// <param name="startValue">Transition from this value.</param>
    /// <param name="targetValue">Transition to this value.</param>
    /// <param name="duration">Transition duration in seconds.</param>
    /// <param name="deltaT">Time since last frame in seconds.</param>
    /// <returns></returns>
    public static List<float> GetLinearFloatTransitionValuesList(float startValue, float targetValue, float duration, float deltaT)
    {
        if (duration == 0) return new([targetValue]);
        List<float> transitionList = new();
        var newValue = startValue;
        var delta = targetValue - startValue;
        double growth = delta / duration * deltaT;
        if (growth == 0)
            return new([targetValue]);
        while (DizzyEqualsF(targetValue, newValue, MathF.Abs((float) growth)))
        {
            newValue += (float) growth;
            transitionList.Add(MathF.Round(newValue, 2));
        }
        transitionList.Add(targetValue);
        return transitionList;
    }
}
