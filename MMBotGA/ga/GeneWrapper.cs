using System;
using GeneticSharp.Domain.Chromosomes;

namespace MMBotGA.ga
{
    class GeneWrapper<T> : IGeneWrapper
    {
        private readonly ChromosomeBase _parent;
        private readonly Func<Gene> _generateFunc;
        private readonly int _index;

        public GeneWrapper(ChromosomeBase parent, Func<Gene> generateFunc, int index)
        {
            _parent = parent;
            _generateFunc = generateFunc;
            _index = index;
        }

        public virtual T Value => _parent.GetGene<T>(_index);

        public Gene Generate()
        {
            return _generateFunc();
        }

        public static implicit operator T(GeneWrapper<T> gene) => gene.Value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}