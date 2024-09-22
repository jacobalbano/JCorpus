using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility;

public class SentenceBreaker
{
    public bool Break(string str)
    {
        foreach (var c in str)
        {
            if (openToClose.TryGetValue(c, out var thisClose))
                stack.Push(thisClose);
            else if (closeToOpen.TryGetValue(c, out var thisOpen))
            {
                if (stack.TryPeek(out var top) && c == top)
                    stack.Pop();
            }
        }

        return !stack.Any() && breakChars.Contains(str[^1]);
    }

    public void Reset()
    {
        stack.Clear();
    }

    public static IEnumerable<string> BreakInitial(string text)
    {
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (breakChars.Contains(text[i]))
            {
                while (++i < text.Length && breakChars.Contains(text[i]))
                    ;

                yield return text[start..i];
                start = i;
            }
        }

        if (start < text.Length)
            yield return text[start..];
    }

    public SentenceBreaker()
    {
        var pairs = "()[]{}“”‘’„„‚‚‹›«»⦅⦆〚〛⦃⦄﹙﹚﹛﹜﹝﹞⸨⸩「」『』〈〉《》【】〖〗〔〕〘〙⦗⦘（）［］｛｝｟｠｢｣"
            .ToCharArray();

        for (int i = 0; i < pairs.Length; i += 2)
        {
            char open = pairs[i], close = pairs[i + 1];
            closeToOpen[close] = open;
            openToClose[open] = close;
        }
    }

    private readonly Stack<char> stack = new();
    private readonly Dictionary<char, char> closeToOpen = new();
    private readonly Dictionary<char, char> openToClose = new();
    private static readonly HashSet<char> breakChars = new("？！。!?");
}
