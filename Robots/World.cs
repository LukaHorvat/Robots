using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	class World
	{
		private Stack<int> stack;
		private int[] parameters;
		private string[] screen;
		public List<Bot> Bots;

		public World()
		{
			Bots = new List<Bot>();
			stack = new Stack<int>();
			parameters = new int[7];
			screen = new string[10];
		}

		public void RunBot(Bot bot)
		{
			stack.Clear();
			var memory = bot.Memory;
			int numParameters = 0;
			for (int i = 2000; i < 3000; ++i) memory[i] = 0;
			for (int i = 0; i < memory.Length; i += 1 + 1 + numParameters)
			{
				bool terminate = false;
				OpCode opCode;
				try
				{
					opCode = (OpCode)memory[i];
				}
				catch
				{
					throw new ExecutionException("Unknown operation code", memory[i]);
				}
				numParameters = memory[i + 1] >> 7;
				LoadParameters(memory, i + 2, memory[i + 1], numParameters);
				try
				{
					switch (opCode)
					{
						case OpCode.End:
							terminate = true;
							break;
						case OpCode.Add:
							stack.Push(parameters[0] + parameters[1]);
							break;
						case OpCode.Sub:
							stack.Push(parameters[0] - parameters[1]);
							break;
						case OpCode.Get:
							stack.Push(memory[stack.Pop()]);
							break;
						case OpCode.Set:
							if (numParameters == 1)
							{
								memory[parameters[0]] = stack.Pop();
							}
							else if (numParameters == 2)
							{
								memory[parameters[0]] = parameters[1];
							}
							break;
						case OpCode.Push:
							stack.Push(parameters[0]);
							break;
						case OpCode.Jump:
							i = parameters[0] - (1 + 1 + numParameters);
							break;
						case OpCode.Debug:
							Debugger.Break();
							break;
						case OpCode.If:
							if (stack.Pop() == 0) i += parameters[0];
							break;
						case OpCode.Greater:
							stack.Push(parameters[0] > parameters[1] ? 1 : 0);
							break;
						case OpCode.Lesser:
							stack.Push(parameters[0] < parameters[1] ? 1 : 0);
							break;
						case OpCode.GreaterEqual:
							stack.Push(parameters[0] >= parameters[1] ? 1 : 0);
							break;
						case OpCode.LesserEqual:
							stack.Push(parameters[0] <= parameters[1] ? 1 : 0);
							break;
						case OpCode.Equal:
							stack.Push(parameters[0] == parameters[1] ? 1 : 0);
							break;
						default:
							throw new ExecutionException("OpCode not implemented", opCode);
					}
				}
				catch (InvalidOperationException)
				{
					throw new ExecutionException("Trying to pop an empty stack at " + i, opCode);
				}
				catch (IndexOutOfRangeException)
				{
					throw new ExecutionException("Trying to access out-of-memory location at " + i, opCode);
				}
				if (terminate) break;
			}

			if (bot.Memory[(int)MemoryAliases.Move] != 0)
			{
				switch (bot.Memory[(int)MemoryAliases.Move])
				{
					case (int)MemoryAliases.Up:
						bot.Y--;
						break;
					case (int)MemoryAliases.Right:
						bot.X++;
						break;
					case (int)MemoryAliases.Down:
						bot.Y++;
						break;
					case (int)MemoryAliases.Left:
						bot.X--;
						break;
				}
			}
		}

		public void DrawScreen()
		{
			for (int i = 0; i < screen.Length; ++i)
			{
				screen[i] = "";
				for (int j = 0; j < 20; ++j)
				{
					screen[i] += " ";
				}
			}
			foreach (var bot in Bots)
			{
				var line = screen[bot.Y];
				line = line.Substring(0, bot.X) + "#" + line.Substring(bot.X + 1);
				screen[bot.Y] = line;
			}

			for (int i = 0; i < screen.Length; ++i)
			{
				Console.WriteLine(screen[i]);
			}
		}

		public void Step()
		{
			foreach (var bot in Bots) RunBot(bot);
			DrawScreen();
		}

		public void LoadParameters(int[] memory, int index, int modifiers, int numParameters)
		{
			for (int i = 0; i < numParameters; ++i)
			{
				if (((modifiers >> (6 - i)) & 1) == 1)
				{
					parameters[i] = memory[index + i];
				}
				else
				{
					parameters[i] = memory[memory[index + i]];
				}
			}
		}
	}
}
