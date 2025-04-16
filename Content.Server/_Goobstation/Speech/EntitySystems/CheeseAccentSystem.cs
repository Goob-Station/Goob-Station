// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 BeeRobynn <166929042+BeeRobynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 BeeRobynn <robynthewarcrime@proton.me>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 smudge <138918973+Cerise-Cattowo@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class CheeseAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CheeseAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, CheeseAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "Cheese");

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        args.Message = message;
    }
};
