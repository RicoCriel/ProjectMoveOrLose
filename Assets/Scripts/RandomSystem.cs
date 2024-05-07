using UnityEngine;

    public static class RandomSystem
    {

        public static string Seedstring = "seed string";

        private static int Seed;

        public static Texture2D NoiseSource;

        public static bool NegateRepeating = true;

        static RandomSystem()
        {
            
        }

        public static void SetSeed(string seedString)
        {
            Seedstring = seedString;
            Seed = seedString.GetHashCode();

            Random.InitState(Seed);
        }

        // public static void SetNoiseSource(Texture2D noiseSource)
        // {
        //     NoiseSource = noiseSource;
        // }

        public static float GetRandomFloat(float minInclusive, float maxExclusive)
        {
            float Randomval = Random.value;
            float range = maxExclusive - minInclusive;
            float scaledRandom = Randomval * range;
            float shiftedRandom = scaledRandom + minInclusive;

            return shiftedRandom;
        }

        public static int GetRandomInt(int minInclusive, int maxExclusive, bool CheckForRepetition = false)
        {
            float Randomval = Random.value;
            int range = maxExclusive - minInclusive;
            float scaledRandom = Randomval * range;
            int shiftedRandom = (int)scaledRandom + minInclusive;

            return CheckforIntRepetition(minInclusive, maxExclusive, CheckForRepetition, shiftedRandom);        
        }

        public static float GetGaussianRandomFloat(float minInclusive, float maxExclusive)
        {
            float sum = 0f;

            for (int i = 0; i < 3; i++)
            {
                float Randomval = Random.value;
                float range = maxExclusive - minInclusive;
                float scaledRandom = Randomval * range;
                sum += scaledRandom + minInclusive;
            }

            sum *= 0.33333f;

            return sum;
        }

        public static int GetGaussianRandomInt(int minInclusive, int maxExclusive, bool CheckForRepetition)
        {
            float sum = 0;

            for (int i = 0; i < 3; i++)
            {
                float Randomval = Random.value;
                int range = maxExclusive - minInclusive;
                float scaledRandom = Randomval * range;
                sum += (int)scaledRandom + minInclusive;
            }

            sum *= 0.33333f;

            int intsum = Mathf.RoundToInt(sum);

            return CheckforIntRepetition(minInclusive, maxExclusive, CheckForRepetition, intsum);

        }

        //repetitionChecks
        private static int CheckforIntRepetition(int minInclusive, int maxExclusive, bool CheckForRepetition, int intsum)
        {
            if (CheckForRepetition)
            {
                if (IsRepeatingNumber(intsum))
                {
                    return GetGaussianRandomInt(minInclusive, maxExclusive, CheckForRepetition);
                }
                else
                {
                    return intsum;
                }
            }
            else
            {
                return intsum;
            }
        }

        private static int lastnumber = int.MaxValue;

        private static bool IsRepeatingNumber(int newnumber)
        {
            if (lastnumber == newnumber)
            {
                Debug.Log("repeatingnumber" + newnumber);
                return true;
            }
            else
            {
                lastnumber = newnumber;
                return false;
            }
        }
    }
