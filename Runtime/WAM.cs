using System.Collections.Generic;

namespace KoheiUtils
{
    using System;
    using System.Linq;
    using Random = UnityEngine.Random;

    public class WAM
    {
        readonly float[] weights;
        readonly float[] p;
        readonly int[]   a;
        
        float wsm;

        public int Length => weights.Length;
        public float TotalWight => wsm;

        public WAM(IEnumerable<float> weightArray)
        {
            weights = weightArray.ToArray();

            p = new float[weights.Length];
            a = new int[weights.Length];

            Setup();
        }

        public void Setup()
        {
            int[] hl = new int[weights.Length];
            int   l  = 0;
            int   h  = weights.Length - 1;

            wsm = weights.Sum();
            Array.Clear(a, 0, weights.Length);
            Array.Clear(hl, 0, weights.Length);

            for (int i = 0; i < p.Length; i++)
            {
                p[i] = weights[i] * p.Length / wsm;
            }

            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] < 1)
                {
                    hl[l] =  i;
                    l     += 1;
                }
                else
                {
                    hl[h] =  i;
                    h     -= 1;
                }
            }

            while (l > 0 && h < p.Length - 1)
            {
                int j = hl[l - 1];
                int k = hl[h + 1];

                a[j] =  k;
                p[k] += p[j] - 1;

                if (p[k] < 1)
                {
                    hl[l - 1] =  k;
                    h         += 1;
                }
                else
                {
                    l -= 1;
                }
            }
        }
        
        public float GetWeight(int index)
        {
            return weights[index];
        }
        
        public void SetWeight(int index, float weight, bool autoSetup = true)
        {
            weights[index] = weight;

            if (autoSetup)
            {
                Setup();
            }
        }
        
        public int SelectOne()
        {
            float r = Random.Range(0f, 0.99999f) * p.Length;
            int   i = (int) Math.Floor(r);
            r -= i;

            return r < p[i] ? i : a[i];
        }
        
        public int SelectOne(System.Random random)
        {
            if (random is not null)
            {
                double r = random.NextDouble() * p.Length;
                int   i = (int) Math.Floor(r);
                r -= i;

                return r < p[i] ? i : a[i];
            }
            else
            {
                return SelectOne();
            }
        }

        public float[] ToWeights()
        {
            return weights.ToArray();
        }

        public override string ToString()
        {
            return $"{nameof(weights)}: {weights.ToString<float>()}, total weights: {wsm}";
        }
    }
}