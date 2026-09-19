if (!(Test-Path .\Effects\ReverseHelper\)) {
    new-item -itemtype directory .\Effects\ReverseHelper\
}
fxc /T fx_2_0 .\Src\paralldox.fx /Fo .\Effects\ReverseHelper\paralldox.fxb
