dotnet build --configuration="Release"
remove-item ./ReverseHelper.zip
#looks like pwsh treats -x*.psd as an file.
winrar a -r "-X*.psd" -r "-X*.pdn" -afzip .\ReverseHelper.zip .\everest.yaml .\bin\net7.0 .\Graphics .\Loenn .\Ahorn .\Audio
#if Ahorn exist?
#for 7-zip
#7z a "-xr!*.psd" "-xr!*.pdn" -tzip .\ReverseHelper.zip .\everest.yaml .\bin .\Graphics .\Loenn .\Ahorn .\Audio