using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.Utilities {
    /// <summary>
    /// A Generic helper class to count an object's events upto a limit
    /// </summary>
    public class OccurrenceCounter<T> {

        private int _limit;
        private ConcurrentDictionary<T, int> _occurrenceCounter;

        /// <summary>
        /// A Callback to raise when the configured limit is reached for the specific key (object)
        /// </summary>
        public Action<T> OnLimitReached;

        /// <summary>
        /// A Generic helper class to count an object's events upto a limit
        /// </summary>
        public OccurrenceCounter(int limit) {
            _limit = limit;
            _occurrenceCounter = new ConcurrentDictionary<T, int>();
        }

        /// <summary>
        /// Thread-safe count signal for the internal counter
        /// </summary>
        /// <param name="value"></param>
        public void Count(T value) {
            // get the current count value of specific key, or create new one if does not exist
            var currentCount = _occurrenceCounter.GetOrAdd(value, 0); 

            currentCount++;
            if (currentCount >= _limit) {
                OnLimitReached?.Invoke(value);
                return;
            }

            // set the count value of the key, based on highest available value (if updated from other thread)
            _occurrenceCounter.AddOrUpdate(value, currentCount, (v, oldCount) => oldCount > currentCount ? oldCount : currentCount);
        }

        /// <summary>
        /// Resets the internal counter
        /// </summary>
        public void Reset() {
            _occurrenceCounter.Clear();
        }

        /// <summary>
        /// Resets the internal counter for specific key
        /// </summary>
        public void Reset(T key) {
            _occurrenceCounter.TryRemove(key, out var value);
        }
    }
}
