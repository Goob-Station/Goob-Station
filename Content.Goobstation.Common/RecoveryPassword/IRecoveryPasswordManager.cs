namespace Content.Goobstation.Common.RecoveryPassword;

public interface IRecoveryPasswordManager
{
    bool HasPassword { get; }
    bool Enabled { get; }
    event Action? Updated;
    void ToggleWindow();
}
