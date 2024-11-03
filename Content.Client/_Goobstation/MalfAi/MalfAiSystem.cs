using Content.Client.Alerts;
using Content.Shared.MalfAi;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.MalfAi;

    public sealed partial class MalfAiSystem
{
        [Dependency] private readonly IPrototypeManager _prototype = default!;
    /*
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }
    */
}

