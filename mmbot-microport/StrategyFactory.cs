using mmbot_microport.Strategy;

internal class StrategyFactory
{
    public static IStrategy Create(string id, string config)
    {
        IStrategy strategy;
        if (id == KeepValueStrategy.ID)
        {
            var cfg = KeepValueStrategy.Config.FromJson(config);
            strategy = new KeepValueStrategy(cfg);
        }

        else if (id == GammaStrategy.ID)
        {
            var cfg = GammaStrategy.Config.FromJson(config);
            strategy = new GammaStrategy(cfg, new GammaStrategy.State());
        }
        else
        {
            throw new NotSupportedException($"Unknown strategy {id}");
        }

        return new StrategyWrapper(strategy);
    }
}