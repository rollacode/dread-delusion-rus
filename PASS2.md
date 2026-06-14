# Проход 2 — литературная редактура (ветка narrative-polish)

Режим: сбалансированный (STYLE.md). Пайплайн на файл: **редактор** (литправка) → **ревизор** (сверка с EN) → ведущий коммитит с тех-проверками. Агенты НЕ трогают git/build.

Статусы: ☐ ожидает · ✏️ у редактора · 🔍 у ревизора · ✅ готово

| Файл | строк | статус |
|------|-------|--------|
| Dialogue_MainQuest | 823 | ✅ редактор+ревизор |
| Dialogue_Pwyll | 720 | ✅ редактор+ревизор |
| Dialogue_HighConfessor | 712 | ✅ редактор+ревизор |
| HallowQuestDialogue | 699 | ✅ редактор+ревизор |
| Dialogue_CharacterQuests | 882 | 🔍 ревизор |
| Dialogue_Clockwork | 3777 | ☐ (по циклам) |
| Dialogue_Endless | 1289 | ☐ (по циклам) |
| AcademyDialogue | 378 | ✅ редактор+ревизор |
| Dialogue_Isles | 215 | 🔍 ревизор |
| Dialogue_Endless_Mines | 195 | 🔍 ревизор |
| FactionDialogue | 162 | 🔍 ревизор |
| Dialogue_GodHunt | 153 | ✅ редактор+ревизор |
| shopsHallowDialogue | 125 | 🔍 ревизор |
| Dialogue_GameEndings | 120 | ☐ |
| ArtifactQuestDialogue | 50 | ☐ |
| bookData_AllLanguages | 36 | ☐ |
| delusionItemText_English | 247 | ☐ |
| delusionTasks_allLanguages | 392 | ☐ (только проза кол.20) |
| generalData_AllLanguages | 949 | ☐ (туториал-проза; UI не трогать) |
| ObjectDialogue | 9 | ☐ |

Пропускаем: credits_Eng, DebugDialogue, тест-файлы, Master-таблицы (структурные), delusionItemText_French/German/Spanish.
