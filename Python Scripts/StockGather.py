import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime
from dateutil.relativedelta import relativedelta
import DividendsDates

global startStockPrice
global startIndexVal
currentDate = str(datetime.date.today()) #sets the current date
global dividend
monthAdded= 0
if(len(sys.argv) > 3):
    monthAdded = int(sys.argv[3])
else:
    monthAdded = 4

def getStockData(stock, startDate, endDate = currentDate):
    return yf.download(stock, start = startDate, end = endDate)

def getDate(stock, start):

    startDates = start.split("Q")
    stockData = getStockData(stock, startDates[0]) #Stock-parameter has to be according to how it is shown in https://finance.yahoo.com/
    #and the start/end date is "yyyy-mm-dd"
    indexData = yf.download('^OMXSPI', start = startDates[0], end = currentDate)
    df = pd.DataFrame(
       {
        "MaxDays": [], "MaxPercentage": [], "MinDays":[], "MinPercentage":[]
    }) #This is where all results are put in
    index = 0
    for startD in startDates:
         st = datetime.datetime.strptime(startD, '%Y-%m-%d').date() #Converts a date yyyy-mm-dd to datetime
         end = st + relativedelta(months=monthAdded)
         result = stockDevelopment(st, end, stockData, indexData, DividendsDates.returnDividendsDate(stock, startDates[0]))#creating a dataframe with stock-index development
         analyzeDataFrame(result, df, index)
         index += 1
    printValues(df)
    
#Analyzes the stock-progress 
def stockDevelopment(startDate, endDate, stockData, indexData, dividendsData):
    
    df = pd.DataFrame(
        {
        "DaysPassed": [], "StockPercentage": [], "IndexPercentage":[], "Difference_StockVIndex":[]
    })
    csd = stockData.loc[startDate : endDate] #currentstockdata
    xid = indexData.loc[startDate : endDate] #currentindexdata
    startStockPrice = csd['Close'].values[0]
    startIndexVal = xid['Close'].values[0]
    df.loc[0] = [0, 0, 0, 0]
    dividend = 0.0
    #adds start-data to frame
    for i in range (1, (min(len(csd), len(xid)))):
        if(len(dividendsData) != 0 and str(csd.index[i]).split(" ")[0] == dividendsData[0][0]):
            dividend += dividendsData[0][1] #if the dividendsdate is the current date then add the value of the dividend to the share value
            dividendsData.pop(0)
        stockPer = valueChange(csd['Close'].values[i] + dividend, startStockPrice)
        indexPer = valueChange(xid['Close'].values[i], startIndexVal)
        df.loc[i] = [i, stockPer, indexPer, (stockPer-indexPer)]
        
    return df
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
    
def printValues(df):
    minVal = df['MinPercentage'].mean()
    minDay = df['MinDays'].mean()
    maxVal = df['MaxPercentage'].mean()
    maxDay = df['MaxDays'].mean()
    print("In order: max value, max days, min value, min days, with value in percentage")
    print(maxVal)
    print(maxDay)
    print(minVal)
    print(minDay)

#Start of program
#if int(sys.argv[2].split("-")[1]) % 4 == 0:
#    lengthOfMonth[2] = 29 #when feb has 29 days 
stock = "VOLV-B.ST"
dates = "2022-02-09 2022-03-16 2022-04-25 2022-04-26"
getDate(sys.argv[1], sys.argv[2]) #Argument 1 is the stock to investigate
    #Argument 2 is all startdates
