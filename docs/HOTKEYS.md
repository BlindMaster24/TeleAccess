# Commands and keyboard shortcuts

TeleAccess is a Telegram client for blind users, so this file is the product, not a
reference appendix. Every action is reachable three ways: a menu item, a context menu
item, and a shortcut.
All three are generated from one command registry, so a button without a shortcut (or a
shortcut without a menu item) cannot exist.

## Command model

```
TeleCommand
├── Id              "message.delete"            stable, used in keymap.json
├── Title           what the user hears         localized
├── Scope           ChatList | Chat | MessageList | Message | Composer | Voice | Global
├── DefaultHotkey   Ctrl+D
├── Hotkey          what is assigned now
├── CanExecute      whether it can run in the current context
└── Execute         what it does
```

A conflict is a duplicate **inside one scope**. `Enter` in the composer (send) and
`Enter` on a message (read aloud) are not a conflict. Assigning an occupied combination
is refused and announced: `Already used by: <command>`.

`Escape` is reserved for cancel. `Shift+F10` and the Applications key open the context
menu of the focused area.

## Default keymap

### Global

| Shortcut | Command |
| --- | --- |
| `Alt+1` | focus chat list |
| `Alt+2` | focus message list |
| `Alt+3` | focus composer |
| `Alt+4` | focus search |
| `Ctrl+K` | search chats |
| `Ctrl+Shift+F` | search across all chats |
| `Ctrl+I` | chat information: type, members, description |
| `Ctrl+Shift+I` | list chat members into the composer |
| `Ctrl+T` | list forum topics |
| `Ctrl+J` | join the current chat |
| `Ctrl+Shift+C` | contacts |
| `Ctrl+Shift+X` | start a secret chat |
| `Ctrl+Comma` | settings: language, api_id, api_hash, phone, sign out |
| `Ctrl+Shift+N` | change my name |
| `Ctrl+Shift+B` | change my bio |
| `Ctrl+F1` | training mode: every command announces itself |
| `Ctrl+N` | new chat |
| `Ctrl+Shift+K` | open keyboard shortcut editor |
| `F1` | read the shortcuts of the current area aloud |
| `F5` | refresh |
| `Alt+Left` / `Alt+Right` | previous / next chat |
| `Ctrl+W` | close current chat |

### Chat list

| Shortcut | Command |
| --- | --- |
| `Up` / `Down` | move between chats |
| `Enter` | open chat |
| `Ctrl+D` | delete chat (asks for confirmation, spoken) |
| `Ctrl+Shift+P` | pin / unpin chat |
| `Ctrl+Shift+M` | mute / unmute |
| `Ctrl+Shift+A` | archive / unarchive |
| `Ctrl+Shift+U` | mark as read / unread |
| `Ctrl+Shift+S` | show scheduled messages |
| `Ctrl+Shift+L` | cycle chat lists: main, each Telegram folder, archive |
| `Ctrl+Shift+L` | leave group or channel |
| `Ctrl+Shift+F` | switch folder |
| `Ctrl+Shift+N` | create folder from selection |

### Message list

| Shortcut | Command |
| --- | --- |
| `Up` / `Down` | previous / next message |
| `PageUp` / `PageDown` | page of messages |
| `Home` / `End` | first / last message |
| `Ctrl+G` | jump to a date: `2026-11-04`, `04.11.2026`, `-7d` |
| `Ctrl+Shift+P` | jump to the pinned message |
| `Space` | select or unselect the current message |
| `Ctrl+A` | select all loaded messages |
| `Ctrl+Up` | load the previous page of history |

### Message

| Shortcut | Command |
| --- | --- |
| `Enter` | read the message aloud; voice notes and audio play instead |
| `Ctrl+R` | reply |
| `Ctrl+Shift+R` | reply in another chat |
| `Ctrl+E` | edit (own messages only) |
| `Ctrl+D` | delete: for everyone when own, for me otherwise |
| `Ctrl+Shift+D` | delete for me only |
| `Ctrl+Shift+F` | forward |
| `Ctrl+P` | pin or unpin |
| `Ctrl+Shift+P` | unpin |
| `Ctrl+Shift+E` | add reaction |
| `Ctrl+Shift+T` | translate the message |
| `Ctrl+F` | search in this chat |
| `Ctrl+C` | copy text |
| `Ctrl+S` | download the attachment and announce the path |
| `Shift+F10` | message actions menu |

