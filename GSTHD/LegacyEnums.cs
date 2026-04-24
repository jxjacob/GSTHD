using Avalonia.Media;

namespace GSTHD;

public enum PictureBoxSizeMode
{
    Normal,
    StretchImage,
    AutoSize,
    CenterImage,
    Zoom
}

public enum BorderStyle
{
    None,
    FixedSingle,
    Fixed3D
}

public enum ContentAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    Underline,
    Strikeout
}

public struct FontData(string fontName, int fontSize, FontStyle fontStyle)
{
    public string fontName = fontName;
    public int fontSize = fontSize;
    public FontStyle fontStyle =  fontStyle;
}

public static class LegacyConverters
{
    public static TextAlignment ConvetTA(ContentAlignment value)
    {
        switch (value)
        {
            case ContentAlignment.TopRight: case ContentAlignment.MiddleRight: case ContentAlignment.BottomRight:
                return TextAlignment.Right;
            case ContentAlignment.TopCenter: case ContentAlignment.MiddleCenter: case ContentAlignment.BottomCenter:
                return TextAlignment.Center;
            case ContentAlignment.TopLeft: case ContentAlignment.MiddleLeft: case ContentAlignment.BottomLeft: default:
                return TextAlignment.Left;
        }
    }

    public static SolidColorBrush ConvetToBrush(System.Drawing.Color value)
    {
        return new SolidColorBrush(new Color(value.A, value.R, value.G, value.B));
    }
    
    public static Color ConvetToColor(System.Drawing.Color value)
    {
        return new Color(value.A, value.R, value.G, value.B);
    }
}