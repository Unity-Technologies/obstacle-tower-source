using System;
using UnityEngine;

namespace MLAgents.InferenceBrain.Utils
{
    /// <summary>
    /// RandomNormal - A random number generator that produces normally distributed random numbers using the Marsaglia
    /// polar method (https://en.wikipedia.org/wiki/Marsaglia_polar_method)
    /// TODO: worth overriding System.Random instead of aggregating?
    /// </summary>
    public class RandomNormal
    {
        private readonly double m_mean;
        private readonly double m_stddev;
        private readonly System.Random m_random;

        public RandomNormal(int seed, float mean = 0.0f, float stddev = 1.0f)
        {
            m_mean = mean;
            m_stddev = stddev;
            m_random = new System.Random(seed);
        }

        // Each iteration produces two numbers. Hold one here for next call
        private bool m_hasSpare = false;
        private double m_spare = 0.0f;

        /// <summary>
        /// Return the next random double number
        /// </summary>
        /// <returns>Next random double number</returns>
        public double NextDouble()
        {
            if (m_hasSpare)
            {
                m_hasSpare = false;
                return m_spare * m_stddev + m_mean;
            }

            double u, v, s;
            do
            {
                u = m_random.NextDouble() * 2.0 - 1.0;
                v = m_random.NextDouble() * 2.0 - 1.0;
                s = u * u + v * v;
            } while (s >= 1.0 || s == 0.0);

            s = Math.Sqrt(-2.0 * Math.Log(s) / s);
            m_spare = u * s;
            m_hasSpare = true;

            return v * s * m_stddev + m_mean;
        }

        /// <summary>
        /// Fill a pre-allocated Tensor with random numbers
        /// </summary>
        /// <param name="t">The pre-allocated Tensor to fill</param>
        /// <exception cref="NotImplementedException">Throws when trying to fill a Tensor of type other than float</exception>
        /// <exception cref="ArgumentNullException">Throws when the Tensor is not allocated</exception>
        public void FillTensor(TensorProxy t)
        {
            if (t.DataType != typeof(float))
            {
                throw new NotImplementedException("Random Normal does not support integer tensors yet!");
            }

            if (t.Data == null)
            {
                throw new ArgumentNullException();
            }

            for (int i = 0; i < t.Data.length; i++)
                t.Data[i] = (float)NextDouble();
        }
    }
}
