using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingBiomassSystem : SharedChangelingBiomassSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingBiomassComponent, PolymorphedEvent>(OnPolymorphed);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
    }

    private void OnPolymorphed(Entity<ChangelingBiomassComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<ChangelingBiomassComponent>(ent, args.NewEntity);

        // have to manually copy over the InternalResourcesData stuff
        var oldComp = Comp<ChangelingBiomassComponent>(args.OldEntity);
        var oldData = oldComp.ResourceData;

        var newComp = Comp<ChangelingBiomassComponent>(args.NewEntity);
        var newData = newComp.ResourceData;

        if (oldData == null
            || newData == null)
            return;

        newData.CurrentAmount = oldData.CurrentAmount;
        newData.MaxAmount = oldData.MaxAmount;
        newData.RegenerationRate = oldData.RegenerationRate;
        newData.Thresholds = oldData.Thresholds;
        newData.InternalResourcesType = oldData.InternalResourcesType;
    }

    protected override void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        _chat.TryEmoteWithChat(ent, ent.Comp.CoughEmote, ignoreActionBlocker: true, forceEmote: true);
    }
}
