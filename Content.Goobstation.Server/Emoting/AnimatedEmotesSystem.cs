// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.Projectiles;
using Content.Server.Chat.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<AnimatedEmotesComponent, BorgFlippingEvent>(OnBeforeEmote);
    }

    private void OnEmote(Entity<AnimatedEmotesComponent> ent, ref EmoteEvent args)
    {
        PlayEmoteAnimation(ent, args.Emote.ID);
    }

    public void PlayEmoteAnimation(Entity<AnimatedEmotesComponent> ent, ProtoId<EmotePrototype> prot)
    {
        ent.Comp.Emote = prot;
        Dirty(ent);

        if (prot == "Flip")
            ApplyFlipEffects(ent);
    }

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BorgFlippingEvent args)
    {
        if (!_battery.TryGetBatteryComponent(ent, out var batteryComponent, out var battery))
        {
            args.BeforeEmote.Cancel();
            return;
        }
        var tenPercent = batteryComponent.MaxCharge * (args.Cost/100);
        if (batteryComponent.CurrentCharge < tenPercent * 0.50) // leeway on final flip so they can flip -> discharge.
        {
            args.BeforeEmote.Cancel();
            return;
        }
        _battery.UseCharge(battery.Value, tenPercent, batteryComponent);
    }
}
