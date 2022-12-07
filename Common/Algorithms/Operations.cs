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

using System.Collections.Generic;

namespace MachinMachines.Algorithms
{
    // Named "Paths" so there is no collision with System.IO.Path
    public static class Operations
    {

        /// <summary> 
        /// Given two sets of data, "reconcile" them similarly to P4 reconcile:
        /// - identify items present in both sets ("similar")
        /// - identify items present in the set to be reconciled with only ("added")
        /// - identify items present in the existing set only ("removed")
        /// </summary>
        /// <param name="existingSet"></param>
        /// <param name="toReconcileWithSet"></param>
        /// <param name="similarSet">items present in both sets, returns the ones from "existingSet"</param>
        /// <param name="addedSet"></param>
        /// <param name="removedSet"></param>
        /// <param name="comparer">Optional custom comparer</param>
        public static void ReconcileItems<T>(IEnumerable<T> existingSet,
                                             IEnumerable<T> toReconcileWithSet,
                                             out HashSet<T> similarSet,
                                             out HashSet<T> addedSet,
                                             out HashSet<T> removedSet,
                                             IEqualityComparer<T> comparer = null)
        {
            IEqualityComparer<T> actualComparer = comparer ?? EqualityComparer<T>.Default;
            similarSet = new HashSet<T>(existingSet, actualComparer);
            addedSet = new HashSet<T>(toReconcileWithSet, actualComparer);
            removedSet = new HashSet<T>(existingSet, actualComparer);
            similarSet.IntersectWith(toReconcileWithSet);
            addedSet.ExceptWith(similarSet);
            removedSet.ExceptWith(similarSet);
        }
    }
}
