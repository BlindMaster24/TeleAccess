# Что умеет TDLib — инвентарь по схеме (2026-11)

Источник — `docs/td_api.tl`, схема с master `tdlib/td`: **17 696 строк,
1 076 функций, 2 289 типов**. Всё, что ниже, проверено по ней, а не по памяти.

## 1. Отправка сообщений: ровно 23 типа контента

Только `sendMessage` (одно) и `sendMessageAlbum` (группа). Отдельных `sendVoice`,
`sendPhoto`, `sendDocument` **не существует** — это устаревшие представления.
Контент задаётся подтипом `InputMessageContent`:

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

**Голосовое** = `sendMessage` + `inputMessageVoiceNote(voiceNote=VoiceNote)`,
где `VoiceNote` содержит `duration`, `waveform`, `mime_type`. Запись с микрофона
и кодирование в Opus — **наша сторона**, TDLib файл только принимает.
Чтение голосового на входе — `messageVoiceNote`.

## 2. Файлы: `uploadFile` больше нет

| Есть | Что делает |
|---|---|
| `preliminaryUploadFile` | залить файл заранее, получить `file_id` |
| `readFilePart` | **потоковая** передача (так и заливается файл) |
| `downloadFile` / `cancelDownloadFile` | скачивание |
| `getFile` / `getRemoteFile` | метаданные и прогресс |
| `deleteFile` | удалить из локального кеша |

## 3. Действия с сообщениями: 156 функций

Для контекстного меню и горячих клавиш хватает этого ядра:

| Действие | Функция |
|---|---|
| Ответ / ответ в тему | `sendMessage` + `reply_to` |
| Изменить текст | `editMessageText` |
| Удалить (у себя / у всех) | `deleteMessages` (`revoke`) |
| Переслать | `forwardMessages` (одиночное — тоже он) |
| Закрепить / открепить | `pinChatMessage`, `unpinChatMessage` |
| Реакция / теги | `getMessageAvailableReactions` → `addMessageReaction` |
| Повторить отправку | `resendMessages` (ждёт `retry_after` при флуде) |
| «печатает…» / «записывает голосовое» | `sendChatAction` |
| Скопировать текст | клиентская сторона, не TDLib |
| Выделить несколько | клиентская сторона (набор `message_id`) |

## 4. Навигация и поиск: 120 функций

`loadChats`, `getChats`, `searchChats`, `searchChatsOnServer`, `searchMessages`,
`searchChatMessages`, `searchSecretMessages`, `getChat`, `getChatHistory`,
`getMessage`, `getMessages`, `joinChat`, `leaveChat`, `createPrivateChat`,
`createNewSecretChat`, `getTopChats`, `toggleChatIsPinned`, `addChatToList`
(папки), плюс темы форумов (`getForumTopics`, …).

## 5. Звонки и секретные чаты

- Сигналинг звонка в TDLib **есть**: `createCall`, `acceptCall`,
  `sendCallSignalingData`, `discardCall`, `sendCallRating`,
  `sendCallDebugInformation`, `sendCallLog`.
- Медиа (WebRTC, ICE/DTLS/SRTP, эхо-подавление) **отсутствует** — это отдельный
  движок, см. `RESEARCH.md` §2–3.
- Секретные чаты: `createNewSecretChat`, `getSecretChat`, `closeSecretChat`;
  дальше обычный `sendMessage` в `secret_chat_id`.

## 6. Чего в TDLib нет вообще — и это наша работа

| Нет | Значит |
|---|---|
| Понятия «горячая клавиша», «команда», «контекстное меню» | только `quickReplyShortcut*` — это заготовки ответов Telegram, не клавиатура |
| Записи с микрофона и кодирования Opus | WASAPI + Opus-энкодер |
| Отрисовки голосовой волны | считаем сами из `waveform` |
| Воспроизведения аудио | NAudio |
| Уведомлений ОС | наши тосты |
| Доступности, скринридера, фокуса | WPF + UIA |
| Форматирования дат и чисел словами | Humanizer |
| Локального поиска по скачанному | либо `searchChatMessages`, либо свой индекс |

**Вывод:** TDLib закрывает сеть, авторизацию, кеш, все действия с сообщениями и
поиск — 100% «телеграмной» части. Всё, что делает клиента клиентом для незрячих
(клавиатура, команды, озвучка, запись голоса), — наш слой, и TDLib тут не мешает.
