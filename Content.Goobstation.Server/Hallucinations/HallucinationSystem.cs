using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Hallucinations;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Database;
using Content.Shared.Chat;
using Content.Shared.Eye;
using Content.Shared.Humanoid;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffectNew.Components;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Hallucinations;

/// <summary>
/// Runs hallucinations.
/// </summary>
public sealed class HallucinationSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HallucinatingComponent, MapInitEvent>(OnStart);
        SubscribeLocalEvent<HallucinatingComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStart(Entity<HallucinatingComponent> ent, ref MapInitEvent args)
    {
        _adminLog.Add(LogType.EntityEffect, LogImpact.Medium, $"{ToPrettyString(ent):entity} started hallucinating");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<HallucinatingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var victim = TryComp<StatusEffectComponent>(uid, out var status) ? status.AppliedTo : uid;

            UpdateVictim((uid, comp), victim);

            if (victim is { } v)
                UpdateHallucinating(v, comp, now);
        }
    }

    private void UpdateVictim(Entity<HallucinatingComponent> ent, EntityUid? victim)
    {
        if (ent.Comp.CurrentVictim == victim)
            return;

        if (ent.Comp.CurrentVictim is { } old)
            SetHallucinationVisibility(old, false);

        if (victim is { } fresh)
            SetHallucinationVisibility(fresh, true);

        ent.Comp.CurrentVictim = victim;
    }

    private void OnShutdown(Entity<HallucinatingComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.CurrentVictim is not { } victim)
            return;

        SetHallucinationVisibility(victim, false);
    }

    private void SetHallucinationVisibility(EntityUid victim, bool enabled)
    {
        if (!TryComp<EyeComponent>(victim, out var eye))
            return;

        var mask = enabled
            ? eye.VisibilityMask | (int) VisibilityFlags.Hallucination
            : eye.VisibilityMask & ~(int) VisibilityFlags.Hallucination;
        _eye.SetVisibilityMask(victim, mask, eye);
    }

    #region Timer

    private void UpdateHallucinating(EntityUid victim, HallucinatingComponent comp, TimeSpan now)
    {
        if (comp.NextTime == TimeSpan.Zero)
        {
            comp.NextTime = now + NextDelay(comp);
            return;
        }

        if (now < comp.NextTime)
            return;

        comp.NextTime = now + NextDelay(comp);

        if (TryPick(comp, out var proto))
            Fire(victim, proto.Effect);
    }

    private TimeSpan NextDelay(HallucinatingComponent comp)
    {
        var severity = MathF.Max(0.01f, comp.Severity);
        return TimeSpan.FromSeconds(_random.NextFloat(comp.DelayMin, comp.DelayMax) / severity);
    }

    /// <summary>
    /// Picks a hallucination group from the table, then a hallucination from within that group.
    /// </summary>
    private bool TryPick(HallucinatingComponent comp, [NotNullWhen(true)] out HallucinationPrototype? picked)
    {
        picked = null;

        if (!_proto.TryIndex(comp.Groups, out var groups) || groups.Weights.Count == 0)
            return false;

        if (!_proto.TryIndex<WeightedRandomPrototype>(groups.Pick(_random), out var group) || group.Weights.Count == 0)
            return false;

        return _proto.TryIndex(group.Pick(_random), out picked);
    }

    #endregion

    #region Effects

    private void Fire(EntityUid victim, HallucinationEffect effect)
    {
        switch (effect)
        {
            case SoundHallucinationEffect sound:
                PlayPhantomSound(victim, sound);
                break;
            case AppearanceHallucinationEffect appearance:
                SendAppearance(victim, appearance);
                break;
            case SpeechHallucinationEffect speech:
                PlayFakeSpeech(victim, speech);
                break;
            case MorphHallucinationEffect morph:
                SendMorph(victim, morph);
                break;
        }
    }

    /// <summary>
    /// Tells the victims client to make a nearby bystander look like one of the mobs.
    /// </summary>
    private void SendMorph(EntityUid victim, MorphHallucinationEffect effect)
    {
        if (effect.Mobs.Count == 0 || !_player.TryGetSessionByEntity(victim, out var session))
            return;

        var mobs = new List<string>();
        foreach (var mob in effect.Mobs)
            mobs.Add(mob);

        RaiseNetworkEvent(new SetHallucinationMorphMessage(mobs), session);
    }

    /// <summary>
    /// Fake conversation: a nearby bystander (or, in mass mode, everyone nearby, repeatedly) seems
    /// to speak to the victim. The chat and speech bubble go to the victims client alone.
    /// </summary>
    private void PlayFakeSpeech(EntityUid victim, SpeechHallucinationEffect effect)
    {
        if (!_proto.TryIndex(effect.Messages, out var dataset)
            || dataset.Values.Count == 0
            || !_player.TryGetSessionByEntity(victim, out var session))
            return;

        var speakers = new List<EntityUid>();
        foreach (var ent in _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(victim).Coordinates, effect.Range))
            if (ent.Owner != victim)
                speakers.Add(ent.Owner);

        if (speakers.Count == 0)
            return;

        var line = dataset.Values[_random.Next(dataset.Values.Count)];
        var message = Loc.GetString(line, ("name", Name(victim)));

        if (!effect.Mass)
        {
            FakeSay(_random.Pick(speakers), message, session);
            return;
        }

        foreach (var speaker in speakers)
        {
            var repeats = _random.Next(effect.RepeatMin, effect.RepeatMax + 1);
            for (var i = 0; i < repeats; i++)
                FakeSay(speaker, message, session);
        }
    }

    private void FakeSay(EntityUid speaker, string message, ICommonSession session)
    {
        var wrapped = _chat.WrapPublicMessage(speaker, Name(speaker), message);
        _chatManager.ChatMessageToOne(ChatChannel.Local, message, wrapped, speaker, false, session.Channel);
    }

    private void PlayPhantomSound(EntityUid victim, SoundHallucinationEffect effect)
    {
        if (!_player.TryGetSessionByEntity(victim, out var session))
            return;

        var offset = _random.NextAngle().ToVec() * _random.NextFloat(effect.RangeMin, effect.RangeMax);
        var coords = Transform(victim).Coordinates.Offset(offset);

        _audio.PlayStatic(effect.Sound, Filter.SinglePlayer(session), coords, false);
    }

    private void SendAppearance(EntityUid victim, AppearanceHallucinationEffect effect)
    {
        if (effect.Prototypes.Count == 0 || effect.States.Count == 0
            || !_player.TryGetSessionByEntity(victim, out var session))
            return;

        RaiseNetworkEvent(new SetHallucinationAppearanceMessage(effect.Prototypes, effect.States, effect.Sound), session);
    }

    #endregion

    /// <summary>
    /// Hides an entity on the hallucination visibility layer and force-sends it to the victim.
    /// </summary>
    public void MakeHallucination(EntityUid ent, EntityUid victim)
    {
        _visibility.SetLayer(ent, (ushort) VisibilityFlags.Hallucination);

        if (_player.TryGetSessionByEntity(victim, out var session))
            _pvsOverride.AddForceSend(ent, session);
    }
}
