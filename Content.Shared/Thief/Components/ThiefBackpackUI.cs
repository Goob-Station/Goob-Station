// SPDX-FileCopyrightText: 2023 Colin-Tel <113523727+Colin-Tel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Thief;

[Serializable, NetSerializable]
public sealed class ThiefBackpackBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly Dictionary<int, ThiefBackpackSetInfo> Sets;
    public int MaxSelectedSets;

    public ThiefBackpackBoundUserInterfaceState(Dictionary<int, ThiefBackpackSetInfo> sets, int max)
    {
        Sets = sets;
        MaxSelectedSets = max;
    }
}

[Serializable, NetSerializable]
public sealed class ThiefBackpackChangeSetMessage : BoundUserInterfaceMessage
{
    public readonly int SetNumber;

    public ThiefBackpackChangeSetMessage(int setNumber)
    {
        SetNumber = setNumber;
    }
}

[Serializable, NetSerializable]
public sealed class ThiefBackpackApproveMessage : BoundUserInterfaceMessage
{
    public ThiefBackpackApproveMessage() { }
}

[Serializable, NetSerializable]
public enum ThiefBackpackUIKey : byte
{
    Key
};

[Serializable, NetSerializable, DataDefinition]
public partial struct ThiefBackpackSetInfo
{
    [DataField]
    public string Name;

    [DataField]
    public string Description;

    [DataField]
    public SpriteSpecifier Sprite;

    public bool Selected;

    public ThiefBackpackSetInfo(string name, string desc, SpriteSpecifier sprite, bool selected)
    {
        Name = name;
        Description = desc;
        Sprite = sprite;
        Selected = selected;
    }
}