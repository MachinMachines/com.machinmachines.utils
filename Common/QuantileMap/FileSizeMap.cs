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

using MachinMachines.Utils;

using UnityEngine;

namespace MachinMachines
{
    namespace Quantile
    {
        [Serializable]
        public class FileSizeMapBucket : MapBucket
        {
            [IsMemorySizeUnit]
            public long Size;
            public List<string> Files = new List<string>();

            public override void Reset()
            {
                Size = 0;
                Files.Clear();
            }
        }

        // A file size map generic enough to handle various usages
        [Serializable]
        public abstract class FileSizeMap<T> : QuantileMap<T, FileSizeMapBucket>
        {
            public override int kLowerBucketIndex { get { return 3; } }
            public override int kHigherBucketIndex { get { return 12; } }

            [SerializeField]
            [IsMemorySizeUnit]
            private long TotalSizeBytes;
            [SerializeField]
            private int TotalItemsCount;

            protected override void AddItemInternal(int bucketIdx, T item)
            {
                string filepath = GetItemFilePath(item);
                long filesize = GetItemFileSize(item);
                Buckets[bucketIdx].Files.Add(filepath);
                Buckets[bucketIdx].Size += filesize;
                TotalSizeBytes += filesize;
                TotalItemsCount += 1;
            }

            protected override string GetNameForBucket(int bucketIdx)
            {
                // The first and last elements are go-to for sizes below or beyond the accepted dimensions
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

            protected override void ResetInternal()
            {
                TotalSizeBytes = 0;
                TotalItemsCount = 0;
            }

            protected abstract string GetItemFilePath(T item);
            protected abstract long GetItemFileSize(T item);
        }
    }
}
