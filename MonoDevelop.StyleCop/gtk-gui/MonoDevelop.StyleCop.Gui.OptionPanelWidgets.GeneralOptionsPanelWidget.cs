
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.StyleCop.Gui.OptionPanelWidgets
{
	public partial class GeneralOptionsPanelWidget
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.CheckButton enableCacheCheckBox;

		private global::Gtk.Label label3;

		private global::Gtk.Fixed fixed7;

		private global::Gtk.CheckButton violationsAsErrorsCheckBox;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Label label4;

		private global::Gtk.SpinButton maxViolationCountSpinButton;

		private global::Gtk.HBox hbox3;

		private global::Gtk.Label label5;

		private global::Gtk.ComboBox cultureComboBox;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget MonoDevelop.StyleCop.Gui.OptionPanelWidgets.GeneralOptionsPanelWidget
			global::Stetic.BinContainer.Attach(this);
			this.WidthRequest = 625;
			this.HeightRequest = 495;
			this.Name = "MonoDevelop.StyleCop.Gui.OptionPanelWidgets.GeneralOptionsPanelWidget";
			// Container child MonoDevelop.StyleCop.Gui.OptionPanelWidgets.GeneralOptionsPanelWidget.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.enableCacheCheckBox = new global::Gtk.CheckButton();
			this.enableCacheCheckBox.CanFocus = true;
			this.enableCacheCheckBox.Name = "enableCacheCheckBox";
			this.enableCacheCheckBox.Label = global::Mono.Unix.Catalog.GetString("Cache StyleCop analysis results");
			this.enableCacheCheckBox.DrawIndicator = true;
			this.enableCacheCheckBox.UseUnderline = true;
			this.vbox2.Add(this.enableCacheCheckBox);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.enableCacheCheckBox]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("To increase the overall performance of StyleCop, results can be cached after each" +
					" analysis.");
			this.vbox2.Add(this.label3);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.label3]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.fixed7 = new global::Gtk.Fixed();
			this.fixed7.Name = "fixed7";
			this.fixed7.HasWindow = false;
			this.vbox2.Add(this.fixed7);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.fixed7]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.violationsAsErrorsCheckBox = new global::Gtk.CheckButton();
			this.violationsAsErrorsCheckBox.CanFocus = true;
			this.violationsAsErrorsCheckBox.Name = "violationsAsErrorsCheckBox";
			this.violationsAsErrorsCheckBox.Label = global::Mono.Unix.Catalog.GetString("Treat violations as errors");
			this.violationsAsErrorsCheckBox.DrawIndicator = true;
			this.violationsAsErrorsCheckBox.UseUnderline = true;
			this.vbox2.Add(this.violationsAsErrorsCheckBox);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.violationsAsErrorsCheckBox]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Maximum number of violations before cancelling analysis");
			this.hbox2.Add(this.label4);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.label4]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.maxViolationCountSpinButton = new global::Gtk.SpinButton(0D, 2147483647D, 1D);
			this.maxViolationCountSpinButton.CanFocus = true;
			this.maxViolationCountSpinButton.Name = "maxViolationCountSpinButton";
			this.maxViolationCountSpinButton.Adjustment.PageIncrement = 10D;
			this.maxViolationCountSpinButton.ClimbRate = 1D;
			this.maxViolationCountSpinButton.Numeric = true;
			this.hbox2.Add(this.maxViolationCountSpinButton);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.maxViolationCountSpinButton]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			this.vbox2.Add(this.hbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
			w7.Position = 4;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Culture for analysis");
			this.hbox3.Add(this.label5);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.label5]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.cultureComboBox = global::Gtk.ComboBox.NewText();
			this.cultureComboBox.Name = "cultureComboBox";
			this.hbox3.Add(this.cultureComboBox);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.cultureComboBox]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			this.vbox2.Add(this.hbox3);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox3]));
			w10.Position = 5;
			w10.Expand = false;
			w10.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.enableCacheCheckBox.Toggled += new global::System.EventHandler(this.EnableCacheCheckBoxToggled);
			this.violationsAsErrorsCheckBox.Toggled += new global::System.EventHandler(this.ViolationsAsErrorsCheckBoxToggled);
			this.maxViolationCountSpinButton.ValueChanged += new global::System.EventHandler(this.MaxViolationCountSpinButtonValueChanged);
			this.cultureComboBox.Changed += new global::System.EventHandler(this.CultureComboBoxChanged);
		}
	}
}
