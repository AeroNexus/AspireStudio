using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Aspire.Studio.Dashboard
{
	public class DesignerSerializationService : IDesignerSerializationService
	{
		private IServiceProvider _serviceProvider;

		public DesignerSerializationService ( IServiceProvider serviceProvider ) {
			this._serviceProvider = serviceProvider;
		}

		public ICollection Deserialize ( object serializationData ) {
			var serializationStore = serializationData as SerializationStore;
			if ( serializationStore != null ) {
				var componentSerializationService = _serviceProvider.GetService ( typeof ( ComponentSerializationService ) ) as ComponentSerializationService;
				ICollection collection = componentSerializationService.Deserialize ( serializationStore );
				return collection;
			}
			return new object[] {};
		}

		public object Serialize ( ICollection objects ) {
			var componentSerializationService = _serviceProvider.GetService ( typeof ( ComponentSerializationService ) ) as ComponentSerializationService;
			SerializationStore returnObject = null;
			using ( SerializationStore serializationStore = componentSerializationService.CreateStore() ) {
				foreach ( object obj in objects ) {
					if ( obj is Control )
						componentSerializationService.Serialize ( serializationStore, obj );
				}
				returnObject = serializationStore;
			}
			return returnObject;
		}
	}
}
