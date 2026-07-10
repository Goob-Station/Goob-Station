// SPDX-License-Identifier: MIT

using Content.Shared.Objectives;
using Robust.Shared.Serialization;

namespace Content.Shared.CharacterInfo;

[Serializable, NetSerializable]
public sealed class RequestCharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;

    public RequestCharacterInfoEvent(NetEntity netEntity)
    {
        NetEntity = netEntity;
    }
}

[Serializable, NetSerializable]
public sealed class CharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public readonly string JobTitle;
    public readonly Dictionary<string, List<ObjectiveInfo>> Objectives;
    public readonly string? Briefing;
    public readonly string? DetailExaminable; // Pirate: allow editing the round description in-game.
    public readonly Dictionary<string, string> Memory; //Pirate banking

    public CharacterInfoEvent(NetEntity netEntity, string jobTitle, Dictionary<string, List<ObjectiveInfo>> objectives, string? briefing, string? detailExaminable, Dictionary<string, string> memory) //Pirate banking
    {
        NetEntity = netEntity;
        JobTitle = jobTitle;
        Objectives = objectives;
        Briefing = briefing;
        DetailExaminable = detailExaminable;
        Memory = memory; //Pirate banking
    }
}

// Pirate: allow editing the round description in-game.
[Serializable, NetSerializable]
public sealed class UpdateDetailExaminableEvent : EntityEventArgs
{
    public readonly string Content;

    public UpdateDetailExaminableEvent(string content)
    {
        Content = content;
    }
}
