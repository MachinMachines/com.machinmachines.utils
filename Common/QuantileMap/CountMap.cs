// Copyright 2023 MachinMachines
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using MachinMachines.Utils;

namespace MachinMachines.Quantile
{
    [Serializable]
    public class CountMapBucket : MapBucket
    {
        internal HashSet<string> UniqueItems = new HashSet<string>();
        // The actual serialisable field
        [SerializeField]
        internal string[] Items;

        public override void Reset()
        {
            UniqueItems.Clear();
        }

        public override void OnPreSerialise()
        {
            Items = UniqueItems.ToArray();
        }
    }

    /// <summary>
    /// A reference count map generic enough to handle various usages
    /// </summary>
    [Serializable]
    public abstract class CountMapGeneric<T> : QuantileMap<T, CountMapBucket>
    {
        [SerializeField]
        private int TotalItemsCount;

        public CountMapGeneric(int lowerBucketIndex = 0, int higherBucketIndex = 10)
            : base(lowerBucketIndex, higherBucketIndex)
        {
        }

        protected override sealed void AddItemInternal(int bucketIdx, T item)
        {
            string filepath = GetItemFilePath(item);
            Buckets[bucketIdx].UniqueItems.Add(filepath);
            TotalItemsCount += 1;
        }
        protected override void ResetInternal()
        {
            TotalItemsCount = 0;
        }
        protected abstract string GetItemFilePath(T item);
    }

    /// <summary>
    /// A ready-made reference count map with a very simple accessor
    /// </summary>
    [Serializable]
    public class CountMap : CountMapGeneric<string>
    {
        public IEnumerable<string> Items
        {
            get
            {
                return RefCountToUsage.Keys.ToHashSet();
            }
        }
        private Dictionary<string, int> RefCountToUsage = new Dictionary<string, int>();

        public CountMap(int lowerBucketIndex = 0, int higherBucketIndex = 10)
            : base(lowerBucketIndex, higherBucketIndex)
        {
        }

        public int GetItemUsage(string item)
        {
            return RefCountToUsage[item];
        }

        protected override void ResetInternal()
        {
            base.ResetInternal();
            RefCountToUsage.Clear();
        }
        protected override string GetNameForBucket(int bucketIdx)
        {
            if (bucketIdx == 0)
            {
                return $"<= {(int)Math.Pow(2.0, kLowerBucketIndex)}";
            }
            if (bucketIdx == kBucketsCount - 1)
            {
                return $">= {(int)Math.Pow(2.0, kHigherBucketIndex + 1)}";
            }
            return $"From {(int)Math.Pow(2.0, bucketIdx + kLowerBucketIndex)} to {(int)Math.Pow(2.0, bucketIdx + kLowerBucketIndex + 1) - 1}";
        }

        protected override string GetItemFilePath(string item)
        {
            return Paths.NormalisePath(item).ToLower();
        }
        protected override int GetBucketIndexForObject(string filepath)
        {
            string actualPath = Paths.NormalisePath(filepath).ToLower();
            int foundBucketIdx;
            int refCounter = 0;
            if (!RefCountToUsage.TryGetValue(actualPath, out refCounter))
            {
                // Easy case: new item
                foundBucketIdx = 0;
                RefCountToUsage.Add(actualPath, 0);
            }
            else
            {
                int previousBucketIdx = (int)Math.Log(refCounter, 2.0);
                int newBucketIdx = (int)Math.Log(refCounter + 1, 2.0);
                foundBucketIdx = newBucketIdx;
                // Need to clamp here as well in case we double the max count
                previousBucketIdx = Math.Clamp(previousBucketIdx, kLowerBucketIndex, kHigherBucketIndex + 1);
                if (previousBucketIdx != newBucketIdx)
                {
                    // Transfer the content for this item into the next bucket
                    Buckets[previousBucketIdx].UniqueItems.Remove(actualPath);
                }
            }
            foundBucketIdx = Math.Clamp(foundBucketIdx, kLowerBucketIndex, kHigherBucketIndex + 1);
            RefCountToUsage[actualPath] += 1;
            return foundBucketIdx;
        }
    }
}
