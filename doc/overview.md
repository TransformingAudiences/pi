# TAPI Overview
Tapi implements an algorithm to analyse pattern of individuals in audience data. The 
algorithm was developed and implemented in SPSS during a PHD. More information can be 
found [here](www.transformingaudiences.com).  
This project is an attempt to reimplement the algorithm in a way that makes it easier 
to use it for new datasets and also to make it more configurable. 
## Requirements
### Source Code and enviroments
- The source code should be open source and available on github.
- The solution should be well documented and easy to understand.
- Windows, Linux and Mac should be supported, both to develop and run the program.
### Program
- The interface of the program should be a command line interface (CLI)
- The Program should be able to export the result both as a Excel File and in a Csv format
- It should be possible to use a template when exporting as an Excel file.
- It should be possible to define metadata for the algorithm in a xml file.
  - Specify the source format
  - Specify a customer and a consumer
  - Define Scripts to calculate a Consumerreportoair and then to aggreagte them
  - Define filters
  - Select output type 
- The program should take arguments for
  - Path to xml metadata
  - Path to directory with source data
  - Path to excel template
  - Path to outputfile
### Non goals
## Architectur

## Design