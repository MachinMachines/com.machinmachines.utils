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

using UnityEngine;

namespace MachinMachines
{
    namespace Quantile
    {
        // One named bucket
        // This should be abstract but it makes reusing it in the full map below harder without generics
        [Serializable]
        public class MapBucket : ISerializationCallbackReceiver
        {
            public string Name;

            public virtual void Reset()
            {
                throw new NotImplementedException();
            }

            // Optional in child classes
            public virtual void OnPreSerialise() { }

            public void OnBeforeSerialize()
            {
                OnPreSerialise();
            }

            public void OnAfterDeserialize()
            {
            }
        }

        // A "quantile map", mapping the input type into buckets depending on its fields
        [Serializable]
        public abstract class QuantileMap<T, BucketType> where BucketType : MapBucket, new()
        {
            // To be overridden by inheriting classes
            public abstract int kLowerBucketIndex { get; }
            public abstract int kHigherBucketIndex { get; }

            // We also plan to have a "<=min" and a ">max" buckets
            protected int kBucketsCount { get { return kHigherBucketIndex - kLowerBucketIndex + 2; } }

            [SerializeField]
            protected BucketType[] Buckets;

            public QuantileMap()
            {
                Buckets = new BucketType[kBucketsCount];
                for (int idx = 0; idx < kBucketsCount; ++idx)
                {
                    Buckets[idx] = new BucketType();
                    Buckets[idx].Name = GetNameForBucket(idx);
                }
            }

            public void Reset()
            {
                foreach (MapBucket bucket in Buckets)
                {
                    bucket.Reset();
                }
                ResetInternal();
            }
            public void AddItem(T item)
            {
                int bucketIdx = GetBucketIndexForObject(item);
                AddItemInternal(bucketIdx, item);
            }

            public void AddItems(IEnumerable<T> items)
            {
                foreach (T item in items)
                {
                    AddItem(item);
                }
            }

            public string SerialiseToJson()
            {
                return JsonUtility.ToJson(this, true);
            }

            // To be overridden by inheriting classes
            protected abstract void AddItemInternal(int bucketIdx, T item);
            protected abstract string GetNameForBucket(int bucketIdx);
            protected abstract void ResetInternal();
            protected abstract int GetBucketIndexForObject(T item);
        }
    }
}
