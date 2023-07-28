using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KoheiUtils.Tests
{
    using NUnit.Framework;

    public class Tests_WAM
    {
        [Test]
        public void Test_IndexRandom()
        {
            for (int i = 0; i < 100000000; i++)
            {
                // float r = Random.Range(0f, 0.999999f) * 2;
                float r = Random.Range(0f, 0.99999f) * 2;
                int index = (int) Math.Floor(r);

                if (!(index == 0 || index == 1))
                {
                    Debug.Log(r);
                    Debug.Log(index);
                }

                Assert.IsTrue(index == 0 || index == 1);
            }
        }

        [Test]
        public void Test_IndexRandom100()
        {
            for (int i = 0; i < 100000000; i++)
            {
                // float r = Random.Range(0f, 0.999999f) * 2;
                float r = Random.Range(0f, 0.99999f) * 100;
                int index = (int) Math.Floor(r);

                if (index < 0 || 100 <= index)
                {
                    Debug.Log(r);
                    Debug.Log(index);
                }

                Assert.IsTrue(0 <= index && index < 100);
            }
        }

        [Test]
        public void Test_Randomness()
        {
            DefaultDictionary<int, int> index2count = new();

            float error = 0.01f;
            float[] weights = new[] {0.55f, 0.3f, 0.09f, 0.059f, 0.001f};
            WAM wam = new WAM(weights);
            int totalSize = 10000000;

            for (int i = 0; i < totalSize; i++)
            {
                index2count[wam.SelectOne()] += 1;
            }

            foreach (var kvp in index2count)
            {
                int index = kvp.Key;
                int count = kvp.Value;

                Assert.IsTrue(0 <= index && index < weights.Length);

                int required = (int) (weights[index] * totalSize);

                if (required - required * error < count && count < required + required * error)
                {
                    Debug.Log($"{index}: {count} --- IdealCount: {required}");
                }
                else
                {
                    Debug.LogError($"{index}: {count} --- IdealCount: {required}");
                }
            }
        }
        
        [Test]
        public void Test_ChangeWAM()
        {
            WAM wam = new WAM(new float[] { 99f, 1f });

            int[] counts = new int[2];
            
            for (int i = 0; i < 10000; i++)
            {
                int index = wam.SelectOne();
                counts[index] += 1;
            }
            
            Debug.Log("WAM=" + wam);
            Debug.Log("結果(0): " + counts[0]);
            Debug.Log("結果(1): " + counts[1]);
            
            wam.SetWeight(0, 5f);
            wam.SetWeight(1, 95f);

            counts = new int[2];
            
            for (int i = 0; i < 10000; i++)
            {
                int index = wam.SelectOne();
                counts[index] += 1;
            }
            
            Debug.Log("WAM=" + wam);
            Debug.Log("結果(0): " + counts[0]);
            Debug.Log("結果(1): " + counts[1]);
            
            wam.SetWeight(0, 0f, autoSetup: false);
            wam.SetWeight(1, 95f, autoSetup: false);
            wam.Setup();

            counts = new int[2];
            
            for (int i = 0; i < 1000000; i++)
            {
                int index = wam.SelectOne();
                counts[index] += 1;
            }
            
            Debug.Log("WAM=" + wam);
            Debug.Log("結果(0): " + counts[0]);
            Debug.Log("結果(1): " + counts[1]);
            
            wam.SetWeight(0, 99.9f);
            wam.SetWeight(1, 0.1f);

            counts = new int[2];
            
            for (int i = 0; i < 1000000; i++)
            {
                int index = wam.SelectOne();
                counts[index] += 1;
            }
            
            Debug.Log("WAM=" + wam);
            Debug.Log("結果(0): " + counts[0]);
            Debug.Log("結果(1): " + counts[1]);
        }
    }
}