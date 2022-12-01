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

namespace MachinMachines.Utils
{
    /// <summary>
    /// Helper class to serialise tuples-like structures with Unity
    /// </summary>
    [Serializable]
    public class SerialisableTuple<U, V>
    {
        public U Item1;
        public V Item2;

        public SerialisableTuple(U item1, V item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
