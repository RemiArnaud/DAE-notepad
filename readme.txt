this is a modified version of Microsoft XML Notepad source code
There are some bugs in the Microsoft schema parser that prevent the original XML Notepad to work. This version forces the application to load correctly the schema.

This tool works really nicely to find COLLADA Schema validation errors, as well as visual editing. Unlike many XML validation tools, it returns line numbers.

This tool can also execute .xslt, which are useful for writting little scripts, such as listing all the textures in a collada document

The build creates the 'drop' folder, which should contain everything needed. I did check-in the necessary binaries in the 'drop' folder, so you can just run the application from the drop folder if you are not interested to build with sources.

Executable also available as zip package https://github.com/downloads/RemiArnaud/DAE-notepad/DAE%20notepad.zip

I would like to keep adding to this application so all the coherency tests can be added to the basic xml validation, and also maybe ignore validation errors returned by elements in the <extra> due to re-use of COLLADA elements

Remi Arnaud
contact remi (at) acm (dot) org

