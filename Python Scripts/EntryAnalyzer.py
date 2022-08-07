import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime
    
startStockPrice = 0
startIndexVal = 0
currentDate = str(datetime.date.today()) #sets the current date

def getDate(stock, start, days, percentageInc):
    startDates = start.split("Q")
    stockData = yf.download(stock, start = startDates[0], end = currentDate) #Stock-parameter has to be according to how it is shown in https://finance.yahoo.com/
    #and the start/end date is "yyyy-mm-dd"
    indexData = yf.download('^OMXSPI', start = startDates[0], end = currentDate)
    maxDays = int(days)
    percent = float(percentageInc)
    for startD in startDates:
         st = datetime.datetime.strptime(startD, '%Y-%m-%d').date() #Converts a date yyyy-mm-dd to datetime
         end = st.replace(st.year + 1) 
         stockVIndex = stockDevelopment(st, end, stockData, indexData, maxDays, percent) #Returns a triplet of values 
            #with stockPer, indexPer and number of days passed
         printVals(stockVIndex)   
        

def currentValueData(startDate, endDate, df):
    cvd = df.loc[startDate : endDate]
    return cvd

#Analyzes the stock-progress 
def stockDevelopment(startDate, endDate, stockData, indexData, maxDays, maxPercentage):
    
    csd = currentValueData(startDate, endDate, stockData) #currentstockdata
    xid = currentValueData(startDate, endDate, indexData) #currentindexdata
    startStockPrice = csd['Close'].values[0]
    startIndexVal = xid['Close'].values[0]
    #adds start-data to frame
    for i in range (1, min(len(csd), len(xid))):
        stockPer = valueChange(csd['Close'].values[i], startStockPrice)
        indexPer = valueChange(xid['Close'].values[i], startIndexVal)
        if(maxDays <= i or (stockPer - indexPer) >= maxPercentage):
            return (indexPer, i, stockPer)
    return (indexPer, i, stockPer) #Returns indexPercentage, days passed for sell and the stockpercentage
        
    #Returns the % difference from startvalue and currentvalue
def valueChange(currentValue, startValue):
    return (currentValue/startValue - 1)*100
                 
#The method analyzing the data            
def analyzeDataFrame(df, newDF, indexPos):
    minVal = df['Difference_StockVIndex'].min(axis=0)
    min_index = df['Difference_StockVIndex'].idxmin()
    maxVal = df['Difference_StockVIndex'].max(axis=0)
    max_index = df['Difference_StockVIndex'].idxmax()
    dateMin = df['DaysPassed'][min_index]
    dateMax = df['DaysPassed'][max_index]
    newDF.loc[indexPos] = [dateMax, maxVal, dateMin, minVal] 
    
def printVals(triplet):
    print(triplet[0])
    print(triplet[1])
    print(triplet[2])

#Start of program
#if int(sys.argv[2].split("-")[1]) % 4 == 0:
#    lengthOfMonth[2] = 29 #when feb has 29 days 
stock = "VOLV-B.ST"
dates = "2022-02-09" 
#2022-03-16 2022-03-17 2022-04-25 2022-04-26"

getDate(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]) 
    #Argument 1 is the stock to investigate
    #Argument 2 is all startdates
    #Argument 3 is maxdays
    #Argument 4 is percentage
#indata is:
    #stock of company
    #Entries (purchase dates)
    #Day to sell from purchase, an int
    #Excpected return compared to OMX
    
#Steps for the script are:
    #Download data from yf, OMX and stock
    #Take one entry and analyze it for the duration (untill time to sell date)
        #if percentage is above or equals to what have been forecasted then sell
        #else waitfor final date
        #when final date is reached then sell no matter what the percentage is, include dividends in the close-value?
