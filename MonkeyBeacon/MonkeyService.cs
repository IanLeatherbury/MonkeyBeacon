using System;
using System.Net.Http;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace MonkeyBeacon
{
	public class MonkeyService : DelegatingHandler
	{
		MobileServiceClient client;
		static MonkeyService instance = new MonkeyService ();

		IMobileServiceTable<MonkeyItem> monkeyTable;

		int busyCount = 0;

		public event Action<bool> BusyUpdate;

		public static MonkeyService DefaultService {
			get {
				return instance;
			}
		}

		public MobileServiceClient GetClient {
			get
			{
				return client;
			}
			private set
			{
				client = value;
			}
		}

		public MonkeyService ()
		{
			CurrentPlatform.Init ();

			client = new MobileServiceClient (Constants.applicationUrl, this);

			// Create an MSTable instance to allow us to work with the TodoItem table
			monkeyTable = client.GetTable <MonkeyItem> ();
		}

		public async Task InsertTodoItemAsync (MonkeyItem monkeyItem)
		{
			try {
				// This code inserts a new MonkeyItem into the database. When the operation completes
				// and Mobile Services has assigned an Id, the item is added to the CollectionView
				await monkeyTable.InsertAsync (monkeyItem);

			} catch (MobileServiceInvalidOperationException e) {
				Console.Error.WriteLine (@"ERROR {0}", e.Response);
			}
		}

		void Busy (bool busy)
		{
			// assumes always executes on UI thread
			if (busy) {
				if (busyCount++ == 0 && BusyUpdate != null)
					BusyUpdate (true);
			} else {
				if (--busyCount == 0 && BusyUpdate != null)
					BusyUpdate (false);
			}
		}

		#region implemented abstract members of HttpMessageHandler

		protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			Busy (true);
			var response = await base.SendAsync (request, cancellationToken);

			Busy (false);
			return response;
		}

		#endregion
	}
}

