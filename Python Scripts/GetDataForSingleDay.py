import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime

def getStockData(stock, startDate):
    endDate = datetime.datetime.strptime(startDate, '%Y-%m-%d').date()
    endDate += datetime.timedelta(days = 1)
    ed = endDate.strftime('%Y-%m-%d')
    stockData = yf.download(stock, start = startDate, end = ed)
    return stockData
    
result = getStockData(sys.argv[1], sys.argv[2]) #Parameters are stock, startdate and enddate
resultOMX = getStockData('^OMXSPI', sys.argv[2])
if(len(result['Close'].values) != 0):
    print(result['Close'].values[0])
    print(resultOMX['Close'].values[0])