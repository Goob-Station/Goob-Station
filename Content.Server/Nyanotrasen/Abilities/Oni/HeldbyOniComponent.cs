// SPDX-FileCopyrightText: 2024 ScyronX <166930367+ScyronX@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Abilities.Oni
{
    [RegisterComponent]
    public sealed partial class HeldByOniComponent : Component
    {
        public EntityUid Holder = default!;
    }
}