#nullable enable
namespace murrty.controls;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
/// <summary>
/// An enumeration of types of characters allowed in the textbox.
/// </summary>
public enum AllowedCharacters {
    /// <summary>
    /// All characters are allowed.
    /// </summary>
    All,
    /// <summary>
    /// Only Upper and lowercase alphabetical letters are allowed.
    /// </summary>
    AlphabeticalOnly,
    /// <summary>
    /// Only numbers are allowed.
    /// </summary>
    NumericOnly,
    /// <summary>
    /// Only letters and numbers are allowed.
    /// </summary>
    AlphaNumericOnly
}

/// <summary>
/// An enumeration of the alignments that the button can be aligned to.
/// </summary>
public enum ButtonAlignment {
    /// <summary>
    /// The Button will appear on the left side of the TextBox.
    /// </summary>
    Left,
    /// <summary>
    /// The Button will appear on the right side of the TextBox. Default value.
    /// </summary>
    Right,
}

/// <summary>
/// An extension of Windows.Forms.TextBox to include extra functionality.
/// </summary>
public class ExtendedTextBox : TextBox {
    #region Fields
    /// <summary>
    /// The text hint.
    /// </summary>
    private string _TextHint = string.Empty;
    /// <summary>
    /// If the button is enabled.
    /// </summary>
    private bool _ButtonEnabled;
    /// <summary>
    /// The alignment of the button.
    /// </summary>
    private ButtonAlignment _ButtonAlignment = ButtonAlignment.Left;
    /// <summary>
    /// If the font should be syncronized across the button and text.
    /// </summary>
    private bool _SyncFont;
    /// <summary>
    /// If the text is currently changing.
    /// </summary>
    private bool Changing;

