# XmlPrime

This repository contains the open-source components of [XmlPrime](https://www.xmlprime.com/), the XML processing library for the Microsoft .NET Framework.

## XmlPrime Command Line Tools

This will contain the source code for the XmlPrime command line tools:

 - Serialize.exe, the XmlPrime command line XML serializer,
 - XInclude.exe, the XmlPrime command line XInclude processor,
 - XQuery.exe, the XmlPrime command line XQuery  processor,
 - Xslt.exe, the XmlPrime command line Extensible Stylesheet Language Transformation (XSLT) processor.

For further information, see the [documentation](https://www.xmlprime.com/xmlprime/doc/3.0/command-line-tools.htm)

## XmlPrime MSBuild Task Library

This Visual Studio solution contains the XlmPrime MSBUILD tasks:

 - Include Task, which performs XInclude processing on an XML input and saves the results to a file.
 - Query Task, which queries XML inputs by using an XQuery program and saves the results to a file.
 - Serialize Task, which applies serialization options to an XML input and saves the results to a file.
 - Transform Task, which transforms XML inputs by using an Extensible Stylesheet Language Transformation (XSLT) program and saves the results to one or more files.

For further information, see the [documentation](https://www.xmlprime.com/xmlprime/doc/3.0/msbuild-tasks.htm).
