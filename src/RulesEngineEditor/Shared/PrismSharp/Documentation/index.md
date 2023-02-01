# PrismSharp Library

PrismSharp is a syntax highlighting library based on an excellent javascript library [PrismJS](https://prismjs.com/), fully written in C#. It currently supports over 270 programming languages and has 44 built in visual themes, also supporting custom themes.


# Main Components

- **Tokenizer** - engine converting a source code to a tree of tokens - pairs of code fragments and their meanings, e.g. `foreach - keyword`

- **Abstract Highlighter** - an abstract base class making it easy to [implement a custom highlighter](articles/customHighlighter.md) creating virtually any output format

- **HTML Highlighter** - a highlighter creating output in html format (HTML `span`s or a single `pre` block). A PrismJS CSS theme must be included in the html document to render the output correctly

- **RTF Highlighter** - a highlighter creating output in RTF format. Can be used in GUI components like RichBox. See [Implementing code editor](articles/codeEditor.md)

The tokenizer and the highlighters support tokenizing (highlighting) of the entire code or just a range of it, when high performance is needed.

# Basic Usage

## HTMl highlighting

```csharp
            var code = "Console.WriteLine(\"Hello, World!\"); // demo";
            var beginning = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\">" +
                "<link href=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.27.0/themes/prism.min.css\" rel=\"stylesheet\"/</head><body>";
            var ending  = "</body></html>";

            var highlighter = new HtmlHighlighter();
            highlighter.WrapByPre = true;
            var res = highlighter.Highlight(code, "csharp");

            File.WriteAllText("output.html", beginning + res + ending);

```

## RTF highlighting

```csharp
 var code = "Console.WriteLine(\"Hello, World!\"); // demo";

            var highlighter = new RtfHighlighter(ThemeNames.Vs);
            highlighter.Font = "Consolas";
            var res = highlighter.Highlight(code, "csharp");

            File.WriteAllText("output.rtf", res);
```
# Installation

The library is available via Nuget