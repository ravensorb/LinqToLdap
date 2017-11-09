using System.Runtime.InteropServices;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public class UserEntryObject : EntryObject
	{
		/// <summary>
        /// Sets the password for the current account. Please refer to
        /// <see cref="SetPassword(string, string)"/> if you are changing
        /// the password for the user, unless you are setting the password
        /// for the account for the first time.
		/// </summary>
		/// <param name="password">The value to set as the password.</param>
		/// <returns>true if the password was set, false otherwise.</returns>
		public virtual bool SetPassword(string password)
		{ 
			if (!string.IsNullOrEmpty(password))
			{
				var setPasswordResult = Entry.Invoke("SetPassword", new object[]
				{
					password
				});

				var result = setPasswordResult == null;

				if (!result)
				{
					Marshal.ReleaseComObject(setPasswordResult);
				}

				return result;
			}

			return false;
		}

        /// <summary>
        /// Sets the password for the current account.
        /// </summary>
        /// <param name="oldPassword">The old value of the password.</param>
        /// <param name="newPassword">The new value to set as the password.</param>
        /// <returns>true if the password was set, false otherwise.</returns>
		public virtual bool SetPassword(string oldPassword, string newPassword)
        {
            if (!string.IsNullOrEmpty(oldPassword))
            {
                var setPasswordResult = Entry.Invoke("ChangePassword", new object[]
				{
					oldPassword,
					newPassword
				});

                var result = setPasswordResult == null;

                if (!result)
                {
                    Marshal.ReleaseComObject(setPasswordResult);
                }

                return result;
            }

            return false;
        }
	}
}
