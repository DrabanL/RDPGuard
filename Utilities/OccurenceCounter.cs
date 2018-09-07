using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.Utilities {
    public class OccurrenceCounter<T> {

        private int _limit;
        private ConcurrentDictionary<T, int> _occurrenceCounter;

        public Action<T> OnLimitReached;

        public OccurrenceCounter(int limit) {
            _limit = limit;
            _occurrenceCounter = new ConcurrentDictionary<T, int>();
        }

        public void Count(T value) {
            var currentCount = _occurrenceCounter.GetOrAdd(value, 0); 

            currentCount++;
            if (currentCount >= _limit) {
                OnLimitReached?.Invoke(value);
                return;
            }

            _occurrenceCounter.AddOrUpdate(value, currentCount, (v, oldCount) => oldCount > currentCount ? oldCount : currentCount);
        }

        public void Reset() {
            _occurrenceCounter.Clear();
        }

        public void Reset(T key) {
            _occurrenceCounter.TryRemove(key, out var value);
        }
    }
}
