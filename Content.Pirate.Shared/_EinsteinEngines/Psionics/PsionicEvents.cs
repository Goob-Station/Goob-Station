namespace Content.Shared.Psionics;

/// <summary>
///     This event is raised whenever a psionic entity sets their casting stats(Amplification and Dampening), allowing other systems to modify the end result
///     of casting stat math. Useful if for example you want a species to have 5% higher Amplification overall. Or a drug inhibits total Dampening, etc.
/// </summary>
/// <param name="receiver"></param>
/// <param name="amplificationChangedAmount"></param>
/// <param name="dampeningChangedAmount"></param>
[ByRefEvent]
public record struct OnSetPsionicStatsEvent(float AmplificationChangedAmount, float DampeningChangedAmount);

[ByRefEvent]
public record struct OnMindbreakEvent();
