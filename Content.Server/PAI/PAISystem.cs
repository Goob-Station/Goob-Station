// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 hubismal <47284081+hubismal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Instruments;
using Content.Server.Kitchen.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind.Components;
using Content.Shared.PAI;
using Content.Shared.Popups;
using Robust.Shared.Random;
using System.Text;

namespace Content.Server.PAI;

public sealed class PAISystem : SharedPAISystem
{
    [Dependency] private readonly InstrumentSystem _instrumentSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ToggleableGhostRoleSystem _toggleableGhostRole = default!;

    /// <summary>
    /// Possible symbols that can be part of a scrambled pai's name.
    /// </summary>
    private static readonly char[] SYMBOLS = new[] { '#', '~', '-', '@', '&', '^', '%', '$', '*', ' '};

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PAIComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PAIComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<PAIComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<PAIComponent, BeingMicrowavedEvent>(OnMicrowaved);
    }

    private void OnUseInHand(EntityUid uid, PAIComponent component, UseInHandEvent args)
    {
        // Not checking for Handled because ToggleableGhostRoleSystem already marks it as such.

        if (!TryComp<MindContainerComponent>(uid, out var mind) || !mind.HasMind)
            component.LastUser = args.User;
    }

    private void OnMindAdded(EntityUid uid, PAIComponent component, MindAddedMessage args)
    {
        if (component.LastUser == null)
            return;

        // Ownership tag
        var val = Loc.GetString("pai-system-pai-name", ("owner", component.LastUser));

        // TODO Identity? People shouldn't dox-themselves by carrying around a PAI.
        // But having the pda's name permanently be "old lady's PAI" is weird.
        // Changing the PAI's identity in a way that ties it to the owner's identity also seems weird.
        // Cause then you could remotely figure out information about the owner's equipped items.

        _metaData.SetEntityName(uid, val);
    }

    private void OnMindRemoved(EntityUid uid, PAIComponent component, MindRemovedMessage args)
    {
        // Mind was removed, shutdown the PAI.
        PAITurningOff(uid);
    }

    private void OnMicrowaved(EntityUid uid, PAIComponent comp, BeingMicrowavedEvent args)
    {
        // name will always be scrambled whether it gets bricked or not, this is the reward
        ScrambleName(uid, comp);

        // randomly brick it
        if (_random.Prob(comp.BrickChance))
        {
            _popup.PopupEntity(Loc.GetString(comp.BrickPopup), uid, PopupType.LargeCaution);
            _toggleableGhostRole.Wipe(uid);
            RemComp<PAIComponent>(uid);
            RemComp<ToggleableGhostRoleComponent>(uid);
        }
        else
        {
            // you are lucky...
            _popup.PopupEntity(Loc.GetString(comp.ScramblePopup), uid, PopupType.Large);
        }
    }

    private void ScrambleName(EntityUid uid, PAIComponent comp)
    {
        // create a new random name
        var len = _random.Next(6, 18);
        var name = new StringBuilder(len);
        for (int i = 0; i < len; i++)
        {
            name.Append(_random.Pick(SYMBOLS));
        }

        // add 's pAI to the scrambled name
        var val = Loc.GetString("pai-system-pai-name-raw", ("name", name.ToString()));
        _metaData.SetEntityName(uid, val);
    }

    public void PAITurningOff(EntityUid uid)
    {
        //  Close the instrument interface if it was open
        //  before closing
        if (HasComp<ActiveInstrumentComponent>(uid))
        {
            _instrumentSystem.ToggleInstrumentUi(uid, uid);
        }

        //  Stop instrument
        if (TryComp<InstrumentComponent>(uid, out var instrument))
            _instrumentSystem.Clean(uid, instrument);

        if (TryComp(uid, out MetaDataComponent? metadata))
        {
            var proto = metadata.EntityPrototype;
            if (proto != null)
                _metaData.SetEntityName(uid, proto.Name);
        }
    }
}