using GeneticSharp.Domain.Chromosomes;

namespace MMBotGA
{
    internal static class ChromosomeBaseExtensions
    {
        public static T GetGene<T>(this ChromosomeBase chromosome, int index)
        {
            return (T)chromosome.GetGene(index).Value;
        }
    }
}
