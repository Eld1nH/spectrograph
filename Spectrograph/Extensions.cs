namespace Spectrograph
{
    public static class Extensions
    {
        public static T[] CheckBuffer<T>(this T[] inst, long size, bool exactSize = false)
        {
            if (inst == null || (!exactSize && inst.Length < size) || (exactSize && inst.Length != size))
            {
                return new T[size];
            }
            return inst;
        }
    }
}
