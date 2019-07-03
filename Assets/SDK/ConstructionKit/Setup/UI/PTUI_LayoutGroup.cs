using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTUI_LayoutGroup : MonoBehaviour {
    public LayoutType layoutType;
    GridLayoutGroup layout;
    RectTransform rectTransform;

    public enum LayoutType
    {
        None, OneRowExtensible, OneColumnExtensible
    }

	// Use this for initialization
	void Start () {
        layout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        AutoResize();
    }

    void AutoResize()
    {
        if (layout == null)
            return;
        int childCount = transform.childCount;
        float newRectWidth = rectTransform.sizeDelta.x;
        float newRectHeight = rectTransform.sizeDelta.y;
        switch (layoutType)
        {
            case LayoutType.None:
                break;
            case LayoutType.OneRowExtensible:
                newRectWidth = childCount * (layout.cellSize.x + layout.spacing.x) - layout.spacing.x;
                newRectHeight = layout.cellSize.y + layout.spacing.y;
                break;
            case LayoutType.OneColumnExtensible:
                newRectHeight = childCount * (layout.cellSize.y + layout.spacing.y) - layout.spacing.y;
                newRectWidth = layout.cellSize.x + layout.spacing.x;
                break;
        }
        rectTransform.sizeDelta = new Vector2(newRectWidth, newRectHeight);
    }
}
