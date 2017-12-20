using System;

namespace Aleab.LoopbackAudioVisualizer
{
    public class SetOncePropertyAlreadySetException : ApplicationException
    {
        public SetOncePropertyAlreadySetException(string propertyName) : base(CreateDefaultMessage(propertyName))
        {
        }

        public SetOncePropertyAlreadySetException(string properyName, string message) : base($"{CreateDefaultMessage(properyName)}\n{message}")
        {
        }

        protected static string CreateDefaultMessage(string propertyName)
        {
            return $"The property \"{propertyName}\" can only be set once and it was already previously set.";
        }
    }
}