#%config Completer.use_jedi = False
import sys
from ast import arg
import yfinance as yf
import pandas as pd
from datetime import date
lengthOfMonth = [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]

class Stock:
    def __init__ (self, name, yfName):
        self.name = name
        self.yfName = yfName
    def getName(self):
        return self.name
    def getYFName(self):
        return self.yfName
    
#Creating the stocks with a stock-class
stocks = [Stock("Pandox", "PNDX-B.ST"), Stock("Volvo", "VOLV-B.ST"), Stock("Electrolux", "ELUX-B.ST"), 
          Stock("Ericsson", "ERIC-B.ST"), Stock("H&M","HM-B.ST"), Stock("SEB", "SEB-A.ST"), 
          Stock("Swedbank", "SWED-A.ST"), Stock("Tele2", "TEL2-B.ST"), Stock("SKF", "SKF-B.ST"), Stock("Sandvik", "SAND.ST"),
          Stock("Skanska", "SKA-B.ST")]
currentDate = str(date.today()) #sets the current date
#Returns the dates
def getDate(stock, startDate, endDate = currentDate, dateInterval = 5):
    stockData = yf.download(stock, start = startDate, end = endDate) #Stock-parameter has to be according to how it is shown in https://finance.yahoo.com/
    #and the start/end date is "yyyy-mm-dd"
    indexData = yf.download('^OMXSPI', start = startDate, end = endDate)
    stockDevelopment(stockData, indexData, dateInterval)#creating a dataframe with stock-index development
    
#Analyzes the stock-progress 
def stockDevelopment(stockData, indexData, interval = 5):
    df = pd.DataFrame(
        {
        "Date": [], "StockPercentage": [], "IndexPercentage":[], "Difference_StockVIndex":[]
    })
    startStockPrice = stockData['Close'].values[0]
    startIndexVal = indexData['Close'].values[0]
    #adds start-data to frame
    df.loc[0] = [convertDateToString(0, stockData), 0, 0, 0]
   
#Takes the minimum as range due to the length of indexData and stockData not being the same (?!)
    for i in range (min(int(len(stockData) / interval), int(len(indexData)/interval))):
        
        stockPer = valueChange(stockData['Close'].values[i*interval], startStockPrice)
        indexPer = valueChange(indexData['Close'].values[i*interval], startIndexVal)
        
        df.loc[i*interval] = [convertDateToString(i*interval, stockData), stockPer, indexPer, (stockPer-indexPer)]
        
    analyzeDataFrame(df)
    
    #Returns the % difference from startvalue and currentvalue
def valueChange(currentValue, startValue):
                 return (currentValue/startValue - 1)*100
                 
#The method analyzing the data            
def analyzeDataFrame(df):
    minVal = df['Difference_StockVIndex'].min(axis=0)
    min_index = df['Difference_StockVIndex'].idxmin()
    maxVal = df['Difference_StockVIndex'].max(axis=0)
    max_index = df['Difference_StockVIndex'].idxmax()
    dateMin = df['Date'][min_index]
    dateMax = df['Date'][max_index]
    #Prints the minimum development of the stock compared to index
    #Prints the amount of days passed for the min point
    #Prints the max development of the stock compared to index
    #Prints the amount of days passed for the max point
    print(minVal, "\n" ,timeElapsed(df['Date'][0], dateMin),
     "\n" ,maxVal, "\n" ,timeElapsed(df['Date'][0], dateMax),)
    
def timeElapsed(firstDate, endDate): #Shows the amount of time elapsed in days
    startSplit = firstDate.split("-")
    endSplit = endDate.split("-")
    monthStart = int(startSplit[1])
    monthEnd = int(endSplit[1])

    if(monthStart == monthEnd and int(endSplit[0]) != int(startSplit[0])): #if same month (and not different year) then only subtract the days
        return int(endSplit[2]) - int(startSplit[2])

    dateDifference = lengthOfMonth[monthStart]-int(startSplit[2]) #gets the number of days remaining of the month 
    #that is the start-date
    for i in range(monthStart+1, monthEnd): #Loops through the months from the first to the last excluding the 
        #months which the months start
        
        dateDifference += lengthOfMonth[i]

    dateDifference += int(endSplit[2])
    return dateDifference


def convertDateToString(i, dataFrame): #Takes one of the dataframes (that are indexed by date, which complicates things...
    #and turn these dates to strings for usage in the stock-dev function)
    dateTimes = dataFrame.index.values
    date = str(dateTimes[i]).split("T")[0]
    return date


if int(sys.argv[2].split("-")[1]) % 4 == 0:
    lengthOfMonth[2] = 29 #when feb has 29 days 
if sys.argv.count == 4:
    getDate(sys.argv[1], sys.argv[2], sys.argv[3], 12)
else:
    getDate(sys.argv[1], sys.argv[2], dateInterval= 1) #Argument 1 is the stock to investigate
    #Argument 2 is the start-date 
    #Argument 3 is the end-date