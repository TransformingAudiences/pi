## TODO

Fixa tester mot skarp data
- 1 månaders data
- 1 dag
  - Reach, Rating, Minut, timme daypart, hel dag
- 1 månad
  - Reach, Rating, Minut, timme daypart, hel dag
Tabbar med resultat ifrån infogalactic och config på hur vi vill att det skall definieras..


Dela på förväntat värde (gäller rating / genomsnitts minut), aggregate inställning?

Refactor av rapport och repertoire xml för att minska risken för fel








- GetMinutes from Period
- Expandera produkter














- xml conf for reports and reportoairs
  - how to choose verticals, 
  - how to aggregate
  - result, value or % , include weight
- use weights in calculations
- some refactoring, break up code into more files
- implement some tests
- prepare for github

- Validate the Xml against the schema to avoid other bugs
- Show nice error messages in the console
- Error handling if the arguments are not ok
- Help text for the cli

- perf regressed by dynamic usage, we can make it without.. 

# Spec
Motor för beräkningar

## läsa in, skapa model
- Mappa

## Definiera konsumtionsreportoarer
- Definieras för en dag och dess minuter
  - kan även vara minuter och panelister och då är dag "konsument", måttet blir typ socialt lyssnande
- Välj konsument, vanligtvis panelistid, men kan va hushåll eller vad som helst
- Välj z axel, vad den konsumerar, vanligtvis en kanal men kan vara genre
- Välj frekvens eller volym, volymprocent

## Reportoar algoritm
- konsumtion över tid, volym frekvens
- Configurara brytpunkter
- spara och återanvända delresultat
- minuter * dagar * kaneler för individ, är en konsumtions reportoar
- filtreringar och vad som skall aggregaras
- då man aggregarar så kan y och z uppstå ,försvinna och återuppstå
- Exportera till Excel
  - Kanalreportoarer
  - slå ihop kuben, ex, y = kanaler, x = dagar, cell = antal minuter, minuter skall alltså aggregeras
  - välj filter
     - målgrupp
       - Tid på dagen
       - period
  - välj att visa i procent, eller frekvens
  sedan medelvärden, med hur många dagar panelisten är in tab
          - Jämför kanalrepotoarer
                 - definiera filter 
                 - kolla på en individs skillnad mot övriga hushållet
  - Konsumtionsreportoar på något annat än dag, kollapsas till en för panelen per dag