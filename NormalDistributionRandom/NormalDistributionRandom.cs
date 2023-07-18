namespace NormalDistributionRandom
{
    public class DistributedRandom
    {
        /// <summary>
        /// The inclusive lower bound of the random number returned.
        /// </summary>
        public int InclusiveMin { get; set; }

        /// <summary>
        /// The inclusive upper bound of the random number returned. Must be greater than or equal to InclusiveMin.
        /// </summary>
        public int InclusiveMax { get; set; }

        /// <summary>
        /// The value that the normal distribution should be weighted towards. Must be within the range of InclusiveMin to Inclusive Max.
        /// </summary>
        public int WeightedTowards { get; set; }

        /// <summary>
        /// The strength of the weighting, written as a percent from 0 to 100.
        /// </summary>
        public int WeightedStrength { get; set; }

        /// <summary>
        /// How wide the normal distribution should be around the WeightedTowards value.
        /// </summary>
        public int WeightedSpread { get; set; }

        /// <summary>
        /// How fine the normal distribution should be. Must be between 10 and 100. Default is 100.
        /// </summary>
        public int Granularity { get; set; }

        /// <summary>
        /// Initializes a new DistributedRandom object, which can be used to configure, test, and run a randomly distributed selection.
        /// </summary>
        /// <param name="inclusiveMin">The inclusive lower bound of the random number returned.</param>
        /// <param name="inclusiveMax">The inclusive upper bound of the random number returned. Must be greater than or equal to <paramref name="inclusiveMin"/>.</param>
        /// <param name="weightedTowards">The value that the normal distribution should be weighted towards. Must be within the range of <paramref name="inclusiveMin"/> to <paramref name="inclusiveMax"/>.</param>
        /// <param name="weightedStrength">The strength of the weighting, written as a percent from 0 to 100.</param>
        /// <param name="weightedSpread">How wide the normal distribution should be around the WeightedTowards value.</param>
        /// <param name="granularity">How fine the normal distribution should be. Must be between 10 and 100. Default is 100.</param>
        public DistributedRandom(int inclusiveMin, int inclusiveMax, int weightedTowards, int weightedStrength, int weightedSpread, int granularity = 100)
        {
            InclusiveMin = inclusiveMin;
            InclusiveMax = inclusiveMax;
            WeightedTowards = weightedTowards;
            WeightedStrength = weightedStrength;
            WeightedSpread = weightedSpread;
            Granularity = granularity;
        }

        /// <summary>
        /// Calculates the values for the currently confgiured random distribution, and returns the likelihood of each value being selected on a random selection.
        /// </summary>
        /// <returns>Dictionary of <int, double>. The key is a number between the InclusiveMin and InclusiveMax. The value is the percentage chance that that number will be the one selected.</returns>
        public Dictionary<int, double> GetDistribution()
        {
            List<int> buckets = new();
            FillBuckets(InclusiveMin, InclusiveMax, WeightedTowards, WeightedStrength, WeightedSpread, Granularity, buckets);
            return buckets.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count() / (double) buckets.Count * 100);
        }

        /// <summary>
        /// Calculates the values for the currently confgiured random distribution, and returns a string representation of the likelihood of each value being selected on a random selection.
        /// </summary>
        /// <returns>String of the format: "Key: Value\n" where Key is a number between the InclusiveMin and InclusiveMax and the Value is the percentage chance that that number will be the one selected.</returns>
        public string GetDistributionToString()
        {
            return string.Join("\n", GetDistribution().Select(item => $"{item.Key}: {String.Format("{0:#,0.000}", item.Value)}"));
        }

        /// <summary>
        /// Selects a random number using the current configuration.
        /// </summary>
        /// <returns>The selected number as an int.</returns>
        public int Next()
        {
            return Next(InclusiveMin, InclusiveMax, WeightedTowards, WeightedStrength, WeightedSpread, Granularity);
        }

        /// <summary>
        /// Selects a random number using the configuration that is passed in. These passed in values will not overwrite the configuration that is already stored on the object.
        /// </summary>
        /// <param name="inclusiveMin">The inclusive lower bound of the random number returned.</param>
        /// <param name="inclusiveMax">The inclusive upper bound of the random number returned. Must be greater than or equal to <paramref name="inclusiveMin"/>.</param>
        /// <param name="weightedTowards">The value that the normal distribution should be weighted towards. Must be within the range of <paramref name="inclusiveMin"/> to <paramref name="inclusiveMax"/>.</param>
        /// <param name="weightedStrength">The strength of the weighting, written as a percent from 0 to 100.</param>
        /// <param name="weightedSpread">How wide the normal distribution should be around the WeightedTowards value.</param>
        /// <param name="granularity">How fine the normal distribution should be. Must be between 10 and 100. Default is 100.</param>
        /// <returns>The selected number as an int.</returns>
        public int Next(int inclusiveMin, int inclusiveMax, int weightedTowards, int weightedStrength, int weightedSpread, int granularity = 100)
        {

            List<int> buckets = new();

            FillBuckets(inclusiveMin, inclusiveMax, weightedTowards, weightedStrength, weightedSpread, granularity, buckets);

            int index = new Random().Next(0, buckets.Count);
            return buckets[index];
        }

        /// <summary>
        /// Runs a random selection for the number of times specified, reporting back how frequently each possible value was selected.
        /// </summary>
        /// <param name="times">The number of times that the random selection should be run.</param>
        /// <returns>Dictionary of <int, int>. The key is a number between the InclusiveMin and InclusiveMax. The value is how many times that number was the one selected.</returns>
        public Dictionary<int, int> MultipleNext(int times)
        {
            Dictionary<int, int> distributionResults = new();
            for (int i = InclusiveMin; i <= InclusiveMax; i++) { distributionResults.Add(i, 0); }
            for (int j = 0; j < times; j++) { distributionResults[Next()]++; }
            return distributionResults;
        }

        /// <summary>
        /// Runs a random selection for the number of times specified, reporting back a string representation of how frequently each possible value was selected.
        /// </summary>
        /// <param name="times">The number of times that the random selection should be run.</param>
        /// <returns>String of the format: "Key: Value\n" where the key is a number between the InclusiveMin and InclusiveMax and the value is how many times that number was the one selected.</returns>
        public string MultipleNextToString(int times)
        {
            return string.Join("\n", MultipleNext(times).Select(item => $"{item.Key}: {item.Value}"));
        }

        private void FillBuckets(int min, int max, int weightedTowards, int weightedStrength, int weightedSpread, int granularity, List<int> buckets)
        {
            ValidateInput();

            int weightedMinIndex = Math.Max(weightedTowards - weightedSpread, min) - min;
            int weightedMaxIndex = Math.Min(weightedTowards + weightedSpread, max) - min;

            int totalSpan = max - min + 1;
            int weightedSpan = weightedMaxIndex - weightedMinIndex + 1;

            int minEntriesPerBucket = (granularity * (100-weightedStrength)) / 100;
            int additionalEntriesForWeightedBuckets = (granularity-minEntriesPerBucket) * totalSpan;

            int[] bucketQuotas = new int[totalSpan];
            for (int i = 0; i < bucketQuotas.Length; i++)
            {
                bucketQuotas[i] = minEntriesPerBucket;
            }

            double mean = weightedTowards;
            double stdDev = weightedSpan / 6.0;
            double[] probabilities = new double[totalSpan];
            double totalProbability = 0;

            for (int i = 0; i < totalSpan; i++)
            {
                int value = min + i;
                probabilities[i] = Math.Exp(-Math.Pow(value - mean, 2) / (2 * Math.Pow(stdDev, 2))) / (stdDev * Math.Sqrt(2 * Math.PI));
                totalProbability += probabilities[i];
            }

            for (int i = weightedMinIndex; i <= weightedMaxIndex; i++)
            {
                bucketQuotas[i] += (int)Math.Round(additionalEntriesForWeightedBuckets * probabilities[i] / totalProbability);
            }

            int bucketQuotaIndex = 0;
            for (int i = min; i <= max; i++)
            {
                for (int j = 1; j <= bucketQuotas[bucketQuotaIndex]; j++)
                {
                    buckets.Add(i);
                }
                bucketQuotaIndex++;
            }
        }

        private void ValidateInput()
        {
            if (InclusiveMin >= InclusiveMax) 
                throw new ArgumentException("The max must be larger than the min");
            if (WeightedTowards < InclusiveMin || WeightedTowards > InclusiveMax) 
                throw new ArgumentException("The weight must be contained within the range of values");
            if(Granularity < 10 || Granularity > 100)
                throw new ArgumentException("Granularity should be between 10 and 100");
        }
    }
}