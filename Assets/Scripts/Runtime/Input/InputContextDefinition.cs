using System;
using UnityEngine;

namespace Game.Input
{
    /// <summary>
    /// Defines an application input context and the Input System action maps
    /// that should be active while the context is effective.
    /// </summary>
    [CreateAssetMenu(fileName = "InputContext", menuName = "Game/Input/Input Context")]
    public sealed class InputContextDefinition : ScriptableObject
    {
        [Header("Context")]
        [SerializeField] InputContextMode _mode = InputContextMode.Replace;

        [Header("Action Maps")]
        [SerializeField] string[] _actionMaps = Array.Empty<string>();

        [Header("Cursor")]
        [SerializeField] bool _requiresCursor;

        /// <summary>
        /// Gets the mode used when combining this context with contexts beneath it.
        /// </summary>
        public InputContextMode Mode => _mode;

        /// <summary>
        /// Gets whether this context requires the system cursor.
        /// </summary>
        public bool RequiresCursor => _requiresCursor;

        /// <summary>
        /// Gets the configured action map names.
        /// </summary>
        public ReadOnlySpan<string> ActionMaps => _actionMaps;
    }
}