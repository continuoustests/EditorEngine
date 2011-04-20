#!/bin/bash

BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries

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

cp $BINARYDIR/Castle.* $DEPLOYDIR/
cp $BINARYDIR/Mono.Cecil.dll $DEPLOYDIR/
cp $BINARYDIR/EditorClient.exe $DEPLOYDIR/
cp $BINARYDIR/EditorEngine.exe $DEPLOYDIR/
cp $BINARYDIR/EditorEngine.Core.dll $DEPLOYDIR/
cp -r $BINARYDIR/gedit.dll $DEPLOYDIR/plugins
