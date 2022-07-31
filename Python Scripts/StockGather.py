import sys
from ast import arg
import yfinance as yf
import pandas as pd
import datetime
    
startStockPrice = 0
startIndexVal = 0
currentDate = str(datetime.date.today()) #sets the current date

def getDate(stock, start, dateInterval = 1):
    startDates = start.split("Q")
    stockData = yf.download(stock, start = startDates[0], end = currentDate) #Stock-parameter has to be according to how it is shown in https://finance.yahoo.com/
    #and the start/end date is "yyyy-mm-dd"
    indexData = yf.download('^OMXSPI', start = startDates[0], end = currentDate)
    print(start)
    
    startStockPrice = stockData['Close'].values[0]
    startIndexVal = indexData['Close'].values[0]
    df = pd.DataFrame(
       {
        "MaxDays": [], "MaxPercentage": [], "MinDays":[], "MinPercentage":[]
    }) #This is where all results are put in
    index = 0
    for startD in startDates:
         st = datetime.datetime.strptime(startD, '%Y-%m-%d').date() #Converts a date yyyy-mm-dd to datetime
         end = st.replace(st.year + 1) 
         result = stockDevelopment(st, end, stockData, indexData, dateInterval)#creating a dataframe with stock-index development
         analyzeDataFrame(result, df, index)
         index += 1
    printValues(df)
    
#Analyzes the stock-progress 
def stockDevelopment(startDate, endDate, stockData, indexData, interval = 5):
    df = pd.DataFrame(
        {
        "DaysPassed": [], "StockPercentage": [], "IndexPercentage":[], "Difference_StockVIndex":[]
    })
    csd = stockData.loc[startDate : endDate] #currentstockdata
    xid = indexData.loc[startDate : endDate] #currentindexdata
    startStockPrice = csd['Close'].values[0]
    startIndexVal = xid['Close'].values[0]
    #adds start-data to frame
    nbrOfDays = 0
    for i in range (min(len(csd), len(xid))): 
        
        stockPer = valueChange(csd['Close'].values[nbrOfDays], startStockPrice)
        indexPer = valueChange(xid['Close'].values[nbrOfDays], startIndexVal)
        df.loc[nbrOfDays] = [nbrOfDays, stockPer, indexPer, (stockPer-indexPer)]
        nbrOfDays += 1
        
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
getDate(sys.argv[1], sys.argv[2], dateInterval= 1) #Argument 1 is the stock to investigate
    #Argument 2 is all startdates
    #Argument 3 is all enddates