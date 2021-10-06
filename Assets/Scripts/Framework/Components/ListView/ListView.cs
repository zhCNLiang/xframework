using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ScrollRect)), RequireComponent(typeof(RectMask2D))]
public class ListView : MonoBehaviour
{
	public enum Alignment
	{
		Horizontal,
		Vertical,
	}

	public enum ItemLiftCycle
	{
		Create,
		Visiable,
		Recycle,
		Destroy,
	}

	[SerializeField]
	private Alignment alignment;
	[SerializeField]
	private ListItem prefab;
	[SerializeField]
	private float space;
	[SerializeField]
	private int columns = 1;
	[SerializeField]
	private float columnSpace = 0;
	private ScrollRect scrollRect;

	private int totalNum;
	private int rowNum;
	private Vector2 prefabSize;

	private Vector2 contentSize;

	private Dictionary<int, ListItem> activeItems = new Dictionary<int, ListItem>();
	private Queue<ListItem> deActiveItems = new Queue<ListItem>();

	private Action<ListItem, int, ItemLiftCycle> itemForUpdate;
	private Func<ListItem, int, float> itemForRowHeight;
	private Dictionary<int, float> cacheRowHeight = new Dictionary<int, float>();

	private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);

	private void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
		scrollRect.onValueChanged.AddListener(OnValueChanged);

		var rectTransfrom = transform as RectTransform;
		contentSize = new Vector2(rectTransfrom.rect.width, rectTransfrom.rect.height);

		var go = new GameObject("Content", new Type[] { typeof(RectTransform) });
		var content = go.transform as RectTransform;
		content.SetParent(transform);
		content.sizeDelta = contentSize;
		scrollRect.content = content;
		scrollRect.viewport = rectTransfrom;

		var prefabRectTransform = prefab.transform as RectTransform;
		prefabSize = new Vector2(prefabRectTransform.rect.width, prefabRectTransform.rect.height);

		scrollRect.horizontal = alignment == Alignment.Horizontal;
		scrollRect.vertical = alignment == Alignment.Vertical;

		var anchored = alignment == Alignment.Horizontal ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1f);
		scrollRect.content.anchoredPosition = Vector2.zero;
		scrollRect.content.anchorMin = anchored;
		scrollRect.content.anchorMax = anchored;
		scrollRect.content.offsetMin = Vector2.zero;
		scrollRect.content.offsetMax = Vector2.zero;
		scrollRect.content.pivot = anchored;

		prefabRectTransform.anchoredPosition = Vector2.zero;
		prefabRectTransform.anchorMin = anchored;
		prefabRectTransform.anchorMax = anchored;
		prefabRectTransform.pivot = anchored;

		prefab.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		scrollRect.onValueChanged.RemoveListener(OnValueChanged);
		CleanListItems();
		itemForUpdate = null;
		itemForRowHeight = null;
	}

	public void SetNum(int num)
	{
		totalNum = num;
		rowNum = Mathf.CeilToInt(num / columns);
		CalcContentSize();
	}

	public void ReloadAll()
	{
		RefreshList(true);
	}

	public void ScrollToTop()
	{
		ScrollToIndex(0);
	}

	public void ScrollToBottom()
	{
		ScrollToIndex(Mathf.Max(totalNum - 1, 0));
	}

	public void RegisterItemUpdate(Action<ListItem, int, ItemLiftCycle> itemForVisiable)
	{
		this.itemForUpdate = itemForVisiable;
	}

	public void RegisterItemRowHeight(Func<ListItem, int, float> itemForRowHeight)
	{
		this.itemForRowHeight = itemForRowHeight;
	}

	public void UpdateItemRowHeight(int rowIndex)
	{
		cacheRowHeight[rowIndex] = itemForRowHeight.Invoke(prefab, rowIndex);
	}

	public float GetItemRowHeight(int rowIndex)
	{
		cacheRowHeight.TryGetValue(rowIndex, out var height);
		return height;
	}

	public void ScrollToIndex(int index, float duration = 0)
	{
		scrollRect.StopMovement();

		StopAllCoroutines();
		StartCoroutine(AnimationScroll(index, duration));
	}

	public IEnumerator AnimationScroll(int index, float duration)
	{
		var rowIndex = Mathf.FloorToInt(index / columns); 
		var offsetMax = alignment == Alignment.Horizontal ? Mathf.Min(scrollRect.viewport.rect.width - scrollRect.content.sizeDelta.x, 0)
			: -Mathf.Min(scrollRect.viewport.rect.height - scrollRect.content.sizeDelta.y, 0);
		var offsetMin = 0f;
		var offset = GetRowIndexPos(rowIndex);
		offset = alignment == Alignment.Horizontal ? -offset : offset;
		var min = Mathf.Min(offsetMin, offsetMax);
		var max = Mathf.Max(offsetMin, offsetMax);
		offset = Mathf.Clamp(offset, min, max);

		var startPos = scrollRect.content.anchoredPosition;
		var accumate = 0f;
		while(accumate <= duration)
		{
			accumate += Time.deltaTime;
			var t = duration > 0 ? accumate / duration : 1f;
			t = animationCurve.Evaluate(t);

			switch(alignment)
			{
				case Alignment.Horizontal:
				{
					var pos = new Vector2(Mathf.Lerp(startPos.x, offset, t), startPos.y);
					scrollRect.content.anchoredPosition = pos;
				}
				break;

				case Alignment.Vertical:
				{
					var pos = new Vector2(startPos.x, Mathf.Lerp(startPos.y, offset, t));
					scrollRect.content.anchoredPosition = pos;
				}
				break;
			}

			yield return new WaitForEndOfFrame();
		}
	}

	public void RefreshItemAtIndex(int index)
	{
		 if (activeItems.TryGetValue(index, out var item))
		 {
			itemForUpdate?.Invoke(item, item.index, ItemLiftCycle.Visiable);
		 }
	}

	private void CleanListItems()
	{
		foreach(var kv in activeItems)
		{
			itemForUpdate?.Invoke(kv.Value, kv.Value.index, ItemLiftCycle.Destroy);
		}

		foreach(var item in deActiveItems)
		{
			itemForUpdate?.Invoke(item, item.index, ItemLiftCycle.Destroy);
		}
	}

	private List<int> waitRemove = new List<int>();
	private void OnValueChanged(Vector2 pos)
	{
		RefreshList();
	}

	private void RefreshList(bool bForceRefreshAll = false)
	{
		var range = GetVisiableIndex();
		for(int i=range.x; i <= range.y; i++)
		{
			for (int j=0; j<columns; j++)
			{
				var idx = i * columns + j;
				if (idx >= totalNum) break;

				var liftCycle = ItemLiftCycle.Create;
				var bNeedRefresh = false;
				if (!activeItems.TryGetValue(idx, out var item))
				{
					if (deActiveItems.Count > 0)
					{
						liftCycle = ItemLiftCycle.Visiable;
						item = deActiveItems.Dequeue();
					}
					else
					{
						liftCycle = ItemLiftCycle.Create;
						item = Instantiate<ListItem>(prefab);
					}
					item.gameObject.SetActive(true);
					item.transform.SetParent(scrollRect.content);
					item.index = idx;
					activeItems.Add(idx, item);
					bNeedRefresh = true;
					item.name = $"{prefab.name}_{idx}";
				}

				if (bNeedRefresh || bForceRefreshAll)
				{
					var rectTransfrom = item.transform as RectTransform;
					rectTransfrom.anchoredPosition = GetItemPos(idx);
					itemForUpdate?.Invoke(item, idx, liftCycle);
				}
			}
		}

		waitRemove.Clear();
		waitRemove.AddRange(activeItems.Keys);
		waitRemove.Sort();
		var index = 0;
		foreach(var i in waitRemove)
		{
			var item = activeItems[i];
			var rowIndex = Mathf.FloorToInt(i / columns);
			if (rowIndex < range.x || rowIndex > range.y)
			{
				deActiveItems.Enqueue(item);
				activeItems.Remove(i);
				item.gameObject.SetActive(false);
			}
			item.transform.SetSiblingIndex(index);
			index++;
		}
	}

	private void CalcContentSize()
	{
		var totalHeight = GetRowIndexHeight(rowNum - 1, true);
		contentSize = alignment == Alignment.Horizontal ? new Vector2(totalHeight, contentSize.y) : new Vector2(contentSize.x, totalHeight);
		scrollRect.content.sizeDelta = contentSize;
	}

	private Vector2Int GetVisiableIndex()
	{
		var startIndex = 0;
		var endIndex = 0;
		var pos = scrollRect.content.anchoredPosition;
		var viewRect = scrollRect.viewport.rect;
		switch(alignment)
		{
			case Alignment.Horizontal:
			{
				startIndex = CalcVisiableRowIndex(-pos.x);
				endIndex = CalcVisiableRowIndex(-pos.x + viewRect.width);
				break;
			}
			case Alignment.Vertical:
			{
				startIndex = CalcVisiableRowIndex(pos.y);
				endIndex = CalcVisiableRowIndex(pos.y + viewRect.height);
				break;
			}
		}
		startIndex = Mathf.Max(startIndex, 0);
		endIndex = Mathf.Min(endIndex, rowNum - 1);
		return new Vector2Int(startIndex, endIndex);
	}

	private int CalcVisiableRowIndex(float offset)
	{
		var rowIndex = -1;
		if (itemForRowHeight != null)
		{
			prefab.gameObject.SetActive(true);
			while(offset > 0)
			{
				rowIndex ++;
				var height = CalcItemRowHeight(rowIndex);
				offset -= (height + space);
			}
			prefab.gameObject.SetActive(false);
		}
		else
		{
			var height = alignment == Alignment.Horizontal ? prefabSize.x : prefabSize.y;
			rowIndex = Mathf.FloorToInt(offset / (height + space));
		}
		return rowIndex;
	}

	private Vector2 GetItemPos(int index)
	{
		var rowIndex = Mathf.FloorToInt(index / columns);
		var columnIndex = index - rowIndex * columns;
		switch(alignment)
		{
			case Alignment.Horizontal:
			{
				var startY = (columns * prefabSize.y + columnSpace * Mathf.Max(columns - 1, 0)) / 2 * -1;
				var posY = startY + (columnIndex + 0.5f) * prefabSize.y + columnIndex * columnSpace;
				return new Vector2(GetRowIndexPos(rowIndex), posY);
			}
			case Alignment.Vertical:
			{
				var startX = (columns * prefabSize.x + columnSpace * Mathf.Max(columns - 1, 0)) / 2 * -1;
				var posX = startX + (columnIndex + 0.5f) * prefabSize.x + columnIndex * columnSpace;
				return new Vector2(posX, -GetRowIndexPos(rowIndex));
			}
		}
		return Vector2.zero;
	}

	private float GetRowIndexPos(int rowIndex)
	{
		return GetRowIndexHeight(rowIndex, false);
	}

	private float CalcItemRowHeight(int rowIndex)
	{
		if (!cacheRowHeight.TryGetValue(rowIndex, out var height))
		{
			height = itemForRowHeight.Invoke(prefab, rowIndex);
			cacheRowHeight.Add(rowIndex, height);
		}
		return height;
	}

	private float GetRowIndexHeight(int rowIndex, bool isIncludeTheIndex = true)
	{
		var height = 0f;
		if (itemForRowHeight != null)
		{
			prefab.gameObject.SetActive(true);
			for(int i=0; i<rowIndex; i++)
			{
				height += CalcItemRowHeight(i);
				height += space;
			}

			if (isIncludeTheIndex)
			{
				height += CalcItemRowHeight(rowIndex);
			}
			prefab.gameObject.SetActive(false);
		}
		else
		{
			height = alignment == Alignment.Horizontal ? rowIndex * (prefabSize.x + space) : rowIndex * (prefabSize.y + space);

			if (isIncludeTheIndex)
				height += alignment == Alignment.Horizontal ? prefabSize.x : prefabSize.y;
		}
		return height;
	}
}
