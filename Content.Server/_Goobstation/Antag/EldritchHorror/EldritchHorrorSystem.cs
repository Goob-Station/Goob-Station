// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Server._Goobstation.Antag.EldritchHorror;
using Content.Shared._Goobstation.EldritchHorror.EldritchHorrorEvents;

public sealed partial class EldritchHorrorSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchHorrorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EldritchHorrorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EldritchHorrorComponent, HorrorSpawnProphetActionEvent>(OnRiseProphet);
    }

    private void OnStartup(EntityUid uid, EldritchHorrorComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.SpawnProphetActionEntity, component.SpawnProphetAction);
    }
    private void OnShutdown(EntityUid uid, EldritchHorrorComponent component, ComponentShutdown args)
    {
        if (component.SpawnProphetActionEntity != null)
        {
            _actions.RemoveAction(uid, component.SpawnProphetActionEntity);
        }
    }

    private void OnRiseProphet(EntityUid uid, EldritchHorrorComponent component, HorrorSpawnProphetActionEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(uid);
        for (int i = 0; i < component.ProphetAmount; i++)
        {
            var ent = Spawn(component.ProphetProtoId, xform.Coordinates);
        }

        args.Handled = true;
    }
}
