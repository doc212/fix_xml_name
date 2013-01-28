Replace the name attribute of some tags by the tag's name + counter
e.g.:

<foo name="bar" blabla=""/> becomes <foo name="foo 1" blabla=""/>

fix.py:
* python implementation that requires lxml.etree
* works with python 2.6 and 2.7
* probably incompatible with python 3.x
* configure it by editing the config section of fix.py

fix.cs:
* C# implementation that requires .NET Runtime environment
* works using the config.xml file. Run it once to generate an example file

Note:
by default, only the tags that already have a name attribute are modified.
If you want to add a name attribute to tags that don't have one, you must specify it
in the config.

