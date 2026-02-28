// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 MoutardOMiel <108993081+Moutardomiel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Emoting;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationEmoteEvent : EntityEventArgs
{
    [DataField]
    public virtual bool CausesVomit { get; set; }
}

[Serializable, NetSerializable]
public sealed partial class AnimationFlipEmoteEvent : AnimationEmoteEvent
{
    public override bool CausesVomit { get; set; } = true;
}

[Serializable, NetSerializable]
public sealed partial class AnimationSpinEmoteEvent : AnimationEmoteEvent
{
    public override bool CausesVomit { get; set; } = true;
}

[Serializable, NetSerializable]
public sealed partial class AnimationJumpEmoteEvent : AnimationEmoteEvent
{
    public override bool CausesVomit { get; set; } = true;
}

[Serializable, NetSerializable]
public sealed partial class AnimationTweakEmoteEvent : AnimationEmoteEvent;

[Serializable, NetSerializable]
public sealed partial class AnimationFlexEmoteEvent : AnimationEmoteEvent;

[RegisterComponent, NetworkedComponent]
public sealed partial class AnimatedEmotesComponent : Component
{
    [DataField]
    public ProtoId<EmotePrototype>? Emote;

    [DataField]
    public EntProtoId VomitStatus = "EmoteVomitCounterStatusEffect";

    [DataField]
    public EntProtoId BlockVomitEmoteStatus = "BlockVomitEmotesStatusEffect";

    [DataField]
    public TimeSpan VomitStatusTime = TimeSpan.FromSeconds(1);

    [DataField]
    public int EmotesToVomit = 5;

    [DataField]
    public TimeSpan BlockVomitStatusTime = TimeSpan.FromSeconds(10);
}

[Serializable, NetSerializable] public sealed partial class AnimatedEmotesComponentState : ComponentState
{
    public ProtoId<EmotePrototype>? Emote;

    public AnimatedEmotesComponentState(ProtoId<EmotePrototype>? emote)
    {
        Emote = emote;
    }
}
