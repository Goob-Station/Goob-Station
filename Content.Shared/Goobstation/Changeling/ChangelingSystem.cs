
namespace Content.Shared.Goobstation.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentInit>(OnInit);
    }

    public void OnInit(EntityUid uid, ChangelingComponent comp, ref ComponentInit args)
    {

    }
}
