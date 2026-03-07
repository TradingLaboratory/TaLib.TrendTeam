# QWEN.md — Контекст проекта TaLib.TrendTeam

## 📋 Обзор проекта

**TaLib.TrendTeam** — это форк и адаптация библиотеки технического анализа TA-Lib для платформы .NET 8.0+. Проект представляет собой полностью управляемую C# библиотеку без внешних зависимостей, предназначенную для технического анализа финансовых временных рядов.

### Ключевая информация

| Параметр | Значение |
|----------|----------|
| **Исходный проект** | [TA-Lib.NETCore](https://github.com/hmG3/TA-Lib.NETCore) (автор: Anatoliy Siryi) |
| **Целевая платформа** | .NET 8.0 |
| **Лицензия** | LGPL-3.0-or-later |
| **NuGet пакет** | `TALib.NETCore` |
| **Версия библиотеки** | 0.5.0 |
| **Документация** | https://hmg3.github.io/TA-Lib.NETCore |

### Назначение

Библиотека предоставляет комплексный набор функций для технического анализа рыночных данных:
- **150+ индикаторов** из оригинальной TA-Lib (C)
- **Полная совместимость** с API оригинальной библиотеки
- **Оптимизирована** для современных .NET приложений
- **Используется** в проектах TrendTeamTrading для алгоритмической торговли

---

## 🏗️ Архитектура проекта

### Структура решения

```
TaLib.TrendTeam.sln
├── src/
│   └── TALib.NETCore/           # Основной проект библиотеки
│       ├── Abstract/            # Абстрактный API (универсальный интерфейс)
│       ├── Candles/             # Работа с свечными данными
│       ├── Core/                # Базовые классы и интерфейсы
│       ├── Indicators/
│       │   ├── Functions/       # Реализации индикаторов
│       │   ├── FunctionHelpers/ # Вспомогательные классы
│       │   └── LookBackPeriods/ # Расчёт периодов запаздывания
│       ├── AssemblyInfo.cs
│       ├── GlobalUsings.cs
│       └── TALib.NETCore.csproj
│
├── tests/
│   └── TALib.NETCore.Tests/     # Юнит-тесты
│       ├── DataSets/            # Тестовые данные (JSON)
│       ├── Models/              # Модели тестов
│       ├── FunctionTests.cs     # Тесты функций
│       ├── JsonFileDataAttribute.cs
│       └── TALib.NETCore.Tests.csproj
│
└── docs/                        # Документация (DocFX)
    ├── manual/                  # Руководства
    │   ├── abstract-api.md      # Абстрактный API
    │   ├── functions-api.md     # API функций
    │   ├── functions-list.md    # Список функций
    │   ├── unstable-period.md   # Нестабильные периоды
    │   └── toc.yml
    ├── template/                # Шаблоны документации
    ├── images/                  # Изображения
    ├── index.md
    ├── docfx.json
    ├── toc.yml
    └── build.cake
```

### Группы индикаторов

Библиотека включает следующие категории индикаторов:

| Группа | Описание | Примеры |
|--------|----------|---------|
| **Overlap Studies** | Перекрывающиеся исследования | SMA, EMA, Bollinger Bands |
| **Momentum Indicators** | Индикаторы момента | RSI, MACD, Stochastic, CCI |
| **Volume Indicators** | Объёмные индикаторы | OBV, Chaikin A/D |
| **Volatility Indicators** | Индикаторы волатильности | ATR, NATR, True Range |
| **Price Transform** | Преобразования цены | AvgPrice, MedianPrice |
| **Cycle Indicators** | Цикловые индикаторы | Hilbert Transform |
| **Pattern Recognition** | Распознавание паттернов | CDL (Candlestick Patterns) |
| **Math Operators** | Математические операторы | Add, Sub, Mult, Div |
| **Math Transform** | Математические преобразования | Sin, Cos, Log, Exp |
| **Statistic Functions** | Статистические функции | Beta, Correlation, LinearReg |

---

## 🛠️ Технологии и зависимости

### Основные технологии

- **Язык**: C# 12.0
- **Платформа**: .NET 8.0 (библиотека), .NET 9.0 (тесты)
- **IDE**: Visual Studio 2022/2026
- **Документация**: DocFX + GitHub Pages

### NuGet зависимости

**Библиотека:**
```xml
<PackageReference Include="JetBrains.Annotations" Version="2024.3.0" />
```

**Тесты:**
```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
<PackageReference Include="Shouldly" Version="4.3.0" />
<PackageReference Include="coverlet.msbuild" Version="6.0.4" />
```

---

## 🚀 Сборка и запуск

### Требования

- **.NET SDK**: 8.0+ (для сборки библиотеки)
- **.NET SDK**: 9.0+ (для запуска тестов)
- **IDE**: Visual Studio 2022/2026 или JetBrains Rider

### Сборка проекта

```powershell
# Перейти в директорию решения
cd C:\Users\vdv-v\source\repos\TaLib.TrendTeam

# Сборка решения
dotnet build TaLib.TrendTeam.sln --configuration Release

# Сборка только библиотеки
dotnet build src/TALib.NETCore/TALib.NETCore.csproj --configuration Release
```

### Запуск тестов

```powershell
# Запуск всех тестов
dotnet test TaLib.TrendTeam.sln

# Запуск тестов с покрытием
dotnet test --collect:"XPlat Code Coverage"

# Запуск тестов с подробным выводом
dotnet test --logger "console;verbosity=detailed"
```

### Публикация NuGet пакета

```powershell
# Упаковка библиотеки
dotnet pack src/TALib.NETCore/TALib.NETCore.csproj --configuration Release

# Публикация на NuGet.org
dotnet nuget push src/TALib.NETCore\bin\Release\TALib.NETCore.0.5.0.nupkg `
  --api-key YOUR_API_KEY `
  --source https://api.nuget.org/v3/index.json
```

### Сборка документации

```powershell
# Установка docfx глобально
dotnet tool install -g docfx

# Сборка документации
cd docs
docfx docfx.json

# Запуск локального сервера документации
docfx serve _site
```

---

## 📦 Использование библиотеки

### Базовое использование

```csharp
using TALib;

// Простое скользящее среднее (SMA)
double[] input = { /* данные цены */ };
int[] lookback = { 14 };
double[] output = new double[input.Length - lookback[0]];

var retCode = Core.Sma.Run(input, lookback, output, Range.All, out int outBegIdx);
```

### Абстрактный API (универсальный интерфейс)

```csharp
using TALib.Abstract;

// Получение функции по имени
var sma = All["SMA"];
var rsi = Function("RSI");

// Выполнение функции
var lookback = sma.Lookback(14);
var output = new double[input.Length - lookback];
var retCode = sma.Run([input], [14], [output], Range.All, out _);

// Получение информации о функции
Console.WriteLine(sma.GetInfo());
// Name: Sma
// Description: Simple Moving Average
// Group: Overlap Studies
```

### LINQ запросы к функциям

```csharp
// Группировка функций по категориям
var formattedGroupList = All.ToFormattedGroupList();

// Поиск функций по названию
var momentumFunctions = All.Where(f => f.Group == "Momentum Indicators");
```

---

## 🧪 Практики разработки

### Соглашения по коду

**Из `.editorconfig`:**
- **Отступы**: 4 пробела для `.cs` файлов, 2 пробела для остальных
- **Стиль**: `space` для отступов
- **Кодировка**: `utf-8`
- **Максимальная длина строки**: 140 символов
- **Trim trailing whitespace**: `true`
- **Insert final newline**: `true`

**.NET конвенции:**
- `dotnet_style_predefined_type_for_locals_parameters_members = true`
- `csharp_style_var_when_type_is_apparent = true`
- `csharp_style_expression_bodied_methods = true`
- `csharp_prefer_braces = true:warning`

### Nullable reference types

В проекте включены nullable reference types:
```xml
<Nullable>enable</Nullable>
```

### Тестирование

- **Фреймворк**: xUnit
- **Assertion библиотека**: Shouldly
- **Покрытие**: Coverlet
- **Формат отчёта**: JUnit XML

**Структура тестов:**
- `FunctionTests.cs` — параметризованные тесты для всех функций
- `DataSets/` — JSON файлы с тестовыми данными
- `JsonFileDataAttribute.cs` — кастомный атрибут для загрузки данных

### Генерация документации

- **Инструмент**: DocFX
- **Шаблон**: `default` + `modern` + кастомный `template`
- **Публикация**: GitHub Pages
- **Автоматизация**: GitHub Actions (`.github/workflows/`)

---

## 🔍 Поиск и навигация по коду

### Полезные команды

```powershell
# Найти все файлы индикаторов
Get-ChildItem -Recurse -Filter "*.cs" -Path src/TALib.NETCore/Indicators

# Найти реализацию конкретного индикатора
Get-ChildItem -Recurse -Filter "Rsi.cs" -Path src/

# Построить дерево проекта
tree /F src/TALib.NETCore
```

### Структура имён файлов

Индикаторы названы в соответствии с оригинальным TA-Lib:
- `Sma.cs` — Simple Moving Average
- `Ema.cs` — Exponential Moving Average
- `Rsi.cs` — Relative Strength Index
- `Cdl*.cs` — Candlestick patterns (например, `CdlDoji.cs`)

---

## 📚 Документация

### Внутренняя документация

| Файл | Описание |
|------|----------|
| `docs/index.md` | Главная страница документации |
| `docs/manual/functions-list.md` | Полный список всех функций |
| `docs/manual/abstract-api.md` | Руководство по абстрактному API |
| `docs/manual/unstable-period.md` | Информация о нестабильных периодах |
| `docs/api/` | API документация (генерируется DocFX) |

### Внешние ресурсы

- **TA-Lib Official**: https://ta-lib.org/
- **TA-Lib.NETCore Docs**: https://hmg3.github.io/TA-Lib.NETCore
- **TA-Lib Git Repository**: https://github.com/TA-Lib/ta-lib
- **TA-Lib SVN (legacy)**: https://svn.code.sf.net/p/ta-lib/code/trunk/ta-lib/c/

---

## 🔧 Конфигурация CI/CD

### GitHub Actions

Файлы workflow расположены в `.github/workflows/`:

- **test-analyse.yml** — запуск тестов, статический анализ, покрытие
- **publish-docs.yml** — публикация документации на GitHub Pages
- **publish-nuget.yml** — публикация пакетов на NuGet.org

### Code Quality

Проект интегрирован с сервисами:

| Сервис | Статус |
|--------|--------|
| **Codecov** | Покрытие кода |
| **SonarCloud** | Качество и безопасность |
| **Codacy** | Автоматический код-ревью |
| **CodeFactor** | Анализ качества кода |

---

## 📊 Статистика проекта

| Метрика | Значение |
|---------|----------|
| **Исходный код** | ~150 файлов `.cs` |
| **Индикаторов** | 150+ функций |
| **Тестов** | Параметризованные тесты для всех функций |
| **Строк кода** | ~20,000+ (оценка) |
| **Покрытие тестами** | Измеряется через Coverlet |

---

## 🎯 Типичные сценарии использования

### 1. Добавление нового индикатора

```powershell
# 1. Создать класс в src/TALib.NETCore/Indicators/Functions/
# 2. Реализовать логику индикатора (наследовать от базового класса)
# 3. Добавить тестовые данные в tests/DataSets/
# 4. Добавить тест в FunctionTests.cs
# 5. Запустить тесты: dotnet test
# 6. Обновить документацию: docfx docs/docfx.json
```

### 2. Исправление бага в индикаторе

```powershell
# 1. Найти файл индикатора (например, src/.../Functions/Rsi.cs)
# 2. Изучить тестовые данные (tests/DataSets/Rsi.json)
# 3. Запустить тест: dotnet test --filter "FullyQualifiedName~Rsi"
# 4. Исправить код
# 5. Перепроверить тест
# 6. Закоммитить изменения
```

### 3. Обновление документации

```powershell
# 1. Отредактировать .md файл в docs/manual/
# 2. Собрать документацию: docfx docs/docfx.json
# 3. Проверить локально: docfx serve docs/_site
# 4. Закоммитить изменения
```

---

## 🐛 Отладка и решение проблем

### Частые проблемы

1. **Несоответствие результатов с оригинальной TA-Lib**
   - Проверить тестовые данные в `tests/DataSets/`
   - Сравнить с эталонными значениями из C библиотеки
   - Убедиться в корректности расчёта lookback периода

2. **Ошибки сборки после обновления .NET SDK**
   - Очистить решение: `dotnet clean`
   - Восстановить пакеты: `dotnet restore`
   - Пересобрать: `dotnet build`

3. **Проблемы с генерацией документации**
   - Проверить версию docfx: `docfx --version`
   - Очистить кэш: удалить `docs/_site/`
   - Пересобрать: `docfx docs/docfx.json`

### Логирование

Библиотека не использует внешние библиотеки логирования (zero dependencies).
Для отладки использовать:
- Debugger breakpoints в Visual Studio
- `Console.WriteLine()` в тестовых методах
- Shouldly assertion messages для тестов

---

## 👥 Контакты и поддержка

- **Оригинальный автор**: Anatoliy Siryi (hmG3)
- **Репозиторий**: https://github.com/hmG3/TA-Lib.NETCore
- **Лицензия**: LGPL-3.0-or-later
- **NuGet**: https://nuget.org/packages/TALib.NETCore

---

*Последнее обновление: 7 марта 2026 г.*
