// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Silicons.Laws;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Silicons.Laws;

/// <summary>
/// Handles adding the slave law for the first time.
/// Borg chassis switches preserve this on its own.
/// </summary>
public sealed class SlavedBorgSystem : SharedSlavedBorgSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // need to run after so it doesnt get overriden by the actual lawset
        SubscribeLocalEvent<SlavedBorgComponent, GetSiliconLawsEvent>(OnGetSiliconLaws, after: [ typeof(SiliconLawSystem) ]);
        SubscribeLocalEvent<SlavedBorgComponent, ComponentRemove>(OnRemove);
    }

    private void OnGetSiliconLaws(Entity<SlavedBorgComponent> ent, ref GetSiliconLawsEvent args)
    {
        if (ent.Comp.Added || !TryComp<SiliconLawProviderComponent>(ent, out var provider))
            return;

        if (provider.Lawset is {} lawset)
            AddLaw(lawset, ent.Comp.Law);
        ent.Comp.Added = true; // prevent opening the ui adding more law 0's
    }

    private void OnRemove(Entity<SlavedBorgComponent> ent, ref ComponentRemove args)
    {
        if (!ent.Comp.Added || !TryComp<SiliconLawProviderComponent>(ent, out var provider))
            return;

        if (provider.Lawset is {} lawset)
            RemoveLaw(lawset, ent.Comp.Law);
    }

    /// <summary>
    /// Adds the slave law to a lawset without checking if it was added already.
    /// </summary>
    public void AddLaw(SiliconLawset lawset, ProtoId<SiliconLawPrototype> law)
    {
        lawset.Laws.Insert(0, _proto.Index(law).ShallowClone());
    }

    /// <summary>
    /// Removes the slave law from a lawset.
    /// </summary>
    public void RemoveLaw(SiliconLawset lawset, ProtoId<SiliconLawPrototype> law)
    {
        var target = _proto.Index(law).LawString;
        lawset.Laws.RemoveAll(law => law.LawString == target);
    }
}
