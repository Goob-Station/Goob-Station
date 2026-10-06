// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Goobstation.Shared.Humanoid;

public sealed class SharedGoobHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    public void SwapSex(EntityUid uid, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.Sex == Sex.Unsexed)
            return;

        var newSex = humanoid.Sex;
        var newGender = humanoid.Gender;
        switch (humanoid.Sex)
        {
            case Sex.Unsexed:
            default: break;
            case Sex.Male: newGender = Gender.Female; newSex = Sex.Female; break;
            case Sex.Female: newGender = Gender.Male; newSex = Sex.Male; break;
        }

        _humanoidAppearanceSystem.SetSex(uid, newSex);
        _humanoidAppearanceSystem.SetGender(uid, newGender);

        if (TryComp<GrammarComponent>(uid, out var grammar))
            _grammar.SetGender((uid, grammar), newGender);

        _identity.QueueIdentityUpdate(uid);
    }
}
