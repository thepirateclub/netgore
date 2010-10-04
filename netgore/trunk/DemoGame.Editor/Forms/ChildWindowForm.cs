﻿using System;
using System.ComponentModel;
using System.Linq;
using NetGore.EditorTools.Docking;

namespace DemoGame.Editor
{
    public partial class ChildWindowForm : DockContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildWindowForm"/> class.
        /// </summary>
        public ChildWindowForm()
        {
            HideOnClose = true;

            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="Editor.ToolBarVisibility"/> for this form. Use <see cref="Editor.ToolBarVisibility.Global"/>
        /// to not set any special visibility.
        /// Default value is <see cref="Editor.ToolBarVisibility.Global"/>.
        /// </summary>
        [Description("The ToolBarVisibility for this form. Use Global to not set any special visibility.")]
        [DefaultValue(ToolBarVisibility.Global)]
        [Browsable(true)]
        public ToolBarVisibility ToolBarVisibility { get; set; }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            ToolBar.CurrentToolBarVisibility = ToolBarVisibility;
        }
    }
}