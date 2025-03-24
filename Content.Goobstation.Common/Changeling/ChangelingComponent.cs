using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Changeling;


/// <summary>
/// Exists to mark an entity as a changeling.
/// For the component holding changeling data, see ChangelingIdentityComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{

}
