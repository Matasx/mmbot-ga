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
            //Map(x => x.Stdev).Index(50);
            //Map(x => x.Sma).Index(51);
            //Map(x => x.Mult).Index(52);
            //Map(x => x.Raise).Index(53);
            //Map(x => x.Fall).Index(54);
            //Map(x => x.Cap).Index(55);
            //Map(x => x.Mode).Index(56);
            //Map(x => x.Freeze).Index(57);
            //Map(x => x.DynMult).Index(58);
            Map(x => x.Metadata).Index(1000);

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
