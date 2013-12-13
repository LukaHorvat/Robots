using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	class CompilationException : Exception
	{
		public CompilationException(string msg, int lineNumber) : base(msg + " {Line: " + lineNumber + "}") { }
	}
}
