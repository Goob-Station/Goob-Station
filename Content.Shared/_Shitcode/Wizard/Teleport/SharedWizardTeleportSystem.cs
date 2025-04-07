// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.UserInterface;

namespace Content.Shared._Goobstation.Wizard.Teleport;

public abstract class SharedWizardTeleportSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeleportScrollComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TeleportScrollComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt);
    }

    public virtual void OnTeleportSpell(EntityUid performer, EntityUid action)
    {
    }

    private void OnUiOpenAttempt(Entity<TeleportScrollComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (ent.Comp.UsesLeft <= 0)
            args.Cancel();
    }

    private void OnExamined(Entity<TeleportScrollComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("teleport-scroll-uses-left", ("uses", ent.Comp.UsesLeft)));
    }
}