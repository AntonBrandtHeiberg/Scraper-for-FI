# Scraper-for-FI
A simple scraper for FI


1. Hämta datan från FI för repektive aktie (check)
2. Anropa python med dessa datum för respektive aktie 
	2.1 C# skickar in varje datum för sig och loopar genom
	2.2 C# skickar in alla datum direkt till python (som loopar igenom)
3. Python ger en analys tillbaka --> 
	1. För varje köp (entry) jämför OMX med aktien under ett år
	2. Ge ett genomsnittligt högsta värde efter insiderhandeln (x antal dagar)
	
4. C# skapar en prognos på avkastning och datumet som det händer
5. "Trader" som alltid säljer på högsta datumet och säljer om kursen överträffar prognosen innan detta datum är nått.

använd resultaten från StockForecast
Stock Forecast for max: x% development after y days

Trader gets an EntryOverTime object and starts a loop
Trader starts at EntryOverTime startDate and cycles through each work day and gets omx index
via python script perhaps
Every day it checks what it's winnings are and compares it to the expected ones

2 scenarios:
trader reaches expected winnings
or:
trader sells after expected days

börs data kan lagras som en array av tupeln: (value, DateTime)
