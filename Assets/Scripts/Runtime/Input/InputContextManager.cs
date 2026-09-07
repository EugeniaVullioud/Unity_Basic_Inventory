using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    /// <summary>
    /// Owns the active input context stack and is the sole authority
    /// responsible for enabling and disabling Input System action maps.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class InputContextManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] PlayerInput _playerInput;
        [SerializeField] InputContextDefinition _initialContext;

        [Header("Dependencies")]
        [SerializeField] InputStateResetRegistry _resetRegistry;

        readonly List<InputContextDefinition> _contextStack = new();
        readonly HashSet<InputActionMap> _desiredMaps = new();
        readonly HashSet<InputActionMap> _activeMaps = new();

        InputActionAsset _actions;

        /// <summary>
        /// Raised whenever the effective input context changes.
        /// </summary>
        public event Action<InputContextDefinition> ContextChanged;

        /// <summary>
        /// Gets the context currently at the top of the stack.
        /// </summary>
        public InputContextDefinition CurrentContext
        {
            get
            {
                return _contextStack.Count == 0 ? null : _contextStack[^1];
            }
        }

        /// <summary>
        /// Gets the number of contexts currently on the stack.
        /// </summary>
        public int ContextCount => _contextStack.Count;

        void Awake()
        {
            if (_playerInput == null)
            {
                Debug.LogError($"{nameof(InputContextManager)} requires a " + $"{nameof(PlayerInput)} reference.", this);

                enabled = false;
                return;
            }

            _actions = _playerInput.actions;

            DisableAllMaps();

            if (_initialContext != null)
            {
                Push(_initialContext);
            }
        }

        /// <summary>
        /// Pushes an input context onto the active context stack.
        /// </summary>
        public void Push(InputContextDefinition context)
        {
            if (context == null)
            {
                Debug.LogError("Cannot push a null input context.", this);

                return;
            }

            _contextStack.Add(context);

            RefreshContexts();
        }

        /// <summary>
        /// Removes the specified context only if it is currently at the top
        /// of the context stack.
        /// </summary>
        public bool Pop(InputContextDefinition expectedContext)
        {
            if (_contextStack.Count == 0) return false;

            int lastIndex = _contextStack.Count - 1;

            if (_contextStack[lastIndex] != expectedContext)
            {
                Debug.LogWarning($"Attempted to pop input context " + $"'{expectedContext?.name}', but the current context is " + $"'{_contextStack[lastIndex]?.name}'.", this);

                return false;
            }

            _contextStack.RemoveAt(lastIndex);

            RefreshContexts();

            return true;
        }

        /// <summary>
        /// Removes the context currently at the top of the stack.
        /// </summary>
        public bool Pop()
        {
            if (_contextStack.Count == 0) return false;

            _contextStack.RemoveAt(_contextStack.Count - 1);
            RefreshContexts();

            return true;
        }

        /// <summary>
        /// Replaces the entire context stack with the specified context.
        /// </summary>
        public void Replace(InputContextDefinition context)
        {
            if (context == null)
            {
                Debug.LogError("Cannot replace input contexts with a null context.", this);

                return;
            }

            _contextStack.Clear();
            _contextStack.Add(context);

            RefreshContexts();
        }

        /// <summary>
        /// Determines which action maps should be active and applies the resulting configuration to the Input System.
        /// </summary>
        void RefreshContexts()
        {
            _desiredMaps.Clear();

            CollectDesiredMaps();

            DisableInactiveMaps();
            EnableDesiredMaps();

            _activeMaps.Clear();

            foreach (InputActionMap map in _desiredMaps)
            {
                _activeMaps.Add(map);
            }

            _resetRegistry?.ResetAll();

            ContextChanged?.Invoke(CurrentContext);
        }

        /// <summary>
        /// Collects action maps from the effective portion of the context
        /// stack.
        /// </summary>
        void CollectDesiredMaps()
        {
            if (_contextStack.Count == 0) return;

            int startIndex = FindEffectiveContextStart();

            for (int i = startIndex; i < _contextStack.Count; i++)
            {
                AddContextMaps(_contextStack[i]);
            }
        }

        /// <summary>
        /// Finds the lowest stack index that should contribute action maps to
        /// the effective context configuration.
        /// </summary>
        int FindEffectiveContextStart()
        {
            for (int i = _contextStack.Count - 1; i >= 0; i--)
            {
                if (_contextStack[i].Mode == InputContextMode.Replace) return i;
            }

            return 0;
        }

        /// <summary>
        /// Adds all action maps configured by an input context.
        /// </summary>
        void AddContextMaps(InputContextDefinition context)
        {
            if (context == null) return;

            foreach (string mapName in context.ActionMaps)
            {
                InputActionMap map = _actions.FindActionMap(mapName, throwIfNotFound: false);

                if (map == null)
                {
                    Debug.LogError($"Input context '{context.name}' references " + $"missing action map '{mapName}'.", context);
                    continue;
                }

                _desiredMaps.Add(map);
            }
        }

        /// <summary>
        /// Disables action maps that are no longer required.
        /// </summary>
        void DisableInactiveMaps()
        {
            foreach (InputActionMap map in _activeMaps)
            {
                if (!_desiredMaps.Contains(map)) map.Disable();
            }
        }

        /// <summary>
        /// Enables action maps required by the current context stack.
        /// </summary>
        void EnableDesiredMaps()
        {
            foreach (InputActionMap map in _desiredMaps)
            {
                if (!_activeMaps.Contains(map)) map.Enable();
            }
        }

        /// <summary>
        /// Disables every action map in the configured Input Action Asset.
        /// </summary>
        void DisableAllMaps()
        {
            if (_actions == null) return;

            foreach (InputActionMap map in _actions.actionMaps) map.Disable();

            _activeMaps.Clear();
        }

        void OnDestroy()
        {
            DisableAllMaps();
        }
    }
}