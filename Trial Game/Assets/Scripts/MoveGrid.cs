using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveGrid : MonoBehaviour
{
    public Enums.Player Player;
    public GameObject MoveSpace;
    public int MaxMove;
    
    List<MoveSpace> _moveSpaces;
    float _currX, _currY, _orgX, _orgY;
    float _timer, _actionTimer;
    float WaitTimer = 1f;

    private void Awake()
    {
        _moveSpaces = new List<MoveSpace>();
        //Instantiate(MoveSpace, transform.position, Quaternion.identity, transform);
        _currX = 0;
        _orgX = transform.position.x;
        _currY = 1;
        _orgY = transform.position.y;
        CreateMoveSpace(_orgX, _orgY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateGrid()
    {

        while ((_currX < MaxMove))// || _currY - transform.position.y < MaxMove))// && _timer <= 0)
        {
            if (_currY > MaxMove || _currY + _currX > MaxMove)
            {
                _currY = _orgY;
                _currX++;
            }

            CreateMoveSpace(_orgX + _currX, _orgY + _currY);
            CreateMoveSpace(_orgX - _currX, _orgY + _currY);
            CreateMoveSpace(_orgX + _currX, _orgY - _currY);
            CreateMoveSpace(_orgX - _currX, _orgY - _currY);

            _currY++;
        }
    }

    private void CreateMoveSpace(float x, float y)
    {
        Vector2 pos = new Vector2(x, y);
        if (!_moveSpaces.Any(mS => mS.Position == pos))
        {
            string name = "MoveSpace" + _moveSpaces.Count;
            var newObject = Instantiate(MoveSpace, pos, Quaternion.identity, transform);
            var newSpace = newObject.GetComponent<MoveSpace>();
            newSpace.Player = Player;
            newObject.name = name;
            _moveSpaces.Add(newSpace);
        }
    }
}
