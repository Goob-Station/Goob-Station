namespace Content.Shared._Lavaland.Mobs.Bosses;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    // holy shit that's a lot of timers.

    [DataField] public float MoveToDelay = 10f;

    [DataField] public float MinorAttackCooldown = 4f;

    [DataField] public float MajorAttackCooldown = 6f;

    [DataField] public float ChaserCooldown = 10f;
}
