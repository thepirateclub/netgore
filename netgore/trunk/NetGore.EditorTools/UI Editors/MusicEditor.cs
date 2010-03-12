using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using NetGore.Audio;

namespace NetGore.EditorTools
{
    /// <summary>
    /// A <see cref="UITypeEditor"/> for selecting the <see cref="IMusic"/>.
    /// </summary>
    public class MusicEditor : UITypeEditor
    {
        /// <summary>
        /// Edits the specified object's value using the editor style indicated by the
        /// <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> method.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be
        /// used to gain additional context information.</param>
        /// <param name="provider">An <see cref="T:System.IServiceProvider"/> that this editor can use to
        /// obtain services.</param>
        /// <param name="value">The object to edit.</param>
        /// <returns>
        /// The new value of the object. If the value of the object has not changed, this should return the
        /// same object it was passed.
        /// </returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (svc != null)
            {
                if (context.PropertyDescriptor.PropertyType == typeof(string))
                {
                    using (var editorForm = new MusicUITypeEditorForm(null, value as string))
                    {
                        if (svc.ShowDialog(editorForm) != DialogResult.OK)
                        {
                            value = editorForm.SelectedItem.Name;
                        }
                    }
                }
                else if (context.PropertyDescriptor.PropertyType == typeof(IMusic))
                {
                    using (var editorForm = new MusicUITypeEditorForm(null, value as IMusic))
                    {
                        if (svc.ShowDialog(editorForm) != DialogResult.OK)
                        {
                            value = editorForm.SelectedItem;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the editor style used by the
        /// <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used
        /// to gain additional context information.</param>
        /// <returns>
        /// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"/> value that indicates the style of editor
        /// used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/>
        /// method. If the <see cref="T:System.Drawing.Design.UITypeEditor"/> does not support this method,
        /// then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> will return
        /// <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"/>.
        /// </returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}