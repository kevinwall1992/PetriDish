using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathUtility
{
    public static int RandomIndex(int count)
    {
        return (int)(Random.value* 0.999999f* count);
    }

    public static T RandomElement<T>(List<T> list)
    {
        return list[RandomIndex(list.Count)];
    }

    public static T RemoveRandomElement<T>(List<T> list)
    {
        return Utility.RemoveElementAt(list, RandomIndex(list.Count));
    }

    public static bool Flip(float heads = 0.5f)
    {
        return Random.value > (1- heads);
    }

    static float SimpleRecursiveLimit(float scale, int recursions_left)
    {
        if (Mathf.Abs(scale) > 0.5f)
            return scale > 0 ? float.PositiveInfinity : float.NegativeInfinity;

        if (recursions_left == 0)
            return scale;

        return scale* (1 + SimpleRecursiveLimit(scale, recursions_left - 1));
    }

    public static float SimpleRecursiveLimit(float scale)
    {
        return SimpleRecursiveLimit(scale, 5);
    }

    public static float Sum<T>(List<T> list, System.Func<T, float> function)
    {
        float sum = 0;

        foreach (T element in list)
            sum += function(element);

        return sum;
    }

    public static int Sum<T>(List<T> list, System.Func<T, int> function)
    {
        int sum = 0;

        foreach (T element in list)
            sum += function(element);

        return sum;
    }
}

public abstract class GenericFunction<T>
{
    public abstract float Compute(T x);
}

public abstract class Function : GenericFunction<float>
{
    public float Integrate(float x0, float x1)
    {
        float total = 0;

        int sample_count = 100;
        float width = (x1 - x0) / sample_count;

        for (int i = 0; i < sample_count; i++)
            total += Compute(Mathf.Lerp(x0, x1, i / (float)(sample_count - 1)) + (width / 2)) * width;

        return total;
    } 
}

public abstract class ProbabilityDistribution : Function
{
    public abstract float Minimum
    {
        get;
    }

    public abstract float Maximum
    {
        get;
    }

    public float Range
    {
        get { return Maximum - Minimum; }
    }

    public float Median
    {
        get { return InverseCDF(0.5f); }
    }

    float InverseCDF(float percentile, float test_sample, int iteration)
    {
        float test_percentile = CDF(test_sample);

        if (iteration < 5)
        {
            if (percentile > test_percentile)
                return InverseCDF(percentile, test_sample + Mathf.Pow(0.5f, iteration + 2) * Range, iteration + 1);
            else
                return InverseCDF(percentile, test_sample - Mathf.Pow(0.5f, iteration + 2) * Range, iteration + 1);
        }

        return test_sample;
    }

    public float InverseCDF(float percentile)
    {
        return InverseCDF(percentile, Minimum + Range / 2, 0);
    }

    public float CDF(float x)
    {
        x = Mathf.Clamp(x, Minimum, Maximum);

        return Integrate(Minimum, x) / Integrate(Minimum, Maximum);
    }

    public float GetSample(float percentile)
    {
        return InverseCDF(percentile);
    }

    public float GetRandomSample()
    {
        return InverseCDF(Random.value);
    }
} 

public class UniformDistribution : ProbabilityDistribution
{
    public override float Minimum
    {
        get { return 0.0f; }
    }

    public override float Maximum
    {
        get { return 1.0f; }
    }

    public override float Compute(float x)
    {
        return x;
    }
}

public class NormalDistribution : ProbabilityDistribution
{
    protected float mean, standard_deviation;

    public override float Minimum
    {
        get { return mean - MaximumDeviation; }
    }

    public override float Maximum
    {
        get { return mean + MaximumDeviation; }
    }

    protected float MaximumDeviation
    {
        get { return standard_deviation * 3; }
    }

    public NormalDistribution(float mean_, float maximum_deviation)
    {
        mean = mean_;
        standard_deviation = maximum_deviation / 3;
    }

    public override float Compute(float x)
    {
        if (x < Minimum || x > Maximum)
            return 0;

        float sample = Mathf.Pow
                       (
                            (float)System.Math.E,
                            -Mathf.Pow(x - mean, 2) / (2 * Mathf.Pow(standard_deviation, 2))
                       )
                      / standard_deviation
                      / Mathf.Sqrt(2 * Mathf.PI);

        return sample;
    }
}

public class SkewedNormalDistribution : ProbabilityDistribution
{
    NormalDistribution normal_distribtion;
    float base_mean;
    float skew;

    public override float Minimum
    {
        get { return normal_distribtion.Minimum; }
    }

    public override float Maximum
    {
        get
        {
            return base_mean + (normal_distribtion.Maximum - base_mean) * skew;
        }
    }

    public SkewedNormalDistribution(float mean, float range, float skew_)
    {
        normal_distribtion = new NormalDistribution(mean, range);
        base_mean = mean;

        skew = skew_;
    }

    public override float Compute(float x)
    {
        return normal_distribtion.Compute(x < base_mean ? x : (x / skew));
    }
}

public class ChoiceFunction : ProbabilityDistribution
{
    float probability;

    public ChoiceFunction(float probability_)
    {
        probability = Mathf.Clamp(probability_, 0, 1);
    }

    public override float Minimum { get { return -1; } }
    public override float Maximum { get { return 1; } }

    public override float Compute(float x)
    {
        if (x < 0)
            return 1 - probability;
        else
            return probability;
    }
}