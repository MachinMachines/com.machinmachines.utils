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

namespace MachinMachines
{
    namespace Utils
    {
        public static class MemorySizeUnit
        {
            public enum Unit
            {
                Byte,
                Kilobyte,
                Megabyte,
                Gigabyte
            }

            public static Dictionary<Unit, string> s_UnitToSuffix = new Dictionary<Unit, string> {
                {Unit.Byte, "B" },
                {Unit.Kilobyte, "KB" },
                {Unit.Megabyte, "MB" },
                {Unit.Gigabyte, "GB" }
            };

            private const float BYTE_CAPACITY = 1024.0f;
            private const float KILOBYTE_LIMIT = BYTE_CAPACITY * BYTE_CAPACITY;
            private const float MEGABYTE_LIMIT = BYTE_CAPACITY * BYTE_CAPACITY * BYTE_CAPACITY;

            public static string DisplayByteSize(long bytes, int nbDecimals = 0)
            {
                string precision = "F" + nbDecimals;
                // Default on bytes
                float value = bytes;
                Unit unit = Unit.Byte;
                if (bytes > BYTE_CAPACITY)
                {
                    if (bytes < KILOBYTE_LIMIT)
                    {
                        value = ConvertInKb(bytes);
                        unit = Unit.Kilobyte;
                    }
                    else if (bytes < MEGABYTE_LIMIT)
                    {
                        value = ConvertInMb(bytes);
                        unit = Unit.Megabyte;
                    }
                    else
                    {
                        value = ConvertInGb(bytes);
                        unit = Unit.Gigabyte;
                    }
                }
                return value.ToString(precision) + s_UnitToSuffix[unit];
            }

            private static float ConvertInKb(float bytes)
            {
                return bytes / BYTE_CAPACITY;
            }

            private static float ConvertInMb(float bytes)
            {
                return ConvertInKb(bytes) / BYTE_CAPACITY;
            }

            private static float ConvertInGb(float bytes)
            {
                return ConvertInMb(bytes) / BYTE_CAPACITY;
            }
        }
    }
}
