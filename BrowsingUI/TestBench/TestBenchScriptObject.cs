using System;
using System.Collections.Generic;
using System.Linq;

using Aspire.CoreModels;
using Aspire.Framework.Scripting;
using Aspire.UiToolbox.Forms;
using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.BrowsingUI.TestBench
{
    /// <summary>
    /// TestBenchScriptObjectScriptObject
    /// </summary>
    public class TestBenchScriptObject : ScriptObjectImplementationBase, IDisposable
    {
        /// <summary>
        /// Gets and sets the name of the current device-under-test
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets and sets the UID for the current device-under-test
        /// </summary>
        public string DeviceUid { get; set; }

        /// <summary>
        /// Gets and sets the KIND for the current device-under-test
        /// </summary>
        public string DeviceKind { get; set; }

        #region Helper Methods

        /// <summary>
        /// Returns a boolean if the <paramref name="value"/> argument equals the name of the <paramref name="varValue"/>'s selected DRange option
        /// </summary>
        /// <param name="varValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool IsDrangeValueEqual(Xteds.Interface.VariableValue varValue, string value)
        {
            if (varValue != null && varValue.Variable != null && varValue.Variable.Drange != null &&
                varValue.Variable.Drange.Options.Length > 0)
            {
                Xteds.Interface.Option option = varValue.Variable.Drange.Options.
                    FirstOrDefault(opt => opt.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
                if (option != null)
                {
                    return Convert.ToByte(varValue.Value).Equals(Convert.ToByte(option.Value));
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the specified xTEDS qualifier value.
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="qualifierName"></param>
        /// <returns></returns>
		protected Xteds.Qualifier GetQualifier(string componentName, string interfaceName, string qualifierName)
        {
            var comp = GetComponent(componentName);
            if (comp != null)
            {
                if (comp.Xteds != null)
                {
                    Xteds.Interface interfaceRequested = comp.Xteds.Interfaces.FirstOrDefault(i => i.Name.Equals(interfaceName));
                    if (interfaceRequested != null)
                    {
                        Xteds.Qualifier qualifier = interfaceRequested.Qualifiers.FirstOrDefault(qual => qual.Name.Equals(qualifierName));
                        return qualifier;
                    }
                }
                return null;
            }
            else
            {
                WriteLine("GetQualifier was called for a non-existent component ({0}).", componentName);
                return null;
            }
        }

        /// <summary>
        /// Get the <see cref="AspireComponent"/> using the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="AspireComponent"/> to retrieve.</param>
        /// <returns>The <see cref="AspireComponent"/> if it exists; null otherwise</returns>
        /// <example>AspireComponent comp = GetComponent( "ReactionWheel_X" );</example>
		    protected AspireComponent GetComponent(string name)
        {
            var comp = PnPBrowsers.AllComponent(name);
            if (comp == null)
            {
                comp = PnPBrowsers.AllComponents.FirstOrDefault(c => c.Name.Equals(name));
            }
            return comp;
        }

        
        /// <summary>
        /// Get the <see cref="AspireComponent"/> using the specified <paramref name="sensorId"/>.
        /// </summary>
        /// <param name="sensorId">The sensor ID of the <see cref="AspireComponent"/> to retrieve.</param>
        /// <returns>The <see cref="AspireComponent"/> if it exists; null otherwise</returns>
        /// <example>AspireComponent comp = GetComponent( 127 );</example>
		    protected AspireComponent GetComponent(uint sensorId)
        {
			    foreach (var browser in PnPBrowsers.Browsers)
			    {
				    var comp = browser.Component(sensorId);
				    if ( comp != null )
					    return comp;
			    }
          return null;
        }

        /// <summary>
        /// Gets the <see cref="Xteds.Interface.Message"/> for the specified component, interface, and message.
        /// </summary>
        /// <param name="componentName">The component name</param>
        /// <param name="interfaceName">The message's interface</param>
        /// <param name="messageName">The message's name</param>
        /// <returns></returns>
		protected Xteds.Interface.Message GetMessage(string componentName, string interfaceName, string messageName)
        {
            // get the AspireComponent
            var component = GetComponent(componentName);

            if (component == null) throw new ArgumentException("componentName", "Component specified was not found.");
            if (component.Xteds == null) throw new ArgumentException("componentName", "The specified component does not have an Xteds object initialized.");

            return component.Xteds.FindMessage(interfaceName, messageName);
        }

        /// <summary>
        /// Gets the <see cref="Xteds.Interface.VariableValue"/> for the specified message and variable.
        /// </summary>
        /// <param name="componentName">The component name</param>
        /// <param name="interfaceName">The message's interface</param>
        /// <param name="messageName">The message name</param>
        /// <param name="variableName">The variable name</param>
        /// <returns></returns>
		protected Xteds.Interface.VariableValue GetVariableValue(string componentName, string interfaceName, string messageName, string variableName)
        {
            return GetVariableValue( GetMessage(componentName, interfaceName, messageName), variableName );
        }

        /// <summary>
        /// Gets the <see cref="Xteds.Interface.VariableValue"/> for the specified message and variable.
        /// </summary>
        /// <param name="message">The message that contains the variable requested</param>
        /// <param name="variableName">The variable name</param>
        /// <returns></returns>
		protected Xteds.Interface.VariableValue GetVariableValue(Xteds.Interface.Message message, string variableName)
        {
            if (message != null && message.Variables != null)
            {
                return message.Variables.FirstOrDefault(var => var.Name == variableName);
            }

            return null;
        }

        #endregion
        
        /// <summary>
        /// Sends the specified message using the specified variable values.
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="messageName"></param>
        /// <param name="variableValues"></param>
		protected Xteds.Interface.CommandMessage Send(string componentName, string interfaceName, string messageName, params object[] variableValues)
        {
			Xteds.Interface.CommandMessage msg = GetMessage(componentName, interfaceName, messageName) as Xteds.Interface.CommandMessage;
            Send(msg, variableValues);
            return msg;
        }

        /// <summary>
        /// Sends the specified message using the specified variable values.
        /// </summary>
        /// <param name="cmdMessage"></param>
        /// <param name="variableValues"></param>
		protected void Send(Xteds.Interface.CommandMessage cmdMessage, params object[] variableValues)
        {
            Dictionary<string, object> nvc = new Dictionary<string, object>();
            if (variableValues != null)
            {
                if (variableValues.Length % 2 != 0)
                    throw new ArgumentOutOfRangeException("parameters", @"The variableValues must be a set of ""variableName"",<value>");

                for (int i = 0; i < variableValues.Length; i += 2)
                {
                    if (variableValues[i].GetType() != typeof(string))
                        throw new ArgumentOutOfRangeException("parameters", string.Format("Parameter #{0} was not a string", i + 4));
                    nvc.Add(variableValues[i].ToString(), variableValues[i + 1]);
                }
            }

            Send(cmdMessage, nvc);
        }

        /// <summary>
        /// Sends the specified message using the specified variable values.
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="messageName"></param>
        /// <param name="variableValues">The variable values to send with the message. Can be null.</param>
        /// <returns>The <see cref="Xteds.Interface.CommandMessage"/> that was sent</returns>
		protected Xteds.Interface.CommandMessage Send(string componentName, string interfaceName, string messageName, Dictionary<string, object> variableValues)
        {            
            Xteds.Interface.CommandMessage cmdMessage = GetMessage(componentName,interfaceName,messageName) as Xteds.Interface.CommandMessage;
            
            Send(cmdMessage, variableValues);
            
            return cmdMessage;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdMessage"></param>
        /// <param name="variableValues"></param>
		protected void Send(Xteds.Interface.CommandMessage cmdMessage, Dictionary<string, object> variableValues)
        {
            if (cmdMessage == null) throw new ArgumentException("The message specified could not be found or is not a CommandMessage.");

            if (variableValues != null)
            {
                //
                // fill in variable values from the messageContents parameter...
                //
                variableValues.Keys.ForEach(variableName =>
                {
                    Xteds.Interface.VariableValue vv = cmdMessage.Variables.FirstOrDefault(vbl => vbl.Name.Equals(variableName));
                    object value = variableValues[variableName];
                    if (vv != null)
                    {
                        if (vv.Variable != null && vv.Variable.Drange != null && vv.Variable.Drange.Options.Length > 0 &&
                            value.GetType() == typeof(string))
                        {
                            Xteds.Interface.Option option =
                                vv.Variable.Drange.Options.FirstOrDefault(opt => opt.Name.Equals(value.ToString()));

                            // TODO: the Convert.ToByte() is a bit of a hack
                            if (option != null) value = Convert.ToByte(option.Value);
                        }

                        vv.Value = value;
                    }
                }
                );
            }

            // send the message
            cmdMessage.Send();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="messageName"></param>
        /// <param name="variableValues"></param>
        /// <returns></returns>
		protected Xteds.Interface.DataReplyMessage Request(string componentName, string interfaceName, string messageName, params object[] variableValues)
        {
            Dictionary<string, object> nvc = new Dictionary<string, object>();
            if (variableValues != null)
            {
                if (variableValues.Length % 2 != 0)
                    throw new ArgumentOutOfRangeException("parameters", @"The variableValues must be a set of ""variableName"",<value>");

                for (int i = 0; i < variableValues.Length; i += 2)
                {
                    if (variableValues[i].GetType() != typeof(string))
                        throw new ArgumentOutOfRangeException("parameters", string.Format("Parameter #{0} was not a string", i + 4));
                    nvc.Add(variableValues[i].ToString(), variableValues[i + 1]);
                }
            }

            return Request(componentName, interfaceName, messageName, nvc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="interfaceName"></param>
        /// <param name="messageName"></param>
        /// <param name="variableValues"></param>
        /// <returns></returns>
		protected Xteds.Interface.DataReplyMessage Request(string componentName, string interfaceName, string messageName, Dictionary<string, object> variableValues)
        {
            Xteds.Interface.CommandMessage cmdMessage = GetMessage(componentName, interfaceName, messageName) as Xteds.Interface.CommandMessage;
            if (cmdMessage == null) throw new ArgumentException("The message specified could not be found or is not a CommandMessage.");
            
            int receivedCount = cmdMessage.Request.DataReplyMsg.Received;
            
            Send(cmdMessage, variableValues);

            while (receivedCount == cmdMessage.Request.DataReplyMsg.Received)
            {
                System.Threading.Thread.SpinWait(1000);
                System.Windows.Forms.Application.DoEvents();
            }

            return cmdMessage.Request.DataReplyMsg;
        }

        private void cmd(string componentName, string interfaceName, string messageName, Dictionary<string,object> variableValues)
        {
            // cmd samples (from SCL script example):
            //    cmd RobustHub_pY_2_SetSoftTrip EP_ID=0 SoftCurrentLimit=20
            //    cmd RealTimeClock_1_SetClockTare ClockTare=100
            //    cmd GPS_pY_1_SetDevMode GPSMode=Ground  
            
            // sample script call:
            // cmd("ReactionWheel_X", "DevicePower", "DevicePwrSetState", "");

            Send(componentName, interfaceName, messageName, variableValues);

            //// get the AspireComponent
            //AspireComponent component = AspireBrowser.TheBrowser.Component(componentName);
            //if( component == null ) throw new ArgumentException("componentName", "Component specified was not found.");
            //if( component.Xteds == null ) throw new ArgumentException("componentName", "The specified component does not have an Xteds object initialized.");
            
            //// get the Xteds.Interface
            //Xteds.Interface messageInterface = component.Xteds.Interfaces.FirstOrDefault( 
            //    xtedsIf => xtedsIf.Name.Equals(interfaceName) );
            //if (messageInterface == null) throw new ArgumentException("interfaceName", "Interface specified was not found.");
            
            //// get the Xteds.Interface.Message
            //Xteds.Interface.Message msg = messageInterface.CommandMessages.FirstOrDefault(
            //    message => message.Name.Equals(messageName));
            //if (msg == null) throw new ArgumentException("messageName", "The message specified was not found.");

            //Xteds.Interface.CommandMessage cmdMessage = msg as Xteds.Interface.CommandMessage;

            ////
            //// fill in variable values from the messageContents parameter...
            ////
            //variableValues.Keys.ForEach(variableName =>
            //    {
            //        Xteds.Interface.VariableValue vv = cmdMessage.Variables.FirstOrDefault(vbl => vbl.Name.Equals(variableName));
            //        object value = variableValues[variableName];
            //        if (vv != null)
            //        {
            //            if (vv.Variable != null && vv.Variable.Drange != null && vv.Variable.Drange.Options.Length > 0 && 
            //                value.GetType() == typeof(string))
            //            {
            //                Xteds.Interface.Option option = 
            //                    vv.Variable.Drange.Options.FirstOrDefault( opt => opt.Name.Equals(value.ToString() ));
                            
            //                // TODO: the Convert.ToByte() is a bit of a hack
            //                if( option != null ) value = Convert.ToByte(option.Value);
            //            }

            //            vv.Value = value;
            //        }
            //    }
            //);

            //// send the message
            //cmdMessage.Send();
        }

        /// <summary>
        /// Wait for the specified number of seconds. Execution is blocked on the current thread.
        /// </summary>
        /// <example>wait( 5 );</example>
        /// <param name="seconds">Number of seconds to wait. Must be greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="seconds"/> is not greater than zero.</exception>
        protected void wait(int seconds)
        {
            if (seconds <= 0) throw new System.ArgumentOutOfRangeException("time", "The time cannot be less than or equal to 0 seconds.");
            wait(seconds.Seconds());
        }

        /// <summary>
        /// Wait for the specified time.
        /// </summary>
        /// <param name="time">A <see cref="TimeSpan"/> representing the time to wait.</param>
        /// <example>wait( 5.Seconds() );</example>
        protected void wait(TimeSpan time)
        {
            if (time == null ) throw new System.ArgumentOutOfRangeException("time", "The time parameter must not be null.");
            if (time.TotalSeconds == 0) throw new System.ArgumentOutOfRangeException("time", "The time cannot be 0 seconds.");
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            while (timer.Elapsed < time)
            {
                System.Threading.Thread.SpinWait(1);
                System.Windows.Forms.Application.DoEvents();
            }

        }

        /// <summary>
        /// Log a message using the <see cref="SimulationConsole"/> object.
        /// </summary>
        /// <param name="msg">The message to display.</param>
        protected void msg(string msg)
        {
            Log.WriteLine(msg);
        }

        /// <summary>
        /// Log a message using the <see cref="SimulationConsole"/> object.
        /// </summary>
        /// <param name="msg">The message to display, as a composite format string.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        protected void msg(string msg, params object[] args)
        {
            Log.WriteLine(msg, args);
        }

        /// <summary>
        /// Display an <see cref="InputBox"/> dialog and return the string entered by the user.
        /// </summary>
        /// <param name="labelText">The label for the <see cref="InputBox"/> dialog.</param>
        /// <returns>The string entered by the user.</returns>
        protected string ask(string labelText)
        {
            return InputBox.ShowInputBox("TestBench", labelText);
        }

        /// <summary>
        /// Evaluates the expression provided and returns the boolean value.
        /// </summary>
        /// <param name="expr">The expression to evaluate</param>
        /// <returns>The result of the boolean expression</returns>
        /// <example>verify( () => DateTime.Now > DateTime.MinValue );</example>
        protected bool verify(Func<bool> expr)
        {
            return expr.Invoke();
        }

        /// <summary>
        /// Evaluates the expression provided and returns the boolean value.
        /// </summary>
        /// <param name="expr">The expression to evaluate</param>
        /// <param name="withinSeconds">The time, in seconds, to wait until evaluating the expression.</param>
        /// <returns>The result of the boolean expression</returns>
        /// <example>verify( () => DateTime.Now > DateTime.MinValue, 10 );</example>
        protected bool verify(Func<bool> expr, int withinSeconds)
        {
            if (withinSeconds <= 0) throw new System.ArgumentOutOfRangeException("withinSeconds", "The withinSeconds parameter must be greater than zero.");
            return verify(expr, withinSeconds.Seconds());
        }

        /// <summary>
        /// Waits the <paramref name="withinSeconds"/> time and evaluates the expression provided, returning the boolean value.
        /// </summary>
        /// <remarks>The expression will be evaluated continuously and will return true immediately when the condition is met, or false if the time has elapsed.</remarks>
        /// <param name="expr">The expression to evaluate</param>
        /// <param name="within">The time to wait until evaluating the expression.</param>
        /// <returns>The result of the boolean expression</returns>
        /// <example>verify( () => DateTime.Now > DateTime.MinValue, 10.Seconds() );</example>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="withinSeconds"/> is not greater than zero.</exception>
        protected bool verify(Func<bool> expr, TimeSpan within)
        {
            if (within == null || within.TotalSeconds <= 0) throw new System.ArgumentOutOfRangeException("within", "The within parameter must be greater than zero.");
            bool result = false;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            while (!(result = expr.Invoke()) && timer.Elapsed < within )
            {
                System.Threading.Thread.SpinWait(1);
                System.Windows.Forms.Application.DoEvents();
            }

            return result;
        }

        /// <summary>
        /// Waits until the <paramref name="expr"/> evaluates to true.
        /// </summary>
        /// <remarks>Calling this method could result in an infinite loop if the expression never evaluates to true.</remarks>
        /// <param name="expr">The expression to evaluate.</param>
        /// <example>long stopTicks = DateTime.Now.Ticks + DateTime.Now.AddSeconds(10).Ticks;<p>
        /// wait_until( () => DateTime.Now.Ticks > stopTicks );</p></example>
        protected void wait_until(Func<bool> expr)
        {
            while (!expr.Invoke())
            {
                System.Threading.Thread.SpinWait(1);
                System.Windows.Forms.Application.DoEvents();
            }
        }
        /// <summary>
        /// Waits until the <paramref name="expr"/> evaluates to true or the timeout period has elapsed
        /// </summary>
        /// <remarks>Calling this method could result in an infinite loop if the expression never evaluates to true.</remarks>
        /// <param name="expr">The expression to evaluate.</param>
        /// <param name="timeout">The timeout period, in seconds.</param>
        /// <example>long stopTicks = DateTime.Now.Ticks + DateTime.Now.AddSeconds(10).Ticks;<p>
        /// wait_until( () => DateTime.Now.Ticks > stopTicks, 15 );</p></example>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeout"/> is not greater than zero.</exception>
        protected bool wait_until(Func<bool> expr, int timeout)
        {
            if (timeout <= 0) throw new System.ArgumentOutOfRangeException("timeout", "The timeout parameter must be greater than zero.");
            return wait_until(expr, timeout.Seconds());
        }

        /// <summary>
        /// Waits until the <paramref name="expr"/> evaluates to true or the timeout period has elapsed
        /// </summary>
        /// <remarks>Calling this method could result in an infinite loop if the expression never evaluates to true.</remarks>
        /// <param name="expr">The expression to evaluate.</param>
        /// <param name="timeout">The timeout period.</param>
        /// <example>long stopTicks = DateTime.Now.Ticks + DateTime.Now.AddSeconds(10).Ticks;<p>
        /// wait_until( () => DateTime.Now.Ticks > stopTicks, 15.Seconds() );</p></example>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeout"/> is not greater than zero.</exception>
        protected bool wait_until(Func<bool> expr, TimeSpan timeout)
        {
            if (timeout == null || timeout.TotalSeconds <= 0) throw new System.ArgumentOutOfRangeException("timeout", "The timeout parameter must be greater than zero.");

            bool result = false;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            while (!(result = expr.Invoke()) && timer.Elapsed < timeout)
            {
                System.Threading.Thread.SpinWait(1);
                System.Windows.Forms.Application.DoEvents();
            }

            return result;
        }

        /// <summary>
        /// When overriden in the script, OnDispose is called during dispose.
        /// </summary>
        protected virtual void OnDispose() { }

        #region IDisposable Members

        /// <summary>
        /// The dispose method
        /// </summary>
        public void Dispose()
        {
            OnDispose();
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for creating <see cref="TimeSpan"/> instances.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Return a <see cref="TimeSpan"/> for the number of seconds
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static TimeSpan Seconds(this int number)
        {
            return new TimeSpan(0, 0, number);
        }

        /// <summary>
        /// Return a <see cref="TimeSpan"/> for the number of minutes
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static TimeSpan Minutes(this int number)
        {
            return new TimeSpan(0, number, 0);
        }

        /// <summary>
        /// Return a <see cref="TimeSpan"/> for the number of milliseconds
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static TimeSpan Milliseconds(this int number)
        {
            return new TimeSpan(0, 0, 0, 0, number);
        }
    }
}
