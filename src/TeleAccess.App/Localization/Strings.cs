using System.Globalization;
using System.Resources;

namespace TeleAccess.App.Localization;

public static class Strings
{
    private static readonly ResourceManager _manager =
        new("TeleAccess.App.Localization.Strings", typeof(Strings).Assembly);

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    private static string Get(string key) => _manager.GetString(key, Culture) ?? key;

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
    public static string Cmd_OpenChat => Get(nameof(Cmd_OpenChat));

    public static string Cmd_Newline => Get(nameof(Cmd_Newline));

    public static string Cmd_MarkUnread => Get(nameof(Cmd_MarkUnread));

    public static string Cmd_DeleteChat => Get(nameof(Cmd_DeleteChat));

    public static string Status_ChatDeleted => Get(nameof(Status_ChatDeleted));

    public static string Status_MarkedUnread => Get(nameof(Status_MarkedUnread));

    public static string Status_MarkedRead => Get(nameof(Status_MarkedRead));

    public static string Prompt_ConfirmDeleteChat => Get(nameof(Prompt_ConfirmDeleteChat));

    public static string Prompt_Yes => Get(nameof(Prompt_Yes));

    public static string Hotkeys_ResetAll => Get(nameof(Hotkeys_ResetAll));

    public static string Error_NotConfigured => Get(nameof(Error_NotConfigured));

    public static string Main_SignedInAs => Get(nameof(Main_SignedInAs));
    public static string Cmd_Mute => Get(nameof(Cmd_Mute));

    public static string Cmd_PinChat => Get(nameof(Cmd_PinChat));

    public static string Cmd_ArchiveChat => Get(nameof(Cmd_ArchiveChat));

    public static string Cmd_Scheduled => Get(nameof(Cmd_Scheduled));

    public static string Cmd_React => Get(nameof(Cmd_React));

    public static string Cmd_SearchInChat => Get(nameof(Cmd_SearchInChat));

    public static string Cmd_SendSilent => Get(nameof(Cmd_SendSilent));

    public static string Cmd_SendScheduled => Get(nameof(Cmd_SendScheduled));

    public static string Status_Muted => Get(nameof(Status_Muted));

    public static string Status_Unmuted => Get(nameof(Status_Unmuted));

    public static string Status_ChatPinned => Get(nameof(Status_ChatPinned));

    public static string Status_ChatUnpinned => Get(nameof(Status_ChatUnpinned));

    public static string Status_Archived => Get(nameof(Status_Archived));

    public static string Status_Unarchived => Get(nameof(Status_Unarchived));

    public static string Status_ReactionSent => Get(nameof(Status_ReactionSent));

    public static string Status_ScheduledFor => Get(nameof(Status_ScheduledFor));

    public static string Status_FoundMessages => Get(nameof(Status_FoundMessages));

    public static string Status_ScheduledMessages => Get(nameof(Status_ScheduledMessages));

    public static string Prompt_Reaction => Get(nameof(Prompt_Reaction));

    public static string Prompt_ScheduleWhen => Get(nameof(Prompt_ScheduleWhen));

    public static string Prompt_SearchInChat => Get(nameof(Prompt_SearchInChat));

    public static string Error_BadTime => Get(nameof(Error_BadTime));

    public static string Status_NotScheduled => Get(nameof(Status_NotScheduled));

    public static string Status_MutedChatSend => Get(nameof(Status_MutedChatSend));

    public static string Schedule_Title => Get(nameof(Schedule_Title));

    public static string Schedule_Presets => Get(nameof(Schedule_Presets));

    public static string Schedule_Preset10Minutes => Get(nameof(Schedule_Preset10Minutes));

    public static string Schedule_PresetHour => Get(nameof(Schedule_PresetHour));

    public static string Schedule_PresetThreeHours => Get(nameof(Schedule_PresetThreeHours));

    public static string Schedule_PresetTomorrow => Get(nameof(Schedule_PresetTomorrow));

    public static string Schedule_PresetMonday => Get(nameof(Schedule_PresetMonday));

    public static string Schedule_Manual => Get(nameof(Schedule_Manual));

    public static string Schedule_ManualHint => Get(nameof(Schedule_ManualHint));

    public static string Schedule_Date => Get(nameof(Schedule_Date));

    public static string Schedule_Time => Get(nameof(Schedule_Time));

