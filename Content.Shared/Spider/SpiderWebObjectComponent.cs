// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jackrost <jackrost@mail.ru>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Spider;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSpiderSystem))]
public sealed partial class SpiderWebObjectComponent : Component
{
}