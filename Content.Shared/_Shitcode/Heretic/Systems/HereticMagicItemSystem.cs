// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Inventory;

namespace Content.Shared.Heretic.Systems;

public sealed class HereticMagicItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticMagicItemComponent, CheckMagicItemEvent>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, HeldRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, ExaminedEvent>(OnMagicItemExamine);
    }

    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref CheckMagicItemEvent args)
        => args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref HeldRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;

    private void OnMagicItemExamine(Entity<HereticMagicItemComponent> ent, ref ExaminedEvent args)
    {
        if (!_heretic.TryGetHereticComponent(args.Examiner, out _, out _))
            return;

        args.PushMarkup(Loc.GetString("heretic-magicitem-examine"));
    }
}
