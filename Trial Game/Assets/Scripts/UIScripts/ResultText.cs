using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultText : MonoBehaviour
{
    public TextMeshProUGUI Text;
    [SerializeField] private SceneManager SceneManager;
    public TMP_FontAsset AztecFont;
    public Color AztecColor;
    public int AztecSpacing;
    public TMP_FontAsset SpanishFont;
    public Color SpanishColor;
    public int SpanishSpacing;
    public UITextColorEditor ColorEditor;
    public UITextSizeEditor SizeEditor;
    public UITextMaterialEditor MaterialEditor;
    public string VictoryText;
    public string DefeatText;
    public Enums.PlayerSides sides;
    public bool victory;

    private ResultText_Properties _currText;
    private ResultText_Properties _aztecText;
    private ResultText_Properties _spanishText;

    public void SetupText(Enums.PlayerSides side)
    {
        if ((side == Enums.PlayerSides.Aztec && victory) ||        // If Aztecs win
                  (side == Enums.PlayerSides.Spanish && !victory))    // Or Spanish loose
            _currText = _aztecText;                                 // Use the Aztec style
        else
            _currText = _spanishText;                               // Otherwise use the Spanish one

        Text.font = _currText.Font;
        Text.color = new Color(_currText.Color.r, _currText.Color.g, _currText.Color.b, 0);
    }

    public void Show(bool victory)
    {          // Otherwise use the Spanish one

        Text.text = victory ? VictoryText : DefeatText;
        ColorEditor.UpdateColorEdit(_currText.Color);
        Text.fontStyle = _currText.Style;
        Text.characterSpacing = _currText.TextSpacing;

        if(victory)
        {
            Text.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 1);
            ColorEditor.UpdateSpeed(100);
            ColorEditor.Edit(true);
            MaterialEditor.Edit(true);
        }
        else
        {
            ColorEditor.Edit(true);
            SizeEditor.Edit(true);
        }

        SceneManager.LoadScene("Title Screen");
    }

    private void Awake()
    {
        _aztecText = new ResultText_Properties()
        {
            Font = AztecFont,
            Color = AztecColor,
            Style = FontStyles.Bold,
            TextSpacing = AztecSpacing,
        };
        _spanishText = new ResultText_Properties()
        {
            Font = SpanishFont,
            Color = SpanishColor,
            Style = FontStyles.Bold | FontStyles.Italic | FontStyles.SmallCaps,
            TextSpacing = SpanishSpacing,
        };

        _aztecText.Font.material.SetFloat(ShaderUtilities.ID_GlowPower, 0);
        _spanishText.Font.material.SetFloat(ShaderUtilities.ID_GlowPower, 0);
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    SetupText(sides);
    //    Invoke("Show", 2);
    //}

    //void Show()
    //{
    //    Show(victory);
    //}
}

public struct ResultText_Properties
{
    public TMP_FontAsset Font;
    public Color Color;
    public TMPro.FontStyles Style;
    public int TextSpacing;

    public ResultText_Properties(TMP_FontAsset font, Color color, TMPro.FontStyles style, int spacing)
    {
        this.Font = font;
        this.Color = color;
        this.Style = style;
        this.TextSpacing = spacing;
    }
}
