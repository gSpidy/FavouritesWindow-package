using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

namespace FavouritesEd
{
	public class FavouritesTreeView: TreeViewWithTreeModel<FavouritesTreeElement>
	{
		private static readonly GUIContent GC_None = new GUIContent("No Favourites.");
		private static readonly string DragAndDropID = "FavouritesTreeElement";

		public TreeModel<FavouritesTreeElement> Model { get { return model; } }
		private TreeModel<FavouritesTreeElement> model;

		private FavouritesData _data;

		// ------------------------------------------------------------------------------------------------------------

		public FavouritesTreeView(TreeViewState treeViewState) 
			: base(treeViewState)
		{
			baseIndent = 5f;
		}

		public void OnGUI()
		{
			if (model != null && model.Data != null && model.Data.Count > 1)
			{
				base.OnGUI(GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
			}
			else
			{
				GUILayout.Label(GC_None);
				GUILayout.FlexibleSpace();
			}
		}

		public void LoadAndUpdate(FavouritesData favsData = null)
		{
			if (favsData != null) _data = favsData;

			// add root
			FavouritesTreeElement treeRoot = new FavouritesTreeElement() { ID = 0, Depth = -1, Name = "Root" };
			model = new TreeModel<FavouritesTreeElement>(new List<FavouritesTreeElement>() { treeRoot });

			// add categories
			List<FavouritesTreeElement> categories = new List<FavouritesTreeElement>();
			Texture2D icon = EditorGUIUtility.IconContent("FolderFavorite Icon").image as Texture2D;
			foreach (FavouritesCategory c in _data.categories)
			{
				FavouritesTreeElement ele = new FavouritesTreeElement()
				{
					Name = c.name,
					Icon = icon,
					ID = model.GenerateUniqueID(),
					category = c
				};

				categories.Add(ele);
				model.QuickAddElement(ele, treeRoot);
			}

			// add favourites from project and scene(s)
			List<FavouritesElement> favs = new List<FavouritesElement>();
			favs.AddRange(_data.favs.Where(x=>x.Obj));

			// sort
			favs.Sort((a, b) => 
			{
				int r = a.categoryId.CompareTo(b.categoryId);
				if (r == 0) r = a.Obj.name.CompareTo(b.Obj.name);
				return r;
			});

			// and add to tree
			foreach (FavouritesElement ele in favs)
			{
				Object obj;
				if (ele == null || (obj = ele.Obj) == null) continue;
				foreach (FavouritesTreeElement c in categories)
				{
					if (c.category.id == ele.categoryId)
					{
						string nm = obj.name;
						GameObject go = obj as GameObject;
						if (go != null && go.scene.IsValid())
						{
							nm = string.Format("{0} ({1})", nm, go.scene.name);
						}
						//else
						//{
						//	nm = string.Format("{0} ({1})", nm, AssetDatabase.GetAssetPath(ele.obj));
						//}

						icon = AssetPreview.GetMiniThumbnail(obj);

						model.QuickAddElement(new FavouritesTreeElement()
						{
							Name = nm,
							Icon = icon,
							ID = model.GenerateUniqueID(),
							fav = ele
						}, c);

						break;
					}
				}
			}

			model.UpdateDataFromTree();
			Init(model);
			Reload();
			SetSelection(new List<int>());
		}		

		protected override void RowGUI(RowGUIArgs args)
		{
			base.RowGUI(args);
		}

		protected override void ContextClickedItem(int id)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Ping"), false, HandleContextOption, id);
			menu.ShowAsContext();
		}

		private void HandleContextOption(object arg)
		{
			int id = (int)arg;
			FavouritesTreeElement ele = Model.Find(id);
			if (ele != null && ele.fav != null && ele.fav.Obj != null)
			{
				EditorGUIUtility.PingObject(ele.fav.Obj);
			}
		}

		protected override void DoubleClickedItem(int id)
		{
			FavouritesTreeElement ele = Model.Find(id);
			if (ele != null && ele.fav != null && ele.fav.Obj != null)
			{
				AssetDatabase.OpenAsset(ele.fav.Obj);				
			}
			else
			{
				SetExpanded(id, !IsExpanded(id));				
			}
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}


		protected override bool CanStartDrag(CanStartDragArgs args)
		{
			if (_data == null || _data.categories.Count == 0 || 
				!rootItem.hasChildren || args.draggedItem.parent == rootItem)
			{
				return false;
			}

			return true;
		}

		protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
		{
			if (args.draggedItemIDs.Count == 0) return;

			FavouritesTreeElement item = Model.Find(args.draggedItemIDs[0]);
			Object obj;
			if (item == null || item.fav == null || (obj = item.fav.Obj) == null) return;

			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData(DragAndDropID, item);
			DragAndDrop.objectReferences = new Object[] { obj };
			DragAndDrop.StartDrag(obj.name);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			if (_data == null || _data.categories.Count == 0 || !rootItem.hasChildren)
			{
				return DragAndDropVisualMode.Rejected;
			}

			if (args.performDrop)
			{
				FavouritesTreeElement ele;
				int id = args.parentItem == null ? -1 : args.parentItem.id;
				if (id < 0 || (ele = model.Find(id)) == null || ele.category == null)
				{
					IList<int> ids = GetSelection();
					if (ids.Count > 0)
					{
						TreeViewItem item = FindItem(ids[0], rootItem);
						if (item == null) return DragAndDropVisualMode.Rejected;
						id = item.parent == rootItem ? item.id : item.parent.id;
					}
					else
					{
						id = rootItem.children[0].id;
					}
					ele = model.Find(id);
				}

				if (ele == null || ele.category == null)
				{
					return DragAndDropVisualMode.Rejected;
				}

				int categoryId = ele.category.id;

				// first check if it is "internal" drag drop from one category to another
				FavouritesTreeElement draggedEle = DragAndDrop.GetGenericData(DragAndDropID) as FavouritesTreeElement;
				if (draggedEle != null)
				{
					draggedEle.fav.categoryId = categoryId;

					// check if in scene and mark scene dirty, else do nothing
					// more since asset is marked dirty at end anyway
					GameObject go = draggedEle.fav.Obj as GameObject;
					if (go != null && go.scene.IsValid())
					{
						EditorSceneManager.MarkSceneDirty(go.scene);
					}
				}

				// else the drag-drop originated somewhere else
				else
				{					
					Object[] objs = DragAndDrop.objectReferences;
					foreach (Object obj in objs)
					{
						// make sure it is not a component
						if (obj is Component) continue;

						_data.favs.Add(new FavouritesElement()
						{
							objId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString(),
							categoryId = categoryId
						});
					}
				}
				
				_data.Save();
				LoadAndUpdate();
			}

			return DragAndDropVisualMode.Generic;
		}

		private static string FolderIconName()
		{
			var sb = new StringBuilder("FolderFavorite Icon");
			if (EditorGUIUtility.isProSkin)
				sb.Insert(0, "d_");
			return sb.ToString();
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}
