using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;

namespace MMBotGA.ga
{
    class StrategyChromosome : ChromosomeBase
    {
        private readonly GeneFactory _factory;

        public StrategyChromosome()
            : base(2)
        {
            _factory = new GeneFactory(this);

            // max is exclusive
            Exponent = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 20));
            Trend = _factory.Create(() => RandomizationProvider.Current.GetDouble(-100, 100));
            Rebalance = _factory.Create(() => RandomizationProvider.Current.GetInt(3, 5)); // 0-5
            FunctionGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 3));
            Stdev = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 240));
            Sma = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 240));
            Mult = _factory.Create(() => RandomizationProvider.Current.GetDouble(0.5, 2)); // 0.95 - 1.05
            Raise = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 1000));
            Fall = _factory.Create(() => RandomizationProvider.Current.GetDouble(0.1, 10));
            Cap = _factory.Create(() => RandomizationProvider.Current.GetDouble(0, 100));
            ModeGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 5));
            DynMultGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));
            FreezeGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));

            //Static gene example:
            //Trend = _factory.Create(0d);

            Resize(_factory.Length);
            CreateGenes();
        }

        #region Strategy

        public GeneWrapper<double> Exponent { get; }
    
        public GeneWrapper<double> Trend { get; }
        public GeneWrapper<int> Rebalance { get; }

        private readonly string[] _functions = { "halfhalf", "keepvalue", "gauss" }; //"exponencial", "invsqrtsinh"
        private GeneWrapper<int> FunctionGene { get; }
        public string Function => _functions[FunctionGene.Value];

        #endregion

        #region Spread

        public GeneWrapper<double> Stdev { get; }
        public GeneWrapper<double> Sma { get; }
        public GeneWrapper<double> Mult { get; }
        public GeneWrapper<double> Raise { get; }
        public GeneWrapper<double> Fall { get; }
        public GeneWrapper<double> Cap { get; }

        private readonly string[] _modes = { "disabled", "independent", "together", "alternate", "half_alternate" };
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

        public override Gene GenerateGene(int geneIndex) => _factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new StrategyChromosome();
    }
}
