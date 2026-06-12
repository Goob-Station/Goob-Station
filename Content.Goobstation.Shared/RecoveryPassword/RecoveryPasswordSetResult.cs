namespace Content.Goobstation.Shared.RecoveryPassword;

public enum RecoveryPasswordSetResult : byte
{
    Success,
    Disabled,
    AlreadySet,
    TooShort,
    TooLong,
    Error,
}
