using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{
    class GammaChromosome : SpreadChromosome
    {
        public GammaChromosome() : base(null, false)
        {
            // max is exclusive
            Exponent = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 20));
            Trend = Factory.Create(() => RandomizationProvider.Current.GetDouble(-110, 110));
            Rebalance = Factory.Create(() => RandomizationProvider.Current.GetInt(3, 5)); // always/smart
            //FunctionGene = Factory.Create(() => RandomizationProvider.Current.GetInt(0, _functions.Length));

            //Static gene example:
            //Trend = _factory.Create(0d);
            FunctionGene = Factory.Create(3);

            FinalizeGenes();
        }

        #region Strategy

        public GeneWrapper<double> Exponent { get; }
    
        public GeneWrapper<double> Trend { get; }
        public GeneWrapper<int> Rebalance { get; }

        private readonly string[] _functions = { "halfhalf", "keepvalue", "gauss", "exponencial" }; //"invsqrtsinh"
        private GeneWrapper<int> FunctionGene { get; }
        public string Function => _functions[FunctionGene.Value];

        #endregion

        public override Type CsvAggregatedMapType => typeof(AggregatedChromosomeCsvMap);

        public override Type CsvSingleMapType => typeof(SingleChromosomeCsvMap);

        public override Type CsvRecordType => typeof(GammaChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new GammaChromosome();

        public override BacktestRequest ToBacktestRequest(bool export)
        {
            return ChromosomeExtensions.ToBacktestRequest(this);
        }
    }
}
