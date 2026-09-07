namespace Game.Input
{
    /// <summary>
    /// Defines how an input context interacts with contexts beneath it.
    /// </summary>
    public enum InputContextMode
    {
        /// <summary>
        /// Adds this context's action maps to the maps inherited from
        /// contexts beneath it.
        /// </summary>
        Overlay,

        /// <summary>
        /// Prevents action maps from contexts beneath this context from
        /// remaining active.
        /// </summary>
        Replace
    }
}