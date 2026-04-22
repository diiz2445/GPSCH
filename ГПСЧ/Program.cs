using System.Diagnostics;
using System.Numerics;

internal class Program
{
    private static void Main(string[] args)
    {
        int count = 100000;
        FullTest();
    }

    private static void HowLong(int count, LinearCongruentialGenerator LCG)
    {
        Stopwatch sw = new Stopwatch();
        LinearCongruentialGenerator exampleLCG = new LinearCongruentialGenerator();
        sw.Start();
        for (int i = 0; i < count; i++)
            exampleLCG.Next();
        sw.Stop();
        Console.WriteLine($"LCG Генерация {count} чисел заняла: {sw.ElapsedMilliseconds}");
    }
    static void FullTest()
    {
        Stopwatch sw = new Stopwatch();
        Console.WriteLine("Генерация 10000 чисел с помощью LCG...\n");

        var lcg = new LinearCongruentialGenerator(12345);
        const int count = 100000;
        const int bins = 50;                    // количество столбцов в гистограмме
        int[] histogram = new int[bins];

        List<double> numbers = new List<double>(count); // если нужно сохранить все числа

        for (int i = 0; i < count; i++)
        {
            sw.Start();
            double value = lcg.NextDouble();
            sw.Stop();
            numbers.Add(value);

            // Определяем, в какой "столбец" попадает число
            int bin = (int)(value * bins);           // [0 .. bins-1]
            if (bin == bins) bin = bins - 1;         // на случай если value == 1.0 (очень редко)
            histogram[bin]++;
        }

        // Выводим гистограмму
        Console.WriteLine("Гистограмма распределения (10000 чисел):");
        Console.WriteLine("Значение → Частота\n");

        int maxCount = 0;
        foreach (int c in histogram)
            if (c > maxCount) maxCount = c;

        const int maxBarWidth = 60;   // максимальная длина столбца в символах

        for (int i = 0; i < bins; i++)
        {
            double binStart = (double)i / bins;
            double binEnd = (double)(i + 1) / bins;

            int barLength = (int)((double)histogram[i] / maxCount * maxBarWidth);

            string bar = new string('█', barLength);   // можно заменить на '#' или '*'

            Console.WriteLine($"{binStart:F3} - {binEnd:F3} | {bar}  ({histogram[i]})");
        }
        Console.WriteLine("=== Результаты анализа LCG ===");
        Console.WriteLine($"\nОбщее количество чисел: {count}");
        Console.WriteLine($"Максимальная частота в одном бине: {maxCount}");
        Console.WriteLine("При идеальном равномерном распределении в каждом бине должно быть примерно {0} чисел.",
                          count / bins);
        Console.WriteLine($"Время на генерацию: {sw.Elapsed}");

        double mean = numbers.Average();
        double variance = numbers.Sum(x => (x - mean) * (x - mean)) / count;           // population variance
        double sampleVariance = numbers.Sum(x => (x - mean) * (x - mean)) / (count - 1); // sample variance

        
        Console.WriteLine($"Количество чисел     : {count}");
        Console.WriteLine($"Среднее значение     : {mean:F8}");
        Console.WriteLine($"Дисперсия            : {variance:F10}");
        Console.WriteLine($"Выборочная дисперсия : {sampleVariance:F10}");
        Console.WriteLine($"Теоретическая дисперсия : 0.0833333333");
        Console.WriteLine($"Отклонение от теории : {Math.Abs(variance - 1.0 / 12):F10}\n");





        sw.Reset();
        var bbs = new BlumBlumShub(BigInteger.Parse("123456789012345"));
        int[] histogramBlum = new int[bins];

        List<double> numbersBlum = new List<double>(count);

        for (int i = 0; i < count; i++)
        {
            sw.Start();
            double value = bbs.NextDouble();
            sw.Stop();
            numbersBlum.Add(value);

            int bin = (int)(value * bins);
            if (bin == bins) bin = bins - 1;
            histogramBlum[bin]++;
        }

        // === Статистика ===
        double meanBlum = numbers.Average();
        double varianceBlum = numbers.Sum(x => (x - meanBlum) * (x - meanBlum)) / count;

        Console.WriteLine("=== Результаты анализа BBS ===");
        Console.WriteLine($"Размер модуля n ≈ {bbs.Modulus}");
        Console.WriteLine($"Сгенерировано чисел: {count}");
        Console.WriteLine($"Среднее значение    : {meanBlum:F8}");
        Console.WriteLine($"Дисперсия           : {varianceBlum:F10}");
        Console.WriteLine($"Теоретическая       : 0.0833333333");
        Console.WriteLine($"Отклонение          : {Math.Abs(varianceBlum - 1.0 / 12):E6}\n");
        Console.WriteLine($"Время на генерацию: {sw.Elapsed}");
        // === ASCII-гистограмма ===
        Console.WriteLine("Гистограмма распределения (50 бинов):");
        Console.WriteLine("Значение → Частота\n");

        int maxCountBlum = histogram.Max();
        const int maxBarWidthBlum = 60;

        for (int i = 0; i < bins; i++)
        {
            double binStart = (double)i / bins;
            double binEnd = (double)(i + 1) / bins;

            int barLength = (int)((double)histogramBlum[i] / maxCount * maxBarWidth);
            string bar = new string('█', barLength);

            Console.WriteLine($"{binStart:F3} - {binEnd:F3} | {bar,-60}  ({histogramBlum[i]})");
        }

        Console.WriteLine($"\nПри идеальном равномерном распределении в каждом бине {count / bins} чисел.");

        //Скипнем пару строк для наглядности
        for (int i = 0; i < 5; i++)
            Console.WriteLine();
        
        Console.WriteLine("Тесты Генератора BBS:");
        //Частотный анализ из NIST
        NistFrequencyTest.RunTest(bbs, count, 0.01);
        //перекрывающиеся перестановки, который проверяет распределение последовательных групп по всем возможным перестановкам
        DiehardOverlappingPermutationsTest.RunTest(bbs, count, 0.05);
    }


}
public class LinearCongruentialGenerator
{
    private long _seed;
    private const long A = 1664529;        // множитель
    private const long C = 1013;     // приращение
    private const long M = 1L << 32;       // модуль (2^32)

