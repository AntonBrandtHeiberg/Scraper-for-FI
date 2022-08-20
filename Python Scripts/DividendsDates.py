import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime


currentDate = str(datetime.date.today()) #sets the current date
def printDividendsDate(stock, startDate, endDate = currentDate):
    stockVals = yf.Ticker(stock)
    dividends = stockVals.dividends.loc[startDate : endDate]
    stockCloses = stockVals.history(period="max") #gets all historic close-data of the stock 
    for index, values in zip(dividends.index.tolist(), dividends): #iterates through all dividends 
        if values > 0:
            date = str(index).split(" ")[0]
            stockVal = stockCloses.loc[[pd.to_datetime(date)]]["Close"][0]
            print(str(index).split(" ")[0] + " " + str(stockVal) + " " + str(values)) #prints the date and then the value of dividend in %
    

def returnDividendsDate(stock, startDate, endDate = currentDate):
    stockVals = yf.Ticker(stock)
    dividends = stockVals.dividends.loc[startDate : endDate]
    return iterateThroughDividends(dividends)

def iterateThroughDividends(dividends):
    result = []
    for index, values in zip(dividends.index.tolist(), dividends):
        result.append( (str(index).split(" ")[0], values))
    return result #return a tuple of dividends dates (as string) and the values from that dividend (floats)

#printDividendsDate(sys.argv[1], sys.argv[2])
