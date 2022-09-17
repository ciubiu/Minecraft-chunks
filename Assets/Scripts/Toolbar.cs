using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    private World world;
    public Player player;

    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    private int slotIndex = 0;

    private void Start()
    {
        world = GameObject.Find("VoxelWorld").GetComponent<World>();

        foreach (ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = world.blockTypes[(ushort)slot.itemID].icon;
            slot.icon.enabled = true;

        }
        // player starts with visually selected block
        player.selectedBlockType = (ushort)itemSlots[slotIndex].itemID;
    }

    private void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        // scrollwheel not in standby
        if (scroll != 0)
        {
            // moving up
            if (scroll > 0)
                slotIndex--;
            // moving down
            else
                slotIndex++;
        }
        
        // make sure its not going over
        if (slotIndex > itemSlots.Length - 1)
            slotIndex = 0;
        if (slotIndex < 0)
            slotIndex = itemSlots.Length -1;

        highlight.position = itemSlots[slotIndex].icon.transform.position;
        player.selectedBlockType = (ushort)itemSlots[slotIndex].itemID;
    }
}

[System.Serializable]
public class ItemSlot
{
    public World.Block itemID;
    public Image icon;
}
