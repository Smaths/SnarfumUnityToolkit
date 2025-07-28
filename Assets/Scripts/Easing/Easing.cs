using System;
using UnityEngine;

namespace Easing
{
    public static class Easing
    {
        public static Func<float, float> Linear = t => t;

        public static Func<float, float> InQuad = t => t * t;
        public static Func<float, float> OutQuad = t => 1 - (1 - t) * (1 - t);
        public static Func<float, float> InOutQuad = t => t < 0.5 ? 2 * t * t : 1 - 2 * (1 - t) * (1 - t);

        public static Func<float, float> InCubic = t => t * t * t;
        public static Func<float, float> OutCubic = t => 1 - (1 - t) * (1 - t) * (1 - t);
        public static Func<float, float>
            InOutCubic = t => t < 0.5 ? 4 * t * t * t : 1 - 4 * (1 - t) * (1 - t) * (1 - t);

        public static Func<float, float> InQuart = t => t * t * t * t;
        public static Func<float, float> OutQuart = t => 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t);
        public static Func<float, float> InOutQuart = t =>
            t < 0.5 ? 8 * t * t * t * t : 1 - 8 * (1 - t) * (1 - t) * (1 - t) * (1 - t);

        public static Func<float, float> InQuint = t => t * t * t * t * t;
        public static Func<float, float> OutQuint = t => 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t) * (1 - t);
        public static Func<float, float> InOutQuint = t =>
            t < 0.5 ? 16 * t * t * t * t * t : 1 - 16 * (1 - t) * (1 - t) * (1 - t) * (1 - t) * (1 - t);

        public static Func<float, float> InSine = t => 1 - MathF.Cos(t * MathF.PI / 2);
        public static Func<float, float> OutSine = t => MathF.Sin(t * MathF.PI / 2);
        public static Func<float, float> InOutSine = t => -(MathF.Cos(MathF.PI * t) - 1) / 2;

        public static Func<float, float> InExpo = t => t == 0 ? 0 : MathF.Pow(2, 10 * t - 10);
        public static Func<float, float> OutExpo = t => t >= 1f - 1e-6f ? 1f : 1f - MathF.Pow(2f, -10f * t);
        public static Func<float, float> InOutExpo = t =>
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            if (t < 0.5f) return MathF.Pow(2, 20 * t - 10) / 2;
            return (2 - MathF.Pow(2, -20 * t + 10)) / 2;
        };

        public static Func<float, float> EaseInCircular = t => -(Mathf.Sqrt(1 - t * t) - 1);
        public static Func<float, float> EaseOutCircular = t => Mathf.Sqrt(1 - (--t) * t);
        public static Func<float, float> EaseInOutCircular = t =>
            (t *= 2) < 1 ? -0.5f * (Mathf.Sqrt(1 - t * t) - 1) : 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);

        public static Func<float, float> InElastic(float amplitude = 1, float period = 0.3f)
        {
            return t =>
            {
                if (t == 0) return 0;
                if (t == 1) return 1;
                float s = period / 4;
                float p = amplitude * MathF.Pow(2, 10 * (t - 1));
                return -(p * MathF.Sin((t - s) * (2 * MathF.PI) / period));
            };
        }

        public static Func<float, float> OutElastic(float amplitude = 1, float period = 0.3f)
        {
            return t =>
            {
                if (t == 0) return 0;
                if (t == 1) return 1;
                float s = period / 4;
                float p = amplitude * MathF.Pow(2, -10 * t);
                return p * MathF.Sin((t - s) * (2 * MathF.PI) / period) + 1;
            };
        }

        public static Func<float, float> InOutElastic(float amplitude = 1, float period = 0.45f)
        {
            return t =>
            {
                if (t == 0) return 0;
                if (t == 1) return 1;
                float s = period / 4;
                float p = amplitude * MathF.Pow(2, 10 * (t - 1));
                if (t < 0.5)
                {
                    return -0.5f * p * MathF.Sin((t * 2 - s) * (2 * MathF.PI) / period);
                }
                else
                {
                    return p * MathF.Sin(((t * 2 - 1) - s) * (2 * MathF.PI) / period) * 0.5f + 1;
                }
            };
        }

        // Missing Bounce and Back easy types
    }
}