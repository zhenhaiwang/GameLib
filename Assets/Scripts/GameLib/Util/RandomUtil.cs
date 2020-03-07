using System;
using System.Collections.Generic;

namespace GameLib
{
    public static class RandomUtil
    {
        private static readonly Random random = new Random((int)DateTime.Now.ToBinary());

        public static int Random(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int MTRandom(int min, int max, MersenneTwisterRandom mtRandom)
        {
            return mtRandom.Next(min, max);
        }

        /// <summary>
        /// 随机从 [0,max) 中取出不重复的 num 个整数
        /// </summary>
        public static int[] MTRandomNumbers(int num, int max, MersenneTwisterRandom mtRandom)
        {
            if (num < 0 || max < 0 || num > max)
            {
                return null;
            }

            int[] result = new int[num];
            int[] seed = new int[max];

            for (int i = 0; i < max; i++)
            {
                seed[i] = i;
            }

            for (int i = 0; i < num; i++)
            {
                int index = mtRandom.Next(0, max - i);
                result[i] = seed[index];
                seed[index] = seed[num - i - 1];
            }

            return result;
        }
    }

    public static class WeightRandomUtil
    {
        /// <summary>
        /// 索引权重随机
        /// </summary>
        public static List<int> MTRandomIndex(List<int> weights, int count, MersenneTwisterRandom mtRandom)
        {
            int weightCount = weights.Count();

            if (weightCount == 0 || count <= 0)
            {
                return null;
            }

            int totalWeight = 0;

            for (int i = 0; i < weightCount; i++)
            {
                totalWeight += weights[i];
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            List<int> resultIndexes = new List<int>(count);
            HashSet<int> resultTags = new HashSet<int>();

            while (resultIndexes.Count < count)
            {
                int randomWeight = mtRandom.Next(0, totalWeight);
                int currentWeight = 0;

                for (int i = 0; i < weightCount; i++)
                {
                    currentWeight += weights[i];

                    if (currentWeight > randomWeight)
                    {
                        if (!resultTags.Contains(i))
                        {
                            resultIndexes.Add(i);
                            resultTags.Add(i);
                        }

                        break;
                    }
                }
            }

            return resultIndexes;
        }

        /// <summary>
        /// 索引权重随机
        /// </summary>
        public static int MTRandomIndex(List<int> weights, MersenneTwisterRandom mtRandom)
        {
            List<int> resultIndexes = MTRandomIndex(weights, 1, mtRandom);

            if (resultIndexes.Count() > 0)
            {
                return resultIndexes[0];
            }

            return -1;
        }

        /// <summary>
        /// 主键权重随机
        /// </summary>
        public static List<string> MTRandomKey(List<KeyValuePair<string, int>> weights, int count, MersenneTwisterRandom mtRandom)
        {
            int weightCount = weights.Count();

            if (weightCount == 0 || count <= 0)
            {
                return null;
            }

            List<int> weightValues = new List<int>(weightCount);

            for (int i = 0; i < weightCount; i++)
            {
                weightValues.Add(weights[i].Value);
            }

            List<int> resultIndexes = MTRandomIndex(weightValues, count, mtRandom);

            if (resultIndexes.Count() != count)
            {
                return null;
            }

            List<string> resultKeys = new List<string>(count);

            for (int i = 0; i < count; i++)
            {
                resultKeys.Add(weights[resultIndexes[i]].Key);
            }

            return resultKeys;
        }

        /// <summary>
        /// 主键权重随机
        /// </summary>
        public static string MTRandomKey(List<KeyValuePair<string, int>> weights, MersenneTwisterRandom mtRandom)
        {
            List<string> resultKeys = MTRandomKey(weights, 1, mtRandom);

            if (resultKeys.Count() > 0)
            {
                return resultKeys[0];
            }

            return null;
        }
    }

    public sealed class WeightRandomGenerator
    {
        private MersenneTwisterRandom mtRandom;
        private List<WeightItem> _weightItems = new List<WeightItem>();
        private int _totalWeight;

        public WeightRandomGenerator(MersenneTwisterRandom mtRandom)
        {
            this.mtRandom = mtRandom;
        }

        public void AppendWeight(int key, int weight)
        {
            _weightItems.Add(new WeightItem(key, weight));
            _totalWeight += weight;
        }

        /// <summary>
        /// 权重随机
        /// </summary>
        public int MTRandom()
        {
            int randomWeight = mtRandom.Next(0, _totalWeight);
            int currentWeight = 0;

            foreach (WeightItem weightItem in _weightItems)
            {
                currentWeight += weightItem.weight;

                if (currentWeight > randomWeight)
                {
                    return weightItem.key;
                }
            }

            return -1;
        }

        public void Reset()
        {
            _weightItems.Clear();
            _totalWeight = 0;
        }

        private struct WeightItem
        {
            public int key;
            public int weight;

            public WeightItem(int key, int weight)
            {
                this.key = key;
                this.weight = weight;
            }
        }
    }
}
