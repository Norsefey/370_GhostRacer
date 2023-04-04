using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_TargetPoints : MonoBehaviour
{


    //[SerializeField]//manually assigned in editor//do so in sequantial order
    //public GameObject[] ArrayOfArrays;

    public List<GameObject> ArrayOfArrays = new List<GameObject>();

    //for checking when end of array has been reached//done so in NPCManager
    public int AOALength;
    
    // Start is called before the first frame update
    void Start()
    {
        
        //AOALength = ArrayOfArrays.Length;
        AOALength = ArrayOfArrays.Count;
    }

    public void FindPointsInArray(int nextArray, List<GameObject> listOfPoints)
    {
        //check if not empty//if empty do nothing
        if(ArrayOfArrays[nextArray] != null)
        {
            //clear list of old points and assign new points
            listOfPoints.Clear();
            foreach (Transform point in ArrayOfArrays[nextArray].transform)
            {
                listOfPoints.Add(point.gameObject);
            }
            
        }
        else
        {
            Debug.Log("NO More Arrays");
        }

    }
}