    public LinearCongruentialGenerator(long seed = 123456789)
    {
        _seed = seed;
    }

    /// <summary>
    /// Возвращает следующее целое число в диапазоне [0, 2^32)
    /// </summary>
    public uint Next()
    {
        _seed = (A * _seed + C) % M;
        return (uint)_seed;
    }
    /// <summary>
    /// Возвращает следующее целое число в диапазоне [0, K)
    /// </summary>
    public int Next(int Max)
    {
        _seed = (A * _seed + C) % Max;
        return (int)_seed; 
    }

    /// <summary>
    /// Возвращает случайное число с равномерным распределением в диапазоне [0.0, 1.0)
    /// </summary>
    public double NextDouble()
    {
        return Next() / (double)M;
    }

    /// <summary>
    /// Возвращает случайное целое число в диапазоне [min, max)
    /// </summary>
    public int Next(int min, int max)
    {
        if (min >= max) throw new ArgumentException("min должен быть меньше max");

        uint range = (uint)(max - min);
        return (int)(Next() % range) + min;
    }
}
public class BlumBlumShub
{
    private BigInteger _state;
    public readonly BigInteger _modulus;   // n = p * q

    public BigInteger Modulus => _modulus;
    /// <summary>
    /// Создает генератор Blum Blum Shub
    /// </summary>
    /// <param name="seed">Начальное значение (должно быть взаимно простым с n)</param>
    /// <param name="p">Большое простое число, p ≡ 3 mod 4</param>
    /// <param name="q">Большое простое число, q ≡ 3 mod 4</param>
    public BlumBlumShub(BigInteger seed, BigInteger p, BigInteger q)
    {
        if (p % 4 != 3 || q % 4 != 3)
            throw new ArgumentException("Оба простых числа p и q должны быть ≡ 3 (mod 4)");

        _modulus = p * q;
        _state = seed % _modulus;

        // Простая проверка, что seed не 0
        if (_state == 0)
            _state = 1;
    }
    public BlumBlumShub(BigInteger seed)
    {
        // Большие рабочие Blum-простые числа (оба ≡ 3 mod 4)
        BigInteger p = BigInteger.Parse("30000000091");   // 30000000091 % 4 = 3
        BigInteger q = BigInteger.Parse("40000000003");   // 40000000003 % 4 = 3

        _modulus = p * q;                                 // n ≈ 1.200000000373e+21

        _state = seed % _modulus;
        if (_state == 0 || _state == 1)
            _state = 536320114;   // хорошее стартовое значение
    }

    /// <summary>
    /// Возвращает следующее случайное число в диапазоне [0, 1)
    /// </summary>
    public double NextDouble()
    {
        _state = BigInteger.ModPow(_state, 2, _modulus);   // x = x^2 mod n
        return (double)_state / (double)_modulus;
    }

    /// <summary>
    /// Возвращает следующее 32-битное целое число
    /// </summary>
    public uint Next()
    {
        _state = BigInteger.ModPow(_state, 2, _modulus);
        return (uint)((_state % uint.MaxValue) + 1); // чтобы не было 0
    }

    /// <summary>
    /// Возвращает случайное целое в диапазоне [min, max)
    /// </summary>
    public int Next(int min, int max)
    {
        if (min >= max)
            throw new ArgumentException("min должен быть меньше max");

        return (int)(NextDouble() * (max - min)) + min;
    }

