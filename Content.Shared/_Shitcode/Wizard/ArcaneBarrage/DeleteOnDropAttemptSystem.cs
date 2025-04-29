// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Shared._Goobstation.Wizard.ArcaneBarrage;

public sealed class DeleteOnDropAttemptSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteOnDropAttemptComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<DeleteOnDropAttemptComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("delete-on-drop-attempt-comp-examine"));
    }
}