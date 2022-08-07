import sys
import DividendsDates

if(len(sys.argv) >= 4):
    DividendsDates.printDividendsDate(sys.argv[1], sys.argv[2], sys.argv[3])
else:
    DividendsDates.printDividendsDate(sys.argv[1], sys.argv[2])
#First arg is the stock, second is start date and third is the end date

