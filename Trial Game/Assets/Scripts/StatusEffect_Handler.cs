using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect_Handler : MonoBehaviour
{
    [SerializeField] private ParticleSystem RageSmoke;
    
    public Enums.UnitStatusEffect Statuses { get { return _statuses; } }

    private Enums.UnitStatusEffect _statuses;
    private SpriteRendererColorEditor _colorEditor;

    public void GiveStatus(Enums.UnitStatusEffect status)
    {
        StartStatus(status);
    }

    public void ClearStatus(Enums.UnitStatusEffect status)
    {
        CleanseStatus(status);
    }

    public void ClearAllStatuses()
    {
        _statuses = Enums.UnitStatusEffect.None;
        _colorEditor.Edit(false);
        RageSmoke.Stop();
    }

    private void Awake()
    {
        _colorEditor = GetComponent<SpriteRendererColorEditor>();
    }

    private void StartStatus(Enums.UnitStatusEffect status)
    {
        _statuses = status;
        if(status.HasFlag(Enums.UnitStatusEffect.Rage))
        {
            _colorEditor.Edit();
            RageSmoke.Play();
        }    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        StatusEffect_Giver sEG = colObj.GetComponent<StatusEffect_Giver>();
        if(sEG)
        {
            StartStatus(sEG.UnitStatusEffects);
        }

    }

    private void CleanseStatus(Enums.UnitStatusEffect status)
    {
        if(status.HasFlag(Enums.UnitStatusEffect.Rage))
        {
            _colorEditor.Edit(false);
            RageSmoke.Stop();
        }

        _statuses &= ~status;
    }
}
