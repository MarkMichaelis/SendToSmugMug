using System;
using System.Collections;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace SmugMug.Toolkit
{
	public class Account
	{
		private string username;
		private string password;
        private string secret;
        private string tokenId;

		public Account()
		{
			
		}

		public Account(string username)
		{
			this.username = username;
		}

		/// <summary>
		/// The username for the account. This is a fully qualified email address.
		/// </summary>
		public string Username
		{
			get { return username; }
			set { username = value; }
		}

        public string TokenId
        {
            get { return tokenId; }
            set { tokenId = value; }
        }

		public string EncryptedPassword
		{
			get
			{
				return this.password;
			}
			set
			{
				this.password = value;
			}
		}

        public string EncryptedSecret
        {
            get
            {
                return this.secret;
            }
            set
            {
                this.secret = value;
            }
        }

        [XmlIgnore]
        public string Secret
        {
            get
            {
                string returnSecret = null;

                try
                {
                    byte[] sourceBytes = Convert.FromBase64String(this.secret);
                    byte[] decryptedBytes = ProtectedData.Unprotect(sourceBytes, null, DataProtectionScope.CurrentUser);
                    returnSecret = System.Text.Encoding.Unicode.GetString(decryptedBytes);
                }
                catch
                {
                    returnSecret = "";
                }

                return returnSecret;
            }

            set
            {
                try
                {
                    byte[] sourceBytes = System.Text.Encoding.Unicode.GetBytes(value);
                    byte[] encryptedBytes = ProtectedData.Protect(sourceBytes, null, DataProtectionScope.CurrentUser);
                    this.secret = Convert.ToBase64String(encryptedBytes);
                }
                catch
                {
                    this.secret = value;
                }
            }
        }

		[XmlIgnore]
		public string Password
		{
			get
			{
				string returnPassword = null;

                try
                {
                    byte[] sourceBytes = Convert.FromBase64String(this.password);
                    byte[] decryptedBytes = ProtectedData.Unprotect(sourceBytes, null, DataProtectionScope.CurrentUser);
                    returnPassword = System.Text.Encoding.Unicode.GetString(decryptedBytes);
                }
                catch
                {
                    returnPassword = "";
                }

				return returnPassword;
			}

			set
			{
                try
                {
                    byte[] sourceBytes = System.Text.Encoding.Unicode.GetBytes(value);
                    byte[] encryptedBytes = ProtectedData.Protect(sourceBytes, null, DataProtectionScope.CurrentUser);
                    this.password = Convert.ToBase64String(encryptedBytes);
                }
                catch
                {
                    this.password = value;
                }
			}
		}
	}
	
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='SmugMug.Toolkit.Account'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='Accounts'/>
	[Serializable()]
	public class Accounts : CollectionBase
	{
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='Accounts'/>.
		///    </para>
		/// </summary>
		public Accounts() 
		{
		}
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='Accounts'/> based on another <see cref='Accounts'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='Accounts'/> from which the contents are copied
		/// </param>
		public Accounts(Accounts value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='Accounts'/> containing any array of <see cref='SmugMug.Toolkit.Account'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='SmugMug.Toolkit.Account'/> objects with which to intialize the collection
		/// </param>
		public Accounts(Account[] value) 
		{
			this.AddRange(value);
		}
        
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='SmugMug.Toolkit.Account'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public Account this[int index] 
		{
			get 
			{
				return ((Account)(List[index]));
			}
			set 
			{
				List[index] = value;
			}
		}
        
		public Account this[string username] 
		{
			get 
			{
				foreach (Account account in this.InnerList)
				{
					if (account.Username.ToLower() == username.ToLower())
					{
						return account;
					}
				}

				return null;
			}
			set 
			{
				for(int i = 0; i <= this.InnerList.Count; i++)
				{
					if (((Account)this.InnerList[i]).Username.ToLower() == username.ToLower())
					{
						this.InnerList[i] = value;
						break;
					}
				}
			}
		}

		/// <summary>
		///    <para>Adds a <see cref='SmugMug.Toolkit.Account'/> with the specified value to the 
		///    <see cref='Accounts'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='SmugMug.Toolkit.Account'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		public int Add(Account value) 
		{
			return List.Add(value);
		}
        
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='Accounts'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='SmugMug.Toolkit.Account'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='Accounts.Add'/>
		public void AddRange(Account[] value) 
		{
			for (int i = 0; (i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='Accounts'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='Accounts'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='Accounts.Add'/>
		public void AddRange(Accounts value) 
		{
			for (int i = 0; (i < value.Count); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
        
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='Accounts'/> contains the specified <see cref='SmugMug.Toolkit.Account'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='SmugMug.Toolkit.Account'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='SmugMug.Toolkit.Account'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='Accounts.IndexOf'/>
		public bool Contains(Account value) 
		{
			return List.Contains(value);
		}
        
		/// <summary>
		/// <para>Copies the <see cref='Accounts'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Accounts'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Accounts'/> is greater than the available space between <paramref name='index'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(Account[] array, int index) 
		{
			List.CopyTo(array, index);
		}
        
		/// <summary>
		///    <para>Returns the index of a <see cref='SmugMug.Toolkit.Account'/> in 
		///       the <see cref='Accounts'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='SmugMug.Toolkit.Account'/> to locate.</param>
		/// <returns>
		/// <para>The index of the <see cref='SmugMug.Toolkit.Account'/> of <paramref name='value'/> in the 
		/// <see cref='Accounts'/>, if found; otherwise, -1.</para>
		/// </returns>
		/// <seealso cref='Accounts.Contains'/>
		public int IndexOf(Account value) 
		{
			return List.IndexOf(value);
		}
        
		/// <summary>
		/// <para>Inserts a <see cref='SmugMug.Toolkit.Account'/> into the <see cref='Accounts'/> at the specified index.</para>
		/// </summary>
		/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
		/// <param name=' value'>The <see cref='SmugMug.Toolkit.Account'/> to insert.</param>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='Accounts.Add'/>
		public void Insert(int index, Account value) 
		{
			List.Insert(index, value);
		}
        
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='Accounts'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new AccountEnumerator GetEnumerator() 
		{
			return new AccountEnumerator(this);
		}
        
		/// <summary>
		///    <para> Removes a specific <see cref='SmugMug.Toolkit.Account'/> from the 
		///    <see cref='Accounts'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='SmugMug.Toolkit.Account'/> to remove from the <see cref='Accounts'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(Account value) 
		{
			List.Remove(value);
		}
        
		public class AccountEnumerator : object, IEnumerator 
		{
            
			private IEnumerator baseEnumerator;
            
			private IEnumerable temp;
            
			public AccountEnumerator(Accounts mappings) 
			{
				this.temp = (mappings);
				this.baseEnumerator = temp.GetEnumerator();
			}
            
			public Account Current 
			{
				get 
				{
					return ((Account)(baseEnumerator.Current));
				}
			}
            
			object IEnumerator.Current 
			{
				get 
				{
					return baseEnumerator.Current;
				}
			}
            
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
            
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
            
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	}
}
