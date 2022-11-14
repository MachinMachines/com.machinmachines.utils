// Copyright 2022 MachinMachines
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

namespace MachinMachines
{
    namespace Quantile
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

        // A reference count map generic enough to handle various usages
        [Serializable]
        public abstract class CountMapGeneric<T> : QuantileMap<T, CountMapBucket>
        {
            [SerializeField]
            private int TotalItemsCount;

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

        // A ready-made reference count map with a very simple accessor
        [Serializable]
        public class CountMap : CountMapGeneric<string>
        {
            public override int kLowerBucketIndex { get { return 0; } }
            public override int kHigherBucketIndex { get { return 10; } }

            private Dictionary<string, int> RefCountToUsage = new Dictionary<string, int>();

            protected override void ResetInternal()
            {
                base.ResetInternal();
                RefCountToUsage.Clear();
            }
            protected override string GetNameForBucket(int bucketIdx)
            {
                if (bucketIdx == 0)
                {
                    return $"<={(int)Math.Pow(2.0, kLowerBucketIndex)}";
                }
                if (bucketIdx == kBucketsCount - 1)
                {
                    return $">{(int)Math.Pow(2.0, kHigherBucketIndex)}";
                }
                return $"{(int)Math.Pow(2.0, bucketIdx + kLowerBucketIndex)}";
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
                    if (previousBucketIdx != newBucketIdx)
                    {
                        // Transfer the content for this item into the next bucket
                        Buckets[previousBucketIdx].UniqueItems.Remove(actualPath);
                    }
                }
                RefCountToUsage[actualPath] += 1;
                return foundBucketIdx;
            }
        }
    }
}
