using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Oskarrr.Yautja.Components;

/// <summary>
/// Флакон: DoAfter нанесення, потім ціль розчиняється протягом <see cref="DissolveDuration"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaCleanserGelComponent : Component
{
    [DataField]
    public EntProtoId AshPrototype = "Ash";

    /// <summary>Візуальний гель на цілі (не підбирається).</summary>
    [DataField]
    public EntProtoId DissolveEffect = "GoobYautjaHealingGel";

    [DataField]
    public SoundSpecifier? DissolveSound =
        new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");

    /// <summary>Час нанесення гелю.</summary>
    [DataField]
    public TimeSpan ApplyDelay = TimeSpan.FromSeconds(1);

    /// <summary>Скільки тримається ефект перед перетворенням на попіл.</summary>
    [DataField]
    public TimeSpan DissolveDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public bool ConsumeOnUse = true;
}

/// <summary>
/// Ціль зараз розчиняється гелем — не можна підібрати, після <see cref="DissolveAt"/> стає попелом.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class YautjaDissolvingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId AshPrototype = "Ash";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan DissolveAt;

    [DataField, AutoNetworkedField]
    public EntityUid? EffectEntity;
}

/// <summary>Маркер візуального гелю — його не можна взяти в руки.</summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaHealingGelEffectComponent : Component;
