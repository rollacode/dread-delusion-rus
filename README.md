# Dread Delusion — Русификатор 1.3.3

Полный русский перевод **Dread Delusion** для версии **1.3.3** (Steam, Unity 2022.3 / IL2CPP).

- **Базовый перевод:** LiquidRing — портирован на 1.3.3 (оригинал был под 2019.4-сборку).
- **Редактура:** полная вычитка EN↔RU всех текстов (диалоги, квесты, книги, предметы, концовки) — единообразие терминологии, грамматика, лор-соответствие.
- **Шрифт:** alagard с кириллицей в рантайме через плагин BepInEx 6 IL2CPP — родной блэклеттер, без квадратов.

## ⬇️ Скачать
→ **[Releases](../../releases/latest)** — `DreadDelusion_RUS_1.3.3_by_rollacode.zip`. Распаковать в `…\Steam\steamapps\common\Dread Delusion\windows_content\` с заменой. Запуск обычный.

### 🎮 Steam Deck
Шрифт-плагин работает через BepInEx, которому нужна дополнительная настройка под Proton:

1. **Параметр запуска** — в Steam → правая кнопка на игре → Свойства → Параметры запуска добавить:
   ```
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```
2. **Первый запуск** — сделать из Режима рабочего стола (не из Gaming Mode), чтобы BepInEx корректно создал свои конфиги.

Без этого кириллица не загружается (квадраты вместо букв).

## 📖 Вики по игре
Энциклопедия мира, фракций, локаций и персонажей (на русском, со спойлерами):

→ **[Открыть Вики](../../wiki)** — Мир · Фракции · Локации · Персонажи · Глоссарий

## 🛠️ Разработка
Как устроена сборка, релиз, структурные таблицы и воркфлоу редактуры — в **[CLAUDE.md](CLAUDE.md)**.

Кратко:
```bash
py -3 scripts/build.py --deploy   # собрать resources.assets из source/ru и поставить в игру
```
- Источник истины — `source/ru/*.txt`; эталон — `source/en/*.txt` (не трогать).
- Термины — [GLOSSARY.md](GLOSSARY.md); лор — [LORE.md](LORE.md); метод — [METHOD.md](METHOD.md); статус — [PROGRESS.md](PROGRESS.md).
- ⚠️ Структурные таблицы (`delusionTasks`, `delusion*_Master`) — переводить только прозу, флаги/ID/Yes-No не трогать (ломает квесты).

## 📦 Сборка релиза

**Состав архива** `DreadDelusion_RUS_<версия>_by_rollacode.zip` — это полноценный drop-in в `windows_content/` (распаковка с заменой, без ручной установки BepInEx). Внутри:

| Что | Откуда берётся | Меняем при релизе? |
|---|---|---|
| `Dread Delusion_Data/resources.assets` | `build/resources.assets` (`scripts/build.py`) | **да, всегда** |
| `BepInEx/plugins/DDRuFont.dll` | сборка `plugin/` (`dotnet build`) | только если правили плагин |
| `BepInEx/plugins/alagard.bundle` | `plugin/alagard.bundle` | почти никогда |
| `BepInEx/{core,interop,unity-libs,patchers,config}`, `dotnet/`, `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`, файл-readme | окружение **BepInEx 6 IL2CPP** из рабочей установки — наш репозиторий его НЕ генерирует | нет |
| TTF-шрифт (`fonts/alagard-unicode.ttf`) | **вход сборки**, впекается в `resources.assets` | в архив НЕ кладётся |

Поэтому релиз обновляется не пересборкой архива с нуля, а **точечной заменой наших файлов** в существующем zip (так сохраняется всё окружение BepInEx и имя readme-файла с кириллицей — полная распаковка/упаковка на Windows его калечит).

**Как выпустить/обновить релиз:**
```bash
# 1) собрать перевод (+ впечь кириллический шрифт)
py -3 scripts/build.py

# 2) если правили плагин — собрать DLL
cd plugin && dotnet build -c Release --no-restore -o bin && cd ..

# 3) скачать текущий архив релиза
export PATH="/c/Program Files/GitHub CLI:$PATH"
gh release download v1.3.3 -R rollacode/dread-delusion-rus -D /tmp/rel

# 4) точечно подменить наши файлы в zip (запись-в-запись, без распаковки),
#    проверить, что md5 подменённых файлов совпадает с build/, и залить:
py -3 - <<'PY'
import zipfile, hashlib
SRC="/tmp/rel/DreadDelusion_RUS_1.3.3_by_rollacode.zip"; OUT=SRC.replace(".zip","_new.zip")
repl={"Dread Delusion_Data/resources.assets":"build/resources.assets",
      "BepInEx/plugins/DDRuFont.dll":"plugin/bin/DDRuFont.dll"}   # DLL только если менялся
data={k:open(v,"rb").read() for k,v in repl.items()}
zin=zipfile.ZipFile(SRC); zout=zipfile.ZipFile(OUT,"w",zipfile.ZIP_DEFLATED)
for it in zin.infolist(): zout.writestr(it, data.get(it.filename) or zin.read(it.filename))
zin.close(); zout.close()
for k in data: print(k, hashlib.md5(zipfile.ZipFile(OUT).read(k)).hexdigest())
PY
# сверь хэши с: md5sum build/resources.assets plugin/bin/DDRuFont.dll
mv -f /tmp/rel/DreadDelusion_RUS_1.3.3_by_rollacode_new.zip /tmp/rel/DreadDelusion_RUS_1.3.3_by_rollacode.zip
gh release upload v1.3.3 /tmp/rel/DreadDelusion_RUS_1.3.3_by_rollacode.zip -R rollacode/dread-delusion-rus --clobber
```
Новая версия игры/новый тег → меняется только номер версии в командах и имени архива.

---
Порт + редактура + сборка: **rollacode**. Базовый перевод: **LiquidRing**.
