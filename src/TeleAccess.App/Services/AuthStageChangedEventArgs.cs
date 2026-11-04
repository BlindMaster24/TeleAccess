namespace TeleAccess.App.Services;

public sealed class AuthStageChangedEventArgs : EventArgs
{
    public AuthStageChangedEventArgs(AuthStage stage)
    {
        Stage = stage;
    }

    public AuthStage Stage { get; }
}
