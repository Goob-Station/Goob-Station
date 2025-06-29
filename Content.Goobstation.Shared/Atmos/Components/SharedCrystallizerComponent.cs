using Content.Shared.Atmos;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Components;

[Serializable, NetSerializable]
public enum CrystallizerUiKey : byte
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class CrystallizerToggleMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class CrystallizerSelectRecipeMessage : BoundUserInterfaceMessage
{
    public string? RecipeId { get; }

    public CrystallizerSelectRecipeMessage(string? recipeId)
    {
        RecipeId = recipeId;
    }
}

[Serializable, NetSerializable]
public sealed class CrystallizerSetGasInputMessage : BoundUserInterfaceMessage
{
    public float GasInput { get; }

    public CrystallizerSetGasInputMessage(float gasInput)
    {
        GasInput = gasInput;
    }
}

[Serializable]
[NetSerializable]
public sealed class CrystallizerBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool Enabled { get; }
    public string? SelectedRecipeId { get; }
    public float GasInput { get; }
    public GasMixture GasMixture { get; }
    public float ProgressBar { get; }
    public float QualityLoss { get; }

    public CrystallizerBoundUserInterfaceState(bool enabled, string? selectedRecipeId, float gasInput, GasMixture gasMixture, float progressBar, float qualityLoss)
    {
        Enabled = enabled;
        SelectedRecipeId = selectedRecipeId;
        GasInput = gasInput;
        GasMixture = gasMixture;
        ProgressBar = progressBar;
        QualityLoss = qualityLoss;
    }
}

[Serializable, NetSerializable]
public sealed class CrystallizerUpdateGasMixtureMessage : BoundUserInterfaceMessage
{
    public GasMixture GasMixture { get; }

    public CrystallizerUpdateGasMixtureMessage(GasMixture gasMixture)
    {
        GasMixture = gasMixture;
    }
}

[Serializable, NetSerializable]
public sealed class CrystallizerProgressBarMessage : BoundUserInterfaceMessage
{
    public float ProgressBar { get; }

    public CrystallizerProgressBarMessage(float progressBar)
    {
        ProgressBar = progressBar;
    }
}

