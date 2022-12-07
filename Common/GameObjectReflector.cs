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
using System.Reflection;

using UnityEngine;
using UnityEngine.Profiling;

namespace MachinMachines.Utils
{
    public static class GameObjectReflector
    {
        /// <summary>
        /// Properties we are not allowed to copy
        /// </summary>
        private static readonly Dictionary<Type, HashSet<string>> s_ComponentPropertiesNamesBlackList = new Dictionary<Type, HashSet<string>>
        {
            { typeof(Transform), new HashSet<string>{"parent"} }
        };

        /// <summary>
        /// Given two sets of game objects:
        /// for every object of the first set apply properties of the matching object in the second set
        /// </summary>
        /// <returns>The count of changed items</returns>
        public static int Merge(IEnumerable<GameObject> baseItems,
                                IEnumerable<GameObject> changedItems, HashSet<Type> componentsTypesWhiteList
            //,HashSet<Type> propertiesTypesWhiteList
            )
        {
            Profiler.BeginSample("MachinMachines - GameObjectReflector - Merge");

            int result = 0;
            GameObjectPathComparer comparer = new(1);
            foreach (GameObject changedGO in changedItems)
            {
                GameObject matchingGO = baseItems.Where(item => comparer.Equals(item, changedGO)).FirstOrDefault();
                if (matchingGO != null)
                {
                    // Early out if game objects are actually identical
                    if (matchingGO == changedGO)
                    {
                        continue;
                    }
                    foreach (Component changedComponent in changedGO.GetComponents<Component>())
                    {
                        if (changedComponent != null
                            && componentsTypesWhiteList.Contains(changedComponent.GetType()))
                        {
                            Type componentType = changedComponent.GetType();
                            HashSet<string> propertiesNamesBlackList = new(0);
                            s_ComponentPropertiesNamesBlackList.TryGetValue(componentType, out propertiesNamesBlackList);

                            Component matchingComponent = matchingGO.GetComponent(componentType);
                            if (matchingComponent == null)
                            {
                                matchingComponent = matchingGO.AddComponent(componentType);
                            }
                            PropertyInfo[] propertyInfos = componentType.GetProperties()
                                                                        .Where(item => item.CanWrite
                                                                               && !propertiesNamesBlackList.Contains(item.Name))
                                                                        //&& propertiesTypesWhiteList.Contains(item.PropertyType))
                                                                        .ToArray();
                            foreach (PropertyInfo propertyInfo in propertyInfos)
                            {
                                object changedValue = propertyInfo.GetValue(changedComponent);
                                propertyInfo.SetValue(matchingComponent, changedValue);
                            }
                            result += 1;
                        }
                    }
                }
            }

            Profiler.EndSample();
            return result;
        }
    }
}
