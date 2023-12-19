using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OOSCIMessage
{

    //---------These parameters are always present-----------//
    public string _MESSAGE_ID;
    public string _MESSAGE_HANDLE;
    public string recipient;
    public int timestamp;
    public string sender;
    //-------------------------------------------------------//
    //---Make sure these are updated to match your message---//
    public float rotation;
    //-----------------------------------------------------//
}

