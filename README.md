# Digital Building Twin Platform

This repository contains the code for running the Digital Building Twin Platform (DBTP) and all the example data created in the frame of the ConSLAM case study.

The DBTP has two main components:
* [Platform](DBT-API)
* [IfcGeometryConverter](IfcGeometryReader)
<br />

## Software Requirements DBTP:
* RDF graph database instance (We were using a GraphDB database. However, any database with a SPARQL endpoint can be used here.)
* Object storage that is S3 compatible (We were using a MinIO instance)
* InfluxDB (The implementation of requesting and updating time series data is specific to InfluxDB and its query language; however, the ITimeseriesRepository interface class allows the extension of the code with adapters for other time series databases with little effort. We were using InfluxDB Cloud 2.0.)

## Software Requirements IfcGeometryConverter:
* TinyPly (For reading and writing of PLY geometry files)[GitHub Page](https://github.com/yk35/tinyply.net)
* IFCtoLBD Converter (Conversion of the topological structure of the building from IFC to RDF)[GitHub Page](https://github.com/jyrkioraskari/IFCtoLBD)

## How to run this code:

### DBTP without IfcGeometryConverter
* Make sure you have all the required software installed and running
* Adjust the appsettings.json file to connect the DBTP with your databases
* Execute the code, e.g., from Visual Studio 2022

### DBTP with IfcGeometryConverter
If you want to use the IfcGeometryConverter to translate the IFC into its RDF representation together with PLY geometry files for every building element and use this as the starting point of your digital twin, follow these steps:
* Make sure you have installed all the software requirements
* Open the IFCtoLBD converter and convert your IFC file to RDF using the default settings
* Load the resulting Turtle file into your RDF database
* Execute the code of the DBTP, e.g., from Visual Studio 2022
* Send a post request to /setup/setbuildingpart1 (e.g., using the Swagger UI). This generates a .txt file (containing information about GUIDs and corresponding IRIs) that is required by the IfcGeometryConverter
* Build the TinyPly library
* Copy the tinyply.dll into the /bin folder of the IfcGeometryConverter and add it to the project dependencies
* Run the IfcGeometryConverter, providing the path to the generated .txt file and the .ifc file. This will generate another .txt file containing information about the bounding boxes of all building elements and PLY geometry files
* Send a post request to /setup/setbuildingpart2, providing the path to the bounding box .txt file and the directory of the PLY files. This will integrate the geometric information into the DBTP
The IfGeometryReader is separated from the DBTP because it uses the Xbim.Geometry Nuget package, which does not support .NET Core, which is used by DBTP.
Any type of converter can be used to translate project intent information like IFC files, schedules and others to RDF and use them as the starting point of the digital twin, which is then constantly updated with information about the current project status.

## Case Study
The case study is based on the ConSLAM dataset published by [Trzeciak et al.](https://github.com/mac137/ConSLAM).
The laser scans available in this dataset were used to run a progress monitoring service that compares as-built elements with as-designed elements, considering schedule-related aspects.
An RDF graph was created for the Data Layer that describes the laser scanner and the data it has captured.

The DBTP was used with the IfcGeometry Converter to initiate the RDF graph for the Information Layer, which represents the PII. 
The generated geometry files that are stored in the MinIO object storage are also included here for completeness.
Five laser scans of the ConSLAM dataset were compared with the as-designed information using the progress monitoring services.
The insights gained from this comparison were added to the Information Layer through the platform's API.
The final graph, after running the progress monitoring for all laser scans, is provided as an RDF graph in Turtle format.

Finally, a KPI was calculated and stored in the InfluxDB Cloud database. 
This data was exported to CSV and published here, together with the RDF graph for the Knowledge Layer, which provides general information about the individual KPIs.

## License
The MIT License (MIT)

Copyright (c) 2024 Jonas Schlenger

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## How to cite
```
@software{jonas_schlenger_2024,
 author       = {Jonas Schlenger and
                  André Borrmann},
  title        = {DBTP API v 1.0},
  month        = jun,
  year         = 2024,
  publisher    = {GitHub},
  version      = {1.0},
  url          = {https://github.com/jschlenger/DBTP}
}
```

## Acknowledgements
The research described in this paper has received funding from the European Union's Horizon 2020 research and innovation programme under grant agreement no. 958398, "BIM2TWIN: Optimal Construction Management & Production Control". 
We thankfully acknowledge the support of the European Commission in funding this project.

https://bim2twin.eu/

https://cordis.europa.eu/project/id/958398/en 