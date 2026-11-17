using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Gibs bodies in stomach or crushes materials into their components aka just recycles them.
/// </summary>
public sealed partial class SwallowerChewSystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private SharedStackSystem _stack = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private readonly List<EntityUid> _contents = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwallowerComponent, SwallowerChewEvent>(OnChew);
    }

    private void OnChew(Entity<SwallowerComponent> ent, ref SwallowerChewEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled || comp.Stomach.ContainedEntities.Count == 0)
            return;

        _contents.Clear();
        _contents.AddRange(comp.Stomach.ContainedEntities);

        var coords = Transform(uid).Coordinates;

        var chewed = false;

        foreach (var item in _contents)
        {
            if (_whitelist.IsWhitelistPass(comp.ChewBlacklist, item))
            {
                _popup.PopupPredicted(Loc.GetString("swallower-cannot-chew"), uid, uid);
                continue;
            }

            chewed = true;

            if (_whitelist.IsWhitelistPass(comp.ChewHealWhitelist, item))
            {
                RaiseLocalEvent(uid, new RejuvenateEvent());
            }

            if (HasComp<BodyComponent>(item))
            {
                _body.GibBody(item, gibOrgans: true);
                continue;
            }

            if (TryComp<PhysicalCompositionComponent>(item, out var composition))
            {
                foreach (var (material, amount) in composition.MaterialComposition)
                {
                    var proto = _prototype.Index(material);
                    var sheetVolume = _materialStorage.GetSheetVolume(proto);

                    if (proto.StackEntity is not { } stackProto || sheetVolume <= 0)
                        continue;

                    var sheets = amount / sheetVolume;

                    if (sheets <= 0)
                        continue;

                    var stack = PredictedSpawnAtPosition(stackProto, coords);
                    _stack.SetCount(new Entity<StackComponent?>(stack, null), sheets);
                }
            }

            QueueDel(item);
        }

        if (!chewed)
            return;

        _popup.PopupPredicted(Loc.GetString("swallower-chew"), Loc.GetString("swallower-chew-others", ("user", Identity.Entity(uid, EntityManager))), uid, uid);
        _audio.PlayPredicted(comp.ChewSound, uid, uid);

        args.Handled = true;
    }
}
