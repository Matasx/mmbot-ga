using System;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class SpreadChromosomeCsvMapBase<T> : ClassMap<T> where T : SpreadChromosome
    {
        public SpreadChromosomeCsvMapBase(bool aggregated)
        {
            Map(x => x.ID).Index(0);
            Map(x => x.Generation).Index(1);
            Map(x => x.Fitness).Index(2);
            Map(x => x.Stdev).Index(7);
            Map(x => x.Sma).Index(8);
            Map(x => x.Mult).Index(9);
            Map(x => x.Raise).Index(10);
            Map(x => x.Fall).Index(11);
            Map(x => x.Cap).Index(12);
            Map(x => x.Mode).Index(13);
            Map(x => x.Freeze).Index(14);
            Map(x => x.DynMult).Index(15);
            Map(x => x.Metadata).Index(100);

            if (aggregated)
            {
                References<StatisticsMap>(x => x.BacktestStats).Prefix("BT_");
                References<StatisticsMap>(x => x.ControlStats).Prefix("CT_");
            }
            else
            {
                References<StatisticsMap>(x => x.Statistics).Prefix("Stats_");
            }

            References<FitnessCompositionMap>(x => x.FitnessComposition).Prefix("Fitness_");
        }

        protected MemberMap<T, GeneWrapper<TMember>> Map<TMember>(Expression<Func<T, GeneWrapper<TMember>>> expression, bool useExistingMap = true)
        {
            var result = base.Map(expression, useExistingMap);
            var type = typeof(TMember);

            if (type == typeof(double)) return result.TypeConverter<DoubleGeneConverter>();
            if (type == typeof(int)) return result.TypeConverter<IntGeneConverter>();

            throw new NotSupportedException($"Type of gene {type} is not supported for writing to csv.");
        }

        private class DoubleGeneConverter : DoubleConverter
        {
            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return base.ConvertToString((double)(GeneWrapper<double>)value, row, memberMapData);
            }
        }

        private class IntGeneConverter : DoubleConverter
        {
            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return base.ConvertToString((int)(GeneWrapper<int>)value, row, memberMapData);
            }
        }
    }
}
