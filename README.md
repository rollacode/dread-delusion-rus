# Dread Delusion — Русификатор 1.3.3

Русский перевод **Dread Delusion** для актуальной версии **1.3.3** (Steam, Unity 2022.3 / IL2CPP).

- **Базовый перевод:** LiquidRing — портирован на 1.3.3 (оригинал был под старую 2019.4-сборку).
- **Редактура:** вычитка EN↔RU, правки терминологии/грамматики/лора (см. [GLOSSARY.md](GLOSSARY.md), [LORE.md](LORE.md)).
- **Шрифт:** alagard с кириллицей подаётся в рантайме через плагин BepInEx 6 IL2CPP — родной блэклеттер, без квадратов.

## Скачать
→ **[Releases](../../releases/latest)** — `DreadDelusion_RUS_1.3.3_by_rollacode.zip`. Распаковать в `...\Steam\steamapps\common\Dread Delusion\windows_content\` с заменой.

---

## Структура проекта (разработка)

```
source/en/   английские тексты каждого TextAsset (чистый бэкап игры) — НЕ редактировать
source/ru/   русские тексты — ИСТОЧНИК ИСТИНЫ перевода, правим здесь
scripts/     build.py — собирает resources.assets из source/ru
GLOSSARY.md  канонические термины (сверяться перед правкой)
LORE.md      лор-база (RU), источник для вики
wiki/        страницы RU-вики по игре
build/       (gitignore) собранный resources.assets
dist/        (gitignore) релизный zip
```

### Воркфлоу
1. Правим текст в `source/ru/<TextAsset>.txt` (сверяясь с `source/en/<...>.txt` и GLOSSARY).
2. `py -3 scripts/build.py --deploy` — собрать и поставить в игру для проверки.
3. Коммит по файлу/смыслу.
4. Релиз = собранный `resources.assets` + плагин шрифта + BepInEx, упакованные в zip → GitHub Releases.

### Принципы редактуры
- Несколько внимательных проходов по каждому файлу, чанками — смысл / грамматика-падежи / кальки / термины / опечатки.
- Сохранять разметку `## [[ ]] << >> (( ))` и плейсхолдеры `{0}` как в оригинале.
- Единая терминология по GLOSSARY.md; лор — в LORE.md.

Порт + редактура + сборка: **rollacode**. Базовый перевод: **LiquidRing**.
