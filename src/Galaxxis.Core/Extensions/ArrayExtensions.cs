namespace Galaxxis.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static string ToValueString(this double[] array, char seperator = '|')
        {
            return string.Join(seperator, array);
        }
    }
}
