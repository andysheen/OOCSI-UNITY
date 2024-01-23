using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OOCSIMessage
{

    //---------These parameters are always present-----------//
    public string recipient;
    public long timestamp;
    public string sender;
    public string text;
    //-------------------------------------------------------//
    //---Make sure these are updated to match your message---//
    public float rotation;
    //-----------------------------------------------------//
}

