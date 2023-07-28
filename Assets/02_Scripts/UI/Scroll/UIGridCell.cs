using FancyScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridItemData
{
    public int id { get; }

    public GridItemData(int index) => id = index;
}

public class GridContext : FancyGridViewContext
{
    public int SelectedIndex = -1;
    public Action<int> OnCellClicked;
}

public class UIGridCell: FancyGridViewCell<GridItemData, GridContext>  
{
    //[SerializeField] Image image = default;
    [SerializeField] Button button = default;

    public override void Initialize()
    {
        button?.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
    }

    public override void UpdateContent(GridItemData itemData)
    {
        var selected = Context.SelectedIndex == Index;
        //image.color = selected
        //    ? new Color32(0, 255, 255, 100)
        //    : new Color32(255, 255, 255, 77);
    }

    protected override void UpdatePosition(float normalizedPosition, float localPosition)
    {
        base.UpdatePosition(normalizedPosition, localPosition);

        //var wave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * 65;
        //transform.localPosition += Vector3.right * wave;
    }
}