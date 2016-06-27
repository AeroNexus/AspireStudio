using System;

namespace Aspire.CoreModels
{
    /// <summary>
    /// Base class for <see cref="System.Drawing.Design.UITypeEditor"/> classes that select items from a list box.
    /// </summary>
    public class DRangeTypeEditor : System.Drawing.Design.UITypeEditor
    {
        // picked-out
        System.Windows.Forms.ListBox Box1 = new System.Windows.Forms.ListBox();

        System.Windows.Forms.Design.IWindowsFormsEditorService edSvc;

        /// <summary>
        /// Gets the object being edited
        /// </summary>
        protected object EditorObject { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DRangeTypeEditor()
        {
            Box1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            // add event handler for drop-down box when item 
            // will be selected
            Box1.Click += new EventHandler(Box1_Click);
        }

        /// <summary>
        /// Gets the style of the editor
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Displays the UI for value selection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue
            (System.ComponentModel.ITypeDescriptorContext
            context, System.IServiceProvider provider,
            object value)
        {

            Box1.Items.Clear();
            EditorObject = value;

            PopulateListbox(Box1, context);

            Box1.Height = Box1.ItemHeight * 5;

            // Set Width of List Box to accomodate longest Log Listener value.FB288.dcw
            //Example from http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c87c.aspx#q651q
            using (System.Drawing.Graphics g = Box1.CreateGraphics())
            {
                float maxWidth = 0f;
                float height = 0f;

                for (int i = 0; i < Box1.Items.Count; ++i)
                {
                    float w = g.MeasureString(Box1.Items[i].ToString(), Box1.Font).Width;
                    if (w > maxWidth)
                        maxWidth = w;
                    height += Box1.GetItemHeight(i);
                }
                Box1.Width = (int)(maxWidth + 6 + ((height > Box1.Height - 4) ? 16 : 0)); // 16 is scrollbar width
            }

            // Uses the IWindowsFormsEditorService to 
            // display a drop-down UI in the Properties 
            // window.

            edSvc =
                (System.Windows.Forms.Design.IWindowsFormsEditorService)provider.
                GetService(typeof
                (System.Windows.Forms.Design.IWindowsFormsEditorService));
            if (edSvc != null)
            {
                edSvc.DropDownControl(Box1);
                if (Box1.SelectedIndex != -1)
                {
                    if (Box1.SelectedItem is Xteds.Interface.Option)
                    {
                        return (Box1.SelectedItem as Xteds.Interface.Option).Value;
                    }

                    return Box1.SelectedItem;
                }
                return value;
            }
            return value;
        }

        private void Box1_Click(object sender, EventArgs e)
        {
            edSvc.CloseDropDown();
        }

        /// <summary>
        /// Populate the listbox. Override if you don't want to populate with strings.
        /// </summary>
        /// <param name="listBox"></param>
        protected virtual void PopulateListbox(System.Windows.Forms.ListBox listBox, System.ComponentModel.ITypeDescriptorContext
            context)
        {
            if (context.Instance is Xteds.Interface.VariableValue)
            {
                Xteds.Interface.VariableValue var = context.Instance as Xteds.Interface.VariableValue;
                if (var.Variable.Drange != null)
                {
                    listBox.Items.AddRange(var.Variable.Drange.Options);
                }
            }
            else
            {
                string[] items = GetItemStrings();
                Array.Sort(items);

                listBox.Items.AddRange(items);
            }
        }

        /// <summary>
        /// Gets the strings to add to the list box
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetItemStrings()
        {
            return new string[0];
        }
    }


}
