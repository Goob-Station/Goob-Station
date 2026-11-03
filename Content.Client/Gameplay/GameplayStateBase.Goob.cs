using System.Linq;
using Content.Shared.Damage.Components;
using Robust.Shared.Map;

namespace Content.Client.Gameplay;

public partial class GameplayStateBase
{
    public EntityUid? GetDamageableClickedEntity(MapCoordinates coordinates)
    {
        var first = GetClickableEntities(coordinates, _eyeManager.CurrentEye)
            .FirstOrDefault(e => _entityManager.HasComponent<DamageableComponent>(e));
        return first.IsValid() ? first : null;
    }
}