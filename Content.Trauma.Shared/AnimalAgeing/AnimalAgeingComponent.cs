using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.AnimalAgeing;

/// <summary>
/// Animals with this component will age up a mob a "year" each ageing update
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class AnimalAgeingComponent : Component
{
    [DataField]
    public int AdultHoodYear = 15;

    [DataField]
    public int SeniorHoodYear = 30;

    [DataField]
    public int DeathYear = 35;

    [DataField]
    public int YearsOld;

    /// <summary>
    ///     Minimum age time used.
    /// </summary>
    [DataField]
    public float AgeTimeMin = 20f;

    /// <summary>
    ///     Maximum age time used.
    /// </summary>
    [DataField]
    public float AgeTimeMax = 30f;

    [DataField]
    public int YearsPerUpdate = 1;

    [DataField]
    public AnimalAgeState CurrentAgeState = AnimalAgeState.Baby;

    [DataField, AutoPausedField]
    public TimeSpan NextAgeTime = TimeSpan.Zero;
}

public enum AnimalAgeState: byte
{
    Baby,
    Adult,
    Senior,
}
