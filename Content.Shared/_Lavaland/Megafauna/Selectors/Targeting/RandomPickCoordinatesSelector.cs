using Content.Shared._Lavaland.Megafauna.Systems;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

public sealed partial class RandomPickCoordinatesSelector : MegafaunaSelector
{
    [DataField]
    public float PickRadius = 5f;

    [DataField]
    public bool AlignTile;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var system = args.EntityManager.System<MegafaunaSystem>();

        if (!system.TryPickRandomPosition(args,PickRadius, AlignTile))
            return FailDelay;

        return DelaySelector.Get(args);
    }
}
