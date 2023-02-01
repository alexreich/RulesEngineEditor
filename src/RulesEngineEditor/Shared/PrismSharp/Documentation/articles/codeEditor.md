# Implementing code editor

The easiest (and limited) way to implement a code editor is to use a RichBox component in WinForms or WPF and replace the content by a highlighted RTF every time the text in the control changes. Something like this:

```csharp
public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(int hWnd, int nBar);

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, System.Windows.Forms.Orientation nBar, int nPos, bool bRedraw);

        private const uint WM_VSCROLL = 0x0115;
        private const uint WM_HSCROLL = 0x0114;
        private const int SB_THUMBPOSITION = 4;
        private const int WM_USER = 0x0400;
        private const int EM_SETEVENTMASK = (WM_USER + 69);
        private const int WM_SETREDRAW = 0x0b;
        private int OldEventMask;

        private readonly RtfHighlighter highlighter;

        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, 0, 0);
            OldEventMask = SendMessage(this.Handle, EM_SETEVENTMASK, 0, 0);
        }

        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, 1, 0);
            SendMessage(this.Handle, EM_SETEVENTMASK, 0, OldEventMask);
            editor.Invalidate();
        }

        public Form1()
        {
            InitializeComponent();

            highlighter = new RtfHighlighter(new Orionsoft.PrismSharp.Tokenizing.Tokenizer(), Theme.Load(ThemeNames.Vs))
            {
                Font = "Consolas"
            };
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            var vertPos = GetScrollPos((int)editor.Handle, (int)Orientation.Vertical);
            var horizPos = GetScrollPos((int)editor.Handle, (int)Orientation.Horizontal);
            BeginUpdate();

            int i = editor.SelectionStart;
            var hl = highlighter.Highlight(editor.Text + "\n", "csharp");

            var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(hl));
            this.editor.LoadFile(stream, RichTextBoxStreamType.RichText);

            SendMessage(editor.Handle, WM_VSCROLL, vertPos << 16 | SB_THUMBPOSITION, 0);
            SendMessage(editor.Handle, WM_HSCROLL, horizPos << 16 | SB_THUMBPOSITION, 0);
            SetScrollPos(editor.Handle, Orientation.Vertical, vertPos, true);
            SetScrollPos(editor.Handle, Orientation.Horizontal, horizPos, true);
            editor.SelectionStart = i;

            EndUpdate();
        }
    }
}
```


However, this works fine only with shorter texts, because RichBox (especially WPF RichBox) gets pretty slow with long texts.

If you need a code editor working well with long source codes, you need to create a custom control that renders (and tokenizes) only the visible part of the code. You can use `TokenizeRange` or `HighlightRange` methods for this purpose. Still, implementing of the custom control must be done. A good starter could be the [CodeBox 2](https://www.codeproject.com/Articles/35413/CodeBox-2-An-Extended-and-Improved-Version-of-the) that handles this issue.
