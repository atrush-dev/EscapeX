# EscapeX

![Version](https://img.shields.io/badge/version-1.0-39FF14?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows%207%20SP1%2B-lightgrey?style=flat-square)

<p align="center">
  <a href="#русский">Русский</a> •
  <a href="#english">English</a>
</p>

---

## Русский

### Аварийная кнопка для зависших игр и приложений Windows

**EscapeX** — небольшая утилита для Windows, созданная для одной простой задачи: помочь вернуть контроль над системой, когда игра или приложение перестали отвечать.

Иногда случается самое неприятное: игра зависает в полноэкранном режиме, окно остаётся поверх всех программ, Alt+Tab не работает, а стандартное закрытие не помогает. В таких ситуациях EscapeX выступает как аварийный инструмент.

### Возможности

#### 🔴 Escape-Kill

Зажмите **Escape на 10 секунд** — зависшее приложение будет принудительно завершено.

Особенность функции в том, что целевое окно запоминается именно в момент нажатия Escape. Если во время ожидания изменится фокус, случайное другое приложение не будет закрыто.

Подходит для:

- зависших игр;
- полноэкранных приложений;
- программ, которые перестали реагировать на обычные действия.

Работает по принципу «Снять задачу» в Диспетчере задач Windows.

#### 🟢 Escape-Stash

Нажмите **Escape + X** (в любом порядке) — активное окно будет мгновенно свёрнуто.

Полезно для старых игр и приложений, которые не хотят нормально сворачиваться через стандартные сочетания Windows.

Функцию можно включать и отключать отдельно через меню в системном трее.

### Особенности

- ✅ Работает тихо в фоне через системный трей
- ✅ Один автономный `.exe` без дополнительных файлов
- ✅ Русский и английский интерфейс
- ✅ Автоматический запуск вместе с Windows
- ✅ Защита важных системных процессов
- ✅ Журнал событий для диагностики

### Установка

1. Скачайте `EscapeX.exe` из раздела [Releases](../../releases).
2. Запустите программу.
3. При первом запуске потребуется подтверждение прав администратора.
4. После настройки программа появится в системном трее.

Дополнительные файлы рядом с программой не требуются.

Создаваемый файл: `escapex_log.txt` — используется только для журнала событий.

### Требования

- Windows 7 SP1 или новее
- Права администратора (запрашиваются только при первой настройке)

### Почему антивирус может реагировать?

EscapeX использует системные возможности Windows, необходимые для работы:

- низкоуровневый клавиатурный хук (`WH_KEYBOARD_LL`);
- управление процессами;
- повышенные права для отдельных операций.

Некоторые антивирусы могут ошибочно реагировать на такие действия у небольших неподписанных утилит. Это связано с особенностями эвристического анализа, а не с назначением программы.

Проверка: [VirusTotal Scan](https://www.virustotal.com/gui/file/636d0065e7cb28c96a445584c33bdf4f4f2ec7f15a3381d205afba3b492a205f)

---

## English

### Emergency button for frozen Windows games and applications

**EscapeX** is a small Windows utility designed for one simple purpose: helping users regain control when a game or application stops responding.

Sometimes a fullscreen game gets stuck, stays above all other windows, Alt+Tab stops working, and normal closing methods fail. EscapeX works as an emergency tool for these situations.

### Features

#### 🔴 Escape-Kill

Hold **Escape for 10 seconds** — the frozen application will be force-terminated.

The target window is captured at the moment Escape is pressed. If focus changes during the countdown, another application will not be accidentally closed.

Useful for:

- frozen games;
- fullscreen applications;
- unresponsive programs.

Works similarly to Windows Task Manager's "End Task" function.

#### 🟢 Escape-Stash

Press **Escape + X** (in any order) — the active window is instantly minimized.

Useful for older games and applications that refuse to minimize normally.

The feature can be enabled or disabled separately from the tray menu.

### Features

- ✅ Runs quietly in the system tray
- ✅ Single portable `.exe` file
- ✅ Russian and English interface
- ✅ Windows startup support
- ✅ Protection against accidental system process termination
- ✅ Event logging

### Installation

1. Download `EscapeX.exe` from [Releases](../../releases).
2. Run the application.
3. Administrator approval is required during first setup.
4. The application will appear in the system tray.

No additional files are required.

Created file: `escapex_log.txt` — used only for event logging.

### Requirements

- Windows 7 SP1 or newer
- Administrator privileges (requested only during first setup)

### Antivirus notes

EscapeX uses Windows features required for its functionality:

- low-level keyboard hook (`WH_KEYBOARD_LL`);
- process management;
- elevated privileges for certain operations.

Some antivirus engines may incorrectly flag small unsigned utilities using these features. This is related to heuristic detection and does not reflect the purpose of the application.

Scan: [VirusTotal Scan](https://www.virustotal.com/gui/file/636d0065e7cb28c96a445584c33bdf4f4f2ec7f15a3381d205afba3b492a205f)
