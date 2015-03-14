// http://weblogs.asp.net/rosherove/archive/2004/06/16/156948.aspx

using System;
using System.ComponentModel;
 
namespace SmugMug.SendToSmugMug
{
	#region BackgroundWorker
	public class BackgroundWorker
	{
		bool m_CancelPending = false;
		bool m_ReportsProgress = false;
		bool m_SupportsCancellation = false;
 
		public event DoWorkEventHandler DoWork;
		public event ProgressChangedEventHandler ProgressChanged;
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;
 
		public bool WorkerSupportsCancellation
		{
			get
			{
				lock(this)
				{
					return m_SupportsCancellation;
				}
			}
			set
			{
				lock(this)
				{
					m_SupportsCancellation = value;
				}
			}
		}
 
		public bool WorkerReportsProgress
		{
			get
			{
				lock(this)
				{
					return m_ReportsProgress;
				}
			}
			set
			{
				lock(this)
				{
					m_ReportsProgress = value;
				}
			}
		}
 
		public bool CancellationPending
		{
			get
			{
				lock(this)
				{
					return m_CancelPending;
				}
			}
		}     
 
		public void RunWorkerAsync()
		{
			RunWorkerAsync(null);
		}
 
		public void RunWorkerAsync(object argument)
		{
			m_CancelPending = false;
			if(DoWork != null)
			{
				DoWorkEventArgs args = new DoWorkEventArgs(argument);
				AsyncCallback callback;
				callback = new AsyncCallback(ReportCompletion);
				DoWork.BeginInvoke(this,args,callback,args);
			}
		}
 
		public void ReportProgress(object userState)
		{
			if(WorkerReportsProgress)
			{
				ProgressChangedEventArgs progressArgs;
				progressArgs = new ProgressChangedEventArgs(userState);                   
				OnProgressChanged(progressArgs);
			}
		}

		public void ReportProgress(object userState, int counter, int total)
		{
			if (WorkerReportsProgress)
			{
				ProgressChangedEventArgs progressArgs;
				progressArgs = new ProgressChangedEventArgs(userState, counter, total);
				OnProgressChanged(progressArgs);
			}
		}

        public void ReportProgress(object userState, int counter, int total, bool error)
        {
            if (WorkerReportsProgress)
            {
                ProgressChangedEventArgs progressArgs;
                progressArgs = new ProgressChangedEventArgs(userState, counter, total, error);
                OnProgressChanged(progressArgs);
            }
        }
 
		public void CancelAsync()
		{
			lock(this)
			{
				m_CancelPending = true;
			}
		}
 
		protected virtual void OnProgressChanged(ProgressChangedEventArgs progressArgs)
		{
			ProcessDelegate(ProgressChanged,this,progressArgs);
		}
 
		protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs completedArgs)
		{
			ProcessDelegate(RunWorkerCompleted,this,completedArgs);
		}
 
		public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);
		public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
		public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
 
 
		void ProcessDelegate(Delegate del,params object[] args)
		{
			Delegate temp = del;
			if(temp == null)
			{
				return;
			}
			Delegate[] delegates = temp.GetInvocationList();
			foreach(Delegate handler in delegates)
			{
				InvokeDelegate(handler,args);
			}
		}
 
		void InvokeDelegate(Delegate del,object[] args)
		{
			System.ComponentModel.ISynchronizeInvoke synchronizer;
			synchronizer = del.Target as System.ComponentModel.ISynchronizeInvoke;
			if(synchronizer != null) //A Windows Forms object
			{
				if(synchronizer.InvokeRequired == false)
				{
					del.DynamicInvoke(args);
					return;
				}
				try
				{
					synchronizer.Invoke(del,args);
				}
				catch
				{}
			}   
			else //Not a Windows Forms object
			{
				del.DynamicInvoke(args);
			}  
		}
 
		void ReportCompletion(IAsyncResult asyncResult)
		{
			System.Runtime.Remoting.Messaging.AsyncResult ar = (System.Runtime.Remoting.Messaging.AsyncResult)asyncResult;
			DoWorkEventHandler del;
			del  = (DoWorkEventHandler)ar.AsyncDelegate;
			DoWorkEventArgs doWorkArgs = (DoWorkEventArgs)ar.AsyncState;
			object result = null;
			Exception error = null;
			try
			{
				del.EndInvoke(asyncResult);
				result = doWorkArgs.Result;
			}
			catch(Exception exception)
			{
				error = exception;
			}
			RunWorkerCompletedEventArgs completedArgs = new RunWorkerCompletedEventArgs(result, error, doWorkArgs.Cancel);
			OnRunWorkerCompleted(completedArgs);
		}
        public override string ToString()
        {
            return "BackgroundWorker";
        }
	}
	#endregion
 
	#region AsyncCompletedEventArgs
 
	public class AsyncCompletedEventArgs : EventArgs
	{     
		public AsyncCompletedEventArgs (bool cancelled,Exception ex)
		{
			Cancelled= cancelled;
			Error = ex;
		}
 
		public AsyncCompletedEventArgs(){}
 
		public readonly Exception Error;
		public readonly bool Cancelled;           
	}
	#endregion
 
	#region CancelEventArgs
	public class CancleEventArgs : EventArgs
	{
		private bool m_cancel = false;
		public bool Cancel
		{
			get
			{
				return m_cancel;
			}
			set
			{
				m_cancel=value;
			}
		}           
	}
	#endregion
 
	#region DoWorkEventArgs
 
	public class DoWorkEventArgs : CancleEventArgs
	{
		private bool result;
		public bool Result
		{
			get
			{
				return result;
			}
			set
			{
                result = value;
			}
		}
 
		public readonly object Argument;    
 
		public DoWorkEventArgs(object objArgument)
		{
			Argument = objArgument;
		}
	}
	#endregion
 
	#region ProgressChangedEventArgs
 
	public class ProgressChangedEventArgs : EventArgs
	{
        public readonly object UserState;
		public readonly int Total;
		public readonly int Counter;
		public readonly bool Error;
		public ProgressChangedEventArgs (object userState)
		{
			UserState = userState;
			Error = false;
		}
		public ProgressChangedEventArgs (object userState, int counter, int total)
		{
			Total = total;
			Counter = counter;
			UserState = userState;
			Error = false;
		}
		public ProgressChangedEventArgs (object userState, int counter, int total, bool error)
		{
			Total = total;
			Counter = counter;
			UserState = userState;
			Error = error;
		}
	}
	#endregion
 
	#region RunWorkerCompletedEventArgs
 
	public class RunWorkerCompletedEventArgs : AsyncCompletedEventArgs
	{           
		public readonly object Result;
 
		public RunWorkerCompletedEventArgs (object objResult, Exception exException, bool bCancel)
			:base(bCancel,exException)
		{                 
			Result = objResult;
		}
	}
	#endregion
}