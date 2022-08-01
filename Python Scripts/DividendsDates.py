import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime


currentDate = str(datetime.date.today()) #sets the current date
def printDividendsDate(stock, startDate, endDate):
    stockVals = yf.Ticker(stock)
    dividends = stockVals.dividends.loc[dates : endDate]
    for index in dividends.index:
        print(str(index).split(" ")[0])
