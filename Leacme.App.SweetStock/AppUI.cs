// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Leacme.Lib.SweetStock;
using YahooFinanceApi;

namespace Leacme.App.SweetStock {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();
		private DataGrid sGrid = App.DataGrid;
		private IDisposable timerD = null;

		public AppUI() {

			sGrid.CellPointerPressed += (z, zz) => {
				if (zz.PointerPressedEventArgs.MouseButton.Equals(MouseButton.Right)) {
					ContextMenu oneM = new ContextMenu();
					MenuItem RemEntryMenuItem = new MenuItem() { Header = "Remove" };
					RemEntryMenuItem.Click += (zzz, zzzz) => {
						var entryToRemove = sGrid.Items.Cast<Security>().ToList().ElementAt(zz.Row.GetIndex());
						sGrid.Items = sGrid.Items.Cast<Security>().ToList().Except(new List<Security>() { entryToRemove });
						lib.DeleteStoredSymbol(entryToRemove.Symbol);
					};
					((AvaloniaList<object>)oneM.Items).Add(RemEntryMenuItem);
					oneM.Open((DataGrid)z);
				}
			};

			Dispatcher.UIThread.InvokeAsync(async () => {
				await lib.InitialPopulateDatabaseWithExampleSymbols(new List<string> { "GOOG", "AAPL", "MSFT" });
				await PopulateGrid();
			});
			var entrHl = App.HorizontalFieldWithButton;
			entrHl.holder.HorizontalAlignment = HorizontalAlignment.Center;
			entrHl.label.Text = "Add a stock symbol to monitor:";
			entrHl.field.Width = 70;
			entrHl.button.Width = 50;
			entrHl.button.Content = "Add";

			entrHl.button.Click += async (z, zz) => {
				try {
					if (await lib.StoreSymbolAsync(entrHl.field.Text)) {
						await PopulateGrid();
						entrHl.field.Text = string.Empty;
					}
				} catch (ArgumentException) {
					entrHl.field.Text = string.Empty;
				}
			};
			var slidLab = App.TextBlock;
			slidLab.Text = "Refresh interval:";

			var slid1 = App.HorizontalSliderWithValue;
			slid1.value.Width = 20;
			slid1.slider.Minimum = 1;
			slid1.slider.Maximum = 60;
			slid1.slider.Value = 2;
			RunTimer(slid1);

			slid1.slider.PropertyChanged += (z, zz) => {
				if (zz.Property.Equals(Slider.ValueProperty)) {
					timerD.Dispose();
					RunTimer(slid1);
				}
			};
			var slidUnit = App.TextBlock;
			slidUnit.Text = "minutes";

			entrHl.holder.Children.AddRange(new List<IControl> { new Control() { Width = 60 }, slidLab, slid1.holder, slidUnit });
			rootPan.Children.AddRange(new List<IControl> { new Control() { Height = 10 }, entrHl.holder, new Control() { Height = 20 }, sGrid });
		}

		private void RunTimer((StackPanel holder, Slider slider, TextBlock value) slid1) {
			timerD = DispatcherTimer.Run(() => {
				Dispatcher.UIThread.InvokeAsync(async () => { await PopulateGrid(); });
				return true;
			}, new TimeSpan(0, (int)slid1.slider.Value, 0));
		}

		private async Task PopulateGrid() {
			sGrid.AutoGeneratingColumn += (z, zz) => {
				switch (zz.Column.Header) {
					case "Fields":
					case "Item":
						zz.Cancel = true;
						break;
					case "Symbol":
						zz.Column.DisplayIndex = 0;
						break;
					case "ShortName":
						zz.Column.DisplayIndex = 0;
						break;
				}
				zz.Column.Header = string.Join(" ", Regex.Matches(
						(string)zz.Column.Header, @"(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9\.*]+|[a-z]+)").OfType<Match>().Select(
							zzz => zzz.Value).ToArray());
			};

			sGrid.Items = (await lib.GetStockDataAsync(lib.GetAllStoredSymbols().ToList()));
		}
	}
}