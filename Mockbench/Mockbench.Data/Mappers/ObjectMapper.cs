namespace Mockbench.Data.Mappers
{
    public static class ObjectMapper
    {
        // in memory cache of all available mapping functions
        private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> Maps = new ();

        public static TOType Map<TOType>(object obj)
        {
            Type toType = typeof(TOType);
            Type fromType = obj.GetType();

            if (Maps.ContainsKey(fromType))
            {
                if (Maps[obj.GetType()].ContainsKey(toType))
                {
                    return (TOType)Maps[fromType][toType].Invoke(obj);
                }
            }

            throw new Exception("Type mapping not found");
        }

        public static bool DoesMapExist(Type toType, Type fromType)
        {
            if (!Maps.ContainsKey(toType))
            {
                return false;
            }

            return Maps[toType].ContainsKey(fromType);
        }

        public static bool RegisterMap<TFromType, TOType>(Func<TFromType, TOType> mappingFunction)
        {
            Type toType = typeof(TOType);
            Type fromType = typeof(TFromType);

            if (!Maps.ContainsKey(fromType))
            {
                Maps.Add(fromType, new Dictionary<Type, Func<object, object>>());
            }

            if (!Maps[fromType].ContainsKey(toType))
            {
                Maps[fromType].Add(toType, (x) => mappingFunction.Invoke((TFromType)x));

                return true;
            }
            else
            {
                return false;
            }
        }        
    }
}