# EscapeX

![Version](https://img.shields.io/badge/version-1.0-39FF14?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows%207%20SP1%2B-lightgrey?style=flat-square)

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

### Антивирусы

Приложение использует низкоуровневый хук клавиатуры (`WH_KEYBOARD_LL`) и
запрашивает права администратора для автозапуска и принудительного
завершения процессов — иногда это вызывает ложные срабатывания
эвристических/ML-антивирусов (типично для небольших неподписанных утилит).

[Результат проверки на VirusTotal](https://www.virustotal.com/gui/file/636d0065e7cb28c96a445584c33bdf4f4f2ec7f15a3381d205afba3b492a205f)

### Поддержать автора

Если EscapeX оказался полезен — можно поддержать разработку здесь:
[Boosty](PASTE_BOOSTY_LINK_HERE)

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

### Antivirus notes

The app uses a low-level keyboard hook (`WH_KEYBOARD_LL`) and requests
administrator rights for autostart and force-killing processes — this
occasionally triggers false positives from heuristic/ML-based antivirus
engines (common for small unsigned tools).

[VirusTotal scan results](https://www.virustotal.com/gui/file/636d0065e7cb28c96a445584c33bdf4f4f2ec7f15a3381d205afba3b492a205f)

### Support

If EscapeX has been useful to you, you can support development here:
[Boosty](PASTE_BOOSTY_LINK_HERE)
