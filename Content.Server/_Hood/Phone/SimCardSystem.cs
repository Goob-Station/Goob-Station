// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NameIdentifier;
using Content.Shared._Hood.Phone;
using Content.Shared.NameIdentifier;
using Robust.Shared.Prototypes;

namespace Content.Server._Hood.Phone;

/// <summary>
/// Assigns fictional SIM numbers from a server-owned round-local pool.
/// Direct allocation deliberately keeps destroyed numbers reserved until the round restarts.
/// </summary>
public sealed class SimCardSystem : EntitySystem
{
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;

    private readonly ProtoId<NameIdentifierGroupPrototype> _numberGroup = "HoodPhone";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SimCardComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SimCardComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Number != null)
            return;

        _nameIdentifier.GenerateUniqueName(ent.Owner, _numberGroup, out var number);
        if (number <= 0)
        {
            Log.Error($"Could not allocate a Hood phone number for {ToPrettyString(ent.Owner)}.");
            return;
        }

        ent.Comp.Number = (uint) number;
        Dirty(ent);
    }
}
