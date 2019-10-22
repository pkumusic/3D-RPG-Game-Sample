using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{

    #region Singleton
    public static EquipmentManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of EquipmentManager found! Broken singleton.");
        }
        instance = this;
    }
    #endregion
    Inventory inventory;

    Equipment[] currentEquipment;
    public SkinnedMeshRenderer targetMesh;
    SkinnedMeshRenderer[] currentMeshes;

    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
    public OnEquipmentChanged onEquipmentChangedCallback;

    // Start is called before the first frame update
    void Start()
    {
        inventory = Inventory.instance;
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new Equipment[numSlots];
        currentMeshes = new SkinnedMeshRenderer[numSlots];
    }

    public void Equip(Equipment newEquipment)
    {
        int slotIndex = (int)newEquipment.equipmentSlot;
        Equipment oldEquipment = null;
        if (currentEquipment[slotIndex] != null)
        {
            oldEquipment = currentEquipment[slotIndex];
            inventory.Add(oldEquipment);
        }
        if(onEquipmentChangedCallback != null)
        {
            onEquipmentChangedCallback.Invoke(newEquipment, oldEquipment);
        }
        currentEquipment[slotIndex] = newEquipment;
        SetEquipmentBlendShapes(newEquipment, 100);

        SkinnedMeshRenderer newMesh = Instantiate<SkinnedMeshRenderer>(newEquipment.mesh);
        newMesh.transform.parent = targetMesh.transform;
        newMesh.bones = targetMesh.bones;
        currentMeshes[slotIndex] = newMesh;
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            if(currentMeshes[slotIndex] != null)
            {
                Destroy(currentMeshes[slotIndex].gameObject);
            }

            Equipment oldEquipment = currentEquipment[slotIndex];
            SetEquipmentBlendShapes(oldEquipment, 0);
            inventory.Add(oldEquipment);
            currentEquipment[slotIndex] = null;
            if (onEquipmentChangedCallback != null)
            {
                onEquipmentChangedCallback.Invoke(null, oldEquipment);
            }
        }
    }

    public void UnequipAll()
    {

        for (int i=0; i<currentEquipment.Length; i++)
        {
            Unequip(i);
        }
    }

    void SetEquipmentBlendShapes(Equipment item, int weight)
    {
        foreach(EquipmentMeshRegion blendShape in item.coveredMeshRegion)
        {
            targetMesh.SetBlendShapeWeight((int)blendShape, weight);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("UnEquiptAll")){
            UnequipAll();
        }
    }
}
