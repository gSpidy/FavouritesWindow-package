using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FavouritesEd
{
	[System.Serializable]
	public class FavouritesElement
	{
		public int categoryId;	// id of the Favourites category this is in
		public string objId; // or 

		[JsonIgnore]
		public Object Obj
		{
			get
			{
				if (!GlobalObjectId.TryParse(objId, out var id))
					return null;
				
				var fndObj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

				if (id.identifierType == 2) // additional checks for scene objects
				{
					var go = fndObj as GameObject;
					if (go == null || !go.scene.IsValid() || id.assetGUID != AssetDatabase.GUIDFromAssetPath(go.scene.path))
						return null;
				}

				return fndObj;
			}
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}
