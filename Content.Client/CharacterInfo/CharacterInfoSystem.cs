// SPDX-License-Identifier: MIT

using Content.Shared.CharacterInfo;
using Content.Shared.Objectives;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client.CharacterInfo;

public sealed class CharacterInfoSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;

    public event Action<CharacterData>? OnCharacterUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CharacterInfoEvent>(OnCharacterInfoEvent);
    }

    public void RequestCharacterInfo()
    {
        var entity = _players.LocalEntity;
        if (entity == null)
        {
            return;
        }

        RaiseNetworkEvent(new RequestCharacterInfoEvent(GetNetEntity(entity.Value)));
    }

    // Pirate: allow editing the round description in-game.
    public void UpdateDetailExaminable(string content)
    {
        RaiseNetworkEvent(new UpdateDetailExaminableEvent(content));
    }

    private void OnCharacterInfoEvent(CharacterInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(msg.NetEntity, out var entity))
            return;

        var data = new CharacterData(entity.Value, msg.JobTitle, msg.Objectives, msg.Briefing, msg.DetailExaminable, Name(entity.Value), msg.Memory); // Pirate banking

        OnCharacterUpdate?.Invoke(data);
    }

    public List<Control> GetCharacterInfoControls(EntityUid uid)
    {
        var ev = new GetCharacterInfoControlsEvent(uid);
        RaiseLocalEvent(uid, ref ev, true);
        return ev.Controls;
    }

    public readonly record struct CharacterData(
        EntityUid Entity,
        string Job,
        Dictionary<string, List<ObjectiveInfo>> Objectives,
        string? Briefing,
        string? DetailExaminable, // Pirate: allow editing the round description in-game.
        string EntityName,
        Dictionary<string, string> Memory //Pirate Banking
    );

    /// <summary>
    /// Event raised to get additional controls to display in the character info menu.
    /// </summary>
    [ByRefEvent]
    public readonly record struct GetCharacterInfoControlsEvent(EntityUid Entity)
    {
        public readonly List<Control> Controls = new();

        public readonly EntityUid Entity = Entity;
    }
}
