remove-item ./ReverseHelper.zip
#looks like pwsh treats -x*.psd as an file.
winrar a -r "-X*.psd" -afzip .\ReverseHelper.zip .\everest.yaml .\bin\Debug\net452 .\Graphics .\Loenn .\Ahorn
#if Ahorn exist?
#for 7-zip
#7z a "-xr!*.png" -tzip .\ReverseHelper.zip .\everest.yaml .\bin\Debug\net452 .\Graphics .\Loenn .\Ahorn