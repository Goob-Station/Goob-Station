using Robust.Shared.Physics.Events;
using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Content.Goobstation.Shared.Speech;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Chat;

public sealed class SpeakOnCollideSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeakOnCollideComponent, StartCollideEvent>(HandleCollide);
    }

    /// <summary>
    /// Sends an In-Character messages through the entity when it collides
    /// </summary>
    /// <param name="ent">Entity sending the message </param>
    private void TryDoCollideSpeak(Entity<SpeakOnCollideComponent> ent)
    {
        string message;
        if (ent.Comp.Text != null)
            message = Loc.GetString(ent.Comp.Text);
        if (_prototypeManager.Resolve(ent.Comp.Pack, out var messagePack))
            _chat.TrySendInGameICMessage(ent, Loc.GetString(_random.Pick(messagePack.Values)), InGameICChatType.Speak, true);
    }

    private void HandleCollide(Entity<SpeakOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (!args.OurFixture.Hard ||
            !args.OtherFixture.Hard ||
            !TryComp<PhysicsComponent>(ent, out var physics))
        {
            return;
        }
        TryDoCollideSpeak(ent);
    }
}
