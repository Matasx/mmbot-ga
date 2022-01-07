using System;
using GeneticSharp.Domain.Chromosomes;

namespace MMBotGA.ga
{
    class GeneFactory
    {
        private readonly ChromosomeBase _parent;
        private readonly int _length;
        private int _current;
        private readonly IGeneWrapper[] _genes;

        public GeneFactory(ChromosomeBase parent, int length)
        {
            _parent = parent;
            _length = length;
            _genes = new IGeneWrapper[length];
        }

        public GeneWrapper<T> Create<T>(Func<T> generateFunc)
        {
            var index = _current;
            _current++;
            if (_current > _length) throw new IndexOutOfRangeException("Maximum number of genes was reached.");
            var gene = new GeneWrapper<T>(_parent, () => new Gene(generateFunc()), index);
            _genes[index] = gene;
            return gene;
        }

        public void Validate()
        {
            if (_current != _length) throw new NotSupportedException("Gene count mismatch.");
        }

        public Gene Generate(int index)
        {
            return _genes[index].Generate();
        }
    }
}