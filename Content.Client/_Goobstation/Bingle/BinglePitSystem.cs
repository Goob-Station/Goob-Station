using Content.Shared.Ghost.Roles.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Content.Shared.Destructible;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Stunnable;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using System.Numerics;
namespace Content.Client._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<BinglePitGrowEvent>(OnPitGrow);
    }
    public void OnPitGrow(BinglePitGrowEvent args){

        if (!TryComp<SpriteComponent>(GetEntity(args.Uid), out var sprite))
            return;

        sprite.Scale = new Vector2(args.Level, args.Level);
    }
}
