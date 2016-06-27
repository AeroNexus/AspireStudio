using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

using Microsoft.CSharp;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Framework.Scripting
{
	/// <summary>
	/// Base class for script object implementations. This class defines helper methods used by script code
	/// </summary>
	public class ScriptObjectImplementationBase: IScriptObjectImplementation, IPublishable
	{

        /// <summary>
        /// Play a sound of the specified frequency for the specified number of milliseconds. Returns immediately.
        /// </summary>
        /// <param name="frequency">The frequency of the sound</param>
        /// <param name="duration">The duration, in milliseconds, of the sound</param>
        public static void PlaySound(int frequency, int duration)
        {
            PlaySound(frequency, duration, false);
        }

        /// <summary>
        /// Play a sound of the specified frequency for the specified number of milliseconds.
        /// </summary>
        /// <param name="frequency">The frequency of the sound</param>
        /// <param name="duration">The duration, in milliseconds, of the sound</param>
        /// <param name="waitForSoundToFinish">If true, waits for the sound to complete before returning.</param>
        public static void PlaySound(int frequency, int duration, bool waitForSoundToFinish)
        {
            MediaUtilities.PlaySound(frequency, duration, waitForSoundToFinish);
        }

		/// <summary>
		/// Write a line to the simulation console listeners. The Source will be set to an empty string and the Severity will be set to Info.
		/// </summary>
		/// <param name="text">The text to write to the simulation console</param>
		protected static void WriteLine( string text )
		{
			WriteLine( string.Empty, Log.Severity.Info, text );
		}

		/// <summary>
		/// Write a line to the simulation console listeners, using format specifiers as in <see cref="string.Format(string,object[])"/>. The Source will be set to an empty string and the Severity will be set to Info.
		/// </summary>
		/// <example>To log a variable using the format string:<p><code>int i = 4;
		/// WriteLine("The value of i is {0}.", i);</code></p></example>
		/// <param name="format">A string containing zero or more format items.</param>
		/// <param name="args">A System.Object array containing zero ro more objects to format.</param>
		protected static void WriteLine( string format, params object[] args )
		{
			WriteLine( string.Empty, Log.Severity.Info, format, args );
		}

		/// <summary>
		/// Write a line to the simulation console listeners.
		/// </summary>
		/// <example>To log a variable using the format string:<p><code>int i = 4;
		/// WriteLine("Me", Severity.Info, "The value of i is {0}.", i);</code></p></example>
		/// <param name="source">The source of the message.</param>
		/// <param name="severity">The severity of the message.</param>
		/// <param name="format">A string containing zero or more format items.</param>
		/// <param name="args">A System.Object array containing zero ro more objects to format.</param>
		protected static void WriteLine( string source, Log.Severity severity, string format, params object[] args )
		{
			WriteLine(  source, severity, string.Format( format, args ) );
		}

		/// <summary>
		/// Write a line to the simulation console listeners.
		/// </summary>
		/// <param name="source">The source of the message.</param>
		/// <param name="severity">The severity of the message.</param>
		/// <param name="text">The string to write.</param>
		protected static void WriteLine( string source, Log.Severity severity, string text )
		{
			Log.WriteLine(source,severity,text);
		}

	  /// <summary>
	  /// Gets a Blackboard.Item for the given path.
	  /// </summary>
	  /// <param name="path"></param>
	  /// <returns></returns>
	  protected static Blackboard.Item GetItem( string path )
		{ return Blackboard.GetExistingItem( path ); }

		/// <summary>
		/// Gets a DoubleDataItem for the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
    protected static Blackboard.Item GetDblItem(string path)
    { return Blackboard.GetExistingItem(path) as Blackboard.Item; }
		
		/// <summary>
		/// Gets a double from the specified path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected static double GetDbl( string path )
		{
			var item = GetDblItem( path );
			if( item == null ) 
			{
				Log.WriteLine("ScriptObject", Log.Severity.Info, "GetDbl was called with a non-existent data item path ({0})", path);
				return 0;
			}
			return (double)item.Value;
		}

		/// <summary>
		/// Get an object value for the specified path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected static object GetValue( string path )
		{
			var item = GetItem( path );
			if ( item == null ) 
			{
        Log.WriteLine("ScriptObject", Log.Severity.Debug, "GetValue was called with a non-existent data item path ({0})", path);
				return null;
			}
			return item.Value;
		}

		/// <summary>
		/// Sets a double value into the specified path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="val"></param>
		protected static void SetValue(string path, double val)
		{
			var item = GetDblItem(path);
			if (item != null)
			{
				item.Value = val;
			}
			else
			{
				var itemObj = GetItem(path);
				if (itemObj != null)
				{
					itemObj.Value = val;
				}
				else
				{
          Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue was called with a non-existent data item path ({0})", path);
				}
			}
		}

		/// <summary>
		/// Sets a float value into the specified path. Required because SetValue(double) coerces floats to doubles,
		/// but the underlying object can't assign a double to a float.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="val"></param>
		protected static void SetValue(string path, float val)
		{
			var item = GetItem(path);
			if (item != null)
			{
				item.Value = val;
			}
			else
			{
				var itemObj = GetItem(path);
				if (itemObj != null)
				{
					itemObj.Value = val;
				}
				else
				{
          Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue was called with a non-existent data item path ({0})", path);
				}
			}
		}

		/// <summary>
		/// Sets an array of double into the specified path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="val"></param>
		protected static void SetValue( string path, double[] val )
		{
			var item = GetDblItem( path );
			if( item == null ) 
			{
        Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue was called with a non-existent data item path ({0})", path);
				return;
			}

            if (!item.IsArray)
            {
              Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue( string, double[] ) was called with a data item path ({0}) that was not an array.", path);
                return;
            }

            if (val == null) throw new ArgumentNullException("val");

			if( item.Items.Count != val.GetLength(0) ) return;
			for( int i = 0; i < item.Items.Count; i++ )
			{ item.Items[ i ].Value = val[ i ]; }
		}

        /// <summary>
        /// Sets an <see cref="IHostArray"/>-based value into a the specified data item.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="val"></param>
        /// <example>
        /// <code>
        /// Vector3 v = new Vector3(1,2,3);
        /// SetValue("SC1.Body.FooVector", v);
        /// </code>
        /// </example>
        protected static void SetValue(string path, IHostArray val)
        {
            var item = GetItem(path);

            if (item == null)
            {
              Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue was called with a non-existent data item path ({0})", path);
                return;
            }

            Array destination = item.GetArray();

            if (!item.IsArray || destination == null )
            {
              Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue( string, double[] ) was called with a data item path ({0}) that was not an array.", path);
                return;
            }

            if (val == null) throw new ArgumentNullException("val");


            if (val.Rank != destination.Rank) throw new ArgumentOutOfRangeException("The source and destination arrays have differing ranks");

            if( val.Rank > 2 ) throw new ArgumentOutOfRangeException("SetValue does not support arrays with Rank > 2");
            if (val.Rank == 1)
            {
                for (int i = 0; i < destination.GetLength(0); i++)
                {
                    destination.SetValue( val[i], i);
                }
            }
            else
            {
                for (int i = 0; i < destination.GetLength(0); i++)
                {
                    for (int j = 0; j < destination.GetLength(1); j++)
                    {
                        destination.SetValue(val.HostedArray.GetValue(i,j), i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value into the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="val"></param>
        protected static void SetValue(string path, object val)
		{
			var item = GetItem( path );
			if( item == null ) 
			{
        Log.WriteLine("ScriptObject", Log.Severity.Debug, "SetValue was called with a non-existent data item path ({0})", path);
				return;
			}
			item.Value = val;
		}

		/// <summary>
		/// The configuration
		/// </summary>
		protected Scenario mScenario = null;

		/// <summary>
		/// Gets and sets the loaded configuration
		/// </summary>
		public Scenario LoadedScenario
		{
			get
			{
        if (mScenario == null)
          mScenario = lastScenario;
        return mScenario;
			}
			set
			{
				mScenario = value;
        lastScenario = value;
			}
		}
		
		private static Scenario lastScenario;

		/// <summary>
		/// Gets and sets a generic object
		/// </summary>
    public object Tag { get; set; }
		
		/// <summary>
		/// Gets and sets whether the instance is debuggable.
		/// </summary>
		public bool Debuggable { get; set; }

		/// <summary>
		/// Break into the debugger if, and only if, the script is compiled with debug information. This overload is called from sub-classes within a <see cref="ScriptObjectImplementationBase"/>
		/// </summary>
		/// <param name="instance">An instance of ScriptObjectImplementationBase</param>
		/// <returns></returns>
		protected static void Break( ScriptObjectImplementationBase instance )
		{
			if( instance.Debuggable )
			{
				System.Diagnostics.Debugger.Break();
			}
		}

		/// <summary>
		/// Break into the debugger if, and only if, the script is compiled with debug information. 
		/// </summary>
		protected void Break()
		{
			Break( this );
		}

		#region IPublisher Members

		private string mName = string.Empty;

		/// <summary>
		/// Gets and sets the Name
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}

		/// <summary>
		/// Gets and sets the Parent
		/// </summary>
    public IPublishable Parent { get; set; }

    /// <summary>
    /// Blackboard path to this item
    /// </summary>
    public string Path { get; set; }

		#endregion
	}
}
