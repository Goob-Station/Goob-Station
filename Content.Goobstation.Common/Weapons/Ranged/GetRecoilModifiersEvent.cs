namespace Content.Goobstation.Common.Weapons.Ranged;

public sealed class GetRecoilModifiersEvent : EntityEventArgs
{
    public EntityUid Gun;
    public EntityUid User;
    public float Modifier = 1f;
}
