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
Удерживайте **Escape 10 секунд** — активная программа принудительно
закрывается, как «Снять задачу» в Диспетчере задач.

Работает с любым режимом окна — настоящим полноэкранным (fullscreen) и
оконным без рамки (borderless) — так что не важно, как именно запущена
игра. Если она зависла намертво и не реагирует ни на что — ни Alt+Tab,
ни Ctrl+Alt+Del не помогают — просто держите Escape, и зависший процесс
закроется.

### 🟢 Escape-Stash
Нажмите **Escape + X вместе** (в любом порядке) — активное окно мгновенно
сворачивается на панель задач. Пригодится для старых или упрямых
полноэкранных игр, которые отказываются сворачиваться обычным способом.

Обе функции можно включать и выключать по отдельности через меню в трее.
Системные процессы — проводник, меню «Пуск», панель задач — защищены от
случайного закрытия.

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
Hold **Escape for 10 seconds** — the active app gets force-terminated,
same as "End Task" in Task Manager.

Works with any window mode — true exclusive fullscreen or borderless
windowed — so it doesn't matter how the game runs. If it's frozen solid
and won't respond to anything — not Alt+Tab, not Ctrl+Alt+Del — just hold
Escape and the frozen process gets closed.

### 🟢 Escape-Stash
Press **Escape + X together** (either order) — the active window is
instantly minimized to the taskbar. Handy for older or stubborn
fullscreen games that won't minimize the normal way.

Both features can be toggled independently from the tray menu. System
processes — explorer, the Start menu, the taskbar — are protected against
accidental termination.

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
