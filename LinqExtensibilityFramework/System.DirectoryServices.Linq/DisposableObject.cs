namespace System
{
	public abstract class DisposableObject : IDisposable
	{
		#region Constructors

		~DisposableObject()
		{
			if (AssertNotDisposedOrDisposing(false))
			{
				Dispose(false);
			}
		}

		#endregion

		#region Properties

		internal bool IsDisposed { get; private set; }

		internal bool IsDisposing { get; private set; }

		#endregion

		#region Methods

		public virtual void Dispose()
		{
			if (AssertNotDisposedOrDisposing(true))
			{
				IsDisposing = true;

				Dispose(true);

				IsDisposing = false;
				IsDisposed = true;

				GC.SuppressFinalize(this);
			}
		}

		protected bool AssertNotDisposedOrDisposing(bool throwIfDisposed)
		{
			if (IsDisposed || IsDisposing)
			{
				if (!throwIfDisposed)
				{
					return false;
				}

				throw new ObjectDisposedException(GetType().FullName);
			}

			return true;
		}

		protected abstract void Dispose(bool disposing);

		#endregion
	}
}
