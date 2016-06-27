using System;
using System.Text;

namespace Aspire.Utilities {
	/// <summary>
	/// TokenParser extracts tokens from strings and file streams.
	/// User specifies the characters which may act as delimiters.
	/// Multiple successive delimiters are treated as one.
	/// </summary>
	public class TokenParser {
		int cursor;
		int length;
		bool useWhiteSpace;
		string text = string.Empty;
		static char[] emptyChar = new char[0];
		char[] chars = emptyChar;
		char[] delims = emptyChar;

		/// <summary>
		/// Default constructor
		/// <P>Whitespace delimited unless changed later by setting delimiters.</P>
		/// </summary>
		public TokenParser(){
			useWhiteSpace = true;
		}
		
		/// <summary>
		/// Constructor with delimiter list
		/// </summary>
		/// <param name="delims"></param>
		public TokenParser(params char[] delims ) {
			SetDelims(delims);
		}
		
		/// <summary>
		/// Constructor with text and delimiters.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="delims"></param>
		public TokenParser(string text, string delims ) {
			SetText(text);
			SetDelims(delims);
		}

		/// <summary>
		/// Returns the last substring delimited by '.'
		/// <P>i.e. returns the portion of the input string following the last period.</P>
		/// <P>If the input string does not contain a period, the entire string is returned.</P>
		/// </summary>
		/// <param name="str">Dot delimited string to take the substring from</param>
		/// <returns></returns>
		public static string BaseName( string str ) {
			int ndx = str.LastIndexOf(".");
			if ( ndx == -1 ) return str;
			return str.Substring(ndx+1);
		}

		/// <summary>
		/// Access the current cursor into the Text.
		/// <P>i.e. the character location used to begin looking for the next token.</P>
		/// </summary>
		public int Cursor {
			get { return cursor; }
			set { cursor = value; }
		}

		/// <summary>
		/// Access the logical length of the input text.
		/// </summary>
		public int Length {
			get { return length; }
			set { length = value; }
		}

		/// <summary>
		/// Property: the current list of delimiters
		/// </summary>
		public char[] Delims {
			get{ return delims; }
			set{ SetDelims(value); }
		}

		/// <summary>
		/// Set the current list of delimiters using variable length char parameters
		/// </summary>
		public void SetDelims( params char[] value ) {
			delims = (value == null ? emptyChar : value);
			// if no delimiters are specified, then use white space as delimiter
			useWhiteSpace = (delims.Length <= 0);
		}

		/// <summary>
		/// Set the current list of delimiters using a string
		/// </summary>
		/// <param name="value"></param>
		public void SetDelims( string value ) {
			delims = (value == null ? emptyChar : value.ToCharArray() );
			// if no delimiters are specified, then use white space as delimiter
			useWhiteSpace = (delims.Length <= 0);
		}

		/// <summary>
		/// Parse a string using white space
		/// </summary>
		/// <param name="text"></param>
		public void Parse( string text ) {
			SetText(text);
			SetDelims(emptyChar);
		}

		/// <summary>
		/// Property: Text
		/// </summary>
		public string Text {
			set{ SetText(value); }
		}

		/// <summary>
		/// Set the parsed text (as a string) and limit the length
		/// </summary>
		/// <param name="txt">The text to parse.</param>
		/// <param name="len">A shorter length than txt.Length</param>
		public void SetText( string txt, int len ) {
			SetText(txt);
			if ( len < length )
				length = len;
		}

		/// <summary>
		/// Set the parsed text (as a character array) and limit the length
		/// </summary>
		/// <param name="chars">The text to parse.</param>
		/// <param name="len">A shorter length than txt.Length</param>
		public void SetText( char[] chars, int len ) {
			SetText(new string(chars));
			if ( len < length )
				length = len;
		}

		/// <summary>
		/// Parse a string using the specified delimiters.
		/// <P>Will not default to using whitespace as the delimiter (even if delims is empty).</P>
		/// </summary>
		/// <param name="text"></param>
		/// <param name="delims"></param>
		public void Parse( string text, params char[] delims ) {
			SetText(text);
			SetDelims(delims);
			useWhiteSpace = false;
		}

		void SetText( string text ) {
			this.text = text;
			length = text.Length;
			chars = text.ToCharArray();
			cursor = 0;
		}