### Composer

| Shortcut | Command |
| --- | --- |
| `Enter` | send |
| `Shift+Enter` | new line |
| `Ctrl+Enter` | send without notification |
| `Ctrl+Shift+Enter` | schedule the message |
| `Ctrl+Shift+V` | record voice message (push to talk, release sends) |
| `Ctrl+Shift+Space` | start / stop voice recording |
| `Escape` | cancel recording, clear reply |
| `Ctrl+Up` | edit the last own message |
| `Ctrl+O` | attach a file: images become photos, GIFs animations, video files videos, webp/tgs stickers, audio formats audio messages, everything else a document |
| `Ctrl+Alt+C` | send a contact |
| `Ctrl+Alt+L` | send a location as `latitude, longitude` |
| `Ctrl+Alt+P` | create a poll (options separated by semicolons) |

### Voice playback

| Shortcut | Command |
| --- | --- |
| `Space` | play / pause |
| `Escape` | stop playback |
| `+` / `-` | faster / slower |
| `Ctrl+Right` / `Ctrl+Left` | seek forward / back |
| `Escape` | stop |

## Scheduling

`Ctrl+Shift+Enter` opens the schedule window. It offers three ways to pick a moment, and
all of them are keyboard-reachable:

| Way | How |
| --- | --- |
| Quick choices | a list of five presets: in 10 minutes, in an hour, in three hours, tomorrow at 9:00, Monday at 9:00. `Up`/`Down` to choose, `Enter` to schedule |
| Year, month, day, hour, minute | five explicit lists — every value is announced, so a year is chosen directly instead of paging a calendar popup |
| Typed value | type into the manual box; it wins over the lists when filled |

Typed values accept:

| Input | Meaning |
| --- | --- |
| `+10m`, `+2h`, `+1d` | relative to now |
| `18:30` | today at that time, or tomorrow if it has already passed |
| `2026-12-31 09:00`, `31.12.2026 09:00` | exact local date and time |

Anything the parser cannot read, or a moment in the past, is refused with a spoken
message and the value stays where it was.

## Voice messages

| Command | Shortcut | Behavior |
| --- | --- | --- |
| `voice.record` | `Ctrl+Shift+V` | push to talk: release sends |
| `voice.recordToggle` | `Ctrl+Shift+V` | press to start, press again to send and upload |
| `voice.cancel` | `Escape` | discard the draft, nothing is sent |
| `voice.listen` | `Enter` on a voice message | play |
| `voice.listenStop` | `Space` | stop |

Pipeline: WASAPI capture, Opus encode, `preliminaryUploadFile` + `readFilePart`,
then `sendMessage` with `inputMessageVoiceNote`. Sending happens only after the
recording stops; cancelling at any step sends nothing.

## Rebinding without a mouse

The shortcut editor is a plain list. `Up`/`Down` choose a command, `Enter` starts
capture, the next key combination is assigned, `Escape` cancels, `Delete` restores the
default, `Ctrl+Shift+Delete` restores everything. Conflicts are spoken and refused.
The map is stored in `%APPDATA%\TeleAccess\keymap.json`.

## Announcements

- Default: a short earcon plus the command title through the live region.
- Training mode (`Ctrl+F1`): every key press and its command are spoken.
- A disabled command still speaks: `Unavailable: nothing selected`.

## Global Windows shortcuts

`Ctrl+Alt+T` is registered with `RegisterHotKey`: it brings TeleAccess to the front and
focuses the chat list from any application.



`RegisterHotKey` is used for exactly two commands: reply to the last chat and open the
new message in it. Everything else stays inside the window so the client never steals
keys from other applications.
