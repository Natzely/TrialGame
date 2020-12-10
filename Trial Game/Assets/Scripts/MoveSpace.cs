
public class MoveSpace : GridSpace
{
    public override Enums.Player Player
    {
        get { return _player; }
        set
        {
            _player = value;
            switch (_player)
            {
                case Enums.Player.Player2:
                    _sR.color = Colors.Player2;
                    break;
                default:
                    _sR.color = Colors.Player1;
                    break;
            }
        }
    }
}
