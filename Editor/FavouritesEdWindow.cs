using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace FavouritesEd
{
	public class FavouritesEdWindow : EditorWindow
	{
		private static readonly GUIContent GC_Add = new GUIContent("+", "Add category");
		private static readonly GUIContent GC_Remove = new GUIContent("-", "Remove selected");

		[SerializeField] private FavouritesData data; 
		[SerializeField] private TreeViewState treeViewState;
		[SerializeField] private FavouritesTreeView treeView; 
		[SerializeField] private SearchField searchField;

		// ------------------------------------------------------------------------------------------------------------------

		[MenuItem("Window/Favourites")]
		private static void ShowWindow()
		{
			GetWindow<FavouritesEdWindow>("Favourites");//.UpdateTreeview();
		}

		private void OnHierarchyChange()
		{
			if(Application.isPlaying) return;
			UpdateTreeview();
		}

		private void OnProjectChange()
		{
			UpdateTreeview();
		}

		private void UpdateTreeview()
		{
			if (data == null) 
				data = FavouritesData.Load();

			if (treeViewState == null)
				treeViewState = new TreeViewState();

			if (treeView == null)
			{
				searchField = null;
				treeView = new FavouritesTreeView(treeViewState);
			}

			if (searchField == null)
			{
				searchField = new SearchField();
				searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
			}

			treeView.LoadAndUpdate(data);
			Repaint();
		}

		// ------------------------------------------------------------------------------------------------------------------

		private void OnGUI()
		{
			if (treeView == null)
			{
				UpdateTreeview();
			}

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				treeView.searchString = searchField.OnToolbarGUI(treeView.searchString, GUILayout.ExpandWidth(true));
				GUILayout.Space(5);
				if (GUILayout.Button(GC_Add, EditorStyles.toolbarButton, GUILayout.Width(25)))
				{
					TextInputWindow.ShowWindow("Favourites", "Enter category name", "", AddCategory, null);
				}
				GUI.enabled = treeView.Model.Data.Count > 0;
				if (GUILayout.Button(GC_Remove, EditorStyles.toolbarButton, GUILayout.Width(25)))
				{
					RemoveSelected();
				}
				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();
			
			treeView.OnGUI();
		}

		// ------------------------------------------------------------------------------------------------------------------

		private void AddCategory(TextInputWindow wiz)
		{
			string s = wiz.Text;
			wiz.Close();
			if (string.IsNullOrEmpty(s)) return;

			data.AddCategory(s);
			data.Save();

			UpdateTreeview();
			Repaint();
		}

		private void RemoveSelected()
		{
			IList<int> ids = treeView.GetSelection();
			if (ids.Count == 0) return;

			FavouritesTreeElement ele = treeView.Model.Find(ids[0]);
			if (ele == null) return;

			if (ele.category != null)
			{
				// remove elements from open scene. those in closed scenes will just
				// have to stay. they will not show up anyway if category is gone

				/*// remove from scene
				foreach (FavouritesContainer c in FavouritesEd.Containers)
				{
					if (c == null || c.favs == null) continue;
					for (int i = c.favs.Count - 1; i >= 0; i--)
					{
						if (c.favs[i].categoryId == ele.category.id)
						{
							c.favs.RemoveAt(i);
							EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
						}
					}
				}*/

				// remove favourites linked to this category
				for (int i = data.favs.Count - 1; i >= 0; i--)
				{
					if (data.favs[i].categoryId == ele.category.id) data.favs.RemoveAt(i);
				}

				// remove category
				for (int i = 0;i < data.categories.Count; i++)
				{
					if (data.categories[i].id == ele.category.id)
					{
						data.categories.RemoveAt(i);
						break;
					}
				}

				data.Save();
			}
			else
			{
				bool found = false;
				for (int i = 0; i < data.favs.Count; i++)
				{
					if (data.favs[i] == ele.fav)
					{
						found = true;
						data.favs.RemoveAt(i);
						data.Save();
						break;
					}
				}

				if (!found)
				{
					/*foreach (FavouritesContainer c in FavouritesEd.Containers)
					{
						if (c == null || c.favs == null) continue;
						for (int i = 0; i < c.favs.Count; i++)
						{
							if (c.favs[i] == ele.fav)
							{
								found = true;
								c.favs.RemoveAt(i);
								EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
								break;
							}
						}
						if (found) break;
					}*/
				}
			}

			UpdateTreeview();
			Repaint();			
		}

		private string GetPackageFolder()
		{
			try
			{
				string[] res = System.IO.Directory.GetFiles(Application.dataPath, "FavouritesEdWindow.cs", System.IO.SearchOption.AllDirectories);
				if (res.Length > 0) return "Assets" + res[0].Replace(Application.dataPath, "").Replace("FavouritesEdWindow.cs", "").Replace("\\", "/");
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
			}
			return "Assets/";
		}

		// ------------------------------------------------------------------------------------------------------------------
	}
}