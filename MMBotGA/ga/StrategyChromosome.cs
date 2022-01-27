using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{

    class StrategyChromosome : SpreadChromosome
    {
        public StrategyChromosome() : base(null, false)
        {
            // max is exclusive
            Exponent = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 20));
            Trend = Factory.Create(() => RandomizationProvider.Current.GetDouble(-110, 110));
            Rebalance = Factory.Create(() => RandomizationProvider.Current.GetInt(3, 5)); // 0-5
            FunctionGene = Factory.Create(() => RandomizationProvider.Current.GetInt(0, 5));

            //Static gene example:
            //Trend = _factory.Create(0d);

            FinalizeGenes();
        }

        #region Strategy

        public GeneWrapper<double> Exponent { get; }
    
        public GeneWrapper<double> Trend { get; }
        public GeneWrapper<int> Rebalance { get; }

        private readonly string[] _functions = { "halfhalf", "keepvalue", "gauss", "exponencial", "invsqrtsinh" };
        private GeneWrapper<int> FunctionGene { get; }
        public string Function => _functions[FunctionGene.Value];

        #endregion

        public override Type CsvAggregatedMapType => typeof(AggregatedChromosomeCsvMap);

        public override Type CsvSingleMapType => typeof(SingleChromosomeCsvMap);

        public override Type CsvRecordType => typeof(StrategyChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new StrategyChromosome();

        public override BacktestRequest ToBacktestRequest()
        {
            return ChromosomeExtensions.ToBacktestRequest(this);
        }
    }
}
