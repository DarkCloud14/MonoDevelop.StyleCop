
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.StyleCop.Gui.OptionPanelWidgets
{
	public partial class ValidPrefixesOptionsPanelWidget
	{
		private global::Gtk.VBox vbox4;

		private global::Gtk.Label label6;

		private global::Gtk.Fixed fixed8;

		private global::Gtk.Table table5;

		private global::Gtk.Button addPrefixButton;

		private global::Gtk.Entry addPrefixEntry;

		private global::Gtk.Fixed fixed10;

		private global::Gtk.Fixed fixed11;

		private global::Gtk.Fixed fixed12;

		private global::Gtk.Fixed fixed7;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.NodeView prefixNodeView;

		private global::Gtk.Label label3;

		private global::Gtk.VBox vbox3;

		private global::Gtk.Button removePrefixButton;

		private global::Gtk.Fixed fixed13;

		private global::Gtk.Fixed fixed9;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget MonoDevelop.StyleCop.Gui.OptionPanelWidgets.ValidPrefixesOptionsPanelWidget
			global::Stetic.BinContainer.Attach(this);
			this.WidthRequest = 625;
			this.HeightRequest = 495;
			this.Name = "MonoDevelop.StyleCop.Gui.OptionPanelWidgets.ValidPrefixesOptionsPanelWidget";
			// Container child MonoDevelop.StyleCop.Gui.OptionPanelWidgets.ValidPrefixesOptionsPanelWidget.Gtk.Container+ContainerChild
			this.vbox4 = new global::Gtk.VBox();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 0F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Any one or two character prefix not included in the following list will be consid" +
					"ered to be invalid Hungarian notation. Add prefixes to this list if they should " +
					"be ignored by Hungarian notation rules.");
			this.label6.Wrap = true;
			this.label6.Justify = ((global::Gtk.Justification)(3));
			this.label6.WidthChars = 80;
			this.vbox4.Add(this.label6);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.label6]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.fixed8 = new global::Gtk.Fixed();
			this.fixed8.Name = "fixed8";
			this.fixed8.HasWindow = false;
			this.vbox4.Add(this.fixed8);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.fixed8]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.table5 = new global::Gtk.Table(((uint)(3)), ((uint)(3)), false);
			this.table5.Name = "table5";
			this.table5.RowSpacing = ((uint)(6));
			this.table5.ColumnSpacing = ((uint)(6));
			// Container child table5.Gtk.Table+TableChild
			this.addPrefixButton = new global::Gtk.Button();
			this.addPrefixButton.CanFocus = true;
			this.addPrefixButton.Name = "addPrefixButton";
			this.addPrefixButton.UseUnderline = true;
			this.addPrefixButton.Label = global::Mono.Unix.Catalog.GetString("Add");
			this.table5.Add(this.addPrefixButton);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table5[this.addPrefixButton]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table5.Gtk.Table+TableChild
			this.addPrefixEntry = new global::Gtk.Entry();
			this.addPrefixEntry.CanFocus = true;
			this.addPrefixEntry.Name = "addPrefixEntry";
			this.addPrefixEntry.IsEditable = true;
			this.addPrefixEntry.InvisibleChar = '●';
			this.table5.Add(this.addPrefixEntry);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table5[this.addPrefixEntry]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.fixed10 = new global::Gtk.Fixed();
			this.fixed10.Name = "fixed10";
			this.fixed10.HasWindow = false;
			this.table5.Add(this.fixed10);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table5[this.fixed10]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(2));
			w5.RightAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(0));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.fixed11 = new global::Gtk.Fixed();
			this.fixed11.Name = "fixed11";
			this.fixed11.HasWindow = false;
			this.table5.Add(this.fixed11);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table5[this.fixed11]));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.XOptions = ((global::Gtk.AttachOptions)(1));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.fixed12 = new global::Gtk.Fixed();
			this.fixed12.Name = "fixed12";
			this.fixed12.HasWindow = false;
			this.table5.Add(this.fixed12);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table5[this.fixed12]));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.fixed7 = new global::Gtk.Fixed();
			this.fixed7.Name = "fixed7";
			this.fixed7.HasWindow = false;
			this.table5.Add(this.fixed7);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table5[this.fixed7]));
			w8.TopAttach = ((uint)(2));
			w8.BottomAttach = ((uint)(3));
			w8.LeftAttach = ((uint)(2));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(0));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.prefixNodeView = new global::Gtk.NodeView();
			this.prefixNodeView.CanFocus = true;
			this.prefixNodeView.Name = "prefixNodeView";
			this.GtkScrolledWindow.Add(this.prefixNodeView);
			this.table5.Add(this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table5[this.GtkScrolledWindow]));
			w10.TopAttach = ((uint)(2));
			w10.BottomAttach = ((uint)(3));
			// Container child table5.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Enter a one or two character prefix to add:");
			this.table5.Add(this.label3);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table5[this.label3]));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.removePrefixButton = new global::Gtk.Button();
			this.removePrefixButton.CanFocus = true;
			this.removePrefixButton.Name = "removePrefixButton";
			this.removePrefixButton.UseUnderline = true;
			this.removePrefixButton.Label = global::Mono.Unix.Catalog.GetString("Remove");
			this.vbox3.Add(this.removePrefixButton);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.removePrefixButton]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.fixed13 = new global::Gtk.Fixed();
			this.fixed13.Name = "fixed13";
			this.fixed13.HasWindow = false;
			this.vbox3.Add(this.fixed13);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.fixed13]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			this.table5.Add(this.vbox3);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table5[this.vbox3]));
			w14.TopAttach = ((uint)(2));
			w14.BottomAttach = ((uint)(3));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox4.Add(this.table5);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.table5]));
			w15.Position = 2;
			// Container child vbox4.Gtk.Box+BoxChild
			this.fixed9 = new global::Gtk.Fixed();
			this.fixed9.Name = "fixed9";
			this.fixed9.HasWindow = false;
			this.vbox4.Add(this.fixed9);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.fixed9]));
			w16.Position = 3;
			w16.Expand = false;
			w16.Fill = false;
			this.Add(this.vbox4);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.removePrefixButton.Clicked += new global::System.EventHandler(this.RemovePrefixButtonClicked);
			this.addPrefixEntry.KeyReleaseEvent += new global::Gtk.KeyReleaseEventHandler(this.AddPrefixEntryKeyReleaseEvent);
			this.addPrefixButton.Clicked += new global::System.EventHandler(this.AddPrefixButtonClicked);
		}
	}
}
