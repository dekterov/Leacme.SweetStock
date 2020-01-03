// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using YahooFinanceApi;

namespace Leacme.Lib.SweetStock {

	public class Library {

		private LiteDatabase db = new LiteDatabase(typeof(Library).Namespace + ".Settings.db");
		private LiteCollection<BsonDocument> symbolCollection;

		private List<Field> fieldList = new List<Field>{
			Field.Symbol,
			Field.ShortName,
			Field.LongName,
			Field.Bid,
			Field.Ask,
			Field.BidSize,
			Field.AskSize,
			Field.QuoteType,
			Field.QuoteSourceName,
			Field.Currency,
			Field.MarketState,
			Field.RegularMarketPrice,
			Field.RegularMarketTime,
			Field.RegularMarketChange,
			Field.RegularMarketOpen,
			Field.RegularMarketDayHigh,
			Field.RegularMarketDayLow,
			Field.RegularMarketVolume,
			Field.FiftyTwoWeekHighChange,
			Field.FiftyTwoWeekHighChangePercent,
			Field.FiftyTwoWeekLow,
			Field.FiftyTwoWeekHigh,
			Field.DividendDate,
			Field.EarningsTimestamp,
			Field.EarningsTimestampStart,
			Field.EarningsTimestampEnd,
			Field.TrailingAnnualDividendRate,
			Field.TrailingPE,
			Field.TrailingAnnualDividendYield,
			Field.EpsTrailingTwelveMonths,
			Field.EpsForward,
			Field.SharesOutstanding,
			Field.BookValue,
			Field.RegularMarketChangePercent,
			Field.RegularMarketPreviousClose,
			Field.MessageBoardId,
			Field.FullExchangeName,
			Field.FinancialCurrency,
			Field.AverageDailyVolume3Month,
			Field.AverageDailyVolume10Day,
			Field.FiftyTwoWeekLowChange,
			Field.FiftyTwoWeekLowChangePercent,
			Field.TwoHundredDayAverageChangePercent,
			Field.MarketCap,
			Field.ForwardPE,
			Field.PriceToBook,
			Field.SourceInterval,
			Field.ExchangeTimezoneName,
			Field.ExchangeTimezoneShortName,
			Field.Market,
			Field.Exchange,
			Field.ExchangeDataDelayedBy,
			Field.PriceHint,
			Field.FiftyDayAverage,
			Field.FiftyDayAverageChange,
			Field.FiftyDayAverageChangePercent,
			Field.TwoHundredDayAverage,
			Field.TwoHundredDayAverageChange,
			Field.Tradeable,
			Field.Language,
			Field.GmtOffSetMilliseconds,
		};

		public Library() {
			symbolCollection = db.GetCollection(nameof(symbolCollection));

		}

		/// <summary>
		/// Populate the database with some stock examples if database is being created.
		/// /// </summary>
		/// <param name="exampleStockSymbols"></param>
		/// <returns></returns>
		public async Task InitialPopulateDatabaseWithExampleSymbols(List<string> exampleStockSymbols) {
			if (!db.CollectionExists(nameof(symbolCollection))) {
				foreach (var symbol in exampleStockSymbols) {
					try {
						await StoreSymbolAsync(symbol);
					} catch {
						// ingnore invalid example symbols and go on
					}
				}
			}
		}

		/// <summary>
		/// Store the stock symbol in database for future retrieval.
		/// /// </summary>
		/// <param name="stockSymbol"></param>
		/// <returns></returns>
		public async Task<bool> StoreSymbolAsync(string stockSymbol) {
			IReadOnlyDictionary<string, Security> securities = new Dictionary<string, Security>();
			try {
				securities = await Yahoo.Symbols(stockSymbol).Fields(fieldList.ToArray()).QueryAsync();
			} catch {
				throw;
			}
			if (securities.Count.Equals(0)) {
				throw new ArgumentException("Invalid stock symbol: " + stockSymbol);
			}
			var insNu = symbolCollection.Insert(new BsonDocument { ["Symbol"] = stockSymbol.ToUpper() });
			if (insNu != null) { return true; } else return false;
		}

		/// <summary>
		/// Delete the stored stock symbol from database.
		/// /// </summary>
		/// <param name="symbol"></param>
		public void DeleteStoredSymbol(string symbol) {
			symbolCollection.Delete(z => z["Symbol"].Equals(symbol.ToUpper()));
		}

		/// <summary>
		/// Get all stored stock symbols from database.
		/// /// </summary>
		/// <returns></returns>
		public HashSet<string> GetAllStoredSymbols() {
			return symbolCollection.FindAll().Select(z => (string)z["Symbol"]).ToHashSet();
		}

		/// <summary>
		/// Query the stock service for information.
		/// /// </summary>
		/// <param name="stockSymbols"></param>
		/// <returns>The stock data response.</returns>
		public async Task<IEnumerable<Security>> GetStockDataAsync(List<string> stockSymbols) {
			var securities = await Yahoo.Symbols(stockSymbols.Select(z => z.ToUpper()).ToArray()).Fields(fieldList.ToArray()).QueryAsync();
			return securities.Values;
		}

	}
}