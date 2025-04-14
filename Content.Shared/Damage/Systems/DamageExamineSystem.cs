// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 KrasnoshchekovPavel <119816022+KrasnoshchekovPavel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Preston Smith <Blackfoot03@outlook.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq; // Goobstation Change

namespace Content.Shared.Damage.Systems;

public sealed class DamageExamineSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageExaminableComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnGetExamineVerbs(EntityUid uid, DamageExaminableComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var ev = new DamageExamineEvent(new FormattedMessage(), args.User);
        RaiseLocalEvent(uid, ref ev);
        if (!ev.Message.IsEmpty)
        {
            _examine.AddDetailedExamineVerb(args, component, ev.Message,
                Loc.GetString("damage-examinable-verb-text"),
                "/Textures/Interface/VerbIcons/smite.svg.192dpi.png",
                Loc.GetString("damage-examinable-verb-message")
            );
        }
    }

    public void AddDamageExamine(FormattedMessage message, DamageSpecifier damageSpecifier, string? type = null)
    {
        var markup = GetDamageExamine(damageSpecifier, type);
        if (!message.IsEmpty)
        {
            message.PushNewline();
        }
        message.AddMessage(markup);
    }

    /// <summary>
    /// Retrieves the damage examine values.
    /// </summary>
    private FormattedMessage GetDamageExamine(DamageSpecifier damageSpecifier, string? type = null)
    {
        var msg = new FormattedMessage();

        if (string.IsNullOrEmpty(type))
        {
            msg.AddMarkupOrThrow(Loc.GetString("damage-examine"));
        }
        else
        {
            if (damageSpecifier.GetTotal() == FixedPoint2.Zero && !damageSpecifier.AnyPositive())
            {
                msg.AddMarkupOrThrow(Loc.GetString("damage-none"));
                return msg;
            }

            msg.AddMarkupOrThrow(Loc.GetString("damage-examine-type", ("type", type)));
        }

        foreach (var damage in damageSpecifier.DamageDict)
        {
            if (damage.Value != FixedPoint2.Zero)
            {
                msg.PushNewline();
                msg.AddMarkupOrThrow(Loc.GetString("damage-value", ("type", _prototype.Index<DamageTypePrototype>(damage.Key).LocalizedName), ("amount", damage.Value)));
            }
        }

        // Goobstation Change
        var meaningfulDamage = GetTotalMeaningfulDamage(damageSpecifier);
        if (meaningfulDamage > 0)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString("damage-hits-to-kill", ("count", (100f / (float) meaningfulDamage).ToString("F1"))));
        }

        return msg;
    }

    // Goobstation Change - Fetches all of the damage that could kill a normal player entity, ignoring helper types.
    private FixedPoint2 GetTotalMeaningfulDamage(DamageSpecifier damageSpecifier)
    {
        var ignoredKeys = new[] { "Structural", "Asphyxiation", "Bloodloss" };
        var total = FixedPoint2.Zero;
        foreach (var (key, value) in damageSpecifier.DamageDict)
        {
            if (ignoredKeys.Contains(key))
                continue;

            total += value;
        }
        return total;
    }
}
