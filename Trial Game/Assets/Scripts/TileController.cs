using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileController : TileBase
{
    public Sprite Sprite;
    public Tile.ColliderType ColliderType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public class TilingRule
    {
        public Neighbor[] m_Neighbors;
        public Sprite[] m_Sprites;
        public float m_AnimationSpeed;
        public float m_PerlinScale;
        public AutoTransform m_AutoTransform;
        public OutputSprite m_Output;
        public Tile.ColliderType m_ColliderType;

        public TilingRule()
        {
            m_Output = OutputSprite.Single;
            m_Neighbors = new Neighbor[8];
            m_Sprites = new Sprite[1];
            m_AnimationSpeed = 1f;
            m_PerlinScale = 0.5f;
            m_ColliderType = Tile.ColliderType.None;

            for (int i = 0; i < m_Neighbors.Length; i++)
                m_Neighbors[i] = Neighbor.DontCare;
        }

        public enum AutoTransform { Fixed, Rotated, MirrorX, MirrorY }
        public enum Neighbor { DontCare, This, NotThis }
        public enum OutputSprite { Single, Random, Animation }
    }

    public List<TilingRule> TilingRules;

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
    }
}
