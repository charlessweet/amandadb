using System;
using System.Runtime.Serialization;

namespace Amanda.IO
{
    [Serializable]
    internal class MaximumFileAccessAttemptsExceededException : Exception
    {
        public MaximumFileAccessAttemptsExceededException()
        {
        }

        public MaximumFileAccessAttemptsExceededException(string message) : base(message)
        {
        }

        public MaximumFileAccessAttemptsExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MaximumFileAccessAttemptsExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}