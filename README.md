# mago-cloud-api
Samples about using the MagoCloud API

## Custom MagicLink profiles
It is not yet possible to create and edit custom profiles in MagoCloud, but it is allowed to upload them.

The custom profiles can be thus created using Mago4 Desktop and upload them in a MagoCloud subscription.

**Note**: the data model exposed by the profiles is backward compatible, so that you can use any Mago4 version released prior to the MagoCloud you are using.

To use a custom profile in MagicLink calls to MagoCloud:
1. create the profile as usual, as an AllUser type
1. locate the profile folder, its path looks like:
```
<your Mago4 installation>\Custom\Companies\<company>\Applications\ERP\<module>\ModuleObjects\<document>\ExportProfiles\AllUsers\<profile name>
```
i.e.: 
```
C:\Program Files (x86)\Microarea\Mago4\Custom\Companies\MyCompany\Applications\ERP\Contacts\ModuleObjects\Contacts\ExportProfiles\AllUsers\MyProfile
```
3. In MagoCloud select the "user" icon (top-right corner), then *TBFS Explorer*.
3. Open the three-dots menu, then select *Upload Profile Files*
3. Navigate to the right path (i.e.: `ERP > Contacts > Contacts`)
3. Create a new folder and name it after the profile (you will notice that the folder name is forced to lower-case)
3. upload the content fo the Mago4 profile folder (3 files normally)
3. in your code set the profile to use in the `xTechProfile` parameter and in the `xmlns` URI, in this way:
```
 <maxs:Contacts tbNamespace='Document.ERP.Contacts.Documents.Contacts' xTechProfile='MyProfile'    
      xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/AllUsers/MyProfile.xsd'>
 ```
 please note the `AllUsers` in the URI
