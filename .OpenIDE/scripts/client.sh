#!/bin/bash 

# Script parameters
#	Param 1: Script run location
#	Param 2: global profile name
#	Param 3: local profile name
#	Param 4-: Any passed argument
#
# When calling oi use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
#
# To post back oi commands print command prefixed by command| to standard output
# To post a comment print to std output prefixed by comment|
# To post an error print to std output prefixed by error|

if [ "$2" = "get-command-definitions" ]; then
	# Definition format usually represented as a single line:

	# Script description|
	# command1|"Command1 description"
	# 	param|"Param description" end
	# end
	# command2|"Command2 description"
	# 	param|"Param description" end
	# end

	echo "Runs editor client from src/EditorEngine/bin/AutoTest.Net"
	exit
fi

options=""
if [ "$4" == "editor" ]; then
	options=$(oi conf read editor.*|grep executable|sed s/editor./--editor./g)
fi
mono --debug ReleaseBinaries/EditorClient.exe $1/src/EditorEngine/bin/AutoTest.Net "${@:4}" $options
