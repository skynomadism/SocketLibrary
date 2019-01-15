using System;

namespace SocketLibrary {
	public class ConnectionCollection:System.Collections.CollectionBase {
		public ConnectionCollection() {
			
		}
		public void Add(Connection value) {
			List.Add(value); 
		}
		public Connection this[int index] {
			get {
				return List[index] as Connection;	 
			}
			set{
				List[index] = value;
			}
		}
		public Connection this[string connectionName] {
			get {
				foreach(Connection connection in List) {
					if(connection.ConnectionName == connectionName)
						return connection;
				}
				return null;
			}
		}
	}
}
