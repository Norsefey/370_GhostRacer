using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ValueCorrection : MonoBehaviourPunCallbacks
{
    public void CompareAndFixValues(int valueToCompare)
    {
        // Check if this is the host client
        if (PhotonNetwork.IsMasterClient)
        {
            // Update the custom room property with the current value
            Hashtable properties = new Hashtable();
            properties.Add("ValueToCompare", valueToCompare);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
        else
        {
            // Get the current value from the custom room property
            object value;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ValueToCompare", out value))
            {
                // Compare the current value with the local value
                int currentValue = (int)value;
                if (currentValue == valueToCompare)
                {
                    Debug.Log("The values are the same.");
                    value = valueToCompare;
                }
                else
                {
                    Debug.Log("The values are different.");
                    value = valueToCompare;
                }
            }
        }
    }

}


