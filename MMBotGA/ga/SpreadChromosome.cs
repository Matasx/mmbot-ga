using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.dto;
using MMBotGA.ga.fitness;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{
    class SpreadChromosome : ChromosomeBase, ICustomChromosome
    {
        protected readonly GeneFactory Factory;
        private readonly Strategy _strategy;

        protected SpreadChromosome(Strategy fixedStrategy, bool finalize) : base(2)
        {
            _strategy = fixedStrategy;
            Factory = new GeneFactory(this);

            // max is exclusive
            Stdev = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 240));
            Sma = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 240));
            Mult = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.5, 2));
            Raise = Factory.Create(() => RandomizationProvider.Current.GetDouble(1, 1000));
            Fall = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.1, 10));
            Cap = Factory.Create(() => RandomizationProvider.Current.GetDouble(0, 100));
            ModeGene = Factory.Create(() => RandomizationProvider.Current.GetInt(0, _modes.Length));
            DynMultGene = Factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));
            FreezeGene = Factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));

            //Static gene example:
            //Trend = _factory.Create(0d);

            if (finalize)
            {
                FinalizeGenes();
            }
        }

        protected void FinalizeGenes()
        {
            Resize(Factory.Length);
            CreateGenes();
        }

        public SpreadChromosome(Strategy fixedStrategy) : this(fixedStrategy, true)
        { }

        #region Spread

        public GeneWrapper<double> Stdev { get; }
        public GeneWrapper<double> Sma { get; }
        public GeneWrapper<double> Mult { get; }
        public GeneWrapper<double> Raise { get; }
        public GeneWrapper<double> Fall { get; }
        public GeneWrapper<double> Cap { get; }

        private readonly string[] _modes = { "independent", "together", "alternate", "half_alternate" }; // "disabled"
        private GeneWrapper<int> ModeGene { get; }
        public string Mode => _modes[ModeGene.Value];

        private GeneWrapper<int> DynMultGene { get; }
        public bool DynMult => DynMultGene.Value == 1;

        private GeneWrapper<int> FreezeGene { get; }
        public bool Freeze => FreezeGene.Value == 1;

        #endregion

        public string ID { get; set; }

        public int Generation { get; set; }

        public string Metadata { get; set; }

        public Statistics Statistics { get; set; }
        public Statistics BacktestStats { get; set; }
        public Statistics ControlStats { get; set; }

        public FitnessComposition FitnessComposition { get; set; }

        public virtual Type CsvAggregatedMapType => typeof(AggregatedSpreadChromosomeCsvMap);

        public virtual Type CsvSingleMapType => typeof(SingleSpreadChromosomeCsvMap);

        public virtual Type CsvRecordType => typeof(SpreadChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new SpreadChromosome(_strategy);

        public virtual BacktestRequest ToBacktestRequest()
        {
            return ChromosomeExtensions.ToBacktestRequest(this, _strategy);
        }
    }
}
