using System.Diagnostics;
using System.Numerics;

internal class Program
{
    private static void Main(string[] args)
    {
        int count = 100000;
        Graph();
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
    static void Graph()
    {
        Stopwatch sw = new Stopwatch();
        Console.WriteLine("Генерация 10000 чисел с помощью LCG...\n");

        var lcg = new LinearCongruentialGenerator(12345);
        const int count = 10000000;
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

        Console.WriteLine($"\nОбщее количество чисел: {count}");
        Console.WriteLine($"Максимальная частота в одном бине: {maxCount}");
        Console.WriteLine("При идеальном равномерном распределении в каждом бине должно быть примерно {0} чисел.",
                          count / bins);
        Console.WriteLine($"Время на генерацию: {sw.Elapsed}");

        double mean = numbers.Average();
        double variance = numbers.Sum(x => (x - mean) * (x - mean)) / count;           // population variance
        double sampleVariance = numbers.Sum(x => (x - mean) * (x - mean)) / (count - 1); // sample variance

        Console.WriteLine("=== Результаты анализа LCG ===");
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
            histogram[bin]++;
        }

        // === Статистика ===
        double meanBlum = numbers.Average();
        double varianceBlum = numbers.Sum(x => (x - meanBlum) * (x - meanBlum)) / count;

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

            int barLength = (int)((double)histogram[i] / maxCount * maxBarWidth);
            string bar = new string('█', barLength);

            Console.WriteLine($"{binStart:F3} - {binEnd:F3} | {bar,-60}  ({histogram[i]})");
        }

        Console.WriteLine($"\nПри идеальном равномерном распределении в каждом бине {count / bins} чисел.");
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