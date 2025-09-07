// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared.Actions;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed class CrawlUnderFloorSystem : SharedCrawlUnderFloorSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlUnderFloorComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, CrawlUnderFloorComponent component, ComponentInit args)
    {
        if (component.ToggleHideAction == null)
            _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
    }

    protected override void SetCrawlAppearance(EntityUid uid, bool enabled)
    {
        if (TryComp<AppearanceComponent>(uid, out var app))
            _appearance.SetData(uid, SneakMode.Enabled, enabled, app);
    }

    protected override void OnEnteredSubfloor(EntityUid uid)
    {
        if (!_random.Prob(0.3f))
            return;

        var idx = _random.Next(1, 8);
        var path = $"/Audio/_Goobstation/FloorGoblin/duende-0{idx}.ogg";
        _audio.PlayPvs(new SoundPathSpecifier(path), uid);
    }
}
