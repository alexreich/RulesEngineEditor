# Implementing custom highlighter

Implementing a highlighter converting the code to any output format you need is quite simple. Just derive a class from  `AbstractHighlighter` and implement a few methods called from the highlighter. It provides the text that should be written to the output together with a `ThemeStyle` object containing the styling of this text.

You always need to implement `BeginDocument()` as it should initialize the output and `EndDocument()` that should put the output to the `Result` property being returned by the `Highlight` methods. `AddSpan()` is called with every piece of text to output, together with the style.


For a full example see FlatHighlighter and TreeHighlighter classes in the source.