using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public GameObject MoveSpace;
    public int MovementCost = 0;
    public bool Unpassable = false;

    public void CreateMoveSpace(Enums.Player player)
    {
       var gO = Instantiate(MoveSpace, transform.position, Quaternion.identity);
        MoveSpace mS = gO.GetComponent<MoveSpace>();
        mS.Player = player;
    }
}
