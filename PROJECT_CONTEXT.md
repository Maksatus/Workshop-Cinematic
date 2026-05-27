# PROJECT_CONTEXT.md

> Обновлено: 2026-05-26. Обновлять при изменении архитектуры, стека, стейтов или структуры папок.

---

## 1. Кратко о проекте

**Workshop-Cinematic** — трейлер/питч-демо на Unity 6, снятый как fake gameplay.  
Цель: показать инвесторам и паблишерам extraction-survival игру с модульными кораблями.  
Платформа: PC (Windows/Mac). Статус: Prototype.

---

## 2. Структура репозитория

```
Assets/
  Core/
    Inventory/      # Инвентарь: Scripts, Prefabs, Scenes, Materials, Shaders, Textures, Animations, Sprites, Meshes
    Map/            # Карта: аналогичная структура подпапок
    Warship/        # Корабль: Meshes, Prefabs, Scenes, Shaders, Textures
    World/          # Мир/окружение: полный набор подпапок + SplineHandler.cs
  SharedAssets/
    FirstPersonController/   # FPS-контроллер + тестовые сцены
    Roslyn/                  # Roslyn compiler support
    Timeline/                # Timeline samples
    UI Effect/               # UIEffect package
    ...
  TripoModels/               # Placeholder 3D-модели из Tripo3d (временные)
  WorldCreatorTerrains/      # Террейн из WorldCreator (MainTerrain, MainTerrainMini)
  Settings/
    PC/                      # URP Renderer/Pipeline Assets под PC
    VolumeProfiles/          # Post-processing Volume Profiles
  Refs/                      # Референсные ассеты
  ProjectSettings/           # Unity Project Settings
Packages/
  packages-lock.json
ARCHITECTURE_SYSTEM_MECHANIC_COMPONENT_CONTAINER.md  # Архитектурный стандарт
PROJECT_CONTEXT.md                                   # Этот файл
```

Соглашение: ассеты и конфиги хранятся рядом с кодом по фиче (`Core/{Feature}/`).

---

## 3. Зависимости и стек

| Компонент | Версия/Пакет |
|-----------|-------------|
| Движок | Unity 6 |
| Render Pipeline | URP |
| Язык | C# |
| Камеры | Cinemachine 3.x |
| Сцены/Анимации | Unity Timeline |
| VFX | VFX Graph |
| Пути | Splines |
| UI-анимация | UIEffect |
| 3D-модели (placeholder) | Tripo3d Unity Bridge |
| Террейн | WorldCreator Bridge |

Запуск: открыть проект в Unity Editor, загрузить нужную сцену.

---

## 4. Точки входа и жизненный цикл

- **Стартовая сцена**: `Assets/Core/Inventory/Scenes/Inventory.unity`
- Системы регистрируются в `GameSystemsHandler` (bootstrap)
- Жизненный цикл системы: `Initialize → UpdateSystem (loop) → DisposeSystem`
- Внутри `UpdateSystem`: фильтрация по текущему `GameState` → вызов `Mechanic.UpdateMechanic`

---

## 5. Сцены и роутинг

| Сцена | Путь | Назначение |
|-------|------|-----------|
| Inventory | `Core/Inventory/Scenes/Inventory.unity` | Стартовая, инвентарь |
| Map | `Core/Map/Scenes/Map.unity` | Карта |
| World | `Core/World/Scenes/World.unity` | Основной мир |
| Warship | `Core/Warship/Scenes/` | Корабль |

Порядок переходов между сценами: **TBD** (пока не зафиксирован).

---

## 6. Архитектурный паттерн

Паттерн **SMCC** (System-Mechanic-Component-Container).  
Полное описание: [`ARCHITECTURE_SYSTEM_MECHANIC_COMPONENT_CONTAINER.md`](ARCHITECTURE_SYSTEM_MECHANIC_COMPONENT_CONTAINER.md)

### Роли

