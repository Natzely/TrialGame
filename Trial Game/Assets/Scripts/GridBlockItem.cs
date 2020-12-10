using UnityEngine;

public class GridBlockItem : MonoBehaviour
{
    public GridBlock ParentGridBlock
    {
        get; set;
    }

    public bool Active
    {
        get { return gameObject.activeSelf; }
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}

