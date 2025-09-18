using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Ghost;
using Content.Shared.Magic.Events;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Spawners;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class PossessObjectSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PossessObjectComponent, PossessObjectEvent>(OnPossess);
        SubscribeLocalEvent<ChangeComponentsEvent>(OnChangeComponents);
    }

    public void OnPossess(Entity<PossessObjectComponent> ent, ref PossessObjectEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        // Block certain target types that should not be inhabitable
        List<(Type, string)> blockers = new()
    {
        (typeof(TimedDespawnComponent), "temporary"),
        (typeof(FadingTimedDespawnComponent), "temporary"),
        (typeof(GhostComponent), "ghost"),
        (typeof(SpectralComponent), "ghost")
    };

        if (blockers.Any(x => CheckMindswapBlocker(x.Item1, x.Item2)))
            return;

        if (!_mind.TryGetMind(uid, out var perMind, out var _))
            return;

        // Transfer wraith mind into the object
        _mind.TransferTo(perMind, target);

        if (_net.IsServer)
        {
            _audio.PlayEntity(comp.Sound, target, target);
        }

        bool CheckMindswapBlocker(Type type, string message)
        {
            if (!HasComp(target, type))
                return false;

            _popup.PopupPredicted(Loc.GetString("wraith-possess"), uid, uid);
            return true;
        }

        args.Handled = true;
    }

    private void OnChangeComponents(ChangeComponentsEvent args)
    {
        var target = args.Target;

        args.Handled = true;

        RemoveComponents(target, args.ToRemove);
        AddComponents(target, args.ToAdd);
    }

    private void AddComponents(EntityUid target, ComponentRegistry comps)
    {
        foreach (var (name, data) in comps)
        {
            if (HasComp(target, data.Component.GetType()))
                continue;

            var component = (Component) Factory.GetComponent(name);
            var temp = (object) component;
            _seriMan.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(target, (Component) temp!);
        }
    }

    private void RemoveComponents(EntityUid target, HashSet<string> comps)
    {
        foreach (var toRemove in comps)
        {
            if (Factory.TryGetRegistration(toRemove, out var registration))
                RemComp(target, registration.Type);
        }
    }
}
