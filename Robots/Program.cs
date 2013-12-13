using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	class Program
	{
		static string testCode = @"
ADD a #1
SET #a
GREATER a #4
IF 
{
	SET #a #1
}
SET #MOVE a
";

		static void Main(string[] args)
		{
			var world = new World();
			var bot = new Bot(CodeParser.ParseCode(testCode))
			{
				X = 5,
				Y = 5
			};
			world.Bots.Add(bot);

			while (true)
			{
				Console.Clear();
				world.Step();
				Console.ReadKey();
			}
		}
	}
}
