using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;

namespace MMBotGA.ga
{
    class GeneFactory
    {
        private readonly ChromosomeBase _parent;
        private int _current;
        private readonly IList<IGeneWrapper>_genes;

        public int Length => _genes.Count;

        public GeneFactory(ChromosomeBase parent)
        {
            _parent = parent;
            _genes = new List<IGeneWrapper>();
        }

        public GeneWrapper<T> Create<T>(Func<T> generateFunc)
        {
            var index = _current;
            _current++;
            var gene = new GeneWrapper<T>(_parent, () => new Gene(generateFunc()), index);
            _genes.Add(gene);
            return gene;
        }

        public GeneWrapper<T> Create<T>(T staticValue)
        {
            return new StaticGeneWrapper<T>(staticValue);
        }

        public Gene Generate(int index)
        {
            return _genes[index].Generate();
        }
    }
}