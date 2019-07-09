using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Common;
using UnityEngine.EventSystems;

public sealed class GNewDropDown : Dropdown
{
    [NoToLua]
    public delegate void ItemDelegate(GameObject list, GameObject item, int index);
    [NoToLua]
    public ItemDelegate OnInitItem;

    public override void OnPointerClick(PointerEventData eventData)
    {
        Transform dropdownT = transform.Find("Dropdown List");

        Show();

        if (!IsActive() || !IsInteractable() || dropdownT != null)
            return;

        dropdownT = transform.Find("Dropdown List");
        if (dropdownT != null && OnInitItem != null)
        {
            DropdownItem[] items = dropdownT.GetComponentsInChildren<DropdownItem>();
            for (int i = 0; i < items.Length; i++)
            {
                DropdownItem item = items[i];
                string s_n = item.name;

                int id = 0;
                s_n = s_n.Replace("item ", "");
                int pos_end = s_n.IndexOf(':');
                if (pos_end > -1)
                {
                    s_n = s_n.Substring(0, pos_end);
                    int.TryParse(s_n, out id);
                }
                OnInitItem(gameObject, item.gameObject, id);
            }

            //OnInitItems(dropdownT.gameObject);
        }
    }
}
