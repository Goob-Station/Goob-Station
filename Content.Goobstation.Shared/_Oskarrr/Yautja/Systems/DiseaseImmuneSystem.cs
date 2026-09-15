// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Disease;
using Content.Shared._Oskarrr.Yautja.Components;

namespace Content.Goobstation.Shared._Oskarrr.Yautja.Systems;

/// <summary>
/// Lives in Goobstation.Shared because <see cref="DiseaseInfectAttemptEvent"/> is defined there.
/// </summary>
public sealed class DiseaseImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseImmuneComponent, DiseaseInfectAttemptEvent>(OnInfectAttempt);
    }

    private void OnInfectAttempt(Entity<DiseaseImmuneComponent> ent, ref DiseaseInfectAttemptEvent args)
    {
        args.CanInfect = false;
    }
}