		static object zero = new Object();
		/// <summary>
		/// Generic type token.
		/// If 'value' is a primitive type, converts the next token string into a
		/// variable of that primitive type and returns it as an object.
		/// If it is not a primitive type, returns the integer zero as an object.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public object Token(object value) {
			string token = Token();
			if ( (token.Length > 0) && value.GetType().IsPrimitive )
				return Convert.ChangeType(token,value.GetType());
			return (object)0;
		}

		// NOT READY FOR PRIME TIME

		//		/// <summary>
		//		/// Scans the text using the format to set the values of the objects
		//		/// </summary>
		//		/// <param name="text"></param>
		//		/// <param name="format"></param>
		//		/// <param name="objects"></param>
		//		/// <returns></returns>
		//		public int Scan( string text, string format, params object[] objects )
		//		{
		//			Text = text;
		//			int i = 0;
		//			string token;
		//			object[] parms = new object[1];
		//			object result;
		//			try
		//			{
		//				for ( i=0; i<6; i++ )
		//				{
		//					token = Token();
		//					parms[0] = text;
		//					if ( token == null ) break;
		//					result = objects[i].GetType().InvokeMember("Parse",
		//						BindingFlags.Public|BindingFlags.Static|BindingFlags.InvokeMethod,
		//						null,objects[i],parms);
		//				}
		//			}
		//			catch ( System.Exception e )
		//			{
		//				SimulationConsole.ReportException("ParseToken.Scan",
		//					Severity.Error,"The text could not be scanned:",e);
		//				return 0;
		//			}
		//			return i;
		//		}

		/// <summary>
		/// Gets the next token (as a string).
		/// <P>Multiple successive delimiters are treated as one.</P>
		/// </summary>
		/// <returns></returns>
		public string Token() {
			int i;
			int j;
			int begin;
			int len;

			if ( cursor >= length ) return string.Empty;
			
			if ( useWhiteSpace ) {
				// parse using whitespace
				// skip past leading whitespace
				for( i=cursor; i<length; i++ )
					if ( !Char.IsWhiteSpace(chars[i]) ) break;
				begin = i;
				// find first trailing whitespace or last character
				for( i=begin+1; i<length; i++ )
					if ( Char.IsWhiteSpace(chars[i]) ) break;
			}
			else {
				// parse using delimiters
				// skip past leading delimiters
				for( i=cursor; i<length; i++ ) {
					for( j=0; j<delims.Length; j++ )
						if ( chars[i] == delims[j] ) break;
					if ( j >= delims.Length )
						break;
				}
				begin = i;
				// find first trailing delimiter or last character
				for( i=begin+1; i<length; i++ ) {
					for( j=0; j<delims.Length; j++ )
						if ( chars[i] == delims[j] ) break;
					if ( j < delims.Length ) 
						break;
				}
			}
			if ( begin >= length )
				return string.Empty;
			len = i - begin;
			cursor = i;
			return text.Substring(begin,len);
		}

		/// <summary>
		/// Gets the next token as a Byte.
		/// </summary>
		/// <returns></returns>
		public byte ReadByte(){ string token = Token(); return token != null ? Convert.ToByte(token):(Byte)0;}

		/// <summary>
		/// Gets the next token as a Int16.
		/// </summary>
		/// <returns></returns>
		public short ReadInt16(){ string token = Token(); return token != null ? Convert.ToInt16(token):(Int16)0;}

		/// <summary>
		/// Gets the next token as a UInt16.
		/// </summary>
		/// <returns></returns>
		public UInt16 ReadUInt16(){ string token = Token(); return token != null ? Convert.ToUInt16(token):(UInt16)0;}

		/// <summary>
		/// Gets the next token as a Int32.
		/// </summary>
		/// <returns></returns>
		public int ReadInt32(){ string token = Token(); return token != null ? Convert.ToInt32(token):0;}

		/// <summary>
		/// Gets the next token as a UInt32.
		/// </summary>
		/// <returns></returns>
		public UInt32 ReadUInt32(){ string token = Token(); return token != null ? Convert.ToUInt32(token):0;}

		/// <summary>
		/// Gets the next token as a Single.
		/// </summary>
		/// <returns></returns>
		public float ReadSingle(){ string token = Token(); return token != null ? Convert.ToSingle(token):0;}

		/// <summary>
		/// Gets the next token as a Double.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble(){ string token = Token(); return token != null ? Convert.ToDouble(token):0;}
	}
}

