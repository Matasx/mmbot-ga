using GeneticSharp.Domain.Chromosomes;
using MMBotGA.backtest;
using MMBotGA.ga.fitness;
using System;

namespace MMBotGA.ga
{
    interface ICustomChromosome : IChromosome
    {
        string ID { get; set; }
        int Generation { get; set; }
        string Metadata { get; set; }

        Type CsvAggregatedMapType { get; }
        Type CsvSingleMapType { get; }
        Type CsvRecordType { get; }

        Statistics Statistics { get; set; }
        Statistics BacktestStats { get; set; }
        Statistics ControlStats { get; set; }

        FitnessComposition FitnessComposition { get; set; }

        BacktestRequest ToBacktestRequest();
    }
}