# Forum topics

Research notes for adding forum topics (TDLib `forumTopic*`), with Unigram as the
reference implementation.

## What TDLib gives us

| Function / type | Purpose |
| --- | --- |
| `getForumTopics(chat_id, query, offset_date, offset_message_id, offset_forum_topic_id, limit)` | page of topics with `total_count` and `next_*` cursors |
| `forumTopic` | `info`, `last_message`, `order`, `is_pinned`, `unread_count`, `last_read_inbox_message_id`, `last_read_outbox_message_id`, `unread_mention_count`, `unread_reaction_count`, `unread_poll_vote_count`, `notification_settings` |
| `forumTopicInfo` | `chat_id`, `forum_topic_id`, `name`, `icon`, `creation_date`, `creator_id`, `is_general`, `is_outgoing`, `is_closed`, `is_hidden`, `is_name_implicit` |
| `getForumTopic(chat_id, forum_topic_id)` | one topic, used after an update |
| `getMessageThreadHistory(chat_id, message_id, from_message_id, offset, limit)` | history **inside** a topic; `message_id` is the topic's last message id |
| `getMessageThread(chat_id, message_id)` | `messageThreadInfo` with the draft of the thread |
| `sendMessage(..., topic_id: MessageTopicForum)` | send into a topic; `reply_to` still applies inside it |
| `createForumTopic`, `editForumTopic`, `deleteForumTopic`, `toggleForumTopicIsClosed`, `toggleForumTopicIsPinned` | lifecycle |
| `readAllForumTopicMentions`, `readAllForumTopicReactions` | clearing the unread badges |
| `updateForumTopicInfo`, `updateForumTopicReadInbox`, `updateForumTopicReadOutbox`, `updateForumTopicIsPinned`, `updateDeleteForumTopic` | live updates |

`MessageTopic` has three shapes: `messageTopicThread`, `messageTopicForum`,
`messageTopicSavedMessagesTopic`. A forum chat is recognised by
`chat.view_as_topics == true` and lists its general topic as a normal topic entry.

## How Unigram does it

- `Services/ForumTopicService.cs` keeps `_topics` by id, plus `_pinnedTopicIds`,
  `_unreadTopicIds`, `_deletedTopicIds`, and an `order` taken from
  `forumTopic.order`; deletion is a tombstone so late updates cannot resurrect a
  topic.
- `ViewModels/TopicListViewModel.cs` is the list: it pins with
  `ToggleForumTopicIsPinned`, closes with `ToggleForumTopicIsClosed`, deletes with
  `DeleteForumTopic`, clears mention and reaction badges with
  `ReadAllForumTopicMentions` / `ReadAllForumTopicReactions`, and reacts to
  `UpdateForumTopicInfo`, `UpdateForumTopicReadInbox`, `UpdateForumTopicReadOutbox`.
- Opening a topic switches the dialog into `DialogType.Topic`-style mode, where
  the composer carries `OutgoingTopicId`; every send and every draft write passes
  that id (`SetChatDraftMessage(chat, OutgoingTopicId, draft)`).
- Messages are loaded per topic, and `MessageCollection` is scoped to the topic.

## Plan for TeleAccess

1. `TelegramService`: `LoadTopicsAsync(chatId)` returning `TopicRow(Id, Name, Unread,
   IsPinned, IsClosed, LastMessageId)`, `LoadTopicMessagesAsync(chatId, lastMessageId)`
   over `getMessageThreadHistory`, and `TopicId` plumbed through `SendTextAsync`,
   `SendDocumentAsync`, `SendVoiceAsync`, `SetDraftAsync`, `LoadOlderAsync`,
   `DeleteMessagesAsync`, `ForwardMessageAsync`.
2. `MessageRow`: add `TopicId` so an action knows where the message lives.
3. UI: when the open chat has `ViewAsTopics`, the chat list pane shows the topic
   list instead of messages; `Enter` opens a topic, `Backspace` returns to the
   topic list, and each row announces name plus unread count.
4. Commands: `chatList.topics` (list), `chatList.topicOpen`, `chatList.topicBack`,
   `chatList.topicCreate` (`Ctrl+Alt+N`), `chatList.topicDelete`, `chatList.topicClose`,
   `chatList.topicPin`. New topics get `icon = null` and `is_name_implicit = false`.
5. Drafts become per topic: `SetChatDraftMessage(chatId, topicId, draft)` and
   `messageThreadInfo.draftMessage` when reading.
6. Accessibility: the topic name is announced before the message count; going back
   says "topic list of <chat>".

Order of work, one commit each: service and rows with tests for the pure parts
(topic ordering by `order`, unread aggregation, name normalisation via
`ChatTitle`), then the UI slice, then the lifecycle commands.
