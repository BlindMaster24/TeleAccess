using System.Globalization;
using System.Resources;

namespace TeleAccess.App.Localization;

public static class Strings
{
    private static readonly ResourceManager Manager =
        new("TeleAccess.App.Localization.Strings", typeof(Strings).Assembly);

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    private static string Get(string key) => Manager.GetString(key, Culture) ?? key;

    public static string AppTitle => Get(nameof(AppTitle));

    public static string Login_Title => Get(nameof(Login_Title));

    public static string Login_ApiId => Get(nameof(Login_ApiId));

    public static string Login_ApiHash => Get(nameof(Login_ApiHash));

    public static string Login_Phone => Get(nameof(Login_Phone));

    public static string Login_Code => Get(nameof(Login_Code));

    public static string Login_Password => Get(nameof(Login_Password));

    public static string Login_Continue => Get(nameof(Login_Continue));

    public static string Login_HintNeedParameters => Get(nameof(Login_HintNeedParameters));

    public static string Login_HintNeedPhone => Get(nameof(Login_HintNeedPhone));

    public static string Login_HintNeedCode => Get(nameof(Login_HintNeedCode));

    public static string Login_HintNeedPassword => Get(nameof(Login_HintNeedPassword));

    public static string Login_HintReady => Get(nameof(Login_HintReady));

    public static string Login_Starting => Get(nameof(Login_Starting));

    public static string Login_Closed => Get(nameof(Login_Closed));

    public static string Login_Failed => Get(nameof(Login_Failed));

    public static string Main_Search => Get(nameof(Main_Search));

    public static string Main_Chats => Get(nameof(Main_Chats));

    public static string Main_Messages => Get(nameof(Main_Messages));

    public static string Main_Composer => Get(nameof(Main_Composer));

    public static string Main_Unread => Get(nameof(Main_Unread));

    public static string Main_NoChats => Get(nameof(Main_NoChats));

    public static string Main_NoMessages => Get(nameof(Main_NoMessages));

    public static string Main_Status => Get(nameof(Main_Status));

    public static string Cmd_Search => Get(nameof(Cmd_Search));

    public static string Cmd_FocusChats => Get(nameof(Cmd_FocusChats));

    public static string Cmd_FocusMessages => Get(nameof(Cmd_FocusMessages));

    public static string Cmd_FocusComposer => Get(nameof(Cmd_FocusComposer));

    public static string Cmd_Send => Get(nameof(Cmd_Send));

    public static string Cmd_Reply => Get(nameof(Cmd_Reply));

    public static string Cmd_Delete => Get(nameof(Cmd_Delete));

    public static string Cmd_Edit => Get(nameof(Cmd_Edit));

    public static string Cmd_Forward => Get(nameof(Cmd_Forward));

    public static string Cmd_Pin => Get(nameof(Cmd_Pin));

    public static string Cmd_Unpin => Get(nameof(Cmd_Unpin));

    public static string Cmd_Copy => Get(nameof(Cmd_Copy));

    public static string Cmd_Voice => Get(nameof(Cmd_Voice));

    public static string Cmd_Listen => Get(nameof(Cmd_Listen));

    public static string Cmd_Menu => Get(nameof(Cmd_Menu));

    public static string Cmd_Hotkeys => Get(nameof(Cmd_Hotkeys));

    public static string Cmd_Refresh => Get(nameof(Cmd_Refresh));

    public static string Cmd_Quit => Get(nameof(Cmd_Quit));

    public static string Status_Sent => Get(nameof(Status_Sent));

    public static string Status_DeletedForAll => Get(nameof(Status_DeletedForAll));

    public static string Status_DeletedLocal => Get(nameof(Status_DeletedLocal));

    public static string Status_Copied => Get(nameof(Status_Copied));

    public static string Status_Forwarded => Get(nameof(Status_Forwarded));

    public static string Status_Pinned => Get(nameof(Status_Pinned));

    public static string Status_Unpinned => Get(nameof(Status_Unpinned));

    public static string Status_NoSelection => Get(nameof(Status_NoSelection));

    public static string Status_NoChat => Get(nameof(Status_NoChat));

    public static string Status_NotImplemented => Get(nameof(Status_NotImplemented));

    public static string Status_Unavailable => Get(nameof(Status_Unavailable));

    public static string Status_Edited => Get(nameof(Status_Edited));

    public static string Status_SearchEmpty => Get(nameof(Status_SearchEmpty));

    public static string Hotkeys_Title => Get(nameof(Hotkeys_Title));

    public static string Hotkeys_Hint => Get(nameof(Hotkeys_Hint));

    public static string Hotkeys_PressKey => Get(nameof(Hotkeys_PressKey));

    public static string Hotkeys_Cancelled => Get(nameof(Hotkeys_Cancelled));

    public static string Hotkeys_Conflict => Get(nameof(Hotkeys_Conflict));

    public static string Hotkeys_Saved => Get(nameof(Hotkeys_Saved));

    public static string Hotkeys_Invalid => Get(nameof(Hotkeys_Invalid));

    public static string Hotkeys_Command => Get(nameof(Hotkeys_Command));

    public static string Hotkeys_Shortcut => Get(nameof(Hotkeys_Shortcut));

    public static string Prompt_Ok => Get(nameof(Prompt_Ok));

    public static string Prompt_Cancel => Get(nameof(Prompt_Cancel));

    public static string Prompt_ForwardTo => Get(nameof(Prompt_ForwardTo));

    public static string Prompt_EditText => Get(nameof(Prompt_EditText));

    public static string Common_Loading => Get(nameof(Common_Loading));
    public static string Error_Generic => Get(nameof(Error_Generic));

    public static string Error_Network => Get(nameof(Error_Network));

    public static string Error_FloodWait => Get(nameof(Error_FloodWait));

    public static string Error_NotFound => Get(nameof(Error_NotFound));

    public static string Error_Permission => Get(nameof(Error_Permission));

    public static string Error_Auth => Get(nameof(Error_Auth));

    public static string Error_File => Get(nameof(Error_File));

    public static string Plural_UnreadMessages => Get(nameof(Plural_UnreadMessages));

    public static string Plural_SelectedMessages => Get(nameof(Plural_SelectedMessages));
    public static string Error_TdlibMissing => Get(nameof(Error_TdlibMissing));
}
