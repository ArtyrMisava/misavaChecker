# Misava Checker

Windows-приложение на C# и WPF для диагностики системы, отображения состояния компонентов Windows, анализа установленного программного обеспечения и сохранения HWID-снимков.

Проект создаётся как собственная реализация checker-приложения с интерфейсом, близким по структуре к Phoenix Checker.

> Важно для AI-агента: не переписывать проект с нуля и не заменять рабочие файлы без необходимости. Сначала изучить текущую структуру, затем вносить минимальные совместимые изменения.

---

## Технологии

```text
Язык:       C#
UI:         WPF
Framework:  .NET 8 for Windows
Target:     net8.0-windows
Сборка:     WinExe
Архитектура: x64
```

Основной проект:

```text
misavaChecker.sln
└── misavaChecker/
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── Window1.xaml
    ├── Window1.xaml.cs
    ├── DashboardCollector.cs
    ├── SecurityCollector.cs
    ├── SystemFeaturesService.cs
    ├── AssemblyInfo.cs
    └── misavaChecker.csproj
```

Namespace проекта:

```csharp
namespace MisavaChecker;
```

Название файла проекта сохраняется с маленькой буквы:

```text
misavaChecker.csproj
```

Это нормально и не требует переименования.

---

## Назначение файлов

### `MainWindow.xaml`

Главное окно программы:

- верхняя панель;
- карточка системы;
- карточка безопасности;
- программное обеспечение;
- антивирусы и античиты;
- диски;
- отладчики;
- лаунчеры;
- оверлеи;
- мониторы;
- правая панель функций.

### `MainWindow.xaml.cs`

Логика главного окна:

- плавное появление окна;
- перемещение окна мышью;
- сворачивание и закрытие;
- открытие окна HWID;
- обновление dashboard;
- обработчики кнопок функций;
- вызов `DashboardCollector`;
- вызов `SecurityCollector`;
- переключатели Hyper-V, VBS и HVCI.

### `Window1.xaml`

Окно HWID-информации.

Почему используется имя `Window1`: файл уже создан в проекте. Не переименовывать его без необходимости.

### `Window1.xaml.cs`

Сбор и отображение HWID:

- сеть;
- MAC-адреса;
- GUID адаптеров;
- диски;
- серийные номера;
- тома;
- CPU;
- GPU;
- RAM;
- материнская плата;
- BIOS;
- USB;
- HID;
- Audio;
- Bluetooth;
- TPM;
- реестр;
- сохранение TXT;
- копирование;
- JSON-снимок;
- сравнение текущего состояния с базовым снимком.

### `DashboardCollector.cs`

Сбор данных для главного экрана:

- Windows;
- версия и сборка;
- дата установки;
- uptime;
- BIOS mode;
- BIOS version;
- motherboard;
- CPU;
- GPU;
- RAM;
- Hyper-V;
- VBS;
- HVCI;
- Secure Boot;
- TPM;
- Defender;
- UAC;
- DMA.

### `SecurityCollector.cs`

Формирование списка карточек безопасности.

Текущий список:

```text
Вирт. CPU
Гипервизор
Hyper-V
Blocklist
HVCI
Безоп. загрузка
VBS
DMA
UAC
TPM
Cred.Guard
Meltdown
Spectre
BitLocker
Hello PIN
Тест. подпись
VMP
Defender
WMI
Репутация
```

### `SystemFeaturesService.cs`

Операции с системными функциями Windows:

- проверка Hyper-V;
- включение и отключение Hyper-V;
- проверка VBS;
- включение и отключение VBS;
- проверка HVCI;
- включение и отключение HVCI.

Операции, которые меняют настройки Windows, требуют прав администратора и могут требовать перезагрузку.

---

## Текущая архитектура интерфейса

Главное окно имеет следующие зоны:

```text
┌─────────────────────────────────────────────────────────────┐
│ Верхняя панель                                               │
├──────────────────────┬───────────────────────┬──────────────┤
│ Система              │ Безопасность          │ ПО           │
├──────────────────────┴───────────────────────┼──────────────┤
│ Антивирусы и античиты                          │ Диски        │
├──────────────────────┬────────────────────────┴──────────────┤
│ Лаунчеры             │ Оверлеи                 │ Мониторы     │
└──────────────────────┴─────────────────────────┴─────────────┘
                                      Правая панель функций
```

Правая панель содержит:

```text
HV
VBS
HVCI
DMA
UAC
Blocklist
Defender
Античиты
Отладчики
```

---

## Цвета статусов

```text
Красный  #F05252 — включено, обнаружено или активно
Зелёный  #22C981 — выключено, не найдено или чисто
Жёлтый   #E8A83E — установлено, неизвестно или требует внимания
Серый    #858A98 — недоступно
Оранжевый #F07832 — заголовки и акцентный цвет
```

Для карточек безопасности используется логика:

```text
Включено  → красный
Отключено → зелёный
Недоступно/Неизвестно → жёлтый
```

---

## Источники данных Windows

Разрешённые источники для информационного сканирования:

```text
WMI / CIM
Реестр Windows
Windows Security Center
Win32_Process
Win32_Service
Win32_PnPEntity
Win32_DiskDrive
Win32_LogicalDisk
Win32_PhysicalMemory
Win32_VideoController
Win32_Processor
Win32_BIOS
Win32_BaseBoard
Win32_ComputerSystemProduct
Win32_OperatingSystem
NetworkInterface
DriveInfo
```

