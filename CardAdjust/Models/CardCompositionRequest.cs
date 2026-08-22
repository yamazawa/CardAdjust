using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CardAdjust.Models;

/// <summary>
/// CardCompositionService.Composeへ渡す入力パラメータ
///
/// 個別調整(CharacterStyleOverride)は呼び出し側で解決済みのCharacterStyleとして渡す。
/// ①～④の貼付先矩形(TitleRect等)は、個別設定が無ければCardTemplateLayoutの全体設定を渡す。
/// </summary>
public sealed class CardCompositionRequest
{
    public required BitmapImage FrameTemplate { get; init; }

    // タイトル・説明文・水平線の文字色。カードセット(使用カード=白、場のカード=黒)によって異なる。
    public Brush Foreground { get; init; } = Brushes.White;

    public string TitleText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> TitleCharacterStyles { get; init; }
    public IReadOnlyList<CharacterStyleOverride> TitleOverrides { get; init; } = [];
    public Rect TitleRect { get; init; } = CardTemplateLayout.TitleRect;

    public BitmapSource? IllustrationImage { get; init; }
    public bool KeepIllustrationAspectRatio { get; init; }
    public Rect IllustrationRect { get; init; } = CardTemplateLayout.IllustrationRect;

    public string DescriptionText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> DescriptionCharacterStyles { get; init; }
    public IReadOnlyList<CharacterStyleOverride> DescriptionOverrides { get; init; } = [];
    public double DescriptionLineSpacing { get; init; }
    public Rect DescriptionRect { get; init; } = CardTemplateLayout.DescriptionRect;

    // trueの場合、個別調整による上書きが適用されている文字を色分け表示し、
    // どの区間が何番の個別設定かを示す番号付きの矩形を重ねて描画する(プレビュー専用。保存・一斉出力ではfalseを渡す)。
    public bool HighlightOverrides { get; init; }
}
