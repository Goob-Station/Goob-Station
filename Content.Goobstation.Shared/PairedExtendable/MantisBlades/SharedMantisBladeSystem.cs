// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.Item;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.PairedExtendable;

// This is hardcoded, but it's not like this is going to be used anywhere else right?
public sealed class SharedMantisBladeSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MantisBladeArmComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInit(Entity<MantisBladeComponent> ent, ref ComponentInit args)
    {
        _item.SetHeldPrefix(ent, "popout");

        Timer.Spawn(TimeSpan.FromSeconds(0.3),
            () =>
            {
            if (!Deleted(ent))
                _item.SetHeldPrefix(ent, null);
            });
    }

    private void OnExamined(EntityUid uid, MantisBladeArmComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mantis-blade-arm-examine"));
    }
}
