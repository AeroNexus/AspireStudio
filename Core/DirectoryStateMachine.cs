using Aspire.Core.Messaging;

namespace Aspire.Core
{
	public enum DirectoryState
	{
		Broken,
		Disconnect,
		FindDirectory,
		Hello,
		HelloAckWait,
		Operational,
		Resumed,
		Shutdown
	}
	public enum DirectoryInput
	{
		DirectoryAdded,
		DirectoryRemoved,
		GetXtedsReceived,
		HelloAckReceived,
		HelloResumed,
		PmRemoved,
		Start,
		Stop,
		Timeout
	}

	public interface IDirectoryStateMachineListener
	{
		void FindDirectory();
		void OnStateChanged(DirectoryState state);
		void SendXteds();
		void SendGoodbye();
		void SendHello();
	};

	public class DirectoryStateMachine : StateMachine<DirectoryState, DirectoryInput>
	{
		IDirectoryStateMachineListener mListener;

		public DirectoryStateMachine(IDirectoryStateMachineListener listener) : base(DirectoryState.Shutdown)
		{
			mListener = listener;
		}

		public void DirectoryAdded()
		{
			Process(DirectoryInput.DirectoryAdded);
		}

		public void DirectoryRemoved()
		{
			Process(DirectoryInput.DirectoryRemoved);
		}

		public void GetXtedsReceived()
		{
			Process(DirectoryInput.GetXtedsReceived);
		}

		public void HelloAckReceived(bool resumed)
		{
			Process(resumed ? DirectoryInput.HelloResumed : DirectoryInput.HelloAckReceived);
		}

		protected override string Name
		{
			get { return "DirSM"; }
		}

		protected override void OnEnterState(DirectoryState state)
		{
			switch(state)
			{
			case DirectoryState.Disconnect:
				mListener.SendGoodbye();
				State = DirectoryState.Shutdown;
				break;
			case DirectoryState.FindDirectory:
				mListener.FindDirectory();
				break;
			case DirectoryState.Hello:
				TimeoutTimerSet(5);
				mListener.SendHello();
				break;
			case DirectoryState.Resumed:
				State = DirectoryState.Operational;
				break;
			}
		}

		protected override void OnExitState(DirectoryState state)
		{
			switch (state)
			{
				case DirectoryState.Hello:
					TimeoutTimerCancel();
					break;
			}
		}

		protected override void OnProcess(DirectoryInput input)
		{
			switch (input)
			{
				case DirectoryInput.DirectoryAdded:
					ProcessDirectoryAdded();
					break;
				case DirectoryInput.DirectoryRemoved:
					ProcessDirectoryRemoved();
					break;
				case DirectoryInput.GetXtedsReceived:
					ProcessGetXtedsReceived();
					break;
				case DirectoryInput.HelloAckReceived:
				case DirectoryInput.HelloResumed:
					ProcessHelloAckReceived(input==DirectoryInput.HelloResumed);
					break;
				case DirectoryInput.PmRemoved:
					ProcessPmRemoved();
					break;
				case DirectoryInput.Start:
					ProcessStart();
					break;
				case DirectoryInput.Stop:
					ProcessStop();
					break;
				case DirectoryInput.Timeout:
					ProcessTimeout();
					break;
			}
		}

		protected override void OnStateChanged(DirectoryState state)
		{
			mListener.OnStateChanged(State);
		}

		protected override void OnTimeout()
		{
			base.OnTimeout();
			Process(DirectoryInput.Timeout);
		}

		public void PmRemoved()
		{
			Process(DirectoryInput.PmRemoved);
		}

		void ProcessDirectoryAdded()
		{
			switch (State)
			{
				case DirectoryState.FindDirectory:
					State = DirectoryState.Hello;
					break;
			}
		}

		void ProcessDirectoryRemoved()
		{
			switch (State)
			{
				case DirectoryState.Hello:
					State = DirectoryState.FindDirectory;
					break;
				case DirectoryState.Operational:
				case DirectoryState.Resumed:
					mListener.SendGoodbye();
					State = DirectoryState.Broken;
					break;
			}
		}

		void ProcessGetXtedsReceived()
		{
			switch (State)
			{
				case DirectoryState.Operational:
				case DirectoryState.Resumed:
					mListener.SendXteds();
					break;
			}
		}

		void ProcessHelloAckReceived(bool resumed)
		{
			switch (State)
			{
				case DirectoryState.Hello:
					State = resumed ? DirectoryState.Resumed : DirectoryState.Operational;
					break;
				case DirectoryState.Shutdown:
					mListener.SendGoodbye();
					break;
			}
		}

		void ProcessPmRemoved()
		{
			switch (State)
			{
				default:
					break;
			}
		}

		void ProcessStart()
		{
			switch (State)
			{
				case DirectoryState.Broken:
				case DirectoryState.Shutdown:
					State = DirectoryState.FindDirectory;
					break;
			}
		}

		void ProcessStop()
		{
			switch (State)
			{
				case DirectoryState.Broken:
				case DirectoryState.FindDirectory:
				case DirectoryState.Hello:
					State = DirectoryState.Shutdown;
					break;
				case DirectoryState.Operational:
				case DirectoryState.Resumed:
					State = DirectoryState.Disconnect;
					break;
			}
		}

		void ProcessTimeout()
		{
			switch(State)
			{
				case DirectoryState.Hello:
					State = DirectoryState.Hello;
					break;
			}
		}

		public void Start()
		{
			Process(DirectoryInput.Start);
		}

		public void Stop()
		{
			Process(DirectoryInput.Stop);
		}

	}

}

