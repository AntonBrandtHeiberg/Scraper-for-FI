import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime


currentDate = str(datetime.date.today()) #sets the current date
def printDividendsDate(stock, startDate, endDate):
    stockVals = yf.Ticker(stock)
    dividends = stockVals.dividends.loc[dates : endDate]
    print(dividends.index.tolist())
    for index, values in zip(dividends.index.tolist(), dividends):
        print(str(index).split(" ")[0] + " " + str(values))

printDividendsDate(sys.argv[1], sys.argv[2], sys.argv[3])
