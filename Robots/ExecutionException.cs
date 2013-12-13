using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	class ExecutionException : Exception
	{
		public ExecutionException(string msg, int code) : base(msg + " {Code: " + code + "}") { }
		public ExecutionException(string msg, OpCode code) : base(msg + " {Code: " + code.ToString() + "}") { }
	}
}
