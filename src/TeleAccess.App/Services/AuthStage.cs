namespace TeleAccess.App.Services;

public enum AuthStage
{
    Starting,
    NeedParameters,
    NeedPhone,
    NeedCode,
    NeedPassword,
    Ready,
    Closed
}
