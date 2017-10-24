using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Contains color constants like UnityEngine.Color, but for _all_ the colors you
  /// can think of. If you can think of a color that doesn't exist here, add it!
  /// 
  /// (Note: This class exists for convenience, not runtime speed.)
  /// </summary>
  public static class LeapColor {

    public static Color black {
      get { return Color.black; }
    }

    public static Color gray {
      get { return Lerp(white, black, 0.5f); }
    }

    public static Color white {
      get { return Color.white; }
    }

    public static Color pink {
      get { return Utils.ParseHtmlColorString("#FFC0CB"); }
    }

    public static Color magenta {
      get { return Color.magenta; }
    }

    /// <summary>
    /// Warning: Not VERY distinct from magenta.
    /// </summary>
    public static Color fuschia {
      get { return Lerp(Color.magenta, Color.blue, 0.1f); }
    }

    public static Color red {
      get { return Color.red; }
    }

    public static Color brown {
      get { return Utils.ParseHtmlColorString("#964B00"); }
    }

    public static Color beige {
      get { return Utils.ParseHtmlColorString("#F5F5DC"); }
    }

    public static Color coral {
      get { return Utils.ParseHtmlColorString("#FF7F50"); }
    }

    public static Color orange {
      get { return Lerp(red, yellow, 0.5f); }
    }

    public static Color khaki {
      get { return Utils.ParseHtmlColorString("	#C3B091"); }
    }

    public static Color amber {
      get { return Utils.ParseHtmlColorString("#FFBF00"); }
    }

    public static Color yellow {
      get { return Color.yellow; }
    }

    public static Color gold {
      get { return Utils.ParseHtmlColorString("#D4AF37"); }
    }

    public static Color green {
      get { return Color.green; }
    }

    public static Color forest {
      get { return Utils.ParseHtmlColorString("#228B22"); }
    }

    public static Color mint {
      get { return Utils.ParseHtmlColorString("#98FB98"); }
    }

    public static Color olive {
      get { return Utils.ParseHtmlColorString("#808000"); }
    }

    public static Color jade {
      get { return Utils.ParseHtmlColorString("#00A86B"); }
    }

    public static Color teal {
      get { return Utils.ParseHtmlColorString("#008080"); }
    }

    public static Color turquoise {
      get { return Utils.ParseHtmlColorString("#40E0D0"); }
    }

    public static Color veridian {
      get { return Utils.ParseHtmlColorString("#40826D"); }
    }

    public static Color cyan {
      get { return Color.cyan; }
    }

    public static Color cerulean {
      get { return Utils.ParseHtmlColorString("#007BA7"); }
    }

    public static Color electricBlue {
      get { return Utils.ParseHtmlColorString("#7DF9FF"); }
    }

    public static Color blue {
      get { return Color.blue; }
    }

    public static Color navy {
      get { return Utils.ParseHtmlColorString("#000080"); }
    }

    public static Color periwinkle {
      get { return Utils.ParseHtmlColorString("#CCCCFF"); }
    }

    public static Color purple {
      get { return Lerp(magenta, blue, 0.3f); }
    }

    public static Color violet {
      get { return Utils.ParseHtmlColorString("#7F00FF"); }
    }

    public static Color lavender {
      get { return Utils.ParseHtmlColorString("#B57EDC"); }
    }

    #region Shorthand

    private static Color Lerp(Color a, Color b, float amount) {
      return Color.Lerp(a, b, amount);
    }

    #endregion

  }

}