// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles using enchanters on altars to enchant items.
/// </summary>
public sealed class EnchanterSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;

    private List<EntProtoId<EnchantComponent>> _pool = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnchanterComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<EnchantingToolComponent, ExaminedEvent>(OnToolExamined);
        SubscribeLocalEvent<EnchantingToolComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnExamined(Entity<EnchanterComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("enchanter-examine"));
    }

    private void OnToolExamined(Entity<EnchantingToolComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("enchanting-tool-examine"));
    }

    private void OnBeforeInteract(Entity<EnchantingToolComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!args.CanReach || args.Target is not {} item)
            return;

        // do nothing if used without an altar
        if (_enchanting.FindTable(item) == null)
            return;

        args.Handled = true;

        // need an enchanter on the altar as well as the target
        var user = args.User;
        if (_enchanting.FindEnchanter(item) is not {} enchanter)
        {
            _popup.PopupClient(Loc.GetString("enchanting-tool-no-enchanter"), user, user);
            return;
        }

        TryEnchant(enchanter, item, user);
    }

    private void GetPossibleEnchants(Entity<EnchanterComponent> ent, EntityUid item)
    {
        _pool.Clear();
        foreach (var id in ent.Comp.Enchants)
        {
            if (_enchanting.CanEnchant(item, id))
                _pool.Add(id);
        }
    }

    /// <summary>
    /// Try to use an enchanter to add random enchant(s) to an item, deleting it if successful.
    /// </summary>
    public bool TryEnchant(Entity<EnchanterComponent> ent, Entity<EnchantedComponent?> item, EntityUid user)
    {
        GetPossibleEnchants(ent, item);
        if (_pool.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("enchanter-cant-enchant"), item, user);
            return false;
        }

        // can't predict any further due to rng + spawning
        if (_net.IsClient)
            return true;

        // pick a random enchant then do it
        var picking = _random.NextFloat(ent.Comp.MinCount, ent.Comp.MaxCount);
        var total = 0f;
        for (int i = 0; i < 20 && total < picking; i++)
        {
            var id = _random.Pick(_pool);
            var level = (int) _random.NextFloat(ent.Comp.MinLevel, ent.Comp.MaxLevel);
            if (_enchanting.Enchant(item, id, level))
                total += 1f;
        }

        _audio.PlayPvs(ent.Comp.Sound, item);
        _popup.PopupEntity(Loc.GetString("enchanter-enchanted", ("item", item)), item, PopupType.Large);

        _adminLogger.Add(LogType.EntityDelete, LogImpact.Low,
            $"{ToPrettyString(user):player} enchanted {ToPrettyString(item):item} using {ToPrettyString(ent):enchanter}");

        if (!TryComp<StackComponent>(ent, out var stack) || !_stack.Use(ent, 1, stack))
        {
            ent.Comp.Enchants = new(); // prevent double enchanting by malf client
            QueueDel(ent);
        }
        return true;
    }
}
