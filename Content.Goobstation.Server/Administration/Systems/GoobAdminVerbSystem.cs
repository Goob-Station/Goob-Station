using Content.Shared.Verbs;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(GetVerbs);
    }

    private void GetVerbs(GetVerbsEvent<Verb> args)
    {
        AddAntagVerbs(args);
    }
}