    public static string Schedule_Send => Get(nameof(Schedule_Send));

    public static string Schedule_Invalid => Get(nameof(Schedule_Invalid));

    public static string Cmd_Attach => Get(nameof(Cmd_Attach));

    public static string Status_Attached => Get(nameof(Status_Attached));

    public static string Error_FileNotFound => Get(nameof(Error_FileNotFound));

    public static string Schedule_PickDate => Get(nameof(Schedule_PickDate));

    public static string Schedule_Year => Get(nameof(Schedule_Year));

    public static string Schedule_Month => Get(nameof(Schedule_Month));

    public static string Schedule_Day => Get(nameof(Schedule_Day));

    public static string Schedule_Hour => Get(nameof(Schedule_Hour));

    public static string Schedule_Minute => Get(nameof(Schedule_Minute));

    public static string Cmd_SwitchList => Get(nameof(Cmd_SwitchList));

    public static string Status_VoiceRecording => Get(nameof(Status_VoiceRecording));

    public static string Status_VoiceSent => Get(nameof(Status_VoiceSent));

    public static string Status_VoiceCancelled => Get(nameof(Status_VoiceCancelled));

    public static string Error_NoMicrophone => Get(nameof(Error_NoMicrophone));

    public static string Status_AudioSent => Get(nameof(Status_AudioSent));

    public static string Status_ListMain => Get(nameof(Status_ListMain));

    public static string Status_ListArchive => Get(nameof(Status_ListArchive));

    public static string Error_UnsupportedAudio => Get(nameof(Error_UnsupportedAudio));

    public static string Cmd_JumpPinned => Get(nameof(Cmd_JumpPinned));

    public static string Cmd_DeleteForMe => Get(nameof(Cmd_DeleteForMe));

    public static string Status_NoPinned => Get(nameof(Status_NoPinned));

    public static string Status_ListSwitched => Get(nameof(Status_ListSwitched));

    public static string Cmd_GlobalSearch => Get(nameof(Cmd_GlobalSearch));

    public static string Prompt_GlobalSearch => Get(nameof(Prompt_GlobalSearch));

    public static string Cmd_Translate => Get(nameof(Cmd_Translate));

    public static string Prompt_TranslateTo => Get(nameof(Prompt_TranslateTo));

    public static string Cmd_SendContact => Get(nameof(Cmd_SendContact));

    public static string Prompt_ContactPhone => Get(nameof(Prompt_ContactPhone));

    public static string Prompt_ContactName => Get(nameof(Prompt_ContactName));

    public static string Status_ContactSent => Get(nameof(Status_ContactSent));

    public static string Cmd_SendLocation => Get(nameof(Cmd_SendLocation));

    public static string Prompt_Location => Get(nameof(Prompt_Location));

    public static string Status_LocationSent => Get(nameof(Status_LocationSent));

    public static string Error_BadLocation => Get(nameof(Error_BadLocation));

    public static string Cmd_SendPoll => Get(nameof(Cmd_SendPoll));

    public static string Prompt_PollQuestion => Get(nameof(Prompt_PollQuestion));

    public static string Prompt_PollOptions => Get(nameof(Prompt_PollOptions));

    public static string Status_PollSent => Get(nameof(Status_PollSent));

    public static string Status_VideoSent => Get(nameof(Status_VideoSent));

    public static string Status_AnimationSent => Get(nameof(Status_AnimationSent));

    public static string Status_StickerSent => Get(nameof(Status_StickerSent));

    public static string Error_VideoUnreadable => Get(nameof(Error_VideoUnreadable));

    public static string Cmd_ChatInfo => Get(nameof(Cmd_ChatInfo));

    public static string Cmd_ChatMembers => Get(nameof(Cmd_ChatMembers));

    public static string Cmd_ChatTopics => Get(nameof(Cmd_ChatTopics));

    public static string Cmd_ChatJoin => Get(nameof(Cmd_ChatJoin));

    public static string Cmd_Contacts => Get(nameof(Cmd_Contacts));

    public static string Cmd_SecretChat => Get(nameof(Cmd_SecretChat));

    public static string Cmd_Settings => Get(nameof(Cmd_Settings));

    public static string Status_NoMembers => Get(nameof(Status_NoMembers));

    public static string Status_NoTopics => Get(nameof(Status_NoTopics));

    public static string Status_NoContacts => Get(nameof(Status_NoContacts));