    /// <summary>
    /// Стандартные криптографически сильные параметры (для демонстрации)
    /// p = 2467246246789246979
    /// q = 3967368945678345893
    /// n ≈ 9.78 × 10^36  (очень большой модуль)
    /// </summary>
    public static BlumBlumShub CreateWithStandardParameters(BigInteger seed)
    {
        BigInteger p = BigInteger.Parse("30000000091");
        BigInteger q = BigInteger.Parse("40000000003");
        return new BlumBlumShub(seed, p, q);
    }
}


/// <summary>
/// Реализация частотного (monobit) теста из набора NIST SP 800-22.
/// Проверяет, близка ли доля единиц в битовой последовательности к 0.5.
/// </summary>
public static class NistFrequencyTest
{
    /// <summary>
    /// Выполняет частотный тест для последовательности битов, сгенерированной BlumBlumShub.
    /// </summary>
    /// <param name="bbs">Экземпляр генератора BBS.</param>
    /// <param name="numberOfBits">Количество битов для тестирования (рекомендуется >= 100).</param>
    /// <param name="alpha">Уровень значимости (обычно 0.01 или 0.05).</param>
    /// <returns>True, если тест пройден (p-value >= alpha), иначе False.</returns>
    public static bool RunTest(BlumBlumShub bbs, int numberOfBits, double alpha = 0.01)
    {
        if (bbs == null)
            throw new ArgumentNullException(nameof(bbs));
        if (numberOfBits < 2)
            throw new ArgumentException("Число битов должно быть не менее 2.", nameof(numberOfBits));

        int onesCount = 0;

        // Генерируем биты, беря младший бит каждого 32-битного числа
        for (int i = 0; i < numberOfBits; i++)
        {
            uint value = bbs.Next();
            // Младший бит
            if ((value & 1) == 1)
                onesCount++;
        }

        double total = numberOfBits;
        double proportion = onesCount / total;
        double sObs = Math.Abs(proportion - 0.5) * Math.Sqrt(total); // |S - N/2| / sqrt(N/4) преобразовано
        double pValue = Erfc(sObs / Math.Sqrt(2));

        Console.WriteLine($"Частотный тест NIST (Monobit)");
        Console.WriteLine($"Всего битов: {numberOfBits}");
        Console.WriteLine($"Количество единиц: {onesCount} ({proportion:P2})");
        Console.WriteLine($"p-value: {pValue:F6}");
        Console.WriteLine($"Уровень значимости: alpha = {alpha}");
        Console.WriteLine(pValue >= alpha ? "Результат: ТЕСТ ПРОЙДЕН (последовательность случайна)" : "Результат: ТЕСТ НЕ ПРОЙДЕН (последовательность не случайна)");

        return pValue >= alpha;
    }

    /// <summary>
    /// Дополнительная функция ошибок (erfc) для вычисления p-value.
    /// Реализация через аппроксимацию (достаточно точна для наших целей).
    /// </summary>
    private static double Erfc(double x)
    {
        // Аппроксимация Abramowitz & Stegun (26.2.17)
        double t = 1.0 / (1.0 + 0.3275911 * Math.Abs(x));
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double erf = 1.0 - (a1 * t + a2 * t * t + a3 * t * t * t + a4 * t * t * t * t + a5 * t * t * t * t * t) * Math.Exp(-x * x);
        double result = (x >= 0) ? 1.0 - erf : 1.0 + erf;
        return result; // для erfc достаточно 1 - erf(x) = erfc(x)
        // Но так как наша аппроксимация даёт erf, то erfc = 1 - erf
        // Однако для x > 0: erf(x) из формулы, тогда erfc(x) = 1 - erf(x)
        // Для x < 0: erf(x) = -erf(|x|), тогда erfc(x) = 1 + erf(|x|)
        // В коде выше уже учтено знаком x
    }
}

