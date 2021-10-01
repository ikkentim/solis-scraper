using System;
using System.Runtime.Serialization;

namespace SolisScraper
{
	[Serializable]
	public class ResponseParseException : Exception
	{
		public ResponseParseException()
		{
		}

		public ResponseParseException(string message) : base(message)
		{
		}

		public ResponseParseException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ResponseParseException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}