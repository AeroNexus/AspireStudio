using System.Collections.Generic;

using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	public class QueryParser
	{
		object[] mMatch, mParseMatch= new object[128];
		Dictionary<string, StructuredQuery.sq> opByName = new Dictionary<string, StructuredQuery.sq>();
		StructuredQuery.sq[] location = new StructuredQuery.sq[16];
		string[] tokenNameByOp;
		string mSpec;
		int mOpIndex, mParseErrors, mPushes;

		StructuredQuery.sq Emit(StructuredQuery.sq op)
		{
			mMatch[mOpIndex++] = op;
		    //Log.WriteLine("[{0}:{1}]",OpName(op),op);
		   return op;
		}

    public int Errors { get { return mParseErrors; } }

		string OpName(StructuredQuery.sq op)
		{
		    if ( tokenNameByOp == null )
		    {
				int maxOp = 0;
				for( int i=0; i<StructuredQuery.Tokens.Length; i++)
					if ((int)StructuredQuery.Tokens[i].op > maxOp)
						maxOp = (int)StructuredQuery.Tokens[i].op;
				tokenNameByOp = new string[maxOp+1];
				for( int i=0; i<StructuredQuery.Tokens.Length; i++)
					tokenNameByOp[(int)StructuredQuery.Tokens[i].op] = StructuredQuery.Tokens[i].name;
		    }

		    if ( (int)op < tokenNameByOp.Length && tokenNameByOp[(int)op] != null )
		        return tokenNameByOp[(int)op];
		    return string.Empty;
		}

		public bool Parse(string spec, QueryMgr query, out int levelBeyondMessage)
		{
			mPushes = 0;
			mParseMatch[0] = StructuredQuery.sq.Interface;

			int n = ParseOps(spec, mParseMatch, 1, mParseMatch.Length-1, out levelBeyondMessage);
			mParseMatch[n] = StructuredQuery.sq.End;

			//print the match table
			//StructuredResponse.PrintOpStr(mParseMatch,n);
			//for( int i=0; imOpPtr-match; dp++)
			//	if ( mMatch[i].GetType() == typeof(string) )
			//		MsgConsole.WriteLine("[{0}] \"{1}\"",i,mMatch[i] as string);
			//	else
			//		MsgConsole.WriteLine("[{0}] ({1}){2}",i,OpName(((StructuredQuery.sq)mMatch[i])),match[i]);

			if (mParseMatch[1].GetType() == typeof(StructuredQuery.sq) &&
				((StructuredQuery.sq)mParseMatch[1]) == StructuredQuery.sq.Interface)
				query.SetMatch(mParseMatch, 1, n);
			else
				query.SetMatch(mParseMatch, 0, n+1);

			return true;
		}

		public int ParseOps(string spec, object[] ops, int offset, int oplength, out int levelBeyondMessage)
		{
		  mParseErrors = 0;
			// Just check for the terminator
			if (spec[spec.Length - 1] == '.')
				mSpec = spec;
			else
				mSpec = spec + '.';
			mSpec += '\0';

			int ic = 0, parenLevel = 0, msgLevel = 0, maxLevel = 0, startString = -1, startToken = -1;
			bool inFunction = false;
			StructuredQuery.sq op = StructuredQuery.sq.End;
			mOpIndex = offset;
			mMatch = ops;

			do
			{
				switch (mSpec[ic])
				{
					case '\n':
					case ' ':
					case '\t':
					case ',':
						if (startString >= 0)
						{
							startString = Stringize(startString, ic);
							startToken = -1;
						}
						else if (startToken >= 0)
						{
							op = Tokenize(startToken, ic);
							if (op == StructuredQuery.sq.Up) maxLevel--;
							location[parenLevel] = op;
							if (op == StructuredQuery.sq.Qualifier || inFunction)
								startString = ic + 1;
							else
								startToken = -1;
						}
						break;
					case '(':
						op = Tokenize(startToken, ic);
						switch (op)
						{
							case StructuredQuery.sq.DataMsg:
							case StructuredQuery.sq.CommandMsg:
							case StructuredQuery.sq.DataReplyMsg:
							case StructuredQuery.sq.Message:
							case StructuredQuery.sq.FaultMsg:
								msgLevel = parenLevel;
								break;
							default:
                break;
						}
						maxLevel = parenLevel;
						location[parenLevel++] = op;
						if (op == StructuredQuery.sq.Qualifier || inFunction)
							startString = ic + 1;
						else
							startToken = -1;
						break;
					case ')':
						if (startString >= 0)
						{
							startString = Stringize(startString, ic);
							startToken = -1;
						}
						inFunction = false;
						parenLevel--;
						break;
					case '=':
					case '<':
					case '>':
						if (startString >= 0)
						{
							startString = Stringize(startString, ic);
							startString = ic + 1;
						}
						else
						{
							op = Tokenize(startToken, ic);
						}
						if (parenLevel > 0 && location[parenLevel - 1] == StructuredQuery.sq.Qualifier)
							Emit(StructuredQuery.sq.Value);
						if (mSpec[ic] != '=')
						{
							if (mSpec[ic + 1] == '=')
							{
								Tokenize(ic, ic + 2);
								ic++;
							}
							else
								Tokenize(ic, ic + 1);
						}
						startToken = -1;
						startString = ic + 1;
						break;
					case '.':
						if (startString == -1 && startToken >= 0)
						{
							op = Tokenize(startToken, ic);
							if (op == StructuredQuery.sq.Up) maxLevel--;
							inFunction = true;
							startToken = ic + 1;
						}
						break;
					case (char)StructuredQuery.sq.LastOp:
						levelBeyondMessage = maxLevel - msgLevel;
						return 0;
					default:
						if (startToken == -1)
							startToken = ic;
						break;
				}
			} while (mSpec[++ic] != '\0');

			levelBeyondMessage = maxLevel - msgLevel;
			if (parenLevel != 0)
			{
				MsgConsole.WriteLine("Must fix unbalanced parentheses in, {0}", spec);
				return 0;
			}
			return mOpIndex;
		}

		public int Pushes { get { return mPushes; } }

		int Stringize(int start, int stop)
		{
			string str = mSpec.Substring(start, stop-start);
			mMatch[mOpIndex++] = str;
			//MsgConsole.WriteLine("\"{0}\"",str);
		    return -1;
		}

		StructuredQuery.sq Tokenize(int start, int stop)
		{
		    StructuredQuery.sq op = StructuredQuery.sq.End;
			string str = mSpec.Substring(start,stop-start);

			if (opByName.Count == 0)
			{
				lock(opByName)
					foreach (var token in StructuredQuery.Tokens)
						opByName.Add(token.name, token.op);
			}
			
			if (!opByName.TryGetValue(str, out op))
		    {
		      MsgConsole.WriteLine("Unrecognized '{0}'; verify it as a proper xTEDS tag",str);
				  op = StructuredQuery.sq.LastOp;
		      mParseErrors++;
		    }
			else if (op == StructuredQuery.sq.Push) mPushes++;
			else if (op == StructuredQuery.sq.Pop) mPushes--;

			mMatch[mOpIndex++] = op;
			//MsgConsole.WriteLine("[{0}:{1}]",str,op);
		    return op;
		}

	}
}
