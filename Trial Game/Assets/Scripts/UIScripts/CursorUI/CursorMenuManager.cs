using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CursorMenuManager : MonoBehaviour
{
    public Enums.CursorMenuState State;
    public bool Active { get; private set; }

    public bool IsPanelSelected { get { return _curPanel != null; } }
    public CursorPanel SelectedPanel { get { return _curPanel; } }
    public int VisiblePanels { get { return _panels.Where(panel => panel.Showing).ToList().Count; } }

    private List<CursorPanel> _panels;
    private CursorPanel _curPanel;

    public void SelectNextAvailablePanel(int dir)
    {
        var visPanels = _panels.Where(panel => panel.Showing).ToList();
        if (!_curPanel)
            SelectFirstPanel();

        var newPanel = visPanels.NextAfter(_curPanel, dir);
        if (newPanel != _curPanel)
        {
            _curPanel.Select = false;
            _curPanel = newPanel;
            _curPanel.Select = true;
        }
    }

    public void SelectFirstPanel()
    {
        _curPanel = _panels.FirstOrDefault(panel => panel.Showing);
        if (_curPanel && _panels.Count > 1)
            _curPanel.Select = true;
        Active = true;
    }

    public void ResetPanels()
    {
        _panels.ForEach(p => p.Select = false);
        _curPanel = null;
        Active = false;
    }

    private void Awake()
    {
        _panels = new List<CursorPanel>();

    }

    // Start is called before the first frame update
    void Start()
    {
        _panels = GetComponentsInChildren<CursorPanel>().ToList();
    }
}
