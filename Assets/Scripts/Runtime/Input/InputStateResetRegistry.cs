using System.Collections.Generic;
using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// Maintains input state resettable objects and clears their cached input whenever the effective input context changes.
    /// </summary>
    public sealed class InputStateResetRegistry : MonoBehaviour
    {
        readonly HashSet<IInputStateResettable> _resettables = new();

        /// <summary>
        /// Registers an input state object for future resets.
        /// </summary>
        public void Register(IInputStateResettable resettable)
        {
            if (resettable == null) return;

            _resettables.Add(resettable);
        }

        /// <summary>
        /// Removes an input state object from the registry.
        /// </summary>
        public void Unregister(IInputStateResettable resettable)
        {
            if (resettable == null) return;

            _resettables.Remove(resettable);
        }

        /// <summary>
        /// Clears cached input state from all registered input readers.
        /// </summary>
        public void ResetAll()
        {
            foreach (IInputStateResettable resettable in _resettables)
            {
                resettable.ResetInputState();
            }
        }

        void OnDisable()
        {
            _resettables.Clear();
        }
    }
}