using Content.Shared.Supermatter.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Supermatter.Systems;

public abstract class SharedSupermatterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SupermatterComponent, ComponentStartup>(OnSupermatterStartup);
    }

    public enum SuperMatterSound : sbyte
    {
        Aggressive = 0,
        Delam = 1
    }

    public enum DelamType : sbyte
    {
        Explosion = 0,
        Singulo = 1,
        Tesla = 2,
        Cascade = 3 // save for later
    }
    #region Getters/Setters

    public void OnSupermatterStartup(EntityUid uid, SupermatterComponent comp, ComponentStartup args)
    {
    }

    #endregion Getters/Setters

    #region Serialization
    /// <summary>
    /// A state wrapper used to sync the supermatter between the server and client.
    /// </summary>
    [Serializable, NetSerializable]
    protected sealed class SupermatterComponentState : ComponentState
    {
        public SupermatterComponentState(SupermatterComponent supermatter)
        {
        }
    }

    #endregion Serialization

}
