# EscapeX

![Version](https://img.shields.io/badge/version-1.0-39FF14?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows%207%20SP1%2B-lightgrey?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

<p align="center">
  <a href="#русский">Русский</a> •
  <a href="#english">English</a>
</p>

---

## Русский

Лёгкая трей-утилита для Windows: две горячие клавиши для принудительного
управления зависшими или полноэкранными окнами — на случай, когда Alt+Tab
и Ctrl+Alt+Del не помогают.

### 🔴 Escape-Kill
Удерживайте **Escape 10 секунд** — процесс, который был активен в этот
момент, принудительно завершается (`Process.Kill()`, как «Снять задачу» в
Диспетчере задач). Целевое окно фиксируется в момент нажатия Escape, а не
в момент срабатывания таймера — так что если фокус за это время сменится,
завершится изначально активное окно, а не то, что оказалось сверху позже.

Полезно для зависших полноэкранных игр и приложений.

### 🟢 Escape-Stash
Нажмите **Escape + X вместе** (в любом порядке) — активное окно мгновенно
сворачивается. Полезно для старых или полноэкранных игр, которые не
сворачиваются обычным способом.

Обе функции переключаются независимо из меню трея и защищены от случайного
завершения системных процессов (explorer, службы Windows, процессы меню
«Пуск» / панели задач).

### Установка

1. Скачайте `EscapeX.exe` из [Releases](../../releases).
2. Запустите — при первом запуске программа запросит права администратора
   (нужны, чтобы `Process.Kill()` работал и с процессами, уже запущенными
   от администратора) и один раз зарегистрирует себя в Планировщике заданий
   Windows для автозапуска.
3. Готово — появится значок в трее, правый клик открывает настройки.

Никаких дополнительных файлов рядом не требуется — всё, включая иконки,
встроено в exe. Единственный файл, который появляется сам — `escapex_log.txt`,
журнал событий.

Требуется Windows 7 SP1 или новее и права администратора (запрашиваются
один раз при первом запуске).

### Сборка из исходников

Весь проект — это один файл `Program.cs`, собирается компилятором `csc.exe`,
который идёт в комплекте с Windows — Visual Studio не нужна:

```
csc.exe /target:winexe /out:EscapeX.exe Program.cs
```

### Об антивирусах

Приложение использует низкоуровневый хук клавиатуры (`WH_KEYBOARD_LL`) для
отслеживания Escape/X и запрашивает права администратора для автозапуска и
принудительного завершения процессов. Такие паттерны иногда вызывают
ложные срабатывания эвристических/ML-антивирусов (типично для небольших
неподписанных утилит). Исходный код открыт — можете проверить сами.

### Лицензия

MIT — см. [LICENSE](LICENSE).

### Автор

Alexander Trush

---

## English

A lightweight Windows tray utility: two hotkeys for force-managing frozen or
fullscreen windows, for when Alt+Tab / Ctrl+Alt+Del don't cut it.

### 🔴 Escape-Kill
Hold **Escape for 10 seconds** — the process that was active at that moment
gets force-terminated (`Process.Kill()`, same as "End Task" in Task
Manager). The target window is locked in at the moment you press Escape,
not when the timer runs out — so if focus shifts during the hold, the
originally active window still gets killed, not whatever happens to be on
top later.

Handy for frozen fullscreen games and apps.

### 🟢 Escape-Stash
Press **Escape + X together** (either order) — the active window is
instantly minimized. Useful for older or fullscreen games that won't
minimize the normal way.

Both features can be toggled independently from the tray menu and are
protected against accidentally hitting system processes (explorer, Windows
services, the Start menu / taskbar host processes).

### Installation

1. Download `EscapeX.exe` from [Releases](../../releases).
2. Run it — on first launch it'll ask for administrator rights (needed so
   `Process.Kill()` also works on processes already running as admin) and
   register itself once with Windows Task Scheduler for autostart.
3. Done — the tray icon appears, right-click for settings.

No extra files are needed next to the exe — everything, including icons,
is embedded. The only file that appears on its own is `escapex_log.txt`, an
event log.

Requires Windows 7 SP1 or newer, and administrator rights (requested once
on first run).

### Building from source

The whole project is a single `Program.cs` file, built with the `csc.exe`
compiler that ships with Windows — no Visual Studio required:

```
csc.exe /target:winexe /out:EscapeX.exe Program.cs
```

### Security / antivirus notes

The app uses a low-level keyboard hook (`WH_KEYBOARD_LL`) to track
Escape/X, and requests administrator rights for autostart and force-killing
processes. These patterns occasionally trigger false positives from
heuristic/ML-based antivirus engines (common for small unsigned tools). The
source is open — feel free to check it yourself.

### License

MIT — see [LICENSE](LICENSE).

### Author

Alexander Trush
