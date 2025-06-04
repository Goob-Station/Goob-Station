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

        SubscribeLocalEvent<EldritchHorrorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<EldritchHorrorComponent, HorrorSpawnProphetActionEvent>(OnRiseProphets);
    }

    private void OnInit(EntityUid uid, EldritchHorrorComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.SpawnProphetsActionEntity, component.SpawnProphetsAction);
    }

    private void OnRiseProphets(EntityUid uid, EldritchHorrorComponent component, HorrorSpawnProphetActionEvent args)
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
