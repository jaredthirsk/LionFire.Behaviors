#if NoDeps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    [Serializable]
    public class AlreadySetException : Exception
    {
        public AlreadySetException() { }
        public AlreadySetException(string message) : base(message) { }
        public AlreadySetException(string message, Exception inner) : base(message, inner) { }
        protected AlreadySetException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    [Serializable]
    public class UnreachableCodeException : Exception
    {
        public UnreachableCodeException() { }
        public UnreachableCodeException(string message) : base(message) { }
        public UnreachableCodeException(string message, Exception inner) : base(message, inner) { }
        protected UnreachableCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class DuplicateNotAllowedException : Exception
    {
        public DuplicateNotAllowedException() { }
        public DuplicateNotAllowedException(string message) : base(message) { }
        public DuplicateNotAllowedException(string message, Exception inner) : base(message, inner) { }
        protected DuplicateNotAllowedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
#endif