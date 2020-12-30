using UnityEngine;

public class Slot : CollectionComponent<Slot>
{
    [HideInInspector]
    public Vector2 coord;

    public Cell MyCell { get; set; }

    public void AttachCell(Slot target)
    {
        MyCell = target.MyCell;
        MyCell.transform.SetParent(transform, true);
        MyCell.transform.localPosition = Vector3.zero;
        MyCell.transform.localScale = Vector3.one;
        MyCell.AttachedSlot = this;

        var rectTransform = MyCell.transform as RectTransform;
        rectTransform.offsetMax = rectTransform.offsetMin = Vector2.zero;

        target.MyCell = null;
    }
}