Для TPM используется отдельный namespace:

```text
root\CIMV2\Security\MicrosoftTpm
```

Класс TPM:

```text
Win32_Tpm
```

---

## План разработки

### Этап 1 — завершить dashboard

- убрать тестовые значения;
- подключить реальные системные данные во все карточки;
- добавить кнопку общего сканирования;
- добавить индикатор процесса сканирования;
- добавить дату и время последнего сканирования.

### Этап 2 — безопасность

- исправить точность Virtualization и Hypervisor;
- добавить отдельную карточку HVCI;
- добавить полноценный DMA status;
- добавить BitLocker по каждому тому;
- добавить Windows Defender status;
- добавить тестовую подпись;
- добавить vulnerable driver blocklist;
- добавить Credential Guard;
- добавить Reputation-based protection.

### Этап 3 — программное обеспечение

Сканировать через реестр:

```text
HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall
```

Определять:

```text
DirectX
Visual C++ 2005–2022
.NET Framework
.NET Runtime
.NET SDK
```

### Этап 4 — антивирусы

Использовать:

```text
root\SecurityCenter2
AntiVirusProduct
службы Windows
процессы
реестр
```

Показывать:

```text
Название
Установлен
Активен
Состояние защиты
Версия
```

### Этап 5 — античиты

Проверять по службам, процессам и стандартным путям установки:

```text
BattlEye
Easy Anti-Cheat
FACEIT
Vanguard
Riot Vanguard
GameGuard
PunkBuster
RICOCHET
VAC / Steam
AntiCheatExpert
EQU8
ESEA
MRAC
```

Не использовать поиск по всему диску. Сначала проверять службы, процессы, реестр и известные каталоги.

### Этап 6 — отладчики и анализаторы

Проверять:

```text
процессы;
службы;
Uninstall registry;
известные каталоги;
ярлыки и App Paths.
```

Разделять состояние:

```text
Установлен
Запущен
Не найден
```

### Этап 7 — лаунчеры и оверлеи

Проверять процессы, службы и установочные пути:

```text
Steam
Epic Games
Battle.net
Riot Client
EA App
GOG Galaxy
Ubisoft Connect
Discord Overlay
Steam Overlay
NVIDIA ShadowPlay
AMD Software
OBS Studio
Overwolf
RTSS
MSI Afterburner
SteelSeries GG
Xbox Game Bar
```

### Этап 8 — обновления

Планируется:

- отображение текущей версии;
- JSON-манифест;
- проверка новой версии;
- SHA-256 файла;
- скачивание обновления;
- перезапуск приложения.

---

## Правила изменения проекта

1. Не переименовывать `Window1` без необходимости.
2. Не менять namespace `MisavaChecker`.
3. Не удалять `System.Management` из `.csproj`.
4. Перед изменением делать Git-коммит.
5. После изменения запускать сборку через `Ctrl + Shift + B`.
6. Не помещать WMI-логику внутрь XAML.
7. Каждый новый collector размещать в отдельном `.cs` файле.
8. Не использовать жёстко прописанные системные значения в dashboard.
9. Не выполнять изменения Windows без подтверждения пользователя.
10. Для операций, требующих перезагрузки, показывать уведомление.
11. Для служб античитов и процессов использовать явные разрешённые списки.
12. Не останавливать неизвестные системные процессы и службы.

---

## Сборка проекта

Открыть:

```text
misavaChecker.sln
```

Собрать:

```text
Ctrl + Shift + B
```

Запустить:

```text
Ctrl + F5
```

Публикация self-contained:

```powershell
dotnet publish misavaChecker/misavaChecker.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

---

## Git-команды

Проверить состояние:

```powershell
git status
```

Сохранить изменения:

```powershell
git add .
git commit -m "Describe changes"
git push origin master
```

Если ветка называется `main`:

```powershell
git push origin main
```

---

## Требования к будущему AI-агенту

Перед началом работы:

1. Изучить этот README.
2. Проверить namespace и имена файлов.
3. Проверить текущую ветку Git.
4. Не удалять рабочие модули HWID и system features.
5. Сначала предложить список изменяемых файлов.
6. Затем выдать полные файлы или применимый patch.
7. Проверить связи между `x:Name`, `Click` и методами `.xaml.cs`.
8. После изменения проверить отсутствие ошибок XAML и C#.
9. Не использовать placeholder-данные там, где можно получить информацию из Windows API.
10. Если значение невозможно получить, выводить `Недоступно` или `Неизвестно`, а не выдумывать результат.

---

## Текущее состояние

Работает:

- WPF-интерфейс;
- тёмная тема;
- анимация запуска;
- перемещение окна;
- HWID-окно;
- сбор HWID;
- JSON-снимок;
- сравнение снимков;
- фильтрация USB/HID;
- Hyper-V toggle;
- VBS toggle;
- HVCI toggle;
- динамическая панель безопасности;
- базовый runtime dashboard.

В разработке:

- полный runtime-анализ ПО;
- полноценный список антивирусов;
- античиты;
- отладчики;
- оверлеи;
- лаунчеры;
- мониторы;
- автообновление;
- установщик.
