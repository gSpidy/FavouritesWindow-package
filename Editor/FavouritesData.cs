using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace FavouritesEd
{
	[System.Serializable]
	public class FavouritesData
	{
		private const string PREF_NAME = "FavWndData";

		public static string SaveName => $"{PREF_NAME}:{PlayerSettings.productGUID}";
		public static string FallbackSaveName => PREF_NAME;
		
		public List<FavouritesElement> favs = new List<FavouritesElement>();
		public List<FavouritesCategory> categories = new List<FavouritesCategory>();
		[SerializeField] private int nextCategoryId = 0;

		public FavouritesCategory AddCategory(string name)
		{
			FavouritesCategory c = new FavouritesCategory()
			{
				id = nextCategoryId,
				name = name,
			};

			nextCategoryId++;
			categories.Add(c);

			return c;
		}

		public void Save()
		{
			EditorPrefs.SetString(SaveName, JsonConvert.SerializeObject(this));
		}

		public static FavouritesData Load()
		{
			var saveId = EditorPrefs.HasKey(SaveName)
				? SaveName
				: FallbackSaveName;
			
			var saved = EditorPrefs.GetString(saveId, null);

			return string.IsNullOrWhiteSpace(saved)
				? new FavouritesData()
				: JsonConvert.DeserializeObject<FavouritesData>(saved);
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}
