using System.ComponentModel;

namespace aRevolucaoDosPetWeb.Enums;

public enum ReactionType
{
    LIKE,
    LOVE,
    HAHA,
    WOW,
    [Description("latir - exclusivo para cães")]
    BARK,
    [Description("miar - exclusivo para gatos")]
    MEOW
}