    public static string Status_Joined => Get(nameof(Status_Joined));

    public static string Prompt_Username => Get(nameof(Prompt_Username));

    public static string Prompt_SecretChatUser => Get(nameof(Prompt_SecretChatUser));

    public static string Status_SecretChatStarted => Get(nameof(Status_SecretChatStarted));

    public static string Status_NoUser => Get(nameof(Status_NoUser));

    public static string Settings_Title => Get(nameof(Settings_Title));

    public static string Settings_Language => Get(nameof(Settings_Language));

    public static string Settings_ApiId => Get(nameof(Settings_ApiId));

    public static string Settings_ApiHash => Get(nameof(Settings_ApiHash));

    public static string Settings_Phone => Get(nameof(Settings_Phone));

    public static string Settings_Save => Get(nameof(Settings_Save));

    public static string Settings_LogOut => Get(nameof(Settings_LogOut));

    public static string Settings_Saved => Get(nameof(Settings_Saved));

    public static string Settings_RestartNeeded => Get(nameof(Settings_RestartNeeded));

    public static string Status_SignedOut => Get(nameof(Status_SignedOut));

    public static string Cmd_Save => Get(nameof(Cmd_Save));

    public static string Cmd_Older => Get(nameof(Cmd_Older));

    public static string Cmd_Training => Get(nameof(Cmd_Training));

    public static string Status_Saved => Get(nameof(Status_Saved));

    public static string Status_NoFile => Get(nameof(Status_NoFile));

    public static string Status_NoOlder => Get(nameof(Status_NoOlder));

    public static string Status_NewMessage => Get(nameof(Status_NewMessage));

    public static string Status_Playing => Get(nameof(Status_Playing));

    public static string Cmd_Select => Get(nameof(Cmd_Select));

    public static string Cmd_SelectAll => Get(nameof(Cmd_SelectAll));

    public static string Status_Window => Get(nameof(Status_Window));

    public static string Cmd_JumpToDate => Get(nameof(Cmd_JumpToDate));

    public static string Prompt_JumpToDate => Get(nameof(Prompt_JumpToDate));

    public static string Error_BadDate => Get(nameof(Error_BadDate));

    public static string Cmd_SetName => Get(nameof(Cmd_SetName));

    public static string Cmd_SetBio => Get(nameof(Cmd_SetBio));

    public static string Prompt_FullName => Get(nameof(Prompt_FullName));

    public static string Prompt_Bio => Get(nameof(Prompt_Bio));

    public static string Status_NameSaved => Get(nameof(Status_NameSaved));

    public static string Status_BioSaved => Get(nameof(Status_BioSaved));

    public static string Error_BadName => Get(nameof(Error_BadName));

    public static string Cmd_Proxy => Get(nameof(Cmd_Proxy));

    public static string Prompt_Proxy => Get(nameof(Prompt_Proxy));

    public static string Status_ProxySet => Get(nameof(Status_ProxySet));

    public static string Error_BadProxy => Get(nameof(Error_BadProxy));

    public static string Cmd_Buttons => Get(nameof(Cmd_Buttons));

    public static string Status_NoButtons => Get(nameof(Status_NoButtons));

    public static string Prompt_ButtonNumber => Get(nameof(Prompt_ButtonNumber));

    public static string Status_ButtonPressed => Get(nameof(Status_ButtonPressed));

    public static string Cmd_Rename => Get(nameof(Cmd_Rename));

    public static string Prompt_ChatTitle => Get(nameof(Prompt_ChatTitle));

    public static string Status_TitleSaved => Get(nameof(Status_TitleSaved));

    public static string Error_BadTitle => Get(nameof(Error_BadTitle));

    public static string Cmd_InviteLink => Get(nameof(Cmd_InviteLink));

    public static string Prompt_InviteDays => Get(nameof(Prompt_InviteDays));

    public static string Error_BadNumber => Get(nameof(Error_BadNumber));

    public static string Cmd_MuteScopes => Get(nameof(Cmd_MuteScopes));

    public static string Cmd_Stories => Get(nameof(Cmd_Stories));

    public static string Prompt_MuteScopes => Get(nameof(Prompt_MuteScopes));

    public static string Prompt_MuteHours => Get(nameof(Prompt_MuteHours));

    public static string Status_NoStories => Get(nameof(Status_NoStories));

}
