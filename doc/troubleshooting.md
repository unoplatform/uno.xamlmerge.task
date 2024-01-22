# Troubleshooting the Uno.XamlMerge.Task generation

If you encountered any issues using Uno.XamlMerge.Task:

- Make sure that all namespaces definitions across files are of the same values. (e.g. `xlmns:ns1="http://site1"` and `xlmns:ns1="http://site2"` in two different files will fail)
- If you include the same resource dictionary (e.g. Colors.xaml) file in multiple merged files (generally used to have a file for custom brushes and colors), you can remove those multiple inclusions.
