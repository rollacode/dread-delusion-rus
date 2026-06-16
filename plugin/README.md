# DDRuFont — шрифт-плагин (BepInEx 6 IL2CPP)

Подключает кириллический **alagard** в рантайме, чтобы русский текст рисовался родным
блэклеттером без квадратов. Попадает в релизный zip как `BepInEx/plugins/DDRuFont.dll`
+ `BepInEx/plugins/alagard.bundle`.

## Файлы
- `Plugin.cs` — код плагина.
- `DDRuFont.csproj` — проект (net6.0).
- `alagard.bundle` — Unity AssetBundle с TMP-шрифтом `ALAGARD-12PX-UNICODE SDF` (источник истины шрифта; деплоится рядом с DLL).

## Что делает плагин
1. Грузит `alagard.bundle`, достаёт TMP_FontAsset.
2. **Fallback** — добавляет его в глобальную и пер-шрифтовые таблицы fallback TMP, так что кириллица берётся из alagard в любом шрифте.
3. **Primary** (`PrimaryFontPass`) — для текстов, где есть кириллица, но базовый шрифт не наш (напр. пиксельный шрифт меню снаряжения), переставляет базовый шрифт на alagard, чтобы вся надпись была одним шрифтом, а не «русские буквы alagard + латиница пиксельным».
4. **Auto-size** — ужимает только тот текст, что не влезает.

## Зависимости сборки
Нужна установленная игра с уже развёрнутым BepInEx 6 IL2CPP — csproj ссылается на
`…\Dread Delusion\windows_content\BepInEx\core` и `…\interop`. Папка `interop`
генерируется самим BepInEx при первом запуске игры (Il2CppInterop-обёртки), поэтому
в репозитории не хранится. Путь к игре задан свойствами `$(Core)`/`$(Interop)` в
`DDRuFont.csproj` — поправь под свою установку или переопредели при сборке.

## Сборка и деплой
```bash
cd plugin
dotnet build -c Release --no-restore -o bin
# деплой в игру:
cp bin/DDRuFont.dll "F:/SteamLibrary/.../Dread Delusion/windows_content/BepInEx/plugins/DDRuFont.dll"
# alagard.bundle деплоить в ту же папку plugins (меняется редко).
```
После замены DLL игру нужно полностью закрыть и запустить заново — BepInEx грузит плагины при старте.
