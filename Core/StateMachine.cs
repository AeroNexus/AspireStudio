using System;
using System.Collections.Generic;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Utilities;

namespace Aspire.Core
{
	/* Rod Green:
	 * 
	 * There is a ton of complexity here because
	 * we need to provide run-to-completion semantics.  This means that 
	 * when an input is processed, the processing needs to finish before any
	 * other work is done.
	 *
	 * This started out simple, then it because clear the run-to-completion
	 * was necessary.  This first implementation is a mess.  I'd really like to
	 * see an implementation based on transition tables.  It starts to look like
	 * a library (and a lot of work).  There is a boost msm library that could
	 * easily replace this with much cleaner code...
	 *
	 * The exact semantics I settled on are:
	 * 
	 * for each input:
	 *   process input
	 *   process state change (if necessary)
	 *
	 * on process state change:
	 *   clear pending inputs
	 *   run state exit logic
	 *   run state entry logic
	 *   
	 */
	public abstract class StateMachine<TState,TInput>
	{
		bool mProcessingInput;
		TState mState;
		Timer mTimeoutTimer;

		Stack<TInput> mInputs = new Stack<TInput>();
		Stack<TState> mStates = new Stack<TState>();

		protected StateMachine(TState initialState)
		{
			mState = initialState;
		}

		static string StateToString(TState state) { return state.ToString(); }

		protected abstract string Name { get; }
		protected abstract void OnEnterState(TState state) ;
		protected abstract void OnExitState(TState state);
		protected abstract void OnProcess(TInput input);
		protected abstract void OnStateChanged(TState state);
		protected virtual void OnTimeout() { TimeoutTimerCancel(); }

		void OnTimeout(object ctx) { (ctx as StateMachine<TState, TInput>).OnTimeout(); }

		protected void Process(TInput input)
		{
			mInputs.Push(input);
			if (mProcessingInput)
				return;
			else
			{
				mProcessingInput = true;
				while (mInputs.Count>0)
				{
					input = mInputs.Pop();
					ProcessInternal(input);
				}
				mProcessingInput = false;
			}
		}

		void ProcessInternal(TInput input)
		{
			OnProcess(input);

			while (mStates.Count > 0)
			{
				var nextState = mStates.Pop();
				if (mStates.Count > 0)
				{
					Log.WriteLine("{0} StateMachineBase::Detected ambiguous transition",Name);
					mStates.Clear();
				}

				mInputs.Clear();

				OnExitState(mState);
				if ( !mState.Equals(nextState) )
				//if ((mState as IEquatable<TState>) != (nextState as IEquatable<TState>))
				{
					mState = nextState;
					OnStateChanged(mState);
				}
				OnEnterState(mState);
			}
		}

		public TState State
		{
			get { return mState; }
			protected set { mStates.Push(value); }
		}

		protected void TimeoutTimerCancel()
		{
			if (mTimeoutTimer != null)
			{
				mTimeoutTimer.Cancel();
				mTimeoutTimer = null;
			}
		}

		protected void TimeoutTimerSet(int timeoutSec, int timeoutMs=0)
		{
			mTimeoutTimer = Timer.Set(timeoutSec, timeoutMs*1000, Timer.Trigger.OneShot, OnTimeout);
		}
	}

}

