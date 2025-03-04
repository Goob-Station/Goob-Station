using Content.Server.Actions;
using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared.Clothing;

namespace Content.Server._Goobstation.MartialArts;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class MartialArtsSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    private void InitializeKravMaga()
    {
        SubscribeLocalEvent<GrantKravMagaComponent, ClothingGotEquippedEvent>(OnGlovesGotEquipped);
        SubscribeLocalEvent<GrantKravMagaComponent, ClothingGotUnequippedEvent>(OnGlovesGotUnequipped);
        SubscribeLocalEvent<KravMagaComponent, KravMagaActionEvent>(OnKravMagaAction);
    }

    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        var actionUid = args.Action.Owner;
    }

    private void OnGlovesGotEquipped(Entity<GrantKravMagaComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var kravMaga = EnsureComp<KravMagaComponent>(args.Wearer);
        foreach (var actionId in kravMaga.BaseKravMagaMoves)
        {
            var actions = _actions.AddAction(args.Wearer, actionId);
            if (actions != null)
                kravMaga.KravMagaMoveEntities.Add(actions.Value);
        }
    }

    private void OnGlovesGotUnequipped(Entity<GrantKravMagaComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!TryComp<KravMagaComponent>(args.Wearer, out var kravMaga))
            return;

        foreach (var action in kravMaga.KravMagaMoveEntities)
        {
            _actions.RemoveAction(action);
        }
    }
}
