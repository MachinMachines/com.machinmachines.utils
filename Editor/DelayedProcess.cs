using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace MachinMachines
{
    namespace Editor
    {
        // A helper to stagger items processing
        // One item gets processed at every next editor update until the queue runs out
        // Threadsafe!
        static public class DelayedProcess<T> where T : struct
        {
            // The method to be called on the next update
            static public Action<T> onNextUpdate;

            // List of imported assets to be processed once import finished
            static private Queue<T> PendingAssets = new Queue<T>();

            // Push a new item to be post processed later on
            static public void Push(T item)
            {
                if (onNextUpdate.Method == null)
                {
                    // if you end up here, you forgot to set the callback method!
                    Debug.LogError($"Forgot to set post-import method for ImportPayload<{typeof(T).Name}>");
                }
                // Setup all post-import steps
                lock (PendingAssets)
                {
                    PendingAssets.Enqueue(item); ;
                    EditorApplication.update += NextEditorUpdate;
                }
            }

            static private void NextEditorUpdate()
            {
                onNextUpdate?.Invoke(Pop());
            }

            static private T Pop()
            {
                T result = new T();
                lock (PendingAssets)
                {
                    if (PendingAssets.Count > 0)
                    {
                        result = PendingAssets.Dequeue();
                    }
                    if (PendingAssets.Count == 0)
                    {
                        // Nothing else to process, this post-process step can be removed
                        EditorApplication.update -= NextEditorUpdate;
                    }
                }
                return result;
            }
        }
    }
}
