using Content.Shared._Goobstation.Emoting;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Chat.Prototypes;

namespace Content.Client._Goobstation.Emoting;

public sealed partial class FartSystem : SharedFartSystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, FartComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not FartComponentState state
        || !_prot.TryIndex<EmotePrototype>(state.Emote, out var emote))
            return;

        if (emote.Event != null)
            RaiseLocalEvent(uid, emote.Event);
    }
}
