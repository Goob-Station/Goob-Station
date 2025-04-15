// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Animations;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.SupermatterHalberd;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ActionTargetMarkSystem _mark = default!;
    [Dependency] private readonly RaysSystem _rays = default!;

    public event Action? StopTargeting;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardComponent, GetStatusIconsEvent>(GetWizardIcon);
        SubscribeLocalEvent<ApprenticeComponent, GetStatusIconsEvent>(GetApprenticeIcon);

        SubscribeNetworkEvent<StopTargetingEvent>(OnStopTargeting);
        SubscribeAllEvent<ChargeSpellRaysEffectEvent>(OnChargeEffect);
    }

    private void OnChargeEffect(ChargeSpellRaysEffectEvent ev)
    {
        var uid = GetEntity(ev.Uid);

        CreateChargeEffect(uid, ev);
    }

    protected override void CreateChargeEffect(EntityUid uid, ChargeSpellRaysEffectEvent ev)
    {
        if (!Timing.IsFirstTimePredicted || uid == EntityUid.Invalid)
            return;

        var rays = _rays.DoRays(TransformSystem.GetMapCoordinates(uid),
            Color.Yellow,
            Color.Fuchsia,
            10,
            15,
            minMaxRadius: new Vector2(3f, 6f),
            proto: "EffectRayCharge",
            server: false);

        if (rays == null)
            return;

        var track = EnsureComp<TrackUserComponent>(rays.Value);
        track.User = uid;
    }

    public void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if (target == null || user == target)
        {
            _mark.SetMark(null);
            RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), null));
            return;
        }

        _mark.SetMark(target);
        RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), GetNetEntity(target.Value)));
    }

    private void OnStopTargeting(StopTargetingEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession != _player.LocalSession)
            return;

        StopTargeting?.Invoke();
    }

    private void GetWizardIcon(Entity<WizardComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ProtoMan.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetApprenticeIcon(Entity<ApprenticeComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ProtoMan.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}