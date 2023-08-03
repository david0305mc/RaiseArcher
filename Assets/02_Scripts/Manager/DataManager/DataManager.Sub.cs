using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class DataManager 
{
    public void MakeClientDT()
    {

    }

    public ItemLevel GetRandomItem()
    {
        return ItemlevelArray[Random.Range(0, ItemlevelArray.Length)]; 
    }

}
