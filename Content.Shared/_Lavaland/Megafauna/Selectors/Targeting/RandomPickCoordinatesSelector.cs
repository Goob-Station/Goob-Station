using Content.Shared._Lavaland.Megafauna.Systems;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

public sealed partial class RandomPickCoordinatesSelector : MegafaunaSelector
{
    [DataField(required: true)]
    public string CoordsKey;

    [DataField(required: true)]
    public float Radius;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var system = args.EntityManager.System<MegafaunaSystem>();

        if (!system.TryPickRandomPosition(args, CoordsKey, Radius))
            return FailDelay;

        return DelaySelector.Get(args);
    }
}
