
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.StyleCop
{
	internal partial class HungarianOptionsPanel
	{
		private global::Gtk.VBox vbox4;
		private global::Gtk.Label label6;
		private global::Gtk.Fixed fixed8;
		private global::Gtk.Label label7;
		private global::Gtk.HBox hbox4;
		private global::Gtk.Entry entry4;
		private global::Gtk.Button button4;
		private global::Gtk.Fixed fixed9;
		private global::Gtk.Label label10;
		private global::Gtk.HBox hbox5;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.NodeView nodeview1;
		private global::Gtk.VBox vbox5;
		private global::Gtk.Button button5;
		private global::Gtk.Fixed fixed11;
		private global::Gtk.Fixed fixed10;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.StyleCop.HungarianOptionsPanel
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.StyleCop.HungarianOptionsPanel";
			// Container child MonoDevelop.StyleCop.HungarianOptionsPanel.Gtk.Container+ContainerChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.Xalign = 0F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("Any one or two character prefix not included in the following list will be considered to be invalid Hungarian notation. Add prefixes to this list if they should be ignored by Hungarian notation rules.");
			this.label6.Wrap = true;
			this.label6.Justify = ((global::Gtk.Justification)(3));
			this.label6.WidthChars = 80;
			this.vbox4.Add (this.label6);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.label6]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.fixed8 = new global::Gtk.Fixed ();
			this.fixed8.Name = "fixed8";
			this.fixed8.HasWindow = false;
			this.vbox4.Add (this.fixed8);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.fixed8]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.label7 = new global::Gtk.Label ();
			this.label7.Name = "label7";
			this.label7.Xalign = 0F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Enter a one or two character prefix to add:");
			this.vbox4.Add (this.label7);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.label7]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox ();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.entry4 = new global::Gtk.Entry ();
			this.entry4.CanFocus = true;
			this.entry4.Name = "entry4";
			this.entry4.IsEditable = true;
			this.entry4.InvisibleChar = '●';
			this.hbox4.Add (this.entry4);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.entry4]));
			w4.Position = 0;
			w4.Expand = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.button4 = new global::Gtk.Button ();
			this.button4.CanFocus = true;
			this.button4.Name = "button4";
			this.button4.UseUnderline = true;
			this.button4.Label = global::Mono.Unix.Catalog.GetString ("Add");
			this.hbox4.Add (this.button4);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.button4]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			this.vbox4.Add (this.hbox4);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.hbox4]));
			w6.Position = 3;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.fixed9 = new global::Gtk.Fixed ();
			this.fixed9.Name = "fixed9";
			this.fixed9.HasWindow = false;
			this.vbox4.Add (this.fixed9);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.fixed9]));
			w7.Position = 4;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.label10 = new global::Gtk.Label ();
			this.label10.Name = "label10";
			this.label10.Xalign = 0F;
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString ("Currently excluded prefixes:");
			this.vbox4.Add (this.label10);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.label10]));
			w8.Position = 5;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox ();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.nodeview1 = new global::Gtk.NodeView ();
			this.nodeview1.CanFocus = true;
			this.nodeview1.Name = "nodeview1";
			this.GtkScrolledWindow.Add (this.nodeview1);
			this.hbox5.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.GtkScrolledWindow]));
			w10.Position = 0;
			// Container child hbox5.Gtk.Box+BoxChild
			this.vbox5 = new global::Gtk.VBox ();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			// Container child vbox5.Gtk.Box+BoxChild
			this.button5 = new global::Gtk.Button ();
			this.button5.CanFocus = true;
			this.button5.Name = "button5";
			this.button5.UseUnderline = true;
			this.button5.Label = global::Mono.Unix.Catalog.GetString ("Remove");
			this.vbox5.Add (this.button5);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.button5]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.fixed11 = new global::Gtk.Fixed ();
			this.fixed11.Name = "fixed11";
			this.fixed11.HasWindow = false;
			this.vbox5.Add (this.fixed11);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.fixed11]));
			w12.Position = 1;
			w12.Expand = false;
			w12.Fill = false;
			this.hbox5.Add (this.vbox5);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.vbox5]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.fixed10 = new global::Gtk.Fixed ();
			this.fixed10.Name = "fixed10";
			this.fixed10.HasWindow = false;
			this.hbox5.Add (this.fixed10);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.fixed10]));
			w14.Position = 2;
			this.vbox4.Add (this.hbox5);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.hbox5]));
			w15.Position = 6;
			this.Add (this.vbox4);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
