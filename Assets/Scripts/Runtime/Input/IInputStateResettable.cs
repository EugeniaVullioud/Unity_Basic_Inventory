using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// Represents an object whose cached input state can be cleared when input contexts change.
    /// </summary>
    public interface IInputStateResettable
    {
        /// <summary>
        /// Clears cached continuous and transient input state.
        /// </summary>
        void ResetInputState();
    }
}