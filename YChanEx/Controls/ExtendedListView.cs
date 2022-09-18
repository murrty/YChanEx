namespace murrty.controls;

using System.Windows.Forms;

internal class ExtendedListView : ListView {
    protected override void OnHandleCreated(EventArgs e) {
        NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
        base.OnHandleCreated(e);
    }
    public string GetColumnWidths() {
        System.Text.StringBuilder Output = new();
        for (int i = 0; i < Columns.Count; i++)
            Output.Append(Columns[i].Width + ",");
        return Output.Remove(Output.Length - 1, 1).ToString();
    }
    public void SetColumnWidths(string ColumnSizes) {
        string[] Values = ColumnSizes.Split(',');

        if (Values.Length > this.Columns.Count)
            Array.Resize(ref Values, this.Columns.Count);

        for (int i = 0; i < Values.Length; i++) {
            if (int.TryParse(Values[i], out int Width))
                this.Columns[i].Width = Width;
        }
    }
}