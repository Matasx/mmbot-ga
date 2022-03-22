using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{
    class DcaChromosome : SpreadChromosome
    {
        public DcaChromosome() : base(null, false)
        {
            // max is exclusives
            Reduction = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 200));
            Power = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 20));
            Exponent = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.1, 10)); // always/smart
            //MaxSpread = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 0));

            //Static gene example:
            MaxSpread = Factory.Create(0d);
            //FunctionGene = Factory.Create(0);

            FinalizeGenes();
        }

        #region Strategy

        public GeneWrapper<double> Exponent { get; }
        public GeneWrapper<double> Power { get; }
        public GeneWrapper<double> Reduction { get; }
        public GeneWrapper<double> MaxSpread { get; }

        #endregion

        public override Type CsvAggregatedMapType => typeof(AggregatedDcaChromosomeCsvMap);

        public override Type CsvSingleMapType => typeof(SingleDcaChromosomeCsvMap);

        public override Type CsvRecordType => typeof(DcaChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new DcaChromosome();

        public override BacktestRequest ToBacktestRequest(bool export)
        {
            return ChromosomeExtensions.ToBacktestRequest(this, export);
        }
    }
}
