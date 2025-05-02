// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;

namespace Content.Client._Shitcode.UserActions.Tabs;

[Virtual]
public class BaseTabControl : Control
{
    public virtual bool UpdateState() { return true; }
}