| Роль | Ответственность |
|------|----------------|
| **System** | Жизненный цикл фичи, фильтрация по GameState, делегирует в Mechanic |
| **Mechanic** | Предметная логика, изменение данных, вызывается System |
| **Component** | Данные и runtime-флаги, без бизнес-логики |
| **Container** | Мост к Unity: хранит Transform, Renderer, UI-ссылки |

### Контракты

```csharp
IGameSystem  : Initialize, UpdateSystem, DisposeSystem
IGameMechanic: UpdateMechanic, DisposeMechanic
```

### Стейты (GameStates)

```
AwaitAnimation → StartGame → DrawElements → SelectElements → CompareRecipe
→ StartBoil → BoilingCoffee → CalculateValue → ApproveRecipe → CoffeeShow
→ CompareLevelComplete → RewardElements → ChangeLevel → UpdateLevelView → GameOver
```

---

## 7. Ключевые подсистемы

| Фича | Сцена | Scripts |
|------|-------|---------|
| Inventory | Inventory.unity | `CursorFollowClip`, `CursorFollowBehaviour`, `CursorFollowTrack` (Timeline) |
| World | World.unity | `SplineHandler` |
| Map | Map.unity | TBD |
| Warship | — | TBD |

---

## 8. Где искать данные

- Конфиги и ScriptableObject'ы — рядом с кодом в папке фичи (`Core/{Feature}/`)
- URP/Volume настройки — `Assets/Settings/`
- Фабрики объектов — `Assets/Scripts/GameObjectFactories/` (единственное место)

---

## 9. Риски и наблюдения

| # | Риск | Статус |
|---|------|--------|
| 1 | Нет автотестов — только ручная проверка в Unity Editor | Открыт |
| 2 | Модели из Tripo3d — временные placeholder'ы | Открыт |
| 3 | Порядок переходов между сценами не зафиксирован | TBD |
| 4 | SMCC-паттерн только вводится — часть кода ещё MonoBehaviour-ориентирована | Открыт |

---

## 10. Как добавить новую фичу

1. Создать папку `Assets/Core/{FeatureName}/` со структурой: Scripts, Prefabs, Scenes, Materials и т.д.
2. Добавить `FeatureSystem.cs`, `FeatureMechanic.cs`, `FeatureComponent.cs`, `FeatureContainer.cs`
3. Зарегистрировать систему в bootstrap (`GameSystemsHandler`)
4. Привязать контейнер в scene-композиции
5. Определить стейты активации
6. Добавить `Dispose`-очистку
7. Обновить документацию

**Создание объектов — только через `GameObjectFactories`.**

### Чеклист ревью

- [ ] Чёткое разделение System / Mechanic / Component / Container
- [ ] Стейты активации определены
- [ ] Unity-ссылки только в Container
- [ ] Объекты только через GameObjectFactory
- [ ] Корректный Dispose
- [ ] Документация обновлена

---

## 11. Антипаттерны (категорически нельзя)

- Unity-ссылки в System или Mechanic
- `new GameObject()` / `Instantiate()` вне GameObjectFactory
- **Монолитные классы**, покрывающие несколько независимых фич
- Несколько папок `GameObjectFactories` в проекте
- Стейты без явной схемы входа/выхода

---

## 12. Когда обновлять этот документ

- При добавлении новой сцены или фичи
- При изменении стека (новый пакет, смена движка)
- При изменении списка GameStates
- При фиксации порядка переходов между сценами
- При изменении архитектурных правил

---

## 13. SMCC MonoBehaviour Naming Convention (Mandatory)

- Any `MonoBehaviour` that belongs to an SMCC feature and stores Unity scene references/settings must use the `Container` suffix.
- Suffixes `View`, `Presenter`, and `Controller` are not allowed for such classes.
- Valid examples: `FusePanelContainer`, `FuseSlotContainer`, `FuseItemContainer`, `SceneSystemsContainer`.
- Invalid examples: `FuseItemView`, `FuseSlotView`, `FusePanelController`.
- If a `MonoBehaviour` is intentionally not an SMCC container, the class should include an explicit code comment explaining why.

### Review Checklist Addendum

- [ ] All `MonoBehaviour` classes in new SMCC features use the `Container` suffix (no `View/Controller/Presenter`).
