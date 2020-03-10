using System.Threading;

namespace Academia
{
    public class Stats
    {
        private long _produced;
        private long _consumed;

        private long Produced => Interlocked.Read(ref _produced);
        private long Consumed => Interlocked.Read(ref _consumed);

        public void Produce()
        {
            Interlocked.Increment(ref _produced);
        }

        public void Consume()
        {
            Interlocked.Increment(ref _consumed);
        }

        public override string ToString()
        {
            return $"{nameof(Produced)}: {Produced}, {nameof(Consumed)}: {Consumed}";
        }
    }
}