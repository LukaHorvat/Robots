using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
	enum OpCode : int
	{
		/// <summary>
		/// End execution.
		/// </summary>
		End = 0,

		/// <summary>
		/// Add parameters and push the result.
		/// </summary>
		Add,

		/// <summary>
		/// Subtract the second parameter from the first and push the result.
		/// </summary>
		Sub,

		/// <summary>
		/// Set the memory at the address of the first parameter to the value second parameter.
		/// </summary>
		Set,

		/// <summary>
		/// Push the value at the memory location equal to the value at top of the stack.
		/// Pops the stack.
		/// </summary>
		Get,

		/// <summary>
		/// Pushes the parameter to the stack.
		/// </summary>
		Push,

		/// <summary>
		/// Jump to the memory location equal to the value at the memory location of the first parameter.
		/// </summary>
		Jump,

		/// <summary>
		/// If the number on the top of the stack is false (== 0), skip the number of instructions equal to the parameter
		/// </summary>
		If,

		/// <summary>
		/// Cannot be used in code
		/// </summary>
		Label,

		/// <summary>
		/// Pushes 1 if the parameters are equal, 0 otherwise
		/// </summary>
		Equal,

		/// <summary>
		/// Pushes 1 if first > second, 0 otherwise
		/// </summary>
		Greater,

		/// <summary>
		/// Pushes 1 if first < second, 0 otherwise
		/// </summary>
		Lesser,

		/// <summary>
		/// Pushes 1 if first >= second, 0 otherwise
		/// </summary>
		GreaterEqual,

		/// <summary>
		/// Pushes 1 if first <= second, 0 otherwise
		/// </summary>
		LesserEqual,

		Eq = Equal,
		Gt = Greater,
		Lt = Lesser,
		GE = GreaterEqual,
		LE = LesserEqual,

		/// <summary>
		/// Breaks the execution of the code when this command executes so it can be debugged.
		/// </summary>
		Debug
	}

	enum MemoryAliases : int
	{
		Up = 1,
		Right,
		Down,
		Left,
		Move = 2000
	}

	class CodeParser
	{
		public static BotCode ParseCode(string code)
		{
			var list = new List<int>();
			var tokens = new List<Tuple<string, int>>();
			var lines = code.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			int lineNumber = 0;
			lines.ForEach(line =>
			{
				lineNumber++;
				var split = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				split.ForEach(word => tokens.Add(new Tuple<string, int>(word, lineNumber)));
			});

			//Scan for illegal substrings
			foreach (var token in tokens)
			{
				if (new[] { "+" }.Any(str => token.Item1.IndexOf(str) != -1))
				{
					throw new CompilationException("Illegal substring", token.Item2);
				}
			}

			//Parse ALIAS
			var aliases = new Dictionary<string, string>();
			Enum.GetNames(typeof(OpCode)).ToList().ForEach(name =>
			{
				aliases[name.ToUpper()] = (int)(OpCode)Enum.Parse(typeof(OpCode), name) + "";
			});
			Enum.GetNames(typeof(MemoryAliases)).ToList().ForEach(name =>
			{
				aliases[name.ToUpper()] = (int)(MemoryAliases)Enum.Parse(typeof(MemoryAliases), name) + "";
			});
			for (int i = 0; i < tokens.Count; ++i)
			{
				if (tokens[i].Item1 == "ALIAS")
				{
					if (tokens.Count - i < 3) throw new CompilationException("ALIAS error", tokens[i].Item2);
					aliases[tokens[i + 1].Item1] = tokens[i + 2].Item1;
					i += 2;
				}
				else
				{
					tokens = tokens.Skip(i).ToList();
					break;
				}
			}

			//Parse braces
			for (int i = 0; i < tokens.Count; ++i)
			{
				if (tokens[i].Item1 == "{") i = ParseBraces(tokens, i);
			}

			//Add modifiers for instructions
			//Format ..000[3 bits for 0-7 arguments][7 bits for value switches for each argument]
			var instructionNames = Enum.GetNames(typeof(OpCode)).Select(str => str.ToUpper()).ToList();
			var instructionIndices = tokens
				.Select((pair, i) => new Tuple<string, int>(pair.Item1, i))
				.Where(pair => instructionNames.Contains(pair.Item1) || pair.Item1 == "{" || pair.Item1 == "}")
				.Select(pair => pair.Item2)
				.ToList();
			for (int i = instructionIndices.Count - 1; i >= 0; --i)
			{
				int index = instructionIndices[i];
				if (new string[] { "LABEL", "{", "}" }.Contains(tokens[index].Item1)) continue;

				int next = 0;
				if (i < instructionIndices.Count - 1) next = instructionIndices[i + 1];
				else next = tokens.Count;
				int numArguments = next - index - 1;
				int modifier = 0;

				modifier += numArguments << 7;
				for (int j = 0; j < numArguments; ++j)
				{
					var token = tokens[index + 1 + j];
					if (token.Item1[0] == '#')
					{
						tokens[index + 1 + j] = new Tuple<string, int>(token.Item1.Substring(1), token.Item2);
						modifier += 1 << (6 - j);
					}
				}
				//Insert modifier to the first 10 bits of the instruction
				tokens[index] = new Tuple<string, int>(tokens[index].Item1 + "+" + modifier, tokens[index].Item2);
			}

			//Parse labels
			for (int i = 0; i < tokens.Count; ++i)
			{
				if (tokens[i].Item1 == "LABEL")
				{
					if (tokens.Count - i < 2) throw new CompilationException("LABEL error", tokens[i].Item2);
					aliases[tokens[i + 1].Item1] = "" + i;
					tokens.RemoveAt(i);
					tokens.RemoveAt(i);
					i--;
				}
			}

			if (tokens.Last().Item1 != "END") tokens.Add(new Tuple<string, int>("END", tokens.Last().Item2 + 1));

			//Apply all aliases
			foreach (var pair in aliases)
			{
				tokens = tokens.Select(token =>
					Tuple.Create(token.Item1 == pair.Key ? pair.Value : token.Item1, token.Item2)).ToList();
			}

			//Check if there are any remaining non-processed strings and auto-alias them
			//Fill the list with numbers
			int autoAliasCounter = 0;
			foreach (var token in tokens)
			{
				int number;
				if (token.Item1.IndexOf("+") != -1)
				{
					var split = token.Item1.Split('+');
					int main = int.Parse(split[0]);
					int modifier = int.Parse(split[1]);
					number = main + (modifier << 22);
				}
				else if (!int.TryParse(token.Item1, out number))
				{
					if (!aliases.ContainsKey(token.Item1))
					{
						aliases[token.Item1] = "" + (3000 + autoAliasCounter++);
					}
					number = int.Parse(aliases[token.Item1]);
				}
				list.Add(number);
			}

			return new BotCode(list);
		}

		private static int ParseBraces(List<Tuple<string, int>> tokens, int index)
		{
			tokens[index] = new Tuple<string, int>("", tokens[index].Item2);
			for (int i = index; i < tokens.Count; ++i)
			{
				if (tokens[i].Item1 == "{")
				{
					i = ParseBraces(tokens, i);
				}
				else if (tokens[i].Item1 == "}")
				{
					tokens[index] = new Tuple<string, int>("#" + (i - index - 1), tokens[index].Item2);
					tokens.RemoveAt(i);
					return i - 1;
				}
			}
			throw new CompilationException("Brace mismatch", tokens.Last().Item2);
		}
	}
}
