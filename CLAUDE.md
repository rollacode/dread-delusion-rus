# Dread Delusion RUS — рабочая памятка проекта

Русификатор игры **Dread Delusion** (Steam, Unity IL2CPP 2022.3). База перевода — LiquidRing; порт на текущую версию, редактура и сборка — rollacode. Репозиторий: `github.com/rollacode/dread-delusion-rus`.

## Как устроен перевод
Все игровые тексты лежат в `resources.assets` игры как Unity **TextAsset**'ы. Перевод = подмена байтов `m_Script` каждого TextAsset на русский вариант. Делается через **UnityPy** поверх чистой английской базы (`resources.assets.orig_backup`), поэтому версия Unity/IL2CPP не важна и сборка детерминирована.

**Источник истины — `source/ru/*.txt`.** Английский эталон — `source/en/*.txt` (НЕ редактировать). Имя файла = имя TextAsset.

Формат строк — `|`-разделённый CSV. В большинстве диалоговых файлов русский текст идёт в колонке 1 (сразу после ID), английский оригинал — в `source/en`. Разметка в тексте: `##` перенос строки, `[[…]]` ссылки-глоссарий, `((…))` акцент, `<<…>>` ремарки, `{0}` плейсхолдеры — сохранять как в оригинале.

## Структура репозитория
```
source/ru/   — русские тексты (правим тут)
source/en/   — английский эталон (read-only)
build/       — собранный resources.assets (gitignored артефакт)
dist/        — зип для раздачи (gitignored)
scripts/build.py — сборщик
wiki/        — RU-вики по игре (рендерится на GitHub)
GLOSSARY.md  — канонические переводы терминов (СВЕРЯТЬСЯ перед правкой)
LORE.md      — лор мира/персонажей (источник для вики)
METHOD.md    — методика редактуры + чек-лист проверок
PROGRESS.md  — статус вычитки по файлам
README.md    — описание для пользователей
```

## Сборка
```bash
py -3 scripts/build.py            # собрать -> build/resources.assets
py -3 scripts/build.py --deploy   # + скопировать в живую игру (перезаписать resources.assets)
```
Путь к игре зашит в `scripts/build.py` (`GAME = F:\SteamLibrary\...\Dread Delusion\windows_content`). Менять там при другом расположении. Запуск игры на тест: `windows_content/Dread Delusion.exe`.

## Окончания строк (важно!)
`.gitattributes` помечает `source/** -text` — git хранит байты как есть. Большинство файлов **CRLF**. Инструменты правки на Windows тоже пишут CRLF — нормально, если файл уже CRLF. Но несколько мелких файлов были **LF**: перед коммитом сверь
```bash
file source/ru/X.txt           # рабочая
git show HEAD:source/ru/X.txt | file -   # как в репо
```
Если HEAD=LF, а рабочая стала CRLF → верни LF: `sed -i 's/\r$//' source/ru/X.txt` (иначе diff раздувается «изменены все строки» и ломает построчное ревью).

## ⚠️ СТРУКТУРНЫЕ ТАБЛИЦЫ — НЕ ЛОМАТЬ КВЕСТЫ
Файлы `delusionTasks_allLanguages`, `delusionItemData_Master`, `delusionAlchemyComboData_Master`, `delusionPlayerHouse_Upgrades` — это таблицы с колонками-ФЛАГАМИ (ID, `Yes/No`, имена фактов `TutorialStarted`, числа, `FactionInq`…) И колонками-прозой. **Переводить ТОЛЬКО прозу. НИКОГДА флаги/ID/Yes-No/числа.** Был баг: перевод `Yes→Да` в `startsTask` ломал триггеры квестов (весы Весовщика, журнал).
- В `delusionTasks` проза = колонка 20 (поле `20: English`); колонки 1-19 — флаги.
- Перед/после правки проверяй целостность флагов:
```bash
diff <(awk -F'|' '{o="";for(i=1;i<=20;i++)o=o $i"|";print o}' source/ru/delusionTasks_allLanguages.txt) \
     <(awk -F'|' '{o="";for(i=1;i<=20;i++)o=o $i"|";print o}' source/en/delusionTasks_allLanguages.txt)
# должна отличаться ТОЛЬКО строка-легенда (заголовок)
awk -F'|' 'NR>2{for(i=1;i<=20;i++) if($i ~ /[А-Яа-яЁё]/) print NR":col"i}' source/ru/delusionTasks_allLanguages.txt
# пусто = во флагах нет кириллицы (OK)
```
Master-таблицы — почти 100% структурные, переводимой прозы там нет (имена/описания предметов живут в `delusionItemText_*`).

## Релиз (GitHub)
Зип русика — drop-in в `windows_content/`: содержит `Dread Delusion_Data/resources.assets` (перевод) + `BepInEx/` (плагин шрифта alagard-кириллица). Обновление релиза = подмена `resources.assets` внутри зипа:
```bash
export PATH="/c/Program Files/GitHub CLI:$PATH"
gh release download v1.3.3 -R rollacode/dread-delusion-rus -D /tmp/rel
cd /tmp/rel && unzip -o -q *.zip -d extracted
cp <repo>/build/resources.assets "extracted/Dread Delusion_Data/resources.assets"
# перепаковать (zip может отсутствовать в Git Bash — пакуем через py -3 zipfile, пути с '/')
py -3 -c "import os,zipfile;b='extracted';z=zipfile.ZipFile('out.zip','w',zipfile.ZIP_DEFLATED);[z.write(os.path.join(r,f),os.path.relpath(os.path.join(r,f),b).replace(os.sep,'/')) for r,_,fs in os.walk(b) for f in fs];z.close()"
gh release upload v1.3.3 out.zip -R rollacode/dread-delusion-rus --clobber
```
Проверяй md5 ассета внутри зипа == `build/resources.assets`.

## Рабочий цикл редактуры (см. METHOD.md)
1. Читать `source/ru/<файл>` чанками ~115 строк, сверяя с `source/en`.
2. Чек-лист: смысл vs EN, машинные хвосты (англ. слова), род/падеж, кальки, единство терминов (GLOSSARY.md), имена, разметка/скобки, идиомы, мохибейк (`έΑΥ`/`вҖ“`→`—`).
3. Аудит терминов грепать **регистронезависимо** (`-i`) — речь Короля Механизмов идёт КАПСОМ.
4. Правка → `build.py --deploy` → `git commit` (по файлу) → `push` → отметка в PROGRESS.md.

## Канон-заметки
Полный глоссарий — `GLOSSARY.md`. Частые: Corruption=Искажение, Corrupted=Искажённый, cutter=катер (НЕ резак), silkslug=шелкопряд, Delusion=Заблуждение, Translator=Переводчик, Sacramental Duty=Сакраментальный Долг, Royal Dispatch=Королевское Сообщение, Venna=Венна, Юсеф (1 с). Женские персонажи с мужскими по форме именами (Тхав, Морозов) — глаголы/прилагательные женского рода.
