using NormalDistributionRandom;

DistributedRandom distributedRandom = new DistributedRandom(inclusiveMin: 1, inclusiveMax: 10, weightedTowards: 7, weightedStrength: 70, weightedSpread: 3, granularity: 100);

Console.WriteLine(distributedRandom.GetDistributionToString());
Console.WriteLine();
Console.WriteLine(distributedRandom.MultipleNextToString(1000));