    /// <summary>
    /// The button that appears inside the textbox.
    /// </summary>
    private readonly Button InsetButton = new() {
        Cursor = Cursors.Default,
        Enabled = false,
        TextAlign = ContentAlignment.MiddleCenter,
        UseVisualStyleBackColor = true,
        Visible = false,
    };
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the ButtonAlignment of the button.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(ButtonAlignment.Right)]
    [Description("The position of the button inside the Text Box.")]
    public ButtonAlignment ButtonAlignment {
        get { return _ButtonAlignment; }
        set {
            _ButtonAlignment = value;
            UpdateButton();
            this.Refresh();
        }
    }

    /// <summary>
    /// Gets or sets the cursor of the button.
    /// </summary>
    [Category("Appearance")]
    [Description("The cursor that will appear when hovering over the Button.")]
    public Cursor ButtonCursor {
        get { return InsetButton.Cursor; }
        set { InsetButton.Cursor = value; }
    }

    /// <summary>
    /// Gets or sets the text font of the button.
    /// </summary>
    [Category("Appearance")]
    [Description("The Font of the text that appears within the Button.")]
    public Font ButtonFont {
        get { return InsetButton.Font; }
        set {
            InsetButton.Font = value;
            if (_SyncFont) {
                base.Font = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the image in the button.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("The Image that appears on the Button.")]
    public Image ButtonImage {
        get { return InsetButton.Image; }
        set { InsetButton.Image = value; }
    }

    /// <summary>
    /// Gets or sets the image alignment of the buttons' image.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleCenter)]
    [Description("The Image Alignment of an Image on the Button.")]
    public ContentAlignment ButtonImageAlign {
        get { return InsetButton.ImageAlign; }
        set { InsetButton.ImageAlign = value; }
    }

    /// <summary>
    /// Gets or sets the image index of the buttons' image key or image list.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("The Image Index of the Image on the Button within the Image List.")]
    public int ButtonImageIndex {
        get { return InsetButton.ImageIndex; }
        set { InsetButton.ImageIndex = value; }
    }

    /// <summary>
    /// Gets or sets the buttons' image key.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("The Image Key of the Image on the Button.")]
    public string ButtonImageKey {
        get { return InsetButton.ImageKey; }
        set { InsetButton.ImageKey = value; }
    }

    /// <summary>
    /// Gets or sets the buttons' image list.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("The Image List for use with the Button.")]
    public ImageList ButtonImageList {
        get { return InsetButton.ImageList; }
        set { InsetButton.ImageList = value; }
    }

    /// <summary>
    /// Gets or sets the size of the button.
    /// </summary>
    [Category("Appearance")]
    [Description("The Size of the Button.")]
    public Size ButtonSize {
        get { return InsetButton.Size; }
        set { InsetButton.Size = value; }
    }

    /// <summary>
    /// Gets or sets the text of the button.
    /// </summary>
    [Category("Appearance")]
    [Description("The text that appears on the Button.")]
    public string ButtonText {
        get { return InsetButton.Text; }
        set { InsetButton.Text = value; }
    }

    /// <summary>
    /// Gets or sets the text alignment of the buttons' text.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleRight)]
    [Description("The Alignment of the text on the Button.")]
    public ContentAlignment ButtonTextAlign {
        get { return InsetButton.TextAlign; }
        set { InsetButton.TextAlign = value; }
    }

    /// <summary>
    /// Gets or sets the bool of whether the button inside the text box is enabled and can be used.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(false)]
    [Description("The Button on the TextBox is enabled and usable.")]
    public bool ButtonEnabled {
        get { return _ButtonEnabled; }
        set {
            InsetButton.Visible = value;
            InsetButton.Enabled = value;
            _ButtonEnabled = value;
        }
    }

    /// <summary>
    /// Gets or sets the Font of the TextBox.
    /// </summary>
    [Category("Appearance")]
    [Description("The Font of the text that appears within the TextBox.")]
    public new Font Font {
        get {
            return base.Font;
        }
        set {
            base.Font = value;
            if (_SyncFont) {
                InsetButton.Font = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the bool whether the font should be in sync between the TextBox and Button.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(false)]
    [Description("Whether the font on the button and text box should be in sync. Text box takes precedent, changing either updates the other.")]
    public bool SyncronizeFont {
        get {
            return _SyncFont;
        }
        set {
            _SyncFont = value;
            if (value) {
                InsetButton.Font = base.Font;
            }
        }
    }

    /// <summary>
    /// Gets or sets the hint on the TextBox.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("The Text that will appear as a hint in the Text Box.")]
    public string TextHint {
        get { return _TextHint; }
        set {
            _TextHint = value;
            _ = SendMessage(this.Handle, 0x1501, (IntPtr)1, value);
        }
    }

    /// <summary>
    /// Gets or sets the allowed characters to be typed in the TextBox.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(AllowedCharacters.All)]
    [Description("Determines if the Text Box wil only accept certain kinds of characters.")]
    public AllowedCharacters TextType { get; set; } = AllowedCharacters.All;
    #endregion

    #region Native Methods
    public const int EM_SETMARGINS = 0xd3;
    public const int EC_RIGHTMARGIN = 2;
    public const int EC_LEFTMARGIN = 1;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern nint SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
    internal static extern nint SendMessage(nint hWnd, int wMsg, nint wParam, string lParam);
    #endregion

    #region Constructor
    public ExtendedTextBox() {
        UpdateButton();
        Controls.Add(InsetButton);
        Refresh();
    }
    #endregion

    #region Overrides
    public override void Refresh() {
        switch (_ButtonEnabled) {
            case true:
                _ = _ButtonAlignment switch {
                    ButtonAlignment.Right => SendMessage(Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)(InsetButton.Width)),
                    _ => SendMessage(Handle, EM_SETMARGINS, (IntPtr)EC_RIGHTMARGIN, (IntPtr)(InsetButton.Width << 16)),
                };
                break;

            case false:
                _ = SendMessage(Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, IntPtr.Zero);
                _ = SendMessage(Handle, EM_SETMARGINS, (IntPtr)EC_RIGHTMARGIN, IntPtr.Zero);
                break;
        }
        base.Refresh();
    }

    protected override void OnResize(EventArgs e) {
        UpdateButton();
        Refresh();
        base.OnResize(e);
    }

    protected override void OnTextChanged(EventArgs e) {
        if (!Changing) {
            base.OnTextChanged(e);
            Changing = true;
            var cursorPosition = this.SelectionStart;
            switch (this.TextType) {
                case AllowedCharacters.AlphabeticalOnly:
                    this.Text = Regex.Replace(this.Text, "[^a-zA-Z ]", "");
                    break;

                case AllowedCharacters.NumericOnly:
                    this.Text = Regex.Replace(this.Text, "[^0-9 ]", "");
                    break;

                case AllowedCharacters.AlphaNumericOnly:
                    this.Text = Regex.Replace(this.Text, "[^0-9a-zA-Z ]", "");
                    break;
            }
            this.SelectionStart = cursorPosition;
            Changing = false;
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// Event raised when the Button in the TextBox is clicked.
    /// </summary>
    public event EventHandler ButtonClick {
        add { InsetButton.Click += value; }
        remove { InsetButton.Click -= value; }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the button to fix appearance issues when size or alignment changes.
    /// </summary>
    private void UpdateButton() {
        InsetButton.Size = new Size(22, ClientSize.Height + 3);
        InsetButton.Location = _ButtonAlignment switch {
            ButtonAlignment.Right => new(0, -2),
            _ => new(ClientSize.Width - InsetButton.Width, -2),
        };
    }
    #endregion
}