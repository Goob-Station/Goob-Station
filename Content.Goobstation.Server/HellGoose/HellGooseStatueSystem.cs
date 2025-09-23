using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.HellGoose.Events;
using Content.Server.Pointing.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.HellGoose;


public sealed class HellGooseStatueSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PrayAtHellGooseStatueEvent>(OnPrayAtHellGoose);
    }

    private void OnPrayAtHellGoose(PrayAtHellGooseStatueEvent ev, EntitySessionEventArgs args)
    {
        var user = args.SenderSession?.AttachedEntity;
        if (user == null)
            return;

        _polymorphSystem.PolymorphEntity(user.Value, "HellGooseMorph");
    }
}