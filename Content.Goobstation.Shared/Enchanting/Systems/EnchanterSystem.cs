// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

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
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private List<EntProtoId<EnchantComponent>> _pool = new();
    private EntityQuery<CanEnchantComponent> _userQuery;
    private EntityQuery<EnchantComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _userQuery = GetEntityQuery<CanEnchantComponent>();
        _query = GetEntityQuery<EnchantComponent>();

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

        if (_userQuery.HasComp(user) == false)
        {
            _popup.PopupClient(Loc.GetString("enchanter-disallowed-enchant"), user, user);
            return;
        }

        if (_enchanting.FindEnchanter(item) is { } enchanter)
            TryEnchant(enchanter, item, user);
        else if (_enchanting.FindEnchantedItems(item)
                     .FirstOrNull(x =>
                         x.Owner != item && x.Comp.Enchants.Any(e => _query.CompOrNull(e)?.Fake is true)) is { } source)
            TransferEnchantments(source, item, user);
        else
            _popup.PopupClient(Loc.GetString("enchanting-tool-no-enchanter"), user, user);
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
    /// Transfers fake enchantments on one item into real enchantments on other item
    /// </summary>
    public bool TransferEnchantments(Entity<EnchantedComponent> source, EntityUid item, EntityUid user)
    {
        var list = source.Comp.Enchants.Where(x => _query.Comp(x).Fake).ToList();

        if (list.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("enchanter-cant-enchant"), item, user);
            return false;
        }

        if (_net.IsClient)
            return true;

        _random.Shuffle(list);

        var added = !EnsureComp<EnchantedComponent>(item, out var comp);
        var oldTier = comp.Tier;
        var newTier = Math.Max(oldTier, source.Comp.TierOnTransferSuccess);
        _enchanting.SetTier((item, comp), newTier);
        var success = false;
        var enchantsToDelete = new List<EntityUid>();

        foreach (var uid in list)
        {
            var enchant = _query.Comp(uid);

            if (!enchant.Fake)
                continue;

            if (!_enchanting.CanEnchant((item, comp), enchant, Name(uid)))
            {
                enchantsToDelete.Add(uid);
                continue;
            }

            if (_enchanting.FindEnchant(comp, Name(uid)) is { } existing)
            {
                if (_enchanting.TryUpgradeEnchant(existing, item, enchant.Level, false))
                    success = true;

                enchantsToDelete.Add(uid);
                continue;
            }

            if (!_container.Insert(uid, comp.Container, force: true))
            {
                enchantsToDelete.Add(uid);
                continue;
            }

            _enchanting.AddEnchant((uid, enchant), item, enchant.Level, false);
            success = true;
        }

        var realTier = comp.Enchants.Count;
        if (realTier < newTier)
            _enchanting.SetTier((item, comp), Math.Max(oldTier, realTier));

        if (success)
        {
            foreach (var e in enchantsToDelete)
            {
                Del(e);
            }

            _audio.PlayPvs(source.Comp.Sound, item);
            _popup.PopupEntity(Loc.GetString("enchanter-enchanted", ("item", item)), item, PopupType.Large);

            _adminLogger.Add(LogType.EntityDelete,
                LogImpact.Low,
                $"{ToPrettyString(user):player} enchanted {ToPrettyString(item):item} using {ToPrettyString(source):enchanter}");

            if (source.Comp.Enchants.Any(Exists))
                return true;

            if (source.Comp.DeleteOnEnchantTransfer)
                QueueDel(source.Owner);
            else
                RemCompDeferred(source.Owner, source.Comp);

            return true;
        }

        if (added)
            RemComp<EnchantedComponent>(item);

        _popup.PopupEntity(Loc.GetString("enchanter-cant-enchant"), item, user);
        return false;
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
