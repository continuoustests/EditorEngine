#!/bin/bash

BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
SOURCEDIR=$(cd $(dirname "$0"); pwd)/src

if [ ! -d $BINARYDIR ]; then
{
	mkdir $BINARYDIR
}
fi
if [ ! -d $DEPLOYDIR ]; then
{
	mkdir $DEPLOYDIR
}
fi

rm -r $BINARYDIR/*
rm -r $DEPLOYDIR/*
mkdir $DEPLOYDIR/plugins

echo $BINARYDIR

xbuild EditorEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

cp $BINARYDIR/Mono.Cecil.dll $DEPLOYDIR/
cp $BINARYDIR/EditorClient.exe $DEPLOYDIR/
cp $BINARYDIR/EditorEngine.exe $DEPLOYDIR/
cp $BINARYDIR/EditorEngine.Core.dll $DEPLOYDIR/
cp -r $BINARYDIR/gedit.dll $DEPLOYDIR/plugins
cp -r $BINARYDIR/vim.dll $DEPLOYDIR/plugins
cp -r $SOURCEDIR/Plugins/vim/vim.executable $DEPLOYDIR/plugins
cp -r $SOURCEDIR/Plugins/vim/vim.parameters $DEPLOYDIR/plugins
cp -r $BINARYDIR/configured.dll $DEPLOYDIR/plugins
cp -r $SOURCEDIR/Plugins/configured/configured.editor $DEPLOYDIR/plugins
cp -r $BINARYDIR/configured.dll $DEPLOYDIR/plugins/emacs.dll
cp -r $SOURCEDIR/Plugins/configured/Configurations/emacs.editor $DEPLOYDIR/plugins
cp -r $BINARYDIR/configured.dll $DEPLOYDIR/plugins/notepad++.dll
cp -r $SOURCEDIR/Plugins/configured/Configurations/notepad++.editor $DEPLOYDIR/plugins
cp -r $BINARYDIR/configured.dll $DEPLOYDIR/plugins/ultraedit.dll
cp -r $SOURCEDIR/Plugins/configured/Configurations/ultraedit.editor $DEPLOYDIR/plugins
cp -r $BINARYDIR/sublime.dll $DEPLOYDIR/plugins
cp -r $SOURCEDIR/Plugins/sublime/sublime.editor $DEPLOYDIR/plugins
