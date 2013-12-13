using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	class Bot
	{
		/// <summary>
		/// Contains the memory of the bot. 
		/// 0-2000 code
		/// 2000-3000 system
		/// 3000-4000 auto assigned aliases
		/// 4000-10000 universal memory
		/// 0-100 protected
		/// 100-10000 volatile
		/// </summary>
		public int[] Memory;

		public int X, Y;

		public Bot(BotCode code)
		{

			Memory = new int[10000];
			code.Code.CopyTo(Memory);			
		}
	}
}
