using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Diagnostics;

namespace Aspire.Studio.Dashboard
{
	public class UndoEngine2 : UndoEngine
	{
		private string _Name_ = "UndoEngine";

		private Stack<UndoEngine.UndoUnit> undoStack = new Stack<UndoEngine.UndoUnit>();
		private Stack<UndoEngine.UndoUnit> redoStack = new Stack<UndoEngine.UndoUnit>();

		public UndoEngine2 ( IServiceProvider provider ) : base ( provider ) {}

		public bool EnableUndo {
			get { return undoStack.Count > 0; }
		}

		public bool EnableRedo {
			get { return redoStack.Count > 0; }
		}

		public void Undo() {
			if ( undoStack.Count > 0 ) {
				try {
					UndoEngine.UndoUnit unit = undoStack.Pop();
					unit.Undo();
					redoStack.Push ( unit );
					//Log("::Undo - undo action performed: " + unit.Name);
				}
				catch ( Exception ex ) {
					Debug.WriteLine( _Name_ + ex.Message );
					//Log("::Undo() - Exception " + ex.Message + " (line:" + new StackFrame(true).GetFileLineNumber() + ")");
				}
			}
			else {
				//Log("::Undo - NO undo action to perform!");
			}
		}

		public void Redo() {
			if ( redoStack.Count > 0 ) {
				try {
					UndoEngine.UndoUnit unit = redoStack.Pop();
					unit.Undo();
					undoStack.Push ( unit );
					//Log("::Redo - redo action performed: " + unit.Name);
				}
				catch ( Exception ex ) {
					Debug.WriteLine( _Name_ + ex.Message );
					//Log("::Redo() - Exception " + ex.Message + " (line:" + new StackFrame(true).GetFileLineNumber() + ")");
				}
			}
			else {
				//Log("::Redo - NO redo action to perform!");
			}
		}


		protected override void AddUndoUnit ( UndoEngine.UndoUnit unit ) {
			undoStack.Push ( unit );
		}
	}
}
