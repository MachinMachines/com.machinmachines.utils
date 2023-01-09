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
using System.Linq;

namespace MachinMachines.Utils
{
    public static class PerfHelpers
    {
        /// <summary>
        /// A simple utility to build a lookup table for enum string values
        /// This is required due to Enum.Parse being slow as hell
        /// </summary>
        public static Dictionary<string, T> BuildLookupTable<T>(bool toLower = false) where T : System.Enum
        {
            return System.Enum.GetNames(typeof(T))
                .ToDictionary(
                    item => toLower ? item.ToLower() : item,
                    item => (T)System.Enum.Parse(typeof(T), item)
                );
        }
    }
}
