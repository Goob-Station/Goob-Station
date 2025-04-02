using Content.Server.Administration.Managers;
using Content.Shared.Verbs;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(GetVerbs);
    }

    private void GetVerbs(GetVerbsEvent<Verb> args)
    {
        AddAntagVerbs(args);
        AddSmiteVerbs(args);
    }
}