/// <summary>
/// Корректная реализация теста "Перекрывающиеся перестановки" из набора DIEHARD.
/// Проверяет равномерность распределения перестановок 5 последовательных 32-битных чисел.
/// </summary>
public static class DiehardOverlappingPermutationsTest
{
    /// <summary>
    /// Выполняет тест перекрывающихся перестановок.
    /// </summary>
    /// <param name="bbs">Генератор BBS.</param>
    /// <param name="blocksCount">Желаемое количество блоков (рекомендуется ≥ 10 000).</param>
    /// <param name="alpha">Уровень значимости.</param>
    /// <returns>True, если тест пройден.</returns>
    public static bool RunTest(BlumBlumShub bbs, int blocksCount, double alpha = 0.05)
    {
        if (bbs == null) throw new ArgumentNullException(nameof(bbs));
        if (blocksCount < 100) throw new ArgumentException("blocksCount >= 100", nameof(blocksCount));

        const int permSize = 5;          // 5 элементов в перестановке
        const int factPerm = 120;        // 5! = 120
        int[] observed = new int[factPerm];

        // Генерируем последовательность 32-битных чисел (uint)
        // Нужно blocksCount + permSize - 1 чисел, чтобы получить blocksCount перекрывающихся блоков
        int totalNumbers = blocksCount + permSize - 1;
        uint[] numbers = new uint[totalNumbers];
        for (int i = 0; i < totalNumbers; i++)
        {
            numbers[i] = bbs.Next();
        }

        int validBlocks = 0;
        // Анализируем каждый блок из 5 последовательных чисел
        for (int i = 0; i < blocksCount; i++)
        {
            uint[] block = new uint[permSize];
            Array.Copy(numbers, i, block, 0, permSize);

            // Проверяем, все ли числа различны (иначе блок пропускаем – классический тест требует строгого порядка)
            bool allDistinct = true;
            for (int j = 0; j < permSize && allDistinct; j++)
                for (int k = j + 1; k < permSize; k++)
                    if (block[j] == block[k])
                    {
                        allDistinct = false;
                        break;
                    }

            if (!allDistinct) continue; // пропускаем блоки с повторами

            // Получаем индекс перестановки (0..119)
            int permIndex = GetPermutationIndex(block);
            observed[permIndex]++;
            validBlocks++;
        }

        if (validBlocks < factPerm * 5)
        {
            Console.WriteLine($"Предупреждение: слишком мало блоков без повторов ({validBlocks}). Увеличьте blocksCount.");
            return false;
        }

        // Критерий хи-квадрат
        double expected = (double)validBlocks / factPerm;
        double chiSquare = 0;
        for (int i = 0; i < factPerm; i++)
        {
            double diff = observed[i] - expected;
            chiSquare += diff * diff / expected;
        }

        int degreesOfFreedom = factPerm - 1;
        double pValue = ChiSquarePValue(chiSquare, degreesOfFreedom);

        Console.WriteLine($"Тест DIEHARD: Перекрывающиеся перестановки (5 uint)");
        Console.WriteLine($"Всего блоков (с повторами): {blocksCount}");
        Console.WriteLine($"Блоков без повторов: {validBlocks}");
        Console.WriteLine($"Ожидаемое число на перестановку: {expected:F2}");
        Console.WriteLine($"Хи-квадрат = {chiSquare:F4}, df = {degreesOfFreedom}");
        Console.WriteLine($"p-value = {pValue:F6}");
        Console.WriteLine(pValue >= alpha ? "ТЕСТ ПРОЙДЕН" : "ТЕСТ НЕ ПРОЙДЕН");
        return pValue >= alpha;
    }

    /// <summary>
    /// Возвращает индекс перестановки (лексикографический) для массива уникальных чисел.
    /// </summary>
    private static int GetPermutationIndex(uint[] values)
    {
        // Сортируем значения, чтобы определить их относительный порядок
        uint[] sorted = new uint[values.Length];
        Array.Copy(values, sorted, values.Length);
        Array.Sort(sorted);

        // Находим ранги (0..4)
        int[] ranks = new int[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            // Ищем позицию values[i] в sorted (учитывая уникальность, можно просто IndexOf)
            ranks[i] = Array.IndexOf(sorted, values[i]);
        }

        // Преобразуем ранги в индекс перестановки факториальной системой счисления
        int index = 0;
        int[] factorial = { 1, 1, 2, 6, 24 };
        bool[] used = new bool[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            int countLess = 0;
            for (int j = 0; j < ranks[i]; j++)
                if (!used[j]) countLess++;
            index += countLess * factorial[values.Length - 1 - i];
            used[ranks[i]] = true;
        }
        return index;
    }

    private static double ChiSquarePValue(double chiSquare, int df)
    {
        // Аппроксимация нормальным распределением для больших df
        double x = Math.Sqrt(2 * chiSquare) - Math.Sqrt(2 * df - 1);
        double p = 0.5 * Erfc(x / Math.Sqrt(2));
        return Math.Max(0, Math.Min(1, p));
    }

    private static double Erfc(double x)
    {
        double t = 1.0 / (1.0 + 0.3275911 * Math.Abs(x));
        double a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741;
        double a4 = -1.453152027, a5 = 1.061405429;
        double erf = 1.0 - (a1 * t + a2 * t * t + a3 * t * t * t + a4 * t * t * t * t + a5 * t * t * t * t * t) * Math.Exp(-x * x);
        return (x >= 0) ? 1.0 - erf : 1.0 + erf;
    }
}
