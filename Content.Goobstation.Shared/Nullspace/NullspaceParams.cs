namespace Content.Goobstation.Shared.Nullspace;

public static class NullspaceParams
{
    public const float Cell = 2.5f;
    public const float WallHeight = 2.5f;
    public const float Eye = 1.6f;
    public const int Chunk = 16;

    public const float Walk = 2.0f;
    public const float Sprint = 4.2f;
    public const float ExhaustedWalk = 1.7f;
    public const float StaminaDrain = 0.2f;
    public const float StaminaRegenMoving = 0.15f;
    public const float StaminaRegenStill = 0.25f;
    public const float ExhaustedUntil = 0.3f;
    public const float MoveDamp = 10f;
    public const float WalkStride = 0.75f;
    public const float SprintStride = 1.1f;
    public const float NoiseWalk = 5f;
    public const float NoiseSprint = 15f;
    public const float NoiseCreep = 2f;

    public const float ClownSpawnMin = 55f;
    public const float ClownSpawnMax = 70f;
    public const int ClownSpawnPathMin = 22;
    public const int ClownSpawnPathMax = 34;
    public const float ClownHuntSpeed = 1.8f;
    public const float ClownSearchSpeed = 2.5f;
    public const float ClownChaseSpeed = 3.0f;
    public const float ClownChaseRampPerMin = 0.15f;
    public const float ClownChaseSpeedCap = 4.6f;
    public const float ClownSightRange = 30f;
    public const float ClownCatchRange = 0.8f;
    public const float ClownLoseSightAfter = 1.5f;
    public const float ClownSearchTime = 12f;
    public const float ClownHuntRefresh = 18f;
    public const float ClownHeight = 2.1f;
    public const float ClownStride = 0.9f;

    public const float LightDisturbRange = 6.5f;
    public const float LightKillRange = 3.2f;
    public const float LightCalmRange = 9f;
    public const float LightKillChance = 0.45f;
    public const float LightActiveRange = 32f;

    public const float DarknessStart = 60f;
    public const float DarknessDuration = 420f;
    public const float DarknessDeadLights = 0.75f;
    public const float DarknessFlickerLights = 0.5f;
    public const float DarknessFlashlightDim = 0.6f;

    public static float Darkness(float gameSeconds)
    {
        return Math.Clamp((gameSeconds - DarknessStart) / DarknessDuration, 0f, 1f);
    }
}
