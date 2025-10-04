using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Speech;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Shizophrenia;

public sealed partial class SchizophreniaSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SpeechSoundSystem _speech = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private int _nextIdx = 1;

    public override void Initialize()
    {
        base.Initialize();
        UpdatesBefore.Add(typeof(ActionsSystem));

        InitializeShizophrenic();
        InitializeHallucinations();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HallucinatingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > _timing.CurTime)
                continue;

            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(0.5f);

            foreach (var item in new Dictionary<string, TimeSpan>(comp.Removes))
            {
                if (item.Value <= _timing.CurTime)
                {
                    comp.Hallucinations.Remove(item.Key);
                    comp.Removes.Remove(item.Key);
                }
            }

            if (comp.Hallucinations.Count <= 0)
            {
                RemComp(uid, comp);
                continue;
            }

            foreach (var item in comp.Hallucinations)
                item.Value.TryPerform(uid, EntityManager, _random, _timing.CurTime);
        }
    }
}
