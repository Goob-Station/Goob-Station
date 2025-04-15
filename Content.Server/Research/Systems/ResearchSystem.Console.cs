// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.EntitySystems;
using Content.Server.Research.Components;
using Content.Shared.UserInterface;
using Content.Shared.Access.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Content.Goobstation.Common.Pirates;
using Content.Goobstation.Common.Research; // R&D Console Rework
using System.Linq; // R&D Console Rework

namespace Content.Server.Research.Systems;

public sealed partial class ResearchSystem
{
    [Dependency] private readonly EmagSystem _emag = default!;

    private void InitializeConsole()
    {
        SubscribeLocalEvent<ResearchConsoleComponent, ConsoleUnlockTechnologyMessage>(OnConsoleUnlock);
        SubscribeLocalEvent<ResearchConsoleComponent, BeforeActivatableUIOpenEvent>(OnConsoleBeforeUiOpened);
        SubscribeLocalEvent<ResearchConsoleComponent, ResearchServerPointsChangedEvent>(OnPointsChanged);
        SubscribeLocalEvent<ResearchConsoleComponent, ResearchRegistrationChangedEvent>(OnConsoleRegistrationChanged);
        SubscribeLocalEvent<ResearchConsoleComponent, TechnologyDatabaseModifiedEvent>(OnConsoleDatabaseModified);
        SubscribeLocalEvent<ResearchConsoleComponent, TechnologyDatabaseSynchronizedEvent>(OnConsoleDatabaseSynchronized);
        SubscribeLocalEvent<ResearchConsoleComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnConsoleUnlock(EntityUid uid, ResearchConsoleComponent component, ConsoleUnlockTechnologyMessage args)
    {
        // goob edit - spirates
        var eqe = EntityQueryEnumerator<ResourceSiphonComponent>();
        while (eqe.MoveNext(out var siphon))
        {
            if (siphon.Active)
            {
                _popup.PopupEntity(Loc.GetString("console-block-something"), args.Actor);
                return;
            }
        }
        // goob edit end

        var act = args.Actor;

        if (!this.IsPowered(uid, EntityManager))
            return;

        if (!PrototypeManager.TryIndex<TechnologyPrototype>(args.Id, out var technologyPrototype))
            return;

        if (TryComp<AccessReaderComponent>(uid, out var access) && !_accessReader.IsAllowed(act, uid, access))
        {
            _popup.PopupEntity(Loc.GetString("research-console-no-access-popup"), act);
            return;
        }

        if (!UnlockTechnology(uid, args.Id, act))
            return;

        if (!_emag.CheckFlag(uid, EmagType.Interaction))
        {
            var getIdentityEvent = new TryGetIdentityShortInfoEvent(uid, act);
            RaiseLocalEvent(getIdentityEvent);

            var message = Loc.GetString(
                "research-console-unlock-technology-radio-broadcast",
                ("technology", Loc.GetString(technologyPrototype.Name)),
                ("amount", technologyPrototype.Cost),
                ("approver", getIdentityEvent.Title ?? string.Empty)
            );
            _radio.SendRadioMessage(uid, message, component.AnnouncementChannel, uid, escapeMarkup: false);
        }

        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleBeforeUiOpened(EntityUid uid, ResearchConsoleComponent component, BeforeActivatableUIOpenEvent args)
    {
        SyncClientWithServer(uid);
    }

    private void UpdateConsoleInterface(EntityUid uid, ResearchConsoleComponent? component = null, ResearchClientComponent? clientComponent = null)
    {
        if (!Resolve(uid, ref component, ref clientComponent, false))
            return;

        // R&D Console Rework Start
        var allTechs = PrototypeManager.EnumeratePrototypes<TechnologyPrototype>().ToList();
        Dictionary<string, ResearchAvailability> techList;
        var points = 0;

        if (TryGetClientServer(uid, out var serverUid, out var server, clientComponent) &&
            TryComp<TechnologyDatabaseComponent>(serverUid, out var db))
        {
            var unlockedTechs = new HashSet<string>(db.UnlockedTechnologies);
            techList = allTechs.ToDictionary(
                proto => proto.ID,
                proto =>
                {
                    if (unlockedTechs.Contains(proto.ID))
                        return ResearchAvailability.Researched;

                    var prereqsMet = proto.TechnologyPrerequisites.All(p => unlockedTechs.Contains(p));
                    var canAfford = server.Points >= proto.Cost;

                    return prereqsMet ?
                        (canAfford ? ResearchAvailability.Available : ResearchAvailability.PrereqsMet)
                        : ResearchAvailability.Unavailable;
                });

            if (clientComponent != null)
                points = clientComponent.ConnectedToServer ? server.Points : 0;
        }
        else
        {
            techList = allTechs.ToDictionary(proto => proto.ID, _ => ResearchAvailability.Unavailable);
        }

        _uiSystem.SetUiState(uid, ResearchConsoleUiKey.Key,
            new ResearchConsoleBoundInterfaceState(points, techList));
        // R&D Console Rework End
    }

    private void OnPointsChanged(EntityUid uid, ResearchConsoleComponent component, ref ResearchServerPointsChangedEvent args)
    {
        if (!_uiSystem.IsUiOpen(uid, ResearchConsoleUiKey.Key))
            return;
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleRegistrationChanged(EntityUid uid, ResearchConsoleComponent component, ref ResearchRegistrationChangedEvent args)
    {
        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleDatabaseModified(EntityUid uid, ResearchConsoleComponent component, ref TechnologyDatabaseModifiedEvent args)
    {
        SyncClientWithServer(uid);
        UpdateConsoleInterface(uid, component);
    }

    private void OnConsoleDatabaseSynchronized(EntityUid uid, ResearchConsoleComponent component, ref TechnologyDatabaseSynchronizedEvent args)
    {
        UpdateConsoleInterface(uid, component);
    }

    private void OnEmagged(Entity<ResearchConsoleComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        args.Handled = true;
    }
}

public sealed partial class ResearchConsoleUnlockEvent : CancellableEntityEventArgs { }