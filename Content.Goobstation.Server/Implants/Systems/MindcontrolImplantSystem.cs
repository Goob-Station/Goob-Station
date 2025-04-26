// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Goobstation.Server.Mindcontrol;
using Content.Goobstation.Shared.Mindcontrol;
using Content.Shared.Implants;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Implants.Systems;
public sealed class MindcontrolImplantSystem : EntitySystem
{
    [Dependency] private readonly MindcontrolSystem _mindcontrol = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindcontrolImplantComponent, EntGotRemovedFromContainerMessage>(OnRemove); //implant gets removed, remove traitor
        SubscribeLocalEvent<MindcontrolImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<MindcontrolImplantComponent, EntGotInsertedIntoContainerMessage>(OnInsert);
    }
    private void OnImplant(EntityUid uid, MindcontrolImplantComponent component, ImplantImplantedEvent args) //called after implanted ?
    {
        if (component.ImplanterUid != null)
        {
            component.HolderUid = Transform(component.ImplanterUid.Value).ParentUid;
        }
        if (args.Implanted != null)
            EnsureComp<MindcontrolledComponent>(args.Implanted.Value);

        component.ImplanterUid = null;
        if (args.Implanted == null)
            return;
        if (!TryComp<MindcontrolledComponent>(args.Implanted.Value, out var implanted))
            return;
        implanted.Master = component.HolderUid;
        _mindcontrol.Start(args.Implanted.Value, implanted);
    }
    private void OnInsert(EntityUid uid, MindcontrolImplantComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == "implanter_slot")  //being inserted in a implanter.
        {
            component.ImplanterUid = args.Container.Owner;    //save Implanter uid
            component.HolderUid = null;
        }
    }
    private void OnRemove(EntityUid uid, MindcontrolImplantComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (args.Container.ID == "implant") //when implant is removed
        {
            if (HasComp<MindcontrolledComponent>(args.Container.Owner))
                RemComp<MindcontrolledComponent>(args.Container.Owner);
        }
    }
}