using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

namespace MMBotGA
{
    class StrategyChromosome : ChromosomeBase
    {
        public StrategyChromosome()
            : base(10)
        {
            CreateGenes();
        }

        #region Strategy
        public double Exponent => this.GetGene<double>(0);
        public double Trend => this.GetGene<double>(1);
        public int Rebalance => this.GetGene<int>(2);

        #endregion

        #region Spread

        public double Stdev => this.GetGene<double>(3);
        public double Sma => this.GetGene<double>(4);
        public double Mult => this.GetGene<double>(5);
        public double Raise => this.GetGene<double>(6);
        public double Fall => this.GetGene<double>(7);
        public double Cap => this.GetGene<double>(8);

        readonly string[] modes = new[] { "disabled", "independent", "together", "alternate", "half_alternate" };
        public string Mode => modes[this.GetGene<int>(9)];

        #endregion

        public string Metadata { get; set; }

        public override Gene GenerateGene(int geneIndex)
        {
            return geneIndex switch
            { // max is exclusive
                0 => new Gene(RandomizationProvider.Current.GetDouble(1, 20)),
                1 => new Gene(RandomizationProvider.Current.GetDouble(-100, 100)),
                2 => new Gene(RandomizationProvider.Current.GetInt(0, 5)),
                3 => new Gene(RandomizationProvider.Current.GetDouble(1, 240)),
                4 => new Gene(RandomizationProvider.Current.GetDouble(1, 240)),
                5 => new Gene(RandomizationProvider.Current.GetDouble(0.5, 2)),
                6 => new Gene(RandomizationProvider.Current.GetDouble(1, 1000)),
                7 => new Gene(RandomizationProvider.Current.GetDouble(0.1, 10)),
                8 => new Gene(RandomizationProvider.Current.GetDouble(0, 100)),
                9 => new Gene(RandomizationProvider.Current.GetInt(0, 5)),
                _ => new Gene(),
            };
        }

        public override IChromosome CreateNew()
        {
            return new StrategyChromosome();
        }
    }
}
