using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{
    class EpaChromosome : SpreadChromosome
    {
        public EpaChromosome() : base(null, false)
        {
            MinAssetPercOfBudget = Factory.Create(0.001);
            InitialBetPercOfBudget = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 1)); //0-1

            MaxEnterPriceDistance = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 0.5));
            PowerMult = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 10));
            PowerCap = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 10));

            Angle = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 70)); //0-90

            TargetExitPriceDistance = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 0.5));
            ExitPowerMult = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 10));

            FinalizeGenes();
        }

        #region Strategy

        public GeneWrapper<double> MinAssetPercOfBudget { get; }
        public GeneWrapper<double> InitialBetPercOfBudget { get; }

        public GeneWrapper<double> MaxEnterPriceDistance { get; }
        public GeneWrapper<double> PowerMult { get; }
        public GeneWrapper<double> PowerCap { get; }

        public GeneWrapper<double> Angle { get; }

        public GeneWrapper<double> TargetExitPriceDistance { get; }
        public GeneWrapper<double> ExitPowerMult { get; }

        #endregion

        public override Type CsvAggregatedMapType => typeof(AggregatedEpaChromosomeCsvMap);

        public override Type CsvSingleMapType => typeof(SingleEpaChromosomeCsvMap);

        public override Type CsvRecordType => typeof(EpaChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new EpaChromosome();

        public override BacktestRequest ToBacktestRequest()
        {
            return ChromosomeExtensions.ToBacktestRequest(this);
        }
    }
}
