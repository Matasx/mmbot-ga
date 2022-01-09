namespace MMBotGA.ga
{
    class StaticGeneWrapper<T> : GeneWrapper<T>
    {
        public StaticGeneWrapper(T value) : base(null, null, -1)
        {
            Value = value;
        }

        public override T Value { get; }
    }
}