# What TDLib can do

Inventory taken from `docs/td_api.tl`, the schema from `tdlib/td` master: **17 696
lines, 1 076 functions, 2 289 types**. Everything below was checked against that file,
not against memory. Fetch it with:

```shell
curl -sL -o docs/td_api.tl https://raw.githubusercontent.com/tdlib/td/master/td/generate/scheme/td_api.tl
```

## 1. Sending messages: 23 content types

There is only `sendMessage` (one) and `sendMessageAlbum` (a group). Separate
`sendVoice`, `sendPhoto` or `sendDocument` functions **do not exist** in the current
schema; content is a subtype of `InputMessageContent`:

```
inputMessageText          inputMessageVoiceNote     inputMessageVideoNote
inputMessagePhoto         inputMessageVideo         inputMessageAnimation
inputMessageAudio         inputMessageDocument      inputMessageSticker
inputMessageLocation      inputMessageLiveLocation  inputMessageVenue
inputMessageContact       inputMessagePoll          inputMessageChecklist
inputMessageDice          inputMessageStakeDice     inputMessageGame
inputMessageInvoice       inputMessagePaidMedia     inputMessageStory
inputMessageForwarded     inputMessageRichMessage
```

A **voice message** is `sendMessage` with `inputMessageVoiceNote(voiceNote=VoiceNote)`,
where `VoiceNote` carries `duration`, `waveform` and the file. Recording from the
microphone and Opus encoding happen on our side; TDLib only accepts the file. Incoming
voice messages arrive as `messageVoiceNote`.

## 2. Files: `uploadFile` no longer exists

| Function | Purpose |
| --- | --- |
| `preliminaryUploadFile` | upload a file ahead of time, receive `file_id` |
| `readFilePart` | streaming transfer (this is how uploads actually run) |
| `downloadFile` / `cancelDownloadFile` | downloads |
| `getFile` / `getRemoteFile` | metadata and progress |
| `deleteFile` | remove from the local cache |

## 3. Message actions: 156 functions

The core used by this client:

| Action | Function |
| --- | --- |
| Reply, reply in a topic | `sendMessage` + `reply_to` |
| Edit text | `editMessageText` |
| Delete (for me / for everyone) | `deleteMessages` (`revoke`) |
| Forward | `forwardMessages` |
| Pin / unpin | `pinChatMessage`, `unpinChatMessage` |
| Jump to pinned | `getChatPinnedMessage` |
| Reactions | `getMessageAvailableReactions` then `addMessageReaction` |
| Resend after flood wait | `resendMessages` |
| Typing / recording indicator | `sendChatAction` |
| Scheduled queue | `getChatScheduledMessages` + `messageSchedulingState` |
| Translate | `translateText` |

## 4. Search and navigation: 120 functions

`loadChats`, `getChats`, `searchChats`, `searchChatsOnServer`, `searchMessages`,
`searchChatMessages`, `searchSecretMessages`, `getChat`, `getChatHistory`,
`getMessage`, `getMessages`, `joinChat`, `leaveChat`, `createPrivateChat`,
`createNewSecretChat`, `getTopChats`, `toggleChatIsPinned`, `addChatToList`,
`setChatNotificationSettings`. Chat lists are `chatListMain`, `chatListArchive` and
`chatListFolder(folder_id)`; folders arrive through `updateChatFolders`. Forums expose
`getForumTopics`, `getForumTopic`, `createForumTopic`.

## 5. Calls, secret chats, forums

- Call signalling lives in TDLib: `createCall`, `acceptCall`, `sendCallSignalingData`,
  `discardCall`, `sendCallRating`, `sendCallDebugInformation`, `sendCallLog`.
- Media (WebRTC, ICE/DTLS/SRTP, echo cancellation) is **not** part of TDLib: that is a
  separate engine, see `RESEARCH.md`.
- Secret chats: `createNewSecretChat`, `getSecretChat`, `closeSecretChat`; afterwards a
  normal `sendMessage` with `secret_chat_id`.
- Forums: topic-aware `sendMessage` takes `topic_id`, and `getForumTopics` lists them.

## 6. What TDLib has no concept of — our own work

| Missing | Consequence |
| --- | --- |
| Hotkeys, commands, context menus | only `quickReplyShortcut*`, which are saved replies, not keyboard |
| Microphone capture and Opus encoding | WASAPI plus an Opus encoder |
| Waveform rendering | computed from the `waveform` bytes |
| Audio playback | NAudio |
| OS notifications | our own toasts |
| Accessibility, screen reader, focus | WPF plus UI Automation |
| Dates and numbers in words | Humanizer-style formatting |
| Local search over downloaded files | `searchChatMessages` or our own index |

**Conclusion:** TDLib covers the network, authorization, cache, every message action and
search — the entire "Telegram" half. Everything that makes the client usable for a blind
user (keyboard, commands, speech, voice recording) is our layer, and TDLib does not get
in its way.
