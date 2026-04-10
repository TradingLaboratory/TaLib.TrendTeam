# QWEN.md — Контекст проекта TALib.NETCore

## 📋 Обзор проекта

**TALib.NETCore** — это современная .NET (C#) реализация популярной библиотеки технического анализа TA-Lib. Проект представляет собой форк/продолжение оригинальной библиотеки с поддержкой .NET 8.0 и расширенными возможностями.

### Ключевые характеристики

- **Платформа**: .NET 8.0
- **Версия**: 0.5.0
- **Лицензия**: LGPL-3.0-or-later
- **Nullable reference types**: включены
- **Основной namespace**: `TALib`

### Назначение

Библиотека предоставляет более **150 функций** для технического анализа финансовых рынков:

- **Распознавание свечных паттернов** (Pattern Recognition) — 50+ паттернов
- **Индикаторы тренда** (Overlap Studies) — SMA, EMA, Bollinger Bands и др.
- **Индикаторы момента** (Momentum Indicators) — RSI, MACD, Stochastic и др.
- **Индикаторы объема** (Volume Indicators) — OBV, Chaikin A/D и др.
- **Индикаторы волатильности** (Volatility Indicators) — ATR, Standard Deviation и др.
- **Математические функции** (Math Transform/Operators) — тригонометрия, арифметика
- **Статистические функции** — корреляция, регрессия, дисперсия
- **Циклические индикаторы** — Hilbert Transform

---

## 🏗️ Архитектура проекта

### Структура каталогов

```
TALib.NETCore/
├── Abstract/                    # Абстрактный API для унифицированного доступа
│   ├── Abstract.cs              # Главный класс Abstract (singleton)
│   ├── AbstractExtensions.cs    # Расширения для Abstract
│   ├── IndicatorFunction.cs     # Описание функции индикатора
│   └── IndicatorFunctionExtensions.cs
│
├── Candles/                     # Распознавание свечных паттернов
│   ├── CandleHelpers.cs         # Вспомогательные методы для свечей
│   ├── TA_AbandonedBaby.cs      # Паттерн "Заброшенный ребёнок"
│   ├── TA_Engulfing.cs          # Паттерн "Поглощение"
│   └── ... (50+ файлов паттернов)
│
├── Core/                        # Базовые типы и настройки
│   ├── CandleColor.cs           # Цвет свечи (White/Black)
│   ├── CandleRangeType.cs       # Тип диапазона свечи
│   ├── CandleSettings.cs        # Настройки свечных паттернов
│   ├── CompatibilityMode.cs     # Режимы совместимости
│   ├── MAType.cs                # Типы скользящих средних
│   ├── OutputDisplayHints.cs    # Подсказки для отображения
│   ├── RetCode.cs               # Коды возврата функций
│   └── UnstablePeriodSettings.cs
│
├── Indicators/                  # Реализации индикаторов
│   ├── Functions/
│   │   ├── CycleIndicators/     # Циклические индикаторы
│   │   ├── MathOperators/       # Математические операторы
│   │   ├── MathTransform/       # Математические преобразования
│   │   ├── MomentumIndicators/  # Индикаторы момента
│   │   ├── OverlapStudies/      # Перекрывающиеся исследования
│   │   ├── PriceTransform/      # Преобразования цены
│   │   ├── StatisticFunctions/  # Статистические функции
│   │   ├── VolatilityIndicators/# Индикаторы волатильности
│   │   └── VolumeIndicators/    # Индикаторы объема
│   │   ├── _Functions.cs        # Статический класс функций
│   │   └── _LookBackPeriods.cs  # Расчёт lookback периодов
│   └── LookBackPeriods/         # Периоды запаздывания
│
├── AssemblyInfo.cs              # Информация о сборке
├── GlobalUsings.cs              # Глобальные using директивы
└── TALib.NETCore.csproj         # Проектный файл
```

### Ключевые компоненты

#### 1. Abstract API

Центральный класс `Abstract` предоставляет единый интерфейс доступа ко всем функциям:

```csharp
// Доступ через singleton
var func = Abstract.All.Function("RSI");
var func = Abstract.All["RSI"];  // Альтернативный синтаксис

// Получение информации о функции
Console.WriteLine($"{func.Name}: {func.Description}");
Console.WriteLine($"Группа: {func.Group}");
Console.WriteLine($"Входы: {string.Join(", ", func.Inputs)}");
Console.WriteLine($"Выходы: {string.Join(", ", func.Outputs)}");

// Расчёт lookback периода
int lookback = func.Lookback(14);  // Для RSI с периодом 14

// Выполнение расчёта
var inputs = new double[][] { closeArray };
var outputs = new double[1][];
var options = new double[] { 14 };
func.Run(inputs, options, outputs, inRange, out var outRange);
```

#### 2. IndicatorFunction

Класс `IndicatorFunction` описывает метаданные функции:

- **Name** — имя функции (например, "RSI", "MACD")
- **Description** — описание
- **Group** — группа индикаторов
- **Inputs** — входные параметры (High, Low, Close, Volume и т.д.)
- **Options** — настраиваемые параметры (периоды, типы MA)
- **Outputs** — выходные данные с подсказками отображения

#### 3. CandleHelpers

Вспомогательный класс для работы со свечными данными:

- `RealBody<T>()` — расчёт размера тела свечи
- `UpperShadow<T>()` — расчёт верхней тени
- `LowerShadow<T>()` — расчёт нижней тени
- `CandleColor<T>()` — определение цвета свечи
- `CandleRange<T>()` — расчёт диапазона по типу настройки
- `CandleAverage<T>()` — расчёт среднего значения

#### 4. Core типы

**MAType** — типы скользящих средних:

- `Sma` — простая скользящая средняя
- `Ema` — экспоненциальная скользящая средняя
- `Wma` — взвешенная скользящая средняя
- `Dema` — двойная экспоненциальная
- `Tema` — тройная экспоненциальная
- `Trima` — треугольная
- `Kama` — адаптивная Кауфмана
- `Mama` — адаптивная MESA
- `T3` — обобщённая двойная экспоненциальная

**OutputDisplayHints** — флаги для отображения результатов:

- `Line`, `DotLine`, `DashLine` — типы линий
- `Histo` — гистограмма
- `PatternBool`, `PatternBullBear`, `PatternStrength` — паттерны
- `UpperLimit`, `LowerLimit` — лимиты

**RetCode** — коды возврата:

- `Success` — успешное выполнение
- `BadParam` — некорректный параметр
- `OutOfRangeParam` — выход за диапазон

---

## 🚀 Сборка и использование

### Требования

- **.NET 8.0 SDK**
- **Visual Studio 2022/2026** или **VS Code**
- **NuGet пакет**: `JetBrains.Annotations` (версия 2024.3.0)

### Сборка проекта

```powershell
# Перейти в директорию проекта
cd C:\Users\vdv-v\source\repos\TaLib.TrendTeam\src\TALib.NETCore

# Сборка через dotnet CLI
dotnet build --configuration Release

# Сборка с конкретной конфигурацией
dotnet build TALib.NETCore.csproj -c Release

# Очистка и пересборка
dotnet clean && dotnet build
```

### Запуск тестов

```powershell
# Перейти в директорию с тестами
cd C:\Users\vdv-v\source\repos\TaLib.TrendTeam\tests

# Запуск тестов
dotnet test
```

### Использование в коде

#### Базовый пример с Abstract API

```csharp
using TALib;

// Расчёт RSI
var rsiFunc = Abstract.All["RSI"];
var inputs = new double[][] { closePrices };
var outputs = new double[1][];
var options = new double[] { 14 };  // Период RSI

rsiFunc.Run(inputs, options, outputs, .., out var outRange);

// outputs[0] содержит значения RSI
```

#### Прямой вызов функции

```csharp
using TALib;

// Прямой вызов функции SMA
int lookback = Functions.SmaLookback(20);
var retCode = Functions.Sma(
    closePrices,           // Входные данные
    ..,                    // Диапазон входных данных
    out var outRange,      // Выходной диапазон
    smaValues,             // Массив для результатов
    20                     // Период
);
```

#### Работа со свечными паттернами

```csharp
using TALib;

// Распознавание паттерна "Поглощение"
var patternFunc = Abstract.All["ENGULFING"];
var inputs = new double[][] { open, high, low, close };
var outputs = new int[1][];
var options = Array.Empty<double>();

patternFunc.Run(inputs, options, outputs, .., out var outRange);

// outputs[0] содержит:
// 100 — бычий паттерн
// -100 — медвежий паттерн
// 0 — паттерна нет
```

#### Получение списка всех функций

```csharp
using TALib;

// Все функции
foreach (var func in Abstract.All)
{
    Console.WriteLine($"{func.Name} ({func.Group}): {func.Description}");
}

// Функции конкретной группы
var momentumIndicators = Abstract.All
    .Where(f => f.Group == "Momentum Indicators")
    .ToList();

// Форматированный вывод по группам
Console.WriteLine(Abstract.All.ToFormattedGroupList());
```

---

## 📦 Группы индикаторов

| Группа                    | Количество | Примеры                                  |
| ------------------------- | ---------- | ---------------------------------------- |
| **Pattern Recognition**   | 50+        | AbandonedBaby, Engulfing, Doji, Hammer   |
| **Overlap Studies**       | 20+        | SMA, EMA, BollingerBands, MACD           |
| **Momentum Indicators**   | 20+        | RSI, Stochastic, CCI, Williams %R        |
| **Volume Indicators**     | 10+        | OBV, AD, Chaikin A/D Oscillator          |
| **Volatility Indicators** | 5+         | ATR, Bollinger Bands, Standard Deviation |
| **Statistic Functions**   | 10+        | Beta, Correlation, Linear Regression     |
| **Math Transform**        | 15+        | ACos, ASin, Tanh, Log10                  |
| **Math Operators**        | 10+        | Add, Sub, Mult, Div, Min, Max            |
| **Price Transform**       | 5+         | AvgPrice, MedPrice, TypPrice             |
| **Cycle Indicators**      | 5+         | Hilbert Transform функции                |

---

## 🔧 Конфигурация и настройки

### Настройки свечных паттернов

Библиотека поддерживает настройку параметров распознавания паттернов через `CandleSettings`:

```csharp
// Пример настройки (псевдокод)
CandleSettings.Set(CandleSettingType.BodyLong, new CandleSettings {
    RangeType = CandleRangeType.RealBody,
    AveragePeriod = 10,
    Factor = 1.0
});
```

### Режимы совместимости

`CompatibilityMode` позволяет переключаться между различными режимами работы для совместимости с другими версиями TA-Lib.

### Нестабильные периоды

Для некоторых функций поддерживается настройка нестабильного периода через `UnstablePeriodSettings`:

```csharp
// Установка нестабильного периода для функции
var func = Abstract.All["RSI"];
func.SetUnstablePeriod(30);  // Первые 30 периодов считаются нестабильными
```

---

## 🧪 Практики разработки

### Соглашения по коду

- **Nullable reference types**: включены (`<Nullable>enable</Nullable>`)
- **Генерики**: используется `IFloatingPointIeee754<T>` для поддержки `float` и `double`
- **Span<T>**: используется для эффективной работы с памятью
- **Аннотации**: `JetBrains.Annotations` для улучшения подсказок IDE

### Структура имён функций

- **Функции индикаторов**: `Functions.{Name}` (например, `Functions.Sma`)
- **Lookback методы**: `{Name}Lookback` (например, `SmaLookback`)
- **Свечные паттерны**: `Candles.{PatternName}` (например, `Candles.Engulfing`)

### Тестирование

- Юнит-тесты расположены в отдельном проекте `TALib.NETCore.Tests`
- Тесты покрывают основные функции и пограничные случаи
- Использование xUnit/NUnit (в зависимости от конфигурации)

---

## 📚 Документация

### XML документация

Проект включает полную XML документацию, генерируемую в файл `TalibTrendTeam.xml`:

```xml
<member name="T:TALib.Abstract">
    <summary>
    Provides Abstraction layer for accessing all functions.
    </summary>
</member>
```

### Официальная документация

- **GitHub**: https://github.com/hmG3/TA-Lib.NETCore
- **Документация**: https://hmg3.github.io/TA-Lib.NETCore

### Основные классы для документации

| Класс                     | Описание                                   |
| ------------------------- | ------------------------------------------ |
| `Abstract`                | Главный класс для доступа ко всем функциям |
| `IndicatorFunction`       | Метаданные функции индикатора              |
| `Functions`               | Статический класс с реализациями функций   |
| `Candles`                 | Статический класс с реализациями паттернов |
| `Core.MAType`             | Перечисление типов скользящих средних      |
| `Core.OutputDisplayHints` | Флаги отображения результатов              |
| `Core.RetCode`            | Коды возврата функций                      |

---

## 🔍 Поиск и навигация по коду

### Поиск функции по имени

```csharp
// Через Abstract API
var func = Abstract.All.Function("MACD");

// Проверка существования функции
if (Abstract.All.Function("RSI") is not null)
{
    // Функция существует
}
```

### Поиск по группе

```csharp
// Все индикаторы момента
var momentum = Abstract.All
    .Where(f => f.Group == "Momentum Indicators")
    .ToList();

// Все свечные паттерны
var patterns = Abstract.All
    .Where(f => f.Group == "Pattern Recognition")
    .ToList();
```

### PowerShell команды для навигации

```powershell
# Найти все файлы свечных паттернов
Get-ChildItem Candles\TA_*.cs

# Найти все файлы индикаторов по группе
Get-ChildItem Indicators\Functions\MomentumIndicators\

# Построить дерево проекта
tree /F
```

---

## 🐛 Отладка и решение проблем

### Частые проблемы

1. **NullReferenceException при вызове функции**
   
   - Проверить, что входные массивы не null
   - Убедиться, что длины массивов достаточны для выбранного периода

2. **OutOfRange ошибки**
   
   - Проверить корректность `Range` для входных данных
   - Убедиться, что `lookback` период учтён при выделении памяти

3. **Некорректные значения результатов**
   
   - Проверить, что данные не содержат NaN/Infinity
   - Убедиться в правильности порядка параметров (Open, High, Low, Close)

### Логирование

- Использовать `RetCode` для проверки успешности выполнения
- Проверять `outRange` для определения валидного диапазона выходных данных

```csharp
var retCode = func.Run(inputs, options, outputs, inRange, out var outRange);
if (retCode != Core.RetCode.Success)
{
    Console.WriteLine($"Ошибка: {retCode}");
}
```

---

## 📊 Интеграция с другими проектами

### TaLib.TrendTeam

Этот проект является частью экосистемы **TaLib.TrendTeam**:

- **TALib.NETCore** — основная библиотека индикаторов
- **TALib.NETCore.Tests** — юнит-тесты
- **TaTooIne.Benchmark** — бенчмарки производительности

### Интеграция с TrendTeamTrading

Библиотека используется в проектах TrendTeamTrading для:

- Расчёта технических индикаторов в стратегиях
- Распознавания свечных паттернов
- Анализа рыночных данных

### Интеграция с Wealth-Lab и TSLab

Индикаторы могут использоваться в стратегиях для:

- Wealth-Lab 8 (через C# стратегии)
- TSLab (через custom индикаторы)

---

## 📖 Дополнительные ресурсы

### Книги по техническому анализу

- Чак Лебо — «Компьютерный анализ фьючерсных рынков»
- Александр Элдер — «Как играть и выигрывать на бирже»
- Стив Нисон — «Японские свечи» (для свечных паттернов)

### Внешние ресурсы

- **TA-Lib Official**: https://ta-lib.org/
- **TA-Lib.NETCore Docs**: https://hmg3.github.io/TA-Lib.NETCore
- **Original TA-Lib**: https://github.com/ta-lib/ta-lib

---

## 👥 Контакты и поддержка

- **Оригинальный автор**: Anatolii Siryi (hmG3)
- **Copyright**: © 2020-2025 Anatolii Siryi
- **Лицензия**: LGPL-3.0-or-later
- **GitHub**: https://github.com/hmG3/TA-Lib.NETCore

---

*Последнее обновление: 6 апреля 2026 г.*
