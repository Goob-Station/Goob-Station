// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlaughterDemon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ReagentCrawlComponent : Component
{
    /// <summary>
    /// This is the search range of the blood puddles
    /// </summary>
    [DataField]
    public float SearchRange = 0.1f;

    /// <summary>
    /// This is the entity action cooldown of this ability. Prevents spamming it.
    /// </summary>
    [DataField]
    public TimeSpan ActionCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// This is the EntProtoId of the ability.
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "BloodCrawlAction";

    /// <summary>
    /// This is the entity of the abilities action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity = EntityUid.Invalid;

    /// <summary>
    /// This is the polymorph this ability uses.
    /// </summary>
    [DataField]
    public ProtoId<PolymorphPrototype> Jaunt = "BloodCrawlJaunt";

    /// <summary>
    /// Message sent when failing to enter the jaunt.
    /// </summary>
    [DataField]
    public LocId EnterJauntFailMessage = "slaughter-blood-jaunt-fail";

    /// <summary>
    /// This indicates whether the entity is crawling, or not. Used for toggling the ability.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsCrawling;

    /// <summary>
    /// The reagents to look out for when searching for puddles
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<ReagentPrototype>> TargetReagent  = [];

    /// <summary>
    /// The sound to play once entering the jaunt
    /// </summary>
    [DataField]
    public SoundPathSpecifier? EnterJauntSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/enter_blood.ogg");

    /// <summary>
    /// The sound to play once exiting the jaunt
    /// </summary>
    [DataField]
    public SoundPathSpecifier? ExitJauntSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/exit_blood.ogg");

    /// <summary>
    ///  The required amount required for a puddle to have in order for the jaunt to activate
    /// </summary>
    [DataField]
    public FixedPoint2 RequiredReagentAmount = 0.5;
}
