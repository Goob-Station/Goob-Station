// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard.Accents;

public sealed class AnimalAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PigAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<FrogAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<CowAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<HorseAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<RatAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<FoxAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<BeeAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<BearAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<BatAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<RavenAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<JackalAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AnimalAccentComponent comp, ref AccentGetEvent args)
    {
        if (comp.AnimalNoises.Count == 0)
            return;

        if (comp is { AltNoiseProbability: > 0f, AnimalAltNoises.Count: > 0 } && _random.Prob(comp.AltNoiseProbability))
        {
            args.Message = Loc.GetString(_random.Pick(comp.AnimalAltNoises));
            return;
        }

        args.Message = Loc.GetString(_random.Pick(comp.AnimalNoises));
    }
}